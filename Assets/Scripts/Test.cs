using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour {

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            IndexedMinHeap im = new IndexedMinHeap();

            TileData[] tileDatas = new TileData[] {
                new TileData {
                    entropy = 99999
                },
                new TileData {
                    entropy = 9999
                },
                new TileData {
                    entropy = 999
                },
                new TileData {
                    entropy = 99
                },
                new TileData {
                    entropy = 9
                }
            };

            im.Insert(tileDatas[0]);
            im.Insert(tileDatas[1]);
            im.Insert(tileDatas[2]);
            im.Insert(tileDatas[3]);
            im.Insert(tileDatas[4]);

            //im.Remove(tileDatas[2]);

            for (int i = 0; i < im.heap.Count; i++) {
                Debug.Log($"{i}, {im.heap[i].entropy}");
            }
        }
    }
}
