using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimizedSorter<T, TKey> where TKey: IComparer<TKey> {

    private TKey[] keys;
    private int[] indices;
    private T[] items;

    public void Initialize(List<T> list, Func<T, TKey> keySelector) {
        int count = list.Count;
        keys = new TKey[count];
        indices = new int[count];
        items = list.ToArray();

        for (int i = 0; i < count; i++) {
            keys[i] = keySelector(list[i]);
            indices[i] = i;
        }
    }

    public void Sort(List<T> list) {
        Array.Sort(keys, indices);
        for (int i = 0; i < list.Count; i++) {
            int targetIndex = indices[i];
            while (targetIndex < i) {
                targetIndex = indices[targetIndex];
            }
            if (targetIndex != i) {
                T temp = list[i];
                list[i] = list[targetIndex];
                list[targetIndex] = temp;
            }
        }
    }
}
