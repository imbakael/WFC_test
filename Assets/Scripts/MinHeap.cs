using System;
using System.Collections.Generic;

public class MinHeap<T> where T : IComparable<T> {
    private List<T> heap; // 动态数组存储堆元素
    public int Count => heap.Count; // 堆中元素数量

    public MinHeap(int capacity = 16) {
        heap = new List<T>(capacity);
    }

    // 插入元素
    public void Insert(T item) {
        heap.Add(item);             // 添加至末尾
        HeapifyUp(heap.Count - 1);  // 从新元素位置开始上浮[2,4](@ref)
    }

    // 移除并返回堆顶（最小元素）
    public T RemoveMin() {
        if (heap.Count == 0)
            throw new InvalidOperationException("Heap is empty");

        T min = heap[0];
        int lastIndex = heap.Count - 1;
        heap[0] = heap[lastIndex]; // 末尾元素移至堆顶
        heap.RemoveAt(lastIndex);    // 移除末尾元素
        HeapifyDown(0);              // 从根节点开始下沉[2,5](@ref)
        return min;
    }

    // 上浮操作（调整新插入元素）
    private void HeapifyUp(int index) {
        while (index > 0) {
            int parentIndex = (index - 1) / 2; // 父节点索引
            if (heap[index].CompareTo(heap[parentIndex]) >= 0)
                break; // 当前节点≥父节点，堆性质满足

            Swap(index, parentIndex); // 交换当前节点与父节点
            index = parentIndex;      // 继续向上检查[4,6](@ref)
        }
    }

    // 下沉操作（调整堆顶元素）
    private void HeapifyDown(int index) {
        while (true) {
            int minIndex = index;
            int leftChild = 2 * index + 1;  // 左子节点索引
            int rightChild = 2 * index + 2; // 右子节点索引

            // 找到当前节点、左子节点、右子节点中的最小值
            if (leftChild < heap.Count && heap[leftChild].CompareTo(heap[minIndex]) < 0)
                minIndex = leftChild;
            if (rightChild < heap.Count && heap[rightChild].CompareTo(heap[minIndex]) < 0)
                minIndex = rightChild;

            if (minIndex == index) {
                break;
            }
            Swap(index, minIndex);
            index = minIndex;
        }
    }

    // 交换元素
    private void Swap(int i, int j) {
        (heap[i], heap[j]) = (heap[j], heap[i]);
    }
}