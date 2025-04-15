using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour {

    public float a;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            float t1 = Mathf.RoundToInt(a);
            Debug.Log(t1);
        }
    }
}
