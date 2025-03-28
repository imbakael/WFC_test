using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour {

    private const float MAX_ENTROPY = 100f;

    public int width;
    public int height;

    [SerializeField] private Sprite[] sprites;

    private TileData[,] map;
    private List<TileData> notCollapsedMap;
    private Dictionary<int, TileTemplate> tileTemplateDic;
    private Dictionary<float, float> entropyCache;
    private int collapseCount = 0;
    // 性能优化
    private Stack<TileData> tempStack = new Stack<TileData>();
    private float[] entropyCacheArray;
    // test
    private int modifyCount = 0;

    // WFC的核心循环是 坍缩 - 传播约束 - 回溯

    private IEnumerator Start() {
        InitTileTemplates();
        InitMap();

        int randomX = width / 2;
        int randomY = height / 2;
        int randomId = 3;
        map[randomY, randomX].ids = new List<int> { randomId };
        map[randomY, randomX].isCollapsed = true;
        collapseCount++;
        TileData curTile = map[randomY, randomX];
        notCollapsedMap.Remove(curTile);
        CreateSprite(randomX, randomY);
        yield return new WaitForSeconds(0.1f);

        float startTime = Time.realtimeSinceStartup;
        while (!IsAllCollapsed()) {
            // 传播约束
            PropagateConstraint(curTile);

            // 坍缩
            //TileData minEntropy = notCollapsedMap.OrderBy(t => CalcEntropy(t)).First();
            TileData minEntropy = notCollapsedMap.OrderBy(t => entropyCacheArray[t.y * width + t.x]).First();

            //notCollapsedMap.Sort((a, b) => {
            //    return entropyCacheArray[b.y * width + b.x].CompareTo(entropyCacheArray[a.y * width + a.x]);
            //});

            //TileData minEntropy = notCollapsedMap[^1];
            //TileData minEntropy = notCollapsedMap[0];
            int rId = GetRandomTile(minEntropy.ids);
            for (int i = minEntropy.ids.Count - 1; i >= 0; i--) {
                if (minEntropy.ids[i] != rId) {
                    minEntropy.ids.RemoveAt(i);
                }
            }
            minEntropy.isCollapsed = true;
            notCollapsedMap.Remove(minEntropy);
            //notCollapsedMap.RemoveAt(notCollapsedMap.Count - 1);
            collapseCount++;
            CreateSprite(minEntropy.x, minEntropy.y);
            curTile = minEntropy;

        }
        Debug.Log($"全部坍缩！用时：{Time.realtimeSinceStartup - startTime}, modifyCount = {modifyCount}");
    }

    // todo 可改为读取scriptableobject
    public void InitTileTemplates() {
        var blank = new TileTemplate {
            id = 3,
            image = "blank",
            p = 0.6f,
            edge = new string[] { "AAA", "AAA", "AAA", "AAA" }
        };
        var up = new TileTemplate {
            id = 5,
            image = "up",
            p = 0.1f,
            edge = new string[] { "ABA", "ABA", "AAA", "ABA" }
        };
        var right = new TileTemplate {
            id = 7,
            image = "right",
            p = 0.1f,
            edge = new string[] { "ABA", "ABA", "ABA", "AAA" }
        };
        var down = new TileTemplate {
            id = 9,
            image = "down",
            p = 0.1f,
            edge = new string[] { "AAA", "ABA", "ABA", "ABA" }
        };
        var left = new TileTemplate {
            id = 11,
            image = "left",
            p = 0.1f,
            edge = new string[] { "ABA", "AAA", "ABA", "ABA" }
        };

        tileTemplateDic = new Dictionary<int, TileTemplate>();
        tileTemplateDic[blank.id] = blank;
        tileTemplateDic[up.id] = up;
        tileTemplateDic[right.id] = right;
        tileTemplateDic[down.id] = down;
        tileTemplateDic[left.id] = left;

        entropyCache = new Dictionary<float, float>();
        foreach (TileTemplate tt in tileTemplateDic.Values) {
            if (!entropyCache.ContainsKey(tt.p)) {
                entropyCache[tt.p] = -tt.p * Mathf.Log(tt.p, 2);
            }
        }
    }

    private void InitMap() {
        map = new TileData[height, width];
        notCollapsedMap = new List<TileData>();
        entropyCacheArray = new float[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                var t = new TileData {
                    x = x,
                    y = y,
                    ids = tileTemplateDic.Keys.ToList(),
                    isCollapsed = false
                };
                map[y, x] = t;
                notCollapsedMap.Add(t);
                entropyCacheArray[y * width + x] = MAX_ENTROPY;
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
                    TileData neighbor = map[y, x];
                    int before = neighbor.ids.Count;
                    for (int j = neighbor.ids.Count - 1; j >= 0; j--) {
                        if (tile.ids.Any(t => CompareTile(t, neighbor.ids[j], direction))) {
                            continue;
                        } else {
                            neighbor.ids.RemoveAt(j);
                        }
                    }
                    if (before != neighbor.ids.Count) {
                        tempStack.Push(neighbor);
                        entropyCacheArray[y * width + x] = CalcEntropy(neighbor);
                        modifyCount++;
                    }
                }
            }
        }
    }

    private void CreateSprite(int x, int y) {
        Sprite sprite = sprites.Where(t => t.name == tileTemplateDic[map[y, x].ids[0]].image).FirstOrDefault();
        var go = new GameObject(sprite.name);
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        go.transform.position = new Vector3(x * 0.5f, y * -0.5f, 0);
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

    private bool IsAllCollapsed() {
        return collapseCount == width * height;
    }

    private bool CompareTile(int tileId, int otherTileId, int direction) {
        return CompareTile(tileTemplateDic[tileId], tileTemplateDic[otherTileId], direction);
    }

    private bool CompareTile(TileTemplate a, TileTemplate b, int direction) {
        string edgeA = a.edge[direction];
        string edgeB = b.edge[direction > 1 ? direction - 2 : direction + 2];
        return Util.IsReverseEqual(edgeA, edgeB);
    }

}
