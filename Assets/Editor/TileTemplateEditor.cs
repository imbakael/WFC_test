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

    private Sprite[] matchSprites;
    private TileTemplate[] matchTemplates;

    private void OnEnable() {
        tt = (TileTemplate)target;
        if (tt.edge == null) {
            return;
        }
        upEdge = tt.edge[0];
        rightEdge = tt.edge[1];
        downEdge = tt.edge[2];
        leftEdge = tt.edge[3];

        matchSprites = new Sprite[4];
        matchTemplates = new TileTemplate[4];
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

        for (int i = 0; i < matchSprites.Length; i++) {
            DrawTextureByDirection(matchSprites[i], i);
        }

        for (int i = 0; i < matchTemplates.Length; i++) {
            DrawTextureByDirection(matchTemplates[i] == null ? null : matchTemplates[i].sprite, i);
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
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        for (int i = 0; i < matchSprites.Length; i++) {
            EditorGUILayout.LabelField(i == 0 ? "¡ü" : i == 1 ? "¡ú" : i == 2 ? "¡ý" : "¡û");
            matchSprites[i] = EditorGUILayout.ObjectField(matchSprites[i], typeof(Sprite), true) as Sprite;
            GUILayout.Space(10f);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        for (int i = 0; i < matchTemplates.Length; i++) {
            EditorGUILayout.LabelField(i == 0 ? "¡ü" : i == 1 ? "¡ú" : i == 2 ? "¡ý" : "¡û");
            matchTemplates[i] = EditorGUILayout.ObjectField(matchTemplates[i], typeof(TileTemplate), true) as TileTemplate;
            GUILayout.Space(10f);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTextureByDirection(Sprite target, int direction) {
        if (target == null) {
            return;
        }
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
