using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tile模版，核心是edge字段
public class TileTemplate {
    public int id;
    public string image;
    public float p; // 此tile的概率，所有tile的p的和等于1
    public string[] edge; // 分为上右下左四个方向的插槽slot，比如当前tile右侧插槽是"ABCCCD"，
                          // 需要左侧插槽是"DCCCBA"才能匹配，因为tile会顺时针旋转，所以每条插槽字符串是按顺时 针的方向进行读取
}

// Tile的状态数据
public class TileData {
    public int x;
    public int y;
    public List<int> ids; // 该Tile的所有id，最终会坍缩成只剩一个id
    public Dictionary<int, List<int>> validRotateTimes; // 每个id对应的可用的旋转数，0表示顺时针旋转0次，最多旋转3次，最终会坍缩成只剩一个旋转数
    public bool isCollapsed;
    public float entropy;

    public static Dictionary<int, List<int>> InitValidRotate(List<int> ids) {
        var validRotate = new Dictionary<int, List<int>>();
        for (int i = 0; i < ids.Count; i++) {
            validRotate[i] = new List<int> { 0, 1, 2, 3 };
        }
        return validRotate;
    }

    /// <summary>
    /// 根据某个方向的边界集合，过滤TileData中的不匹配id和旋转次数
    /// </summary>
    /// <param name="edges"></param>
    /// <param name="direction"></param>
    /// <param name="td"></param>
    /// <param name="GetEdgeById"></param>
    public void Filter(string[] edges, int direction, TileData td, Func<int, string[]> GetEdgeById) {
        for (int i = td.ids.Count - 1; i >= 0; i--) {
            int curId = td.ids[i];
            string[] curEdge = GetEdgeById(curId);
            if (edges.Any(t => HasAtLeastOneEdgeFit(curEdge, t))) {
                continue;
            }
            td.ids.RemoveAt(i);
            td.validRotateTimes.Remove(curId);
        }
        int reverseDirection = (direction + 2) % 4;
        foreach (var item in td.validRotateTimes) {
            int id = item.Key;
            List<int> validRotate = item.Value;
            string[] curEdge = GetEdgeById(id);
            for (int i = validRotate.Count - 1; i >= 0; i--) {
                string rotEdge = GetEdgeByRotateAndDirection(curEdge, validRotate[i], reverseDirection);
                if (edges.Any(t => Util.IsReverseEqual(t, rotEdge))) {
                    continue;
                }
                validRotate.RemoveAt(i);
            }
        }  
    }

    private bool HasAtLeastOneEdgeFit(string[] curEdge, string targetEdge) {
        return curEdge.Any(t => Util.IsReverseEqual(t, targetEdge));
    }

    /// <summary>
    /// 根据初始边界，旋转次数，获得对应方向的边界
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="rotateTimes"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static string GetEdgeByRotateAndDirection(string[] edge, int rotateTimes, int direction) {
        int delta = direction - rotateTimes;
        if (delta < 0) {
            delta += 4;
        }
        return edge[delta];
    }
}
