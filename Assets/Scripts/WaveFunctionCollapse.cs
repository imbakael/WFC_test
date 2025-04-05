using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour {

    private const float MAX_ENTROPY = 100f;

    public int width;
    public int height;

    [SerializeField] private Sprite[] sprites;

    private TileData[,] map;
    private IndexedMinHeap indexdMinHeap;
    private Dictionary<int, TileTemplate> tileTemplateDic;

    // 性能优化
    private Dictionary<float, float> entropyCache;
    private Stack<TileData> tempStack = new Stack<TileData>();
    // test
    private int modifyCount = 0;

    // WFC的核心循环是 坍缩 - 传播约束 - 回溯

    private IEnumerator Start() {
        InitTileTemplates();
        InitMap();

        int randomX = width / 2;
        int randomY = height / 2;
        int randomId = 15;
        map[randomY, randomX].ids = new List<int> { randomId };
        map[randomY, randomX].validRotateTimes = new Dictionary<int, List<int>> {
            { randomId, new List<int> { 0 } }
        };
        map[randomY, randomX].entropy = CalcEntropy(map[randomY, randomX]);
        map[randomY, randomX].isCollapsed = true;
        TileData curTile = map[randomY, randomX];
        indexdMinHeap.Remove(curTile);

        CreateSprite(curTile);
        yield return new WaitForSeconds(0.1f);

        float startTime = Time.realtimeSinceStartup;
        while (indexdMinHeap.Count > 0) {
            // 传播约束
            PropagateConstraint(curTile);

            // 坍缩
            TileData minEntropy = indexdMinHeap.ExtractMin();

            // 从ids中随机一个id
            int rId;
            if (minEntropy.ids.Count > 1) {
                rId = GetRandomTile(minEntropy.ids);
                for (int i = minEntropy.ids.Count - 1; i >= 0; i--) {
                    int curId = minEntropy.ids[i];
                    if (curId != rId) {
                        minEntropy.validRotateTimes.Remove(curId);
                        minEntropy.ids.RemoveAt(i);
                    }
                }
            } else if (minEntropy.ids.Count == 1){
                rId = minEntropy.ids[0];
            } else {
                Debug.Log($"出错坐标 = {minEntropy.x}, {minEntropy.y}");
                for (int d = 0; d < 4; d++) {
                    PrintLog(minEntropy, d);
                }
                yield break;
            }
            // 从id中随机一个旋转方向
            List<int> vRotate = minEntropy.validRotateTimes[rId];
            int randomRotate = vRotate[UnityEngine.Random.Range(0, vRotate.Count)];
            for (int i = vRotate.Count - 1; i >= 0; i--) {
                if (vRotate[i] != randomRotate) {
                    vRotate.RemoveAt(i);
                }
            }

            minEntropy.isCollapsed = true;
            CreateSprite(minEntropy);
            curTile = minEntropy;
            //yield return new WaitForSeconds(0.1f);
        }
        Debug.Log($"全部坍缩！用时：{Time.realtimeSinceStartup - startTime}, modifyCount = {modifyCount}");
    }

    private void PrintLog(TileData td, int direction) {
        var directionInfo = new List<string> { "上方", "右方", "下方", "左方" };
        int x = GetDeltaXByDirection(td.x, direction);
        int y = GetDeltaYByDirection(td.y, direction);
        if (IsPosValid(x, y)) {
            Debug.Log($"{directionInfo[direction]}: {x}, {y}");
            foreach (var item in map[y, x].validRotateTimes) {
                int id = item.Key;
                List<int> rotateTime = item.Value;
                string sum = "";
                for (int i = 0; i < rotateTime.Count; i++) {
                    sum += rotateTime[i];
                }
                Debug.Log($"id : {id}, rotate : {sum}");
            }
        }
    }

    // todo 可改为读取scriptableobject
    public void InitTileTemplates() {
        var white = new TileTemplate {
            id = 12,
            image = "white",
            p = 0.2f,
            edge = new string[] { "AAA", "AAA", "AAA", "AAA" }
        };

        var line = new TileTemplate {
            id = 13,
            image = "line",
            p = 0.3f,
            edge = new string[] { "ABA", "AAA", "ABA", "AAA" }
        };

        var circle = new TileTemplate {
            id = 15,
            image = "circle",
            p = 0.5f,
            edge = new string[] { "ABA", "ABA", "AAA", "AAA" }
        };

        tileTemplateDic = new Dictionary<int, TileTemplate>();
        tileTemplateDic[white.id] = white;
        tileTemplateDic[line.id] = line;
        tileTemplateDic[circle.id] = circle;

        entropyCache = new Dictionary<float, float>();
        foreach (TileTemplate tt in tileTemplateDic.Values) {
            if (!entropyCache.ContainsKey(tt.p)) {
                entropyCache[tt.p] = -tt.p * Mathf.Log(tt.p, 2);
            }
        }
    }

    private void InitMap() {
        map = new TileData[height, width];
        indexdMinHeap = new IndexedMinHeap();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                List<int> defaultIds = tileTemplateDic.Keys.ToList();
                var t = new TileData {
                    x = x,
                    y = y,
                    ids = defaultIds,
                    validRotateTimes = TileData.InitValidRotate(defaultIds),
                    isCollapsed = false,
                    entropy = MAX_ENTROPY
                };
                map[y, x] = t;
                indexdMinHeap.Insert(t);
            }
        }
    }

    private int GetRandomTile(List<int> ids) {
        float random = UnityEngine.Random.Range(0, 1f);
        float sumP = ids.Sum(t => tileTemplateDic[t].p);
        float curP = 0f;
        for (int i = 0; i < ids.Count; i++) {
            curP += tileTemplateDic[ids[i]].p / sumP;
            if (random <= curP) {
                return ids[i];
            }
        }
        return ids[0];
    }

    private float CalcEntropy(TileData td) {
        if (td.ids.Count == tileTemplateDic.Count) {
            return MAX_ENTROPY;
        }
        float sum = 0f;
        for (int i = 0; i < td.ids.Count; i++) {
            int id = td.ids[i];
            float p = tileTemplateDic[id].p;
            sum += entropyCache[p];
        }
        return sum;
    }

    private void PropagateConstraint(TileData curTile) {
        tempStack.Push(curTile);
        while (tempStack.Count != 0) {
            TileData tile = tempStack.Pop(); 
            for (int direction = 0; direction < 4; direction++) {
                int x = GetDeltaXByDirection(tile.x, direction);
                int y = GetDeltaYByDirection(tile.y, direction);

                if (IsPosValid(x, y) && map[y, x].isCollapsed == false) {
                    HashSet<string> allEdgeInDirection = TileData.GetAllEdgeInDirection(tile, direction, GetEdgeById);
                    TileData neighbor = map[y, x];
                    if (TileData.Filter(allEdgeInDirection, direction, neighbor, GetEdgeById)) {
                        tempStack.Push(neighbor);
                        float newEntropy = CalcEntropy(neighbor);
                        indexdMinHeap.Update(neighbor, newEntropy);
                        modifyCount++;
                    }
                }
            }
        }
    }

    private string[] GetEdgeById(int id) {
        return tileTemplateDic[id].edge;
    }

    private void CreateSprite(TileData td) {
        int id = td.ids[0];
        Sprite sprite = sprites.Where(t => t.name == tileTemplateDic[id].image).FirstOrDefault();
        var go = new GameObject(sprite.name + td.x + "," + td.y);
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        go.transform.position = new Vector3(td.x * 0.5f, td.y * -0.5f, 0);
        go.transform.localEulerAngles = new Vector3(0, 0, td.validRotateTimes[id][0] * -90f);
    }

    private bool IsPosValid(int x, int y) {
        return x >= 0 && x < map.GetLength(1) && y >= 0 && y < map.GetLength(0);
    }

    private int GetDeltaXByDirection(int originX, int direction) {
        return originX + (direction == 1 ? 1 : direction == 3 ? -1 : 0);
    }

    private int GetDeltaYByDirection(int originY, int direction) {
        return originY + (direction == 0 ? -1 : direction == 2 ? 1 : 0);
    }

}
