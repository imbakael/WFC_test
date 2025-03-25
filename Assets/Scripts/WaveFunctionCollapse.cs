using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour {

    public int width;
    public int height;

    [SerializeField] private Sprite[] sprites;

    private TileData[,] map;
    private List<TileData> mapList;
    private Dictionary<int, TileTemplate> tileTemplateDic;
    private int collapseCount = 0;

    // WFC的核心循环是 坍缩 - 传播约束 - 回溯

    private IEnumerator Start() {
        InitTileTemplates();
        map = new TileData[height, width];
        mapList = new List<TileData>();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                TileData t = new TileData {
                    x = x,
                    y = y,
                    ids = tileTemplateDic.Keys.ToList(),
                    isCollapsed = false
                };
                map[y, x] = t;
                mapList.Add(t);
            }
        }

        // 1、随机坍缩一个位置
        int randomX = UnityEngine.Random.Range(0, width);
        int randomY = UnityEngine.Random.Range(0, height);
        int randomId = tileTemplateDic.Keys.ToList()[UnityEngine.Random.Range(0, tileTemplateDic.Count)];
        map[randomY, randomX].ids = new List<int> { randomId };
        map[randomY, randomX].isCollapsed = true;
        collapseCount++;
        TileData curTile = map[randomY, randomX];
        CreateSprite(randomX, randomY);
        yield return new WaitForSeconds(0.5f);

        // 2、只要map没有全部坍缩，就不断查找最低可能性的一条边进行坍缩
        while (!IsAllCollapsed()) {
            // 约束传播
            PropagateConstraint(curTile);

            // 坍缩
            // todo 求熵
            TileData minPossible = mapList.Where(t => t.isCollapsed == false).OrderBy(t => t.ids.Count).First();
            int rId = minPossible.ids[UnityEngine.Random.Range(0, minPossible.ids.Count)];
            for (int i = minPossible.ids.Count - 1; i >= 0; i--) {
                if (minPossible.ids[i] != rId) {
                    minPossible.ids.RemoveAt(i);
                }
            }
            minPossible.isCollapsed = true;
            collapseCount++;
            CreateSprite(minPossible.x, minPossible.y);
            curTile = minPossible;
            yield return new WaitForSeconds(0.5f);

        }
        Debug.Log("全部坍缩！");
    }

    private void PropagateConstraint(TileData curTile) {
        var queue = new Queue<TileData>();
        queue.Enqueue(curTile);
        while (queue.Count != 0) {
            TileData tile = queue.Dequeue(); 
            for (int i = 0; i < 4; i++) {
                int x = GetDeltaXByDirection(tile.x, i);
                int y = GetDeltaYByDirection(tile.y, i);

                if (IsPosValid(x, y) && map[y, x].isCollapsed == false) {
                    TileData neighbor = map[y, x];
                    int before = neighbor.ids.Count;
                    for (int j = neighbor.ids.Count - 1; j >= 0; j--) {
                        if (tile.ids.Any(t => CompareTile(t, neighbor.ids[j], i))) {
                            continue;
                        } else {
                            neighbor.ids.RemoveAt(j);
                        }
                    }
                    if (before != neighbor.ids.Count) {
                        queue.Enqueue(neighbor);
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

    // todo 可改为读取scriptableobject
    public void InitTileTemplates() {
        var blank = new TileTemplate {
            id = 3,
            image = "blank",
            edge = new string[] { "AAA", "AAA", "AAA", "AAA" }
        };
        var up = new TileTemplate {
            id = 5,
            image = "up",
            edge = new string[] { "ABA", "ABA", "AAA", "ABA"}
        };
        var right = new TileTemplate {
            id = 7,
            image = "right",
            edge = new string[] { "ABA", "ABA", "ABA", "AAA" }
        };
        var down = new TileTemplate {
            id = 9,
            image = "down",
            edge = new string[] { "AAA", "ABA", "ABA", "ABA" }
        };
        var left = new TileTemplate {
            id = 11,
            image = "left",
            edge = new string[] { "ABA", "AAA", "ABA", "ABA" }
        };

        tileTemplateDic = new Dictionary<int, TileTemplate>();
        tileTemplateDic[blank.id] = blank;
        tileTemplateDic[up.id] = up;
        tileTemplateDic[right.id] = right;
        tileTemplateDic[down.id] = down;
        tileTemplateDic[left.id] = left;
    }

    private bool CompareTile(TileData a, TileTemplate b, int direction) {
        return CompareTile(tileTemplateDic[a.ids[0]], b, direction);
    }

    private bool CompareTile(int tileId, int otherTileId, int direction) {
        return CompareTile(tileTemplateDic[tileId], tileTemplateDic[otherTileId], direction);
    }

    private bool CompareTile(TileTemplate a, TileTemplate b, int direction) {
        string edgeA = a.edge[direction];
        string edgeB = b.edge[direction > 1 ? direction - 2 : direction + 2];
        return CompareEdge(edgeA, edgeB);
    }

    private bool CompareEdge(string a, string b) {
        var reverseA = new string(a.Reverse().ToArray());
        return reverseA == b;
    }
}
