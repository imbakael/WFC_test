using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour {

    public int width;
    public int height;

    [SerializeField] private Sprite[] sprites;

    private TileData[,] map;
    private Dictionary<int, TileTemplate> tileTemplateDic;

    // 1���ȹ���map���ٸ���map����mapSprites

    private IEnumerator Start() {
        InitTileTemplates();
        map = new TileData[height, width];

        // 1�����ȡһ��λ�ã��趨һ�����tile
        int randomX = Random.Range(0, width);
        int randomY = Random.Range(0, height);
        int randomId = tileTemplateDic.Keys.ToList()[Random.Range(0, tileTemplateDic.Count)];
        TileData curTile = map[randomY, randomX] = new TileData { 
            id = randomId,
            isCollapsed = true
        };
        CreateSprite(randomX, randomY);
        yield return new WaitForSeconds(0.7f);

        (int, int) curPos = (randomX, randomY);
        // 2��ֻҪmapû��ȫ��̮�����Ͳ��ϲ�����Ϳ����Ե�һ���߽���̮��
        while (!IsAllCollapsed()) {
            (int, int, int) posAndId = GetNextPosAndTileId(curPos, curTile);
            (int, int) nextPos = (posAndId.Item1, posAndId.Item2);
            curTile = map[nextPos.Item2, nextPos.Item1] = new TileData {
                id = posAndId.Item3,
                isCollapsed = true
            };
            curPos = nextPos;
            CreateSprite(nextPos.Item1, nextPos.Item2);
            yield return new WaitForSeconds(0.7f);
        }

        // 3������map����sprite
        //for (int y = 0; y < map.GetLength(0); y++) {
        //    for (int x = 0; x < map.GetLength(1); x++) {
        //        CreateSprite(x, y);
        //        //Sprite sprite = sprites.Where(t => t.name == tileTemplateDic[map[y, x].id].image).FirstOrDefault();
        //        //var go = new GameObject(sprite.name);
        //        //go.AddComponent<SpriteRenderer>().sprite = sprite;
        //        //go.transform.position = new Vector3(x * 0.5f, y * -0.5f, 0);
        //    }
        //}

    }

    private void CreateSprite(int x, int y) {
        Sprite sprite = sprites.Where(t => t.name == tileTemplateDic[map[y, x].id].image).FirstOrDefault();
        var go = new GameObject(sprite.name);
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        go.transform.position = new Vector3(x * 0.5f, y * -0.5f, 0);
    }

    private (int, int, int) GetNextPosAndTileId((int, int) curPos, TileData curTile) {
        int minPossibleCount = tileTemplateDic.Count;
        (int, int) nextPos = (-1, -1);
        int randomTileId = -1;
        for (int i = 0; i < 4; i++) {
            int x = GetDeltaXByDirection(curPos.Item1, i);
            int y = GetDeltaYByDirection(curPos.Item2, i);

            if (IsPosValid(x, y) && (map[y, x] == null || map[y, x].isCollapsed == false)) {
                var nextPossible = new List<int>();
                // ֻ����curTileһ����Ƭ��i����ʱ��nextPossible�����п�������Ƭ
                foreach (TileTemplate item in tileTemplateDic.Values) {
                    if (CompareTile(curTile, item, i)) {
                        nextPossible.Add(item.id);
                    }
                }

                int curDirection = i > 1 ? i - 2 : i + 2;
                RmoveInvalid(nextPossible, x, y, curDirection);
                // todo һ����������ͬ����Ҫ���ݣ���ÿ��̮��ǰ������һ�ε�ǰ���գ�һ��̮����������ͬ����ݵ�ǰ����
                Debug.Assert(nextPossible.Count != 0, "nextPossible.count == 0!");
                if (nextPossible.Count == 0) {
                    return (nextPos.Item1, nextPos.Item2, randomTileId);
                }

                if (nextPossible.Count < minPossibleCount) {
                    minPossibleCount = nextPossible.Count;
                    nextPos = (x, y);
                    randomTileId = nextPossible[Random.Range(0, nextPossible.Count)];
                }
            }
        }
        return (nextPos.Item1, nextPos.Item2, randomTileId);
    }

    private bool IsPosValid(int x, int y) {
        return x >= 0 && x < map.GetLength(1) && y >= 0 && y < map.GetLength(0);
    }

    private void RmoveInvalid(List<int> curPossible, int x, int y, int direction) {
        for (int i = 0; i < 4; i++) {
            if (i != direction) {
                int x1 = GetDeltaXByDirection(x, i);
                int y1 = GetDeltaYByDirection(y, i);
                if (IsPosValid(x1, y1) && map[y1, x1] != null && map[y1, x1].isCollapsed == true) {
                    for (int k = curPossible.Count - 1; k >= 0; k--) {
                        if (!CompareTile(curPossible[k], map[y1, x1].id, i)) {
                            curPossible.RemoveAt(k);
                        }
                    }
                }
            }
        }
    }

    private int GetDeltaXByDirection(int originX, int direction) {
        return originX + (direction == 1 ? 1 : direction == 3 ? -1 : 0);
    }

    private int GetDeltaYByDirection(int originY, int direction) {
        return originY + (direction == 0 ? -1 : direction == 2 ? 1 : 0);
    }

    private bool IsAllCollapsed() {
        for (int y = 0; y < map.GetLength(0); y++) {
            for (int x = 0; x < map.GetLength(1); x++) {
                if (map[y, x] == null || map[y, x].isCollapsed == false) {
                    return false;
                }
            }
        }
        return true;
    }

    // todo �ɸ�Ϊ��ȡscriptableobject
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
        return CompareTile(tileTemplateDic[a.id], b, direction);
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
