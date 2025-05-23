using System;
using System.Collections.Generic;

public class IndexedMinHeap {
    private List<TileData> heap = new List<TileData>();
    private Dictionary<TileData, int> indices = new Dictionary<TileData, int>();

    public int Count => heap.Count;

    public void Clear() {
        heap.Clear();
        indices.Clear();
    }

    public void Insert(TileData td) {
        heap.Add(td);
        indices[td] = heap.Count - 1;
        HeapIfUp(heap.Count - 1);
    }

    public void Remove(TileData td) {
        if (!indices.TryGetValue(td, out int index)) {
            throw new KeyNotFoundException("Element not found in heap");
        }
        int lastIndex = heap.Count - 1;
        Swap(index, lastIndex);
        heap.RemoveAt(lastIndex);
        indices.Remove(td);
        HeapIfDown(index);
    }

    public TileData ExtractMin() {
        if (heap.Count == 0) {
            throw new InvalidOperationException("Heap is empty");
        }
        TileData min = heap[0];
        Swap(0, heap.Count - 1);
        heap.RemoveAt(heap.Count - 1);
        indices.Remove(min);
        HeapIfDown(0);
        return min;
    }

    public void Update(TileData td, double oldEntropy, double newEntropy) {
        if (!indices.TryGetValue(td, out int index)) {
            throw new KeyNotFoundException("Element not found in heap");
        }
        if (newEntropy < oldEntropy) {
            HeapIfUp(index);
        } else if (newEntropy > oldEntropy) {
            HeapIfDown(index);
        }
    }

    private void HeapIfUp(int index) {
        while (index > 0) {
            int parent = (index - 1) / 2;
            if (heap[parent].entropy <= heap[index].entropy) {
                break;
            }
            Swap(parent, index);
            index = parent;
        }
    }

    private void HeapIfDown(int index) {
        int left = 2 * index + 1;
        while (left < heap.Count) {
            int smallest = left;
            int right = left + 1;
            if (right < heap.Count && heap[right].entropy < heap[left].entropy) {
                smallest = right;
            }
            if (heap[index].entropy <= heap[smallest].entropy) {
                break;
            }
            Swap(index, smallest);
            index = smallest;
            left = 2 * index + 1;
        }
    }

    private void Swap(int a, int b) {
        if (a == b) {
            return;
        }
        (heap[b], heap[a]) = (heap[a], heap[b]);
        indices[heap[a]] = a;
        indices[heap[b]] = b;
    }
}
