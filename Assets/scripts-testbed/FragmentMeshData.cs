using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentMeshData 
{
    public Vector3[] vertices;
    public int[] triangles;
    public LodInfos lodInfos;
    public GameObject gameObject;

    public FragmentMeshData(Vector3[] vertices, int[] triangles, LodInfos lodInfos)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.lodInfos = lodInfos;
    }
}
