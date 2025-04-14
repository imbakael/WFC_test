using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileTemplate))]
public class TileTemplateEditor : Editor {

    private TileTemplate tt;
    private float spriteShowWidth = 128f;
    private float textShowWidth = 50f;

    private string upEdge;
    private string leftEdge;
    private string rightEdge;
    private string downEdge;

    private Sprite upSprite;
    private Sprite rightSprite;
    private Sprite downSprite;
    private Sprite leftSprite;

    private void OnEnable() {
        tt = (TileTemplate)target;
        if (tt.edge == null) {
            return;
        }
        upEdge = tt.edge[0];
        rightEdge = tt.edge[1];
        downEdge = tt.edge[2];
        leftEdge = tt.edge[3];
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (tt.sprite == null) {
            return;
        }
        var texture = AssetPreview.GetAssetPreview(tt.sprite);
        if (texture == null) {
            return;
        }

        texture.filterMode = FilterMode.Point;
        var textureRect = new Rect(EditorGUIUtility.currentViewWidth / 2 - spriteShowWidth / 2, 300, spriteShowWidth, spriteShowWidth);
        GUI.DrawTexture(textureRect, texture);

        if (upSprite != null) {
            DrawTextureByDirection(upSprite, 0);
        }
        if (rightSprite != null) {
            DrawTextureByDirection(rightSprite, 1);
        }
        if (downSprite != null) {
            DrawTextureByDirection(downSprite, 2);
        }
        if (leftSprite != null) {
            DrawTextureByDirection(leftSprite, 3);
        }

        var style = new GUIStyle(GUI.skin.textField) {
            alignment = TextAnchor.MiddleCenter
        };

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUIUtility.currentViewWidth / 2 - textShowWidth / 2);
        upEdge = GUILayout.TextField(upEdge, style, GUILayout.Width(textShowWidth), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(spriteShowWidth / 2f + 20f);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUIUtility.currentViewWidth / 2 - spriteShowWidth / 2 - textShowWidth * 1.5F);
        leftEdge = GUILayout.TextField(leftEdge, style, GUILayout.Width(textShowWidth), GUILayout.Height(20));
        GUILayout.Space(spriteShowWidth + textShowWidth * 0.5f);
        rightEdge = GUILayout.TextField(rightEdge, style, GUILayout.Width(textShowWidth), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(spriteShowWidth / 2f + 20f);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUIUtility.currentViewWidth / 2 - textShowWidth / 2);
        downEdge = GUILayout.TextField(downEdge, style, GUILayout.Width(textShowWidth), GUILayout.Height(20));

        GUILayout.Space(100f);

        if (GUILayout.Button("¸ü¸Ä±ß½ç", GUILayout.Width(100), GUILayout.Height(30))) {
            if (tt.edge == null || tt.edge.Length != 4) {
                tt.edge = new string[4];
            }
            tt.edge[0] = upEdge;
            tt.edge[1] = rightEdge;
            tt.edge[2] = downEdge;
            tt.edge[3] = leftEdge;
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(tt);
            AssetDatabase.SaveAssets();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(100f);
        EditorGUILayout.LabelField("¡ü Tile");
        upSprite = EditorGUILayout.ObjectField(upSprite, typeof(Sprite), true) as Sprite;
        GUILayout.Space(10f);
        EditorGUILayout.LabelField("¡ú Tile");
        rightSprite = EditorGUILayout.ObjectField(rightSprite, typeof(Sprite), true) as Sprite;
        GUILayout.Space(10f);
        EditorGUILayout.LabelField("¡ý Tile");
        downSprite = EditorGUILayout.ObjectField(downSprite, typeof(Sprite), true) as Sprite;
        GUILayout.Space(10f);
        EditorGUILayout.LabelField("¡û Tile");
        leftSprite = EditorGUILayout.ObjectField(leftSprite, typeof(Sprite), true) as Sprite;
    }

    private void DrawTextureByDirection(Sprite target, int direction) {
        var texture = AssetPreview.GetAssetPreview(target);
        if (texture != null) {
            texture.filterMode = FilterMode.Point;
            var rect =
                direction == 0 ? new Rect(EditorGUIUtility.currentViewWidth / 2 - spriteShowWidth / 2, 300 - spriteShowWidth, spriteShowWidth, spriteShowWidth) :
                direction == 1 ? new Rect(EditorGUIUtility.currentViewWidth / 2 - spriteShowWidth / 2 + spriteShowWidth, 300, spriteShowWidth, spriteShowWidth) :
                direction == 2 ? new Rect(EditorGUIUtility.currentViewWidth / 2 - spriteShowWidth / 2, 300 +spriteShowWidth, spriteShowWidth, spriteShowWidth) :
                new Rect(EditorGUIUtility.currentViewWidth / 2 - spriteShowWidth / 2 - spriteShowWidth, 300, spriteShowWidth, spriteShowWidth);
            GUI.DrawTexture(rect, texture);
        }
    }
}
