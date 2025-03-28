using System.Collections;
using System.Collections.Generic;

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
    public List<int> ids;
    public int rotateTimes; // 0表示顺时针旋转0次，最多旋转3次，旋转后对应的edge也要做处理
    public bool isCollapsed;
}
