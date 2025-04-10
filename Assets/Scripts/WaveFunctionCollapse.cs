using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour {

    private const double MAX_ENTROPY = 100f;

    public int width;
    public int height;
    public float spriteLength = 1f;

    [SerializeField] private Sprite[] sprites;

    private TileData[,] map;
    private IndexedMinHeap indexdMinHeap;
    private Dictionary<int, TileTemplate> tileTemplateDic;
    private GameObject[,] goMap;

    // WFC的核心循环是 坍缩 - 传播约束 - 回溯

    private IEnumerator Start() {
        InitTileTemplates();
        InitMap();

        float startTime = Time.realtimeSinceStartup;
        var recordBeforeContaminate = new HashSet<TileData>();
        while (indexdMinHeap.Count > 0) {
            recordBeforeContaminate.Clear();
            // 记录污染之前的TileData状态
            // 坍缩
            TileData minEntropy = indexdMinHeap.ExtractMin();
            minEntropy.Record();
            recordBeforeContaminate.Add(minEntropy);
            RandomIdAndRotateTimes(minEntropy);
            minEntropy.isCollapsed = true;
            CreateSprite(minEntropy);

            // 传播约束，如果出现无解情况，则回到此次坍缩污染和传播污染前
            if (PropagateConstraint(minEntropy, ref recordBeforeContaminate)) {
                Debug.Log($"{minEntropy.x}, {minEntropy.y}");
                Backtrack(recordBeforeContaminate);
                yield return null;
            }
            //yield return null;
        }
        Debug.Log($"全部坍缩！用时：{Time.realtimeSinceStartup - startTime}");
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

    // todo 可改为读取scriptableobject
    public void InitTileTemplates() {
        

        //var white = new TileTemplate {
        //    id = 12,
        //    image = "white",
        //    weight = 1,
        //    edge = new string[] { "AAA", "AAA", "AAA", "AAA" }
        //};

        //var line = new TileTemplate {
        //    id = 13,
        //    image = "line",
        //    weight = 5,
        //    edge = new string[] { "ABA", "AAA", "ABA", "AAA" }
        //};

        //var circle = new TileTemplate {
        //    id = 15,
        //    image = "circle",
        //    weight = 4,
        //    edge = new string[] { "ABA", "ABA", "AAA", "AAA" }
        //};

        //tileTemplateDic[white.id] = white;
        //tileTemplateDic[line.id] = line;
        //tileTemplateDic[circle.id] = circle;

        var allTile = new List<TileTemplate> {
            new TileTemplate {
                id = 0,
                image = "empty",
                weight = 1,
                edge = new string[] { "EE", "EE", "EE", "EE"},
                describe = "无"
            },
            new TileTemplate {
                id = 1,
                image = "GB-LandTileset_0",
                weight = 10,
                edge = new string[] { "AA", "AA", "AA", "AA" },
                describe = "全草地"
            },
            new TileTemplate {
                id = 2,
                image = "GB-LandTileset_34",
                weight = 1,
                edge = new string[] { "AA", "BE", "EC", "AA" },
                describe = "↖河草"
            },
            new TileTemplate {
                id = 3,
                image = "GB-LandTileset_35",
                weight = 1,
                edge = new string[] { "AA", "AA", "DE", "EB" },
                describe = "↗河草"
            },
            new TileTemplate {
                id = 4,
                image = "GB-LandTileset_47",
                weight = 1,
                edge = new string[] { "CE", "EF", "AA", "AA" },
                describe = "↙河草"
            },
            new TileTemplate {
                id = 5,
                image = "GB-LandTileset_48",
                weight = 1,
                edge = new string[] { "ED", "AA", "AA", "FE" },
                describe = "↘河草"
            },


            new TileTemplate {
                id = 6,
                image = "GB-LandTileset_36",
                weight = 1,
                edge = new string[] { "AA", "BE", "EC", "AA" },
                describe = "↖河草"
            },
             new TileTemplate {
                id = 7,
                image = "GB-LandTileset_37",
                weight = 1,
                edge = new string[] { "AA", "BE", "EE", "EB" },
                describe = "↑河草"
            },
              new TileTemplate {
                id = 8,
                image = "GB-LandTileset_38",
                weight = 1,
                edge = new string[] { "AA", "AA", "DE", "EB" },
                describe = "↗河草"
            },
               new TileTemplate {
                id = 9,
                image = "GB-LandTileset_49",
                weight = 1,
                edge = new string[] { "CE", "EE", "EC", "AA" },
                describe = "←河草"
            },
            new TileTemplate {
                id = 10,
                image = "GB-LandTileset_50",
                weight = 1,
                edge = new string[] { "ED", "AA", "DE", "EE" },
                describe = "→河草"
            },
             new TileTemplate {
                id = 11,
                image = "GB-LandTileset_63",
                weight = 1,
                edge = new string[] { "CE", "EF", "AA", "AA" },
                describe = "↙河草"
            },
              new TileTemplate {
                id = 12,
                image = "GB-LandTileset_64",
                weight = 1,
                edge = new string[] { "EE", "EF", "AA", "FE" },
                describe = "↓河草"
            },
               new TileTemplate {
                id = 13,
                image = "GB-LandTileset_65",
                weight = 1,
                edge = new string[] { "ED", "AA", "AA", "FE" },
                describe = "↘河草"
            }
        };

        tileTemplateDic = new Dictionary<int, TileTemplate>();
        foreach (var item in allTile) {
            tileTemplateDic[item.id] = item;
        }
    }

    private void InitMap() {
        map = new TileData[height, width];
        goMap = new GameObject[height, width];
        indexdMinHeap = new IndexedMinHeap();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                List<int> defaultIds = tileTemplateDic.Keys.ToList();
                var td = new TileData {
                    x = x,
                    y = y,
                    ids = defaultIds,
                    validRotateTimes = TileData.InitValidRotate(defaultIds),
                    isCollapsed = false,
                    entropy = MAX_ENTROPY
                };
                map[y, x] = td;
                indexdMinHeap.Insert(td);
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
        Sprite sprite = sprites.Where(t => t.name == tileTemplateDic[id].image).FirstOrDefault();
        var go = new GameObject($"{sprite.name} ({td.x}, {td.y})");
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        go.transform.position = new Vector3(td.x * spriteLength, td.y * -spriteLength, 0);
        go.transform.localEulerAngles = new Vector3(0, 0, td.validRotateTimes[id][0] * -90f);
        goMap[td.y, td.x] = go;
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
