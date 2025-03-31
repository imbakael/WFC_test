using System.Collections;
using System.Collections.Generic;

// Tileģ�棬������edge�ֶ�
public class TileTemplate {
    public int id;
    public string image;
    public float p; // ��tile�ĸ��ʣ�����tile��p�ĺ͵���1
    public string[] edge; // ��Ϊ���������ĸ�����Ĳ��slot�����統ǰtile�Ҳ�����"ABCCCD"��
                          // ��Ҫ�������"DCCCBA"����ƥ�䣬��Ϊtile��˳ʱ����ת������ÿ������ַ����ǰ�˳ʱ ��ķ�����ж�ȡ
}

// Tile��״̬����
public class TileData {
    public int x;
    public int y;
    public List<int> ids;
    public int rotateTimes; // 0��ʾ˳ʱ����ת0�Σ������ת3�Σ���ת���Ӧ��edgeҲҪ������
    public bool isCollapsed;
    public float entropy;
}
