using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : new()
{

    T poolType;
    List<T> activePool;
    List<T> inactivePool;

    public Pool()
    {
        activePool = new List<T>();
        inactivePool = new List<T>();
    }

    public T Acquire()
    {
        T t = default(T);
        if (inactivePool.Count > 0)
        {
            t = inactivePool[0];
            inactivePool.RemoveAt(0);
        }
        else
        {
            t = new T();
        }
        activePool.Add(t);
        return t;
    }

    public void Recover(T elem)
    {
        activePool.Remove(elem);
        inactivePool.Add(elem);
    }
}


public class FragmentObjectPool : Pool<GameObject> {

    public Pool<Mesh> meshPool = new Pool<Mesh>();
    Material material;
    Transform parent;

    public FragmentObjectPool(Material material, Transform parent) : base() {
        this.material = material;
        this.parent = parent;
    }

    public void Recover(GameObject elem) {
        base.Recover(elem);

        MeshFilter meshFilter = elem.GetComponent<MeshFilter>();
        if (meshFilter != null) {
            meshFilter.sharedMesh.Clear();
            meshPool.Recover(meshFilter.sharedMesh);
        }

        elem.SetActive(false);
    }

    public GameObject Acquire() {
        GameObject obj = base.Acquire();
        obj.SetActive(true);
        obj.transform.parent = parent;

        if (obj.GetComponent<MeshRenderer>() == null) {
            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
        }
        if (obj.GetComponent<MeshFilter>() == null) {
            obj.AddComponent<MeshFilter>();
        }

        return obj;
    }
}

public class ListPool<T> : Pool<List<T>>
{

    public void Recover(List<T> elem)
    {
        base.Recover(elem);
        elem.Clear();
    }
}

public class ArrayListPool : Pool<ArrayList>
{

    public void Recover(ArrayList elem)
    {
        base.Recover(elem);
        elem.Clear();
    }
}