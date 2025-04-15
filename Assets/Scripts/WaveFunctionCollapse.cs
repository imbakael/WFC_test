using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour {

    private const double MAX_ENTROPY = 100f;

    public int width;
    public int height;
    public float spriteLength = 1f;

    public TileTemplate[] allTile;
    public bool useRotate = true;

    private TileData[,] map;
    private IndexedMinHeap indexdMinHeap;
    private Dictionary<int, TileTemplate> tileTemplateDic;
    private GameObject[,] goMap;

    // 表现
    private TextMeshPro[,] tmpMap;
    private Transform tileParent;
    private Transform tmpParent;

    // WFC的核心循环是 坍缩 - 传播约束 - 回溯

    private IEnumerator Start() {
        tileParent = GameObject.Find("Tiles").transform;
        tmpParent = GameObject.Find("Tmps").transform;

        InitTileTemplates();
        InitMap();

        float startTime = Time.realtimeSinceStartup;
        var recordBeforeContaminate = new HashSet<TileData>();
        while (indexdMinHeap.Count > 0) {
            SetAllTmpToWhite();

            recordBeforeContaminate.Clear();

            // 1.坍缩
            TileData minEntropy = indexdMinHeap.ExtractMin();
            minEntropy.Record();
            recordBeforeContaminate.Add(minEntropy);
            RandomIdAndRotateTimes(minEntropy);
            minEntropy.isCollapsed = true;
            CreateSprite(minEntropy);

            // 表现
            float beforeEntropy = (float)minEntropy.entropy;
            DOTween.To((t) => {
                tmpMap[minEntropy.y, minEntropy.x].text = Mathf.Lerp(beforeEntropy, 0f, t).ToString("f2");
            }, 0, 1f, 2f);
            tmpMap[minEntropy.y, minEntropy.x].color = Color.blue;
            minEntropy.entropy = 0;

            // 2.传播约束，如果出现无解情况，则回到此次坍缩污染和传播污染前
            if (PropagateConstraint(minEntropy, ref recordBeforeContaminate)) {
                Debug.Log($"backtrack to {minEntropy.x}, {minEntropy.y}");
                // 3.回溯
                Backtrack(recordBeforeContaminate);
                yield return null;
            }

            while (true) {
                yield return null;
                if (Input.GetKeyDown(KeyCode.Space)) {
                    break;
                }
            }
            
        }
        Debug.Log($"全部坍缩！用时：{Time.realtimeSinceStartup - startTime}");
    }

    private void Update() {
        
    }
    private void SetAllTmpToWhite() {
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                tmpMap[y, x].color = map[y, x].isCollapsed ? Color.green : Color.white;
            }
        }
    }

    private void Backtrack(HashSet<TileData> data) {
        foreach (TileData item in data) {
            double oldEntropy = item.entropy;
            bool hasCollapsed = item.Backtrack(ShannonEntropy);
            if (hasCollapsed) {
                indexdMinHeap.Insert(item);
                Destroy(goMap[item.y, item.x]);
            } else {
                indexdMinHeap.Update(item, oldEntropy, item.entropy);
            }
        }
    }

    private void RandomIdAndRotateTimes(TileData minEntropy) {
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
        } else {
            rId = minEntropy.ids[0];
        }
        // 从id中随机一个旋转方向
        List<int> vRotate = minEntropy.validRotateTimes[rId];
        int randomRotate = vRotate[UnityEngine.Random.Range(0, vRotate.Count)];
        for (int i = vRotate.Count - 1; i >= 0; i--) {
            if (vRotate[i] != randomRotate) {
                vRotate.RemoveAt(i);
            }
        }
    }

    public void InitTileTemplates() {
        tileTemplateDic = new Dictionary<int, TileTemplate>();
        foreach (var item in allTile) {
            tileTemplateDic[item.id] = item;
        }
    }

    private void InitMap() {
        map = new TileData[height, width];
        goMap = new GameObject[height, width];
        tmpMap = new TextMeshPro[height, width];
        indexdMinHeap = new IndexedMinHeap();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                List<int> defaultIds = tileTemplateDic.Keys.ToList();
                var td = new TileData {
                    x = x,
                    y = y,
                    ids = defaultIds,
                    validRotateTimes = TileData.InitValidRotate(defaultIds, useRotate),
                    isCollapsed = false,
                    entropy = MAX_ENTROPY
                };
                map[y, x] = td;
                indexdMinHeap.Insert(td);

                var textGo = new GameObject($"{x}, {y}");
                TextMeshPro tmp = textGo.AddComponent<TextMeshPro>();
                textGo.transform.SetParent(tmpParent, false);
                textGo.transform.localPosition = new Vector3(x * spriteLength, y * -spriteLength, 0);
                tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 4;
                tmp.text = td.entropy == MAX_ENTROPY ? "100" : td.entropy.ToString("f2");
                tmpMap[y, x] = tmp;
            }
        }
    }

    private int GetRandomTile(List<int> ids) {
        double sumWeight = ids.Sum(t => tileTemplateDic[t].weight);
        float random = UnityEngine.Random.Range(0, (float)sumWeight);
        double curP = 0f;
        for (int i = 0; i < ids.Count; i++) {
            curP += tileTemplateDic[ids[i]].weight;
            if (random <= curP) {
                return ids[i];
            }
        }
        return ids[0];
    }

    // 熵值计算，熵越小说明混乱程度越低，确定性越高。例如某个瓦片都有A和B两个瓦片的可能性，但是A只有0,1两种旋转，B有0,1,2三种旋转
    // 根据公式，因为B的旋转次数更多，所以熵更高，确定性更低
    private double ShannonEntropy(TileData td) {
        double sumWeight = 0f;
        double sumWeightLog = 0f;
        for (int i = 0; i < td.ids.Count; i++) {
            int id = td.ids[i];
            double curWeight = tileTemplateDic[id].weight;
            sumWeight += curWeight;
            int rotateTimes = td.validRotateTimes[id].Count;
            sumWeightLog += curWeight * Math.Log(curWeight / rotateTimes);
        }
        return Math.Log(sumWeight) - (sumWeightLog / sumWeight);
    }

    private bool PropagateConstraint(TileData curTile, ref HashSet<TileData> record) {
        bool isZero = false;
        var tempStack = new Stack<TileData>();
        tempStack.Push(curTile);
        while (tempStack.Count != 0) {
            TileData tile = tempStack.Pop();
            for (int direction = 0; direction < 4; direction++) {
                int x = GetDeltaXByDirection(tile.x, direction);
                int y = GetDeltaYByDirection(tile.y, direction);

                if (IsPosValid(x, y) && map[y, x].isCollapsed == false) {
                    HashSet<string> allEdgeInDirection = TileData.GetAllEdgeInDirection(tile, direction, GetEdgeById);
                    TileData neighbor = map[y, x];
                    if (!record.Contains(neighbor)) {
                        neighbor.Record();
                    }

                    if (TileData.Filter(allEdgeInDirection, direction, neighbor, GetEdgeById, out isZero)) {
                        record.Add(neighbor);
                        if (isZero) {
                            return true;
                        }
                        tempStack.Push(neighbor);
                        double oldEntropy = neighbor.entropy;
                        neighbor.entropy = ShannonEntropy(neighbor);
                        indexdMinHeap.Update(neighbor, oldEntropy, neighbor.entropy);

                        // 表现
                        DOTween.To((t) => {
                            tmpMap[neighbor.y, neighbor.x].text = Mathf.Lerp((float)oldEntropy, (float)neighbor.entropy, t).ToString("f2");
                        }, 0, 1f, 2f);
                        tmpMap[neighbor.y, neighbor.x].color = Color.red;
                    }
                }
            }
        }
        return false;
    }

    private string[] GetEdgeById(int id) {
        return tileTemplateDic[id].edge;
    }

    private void CreateSprite(TileData td) {
        int id = td.ids[0];
        Sprite sprite = tileTemplateDic[id].sprite;
        var go = new GameObject($"{tileTemplateDic[id].describe} : ({td.x}, {td.y})");
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        go.transform.position = new Vector3(td.x * spriteLength, td.y * -spriteLength, 0);
        go.transform.localEulerAngles = new Vector3(0, 0, td.validRotateTimes[id][0] * -90f);
        goMap[td.y, td.x] = go;
        go.transform.SetParent(tileParent, false);
    }

    private bool IsPosValid(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private int GetDeltaXByDirection(int originX, int direction) {
        return originX + (direction == 1 ? 1 : direction == 3 ? -1 : 0);
    }

    private int GetDeltaYByDirection(int originY, int direction) {
        return originY + (direction == 0 ? -1 : direction == 2 ? 1 : 0);
    }

}
