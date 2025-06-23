using System;

using UnityEngine;


[Serializable]
public class SArray<T>
{
    [SerializeField] private T[] array;
    [SerializeField] private int w, h;

    public SArray(int w, int h)
    {
        this.w = w;
        this.h = h;
        array = new T[w * h];
    }

    public SArray(int w, int h, T t)
    {
        this.w = w;
        this.h = h;
        array = new T[w * h];
        for (int i=0; i<array.Length; i++)
        {
            array[i] = t;
        }
    }

    public T this[int x, int y]
    {
        get { return array[w * y + x]; }
        set { array[w * y + x] = value; }
    }

    public T this[Vector2Int v]
    {
        get { return array[w * v.y + v.x]; }
        set { array[w * v.y + v.x] = value; }
    }
}
