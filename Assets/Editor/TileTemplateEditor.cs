using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(TileTemplate))]
public class TileTemplateEditor : Editor {

    private TileTemplate tt;
    private float spriteShowWidth = 256;
    private float textShowWidth = 100f;

    private string upEdge;
    private string leftEdge;
    private string rightEdge;
    private string downEdge;

    private void OnEnable() {
        tt = (TileTemplate)target;
        if (tt.edge == null) {
            return;
        }
        if (tt.edge.Length >= 1) {
            upEdge = tt.edge[0];
        }
        if (tt.edge.Length >= 2) {
            rightEdge = tt.edge[1];
        }
        if (tt.edge.Length >= 3) {
            downEdge = tt.edge[2];
        }
        if (tt.edge.Length >= 4) {
            leftEdge = tt.edge[3];
        }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (tt.sprite == null) {
            return;
        }
        var texture = AssetPreview.GetAssetPreview(tt.sprite);
        if (texture == null) {
            Debug.Log("2 tt.sprite.name = " + tt.sprite.name);
            return;
        }

        texture.filterMode = FilterMode.Point;
        var textureRect = new Rect(EditorGUIUtility.currentViewWidth / 2 - spriteShowWidth / 2, 300, spriteShowWidth, spriteShowWidth);
        GUI.DrawTexture(textureRect, texture);

        GUIStyle style = new GUIStyle(GUI.skin.textField);
        style.alignment = TextAnchor.MiddleCenter;

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


    }

}
