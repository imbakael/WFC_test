using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveFunctionCollapse))]
public class WFCEditor : Editor {

    private WaveFunctionCollapse wfc;

    private void OnEnable() {
        wfc = (WaveFunctionCollapse)target;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Éú³ÉµØÍ¼")) {
            wfc.ReGenerateMap();
        }
    }
}
