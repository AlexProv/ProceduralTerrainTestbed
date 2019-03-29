using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragmentMeshData 
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;
    public LodInfos lodInfos;
    public GameObject gameObject;

    public int[] upIndexes;
    public int[] downIndexes;
    public int[] rightIndexes;
    public int[] leftIndexes;

    public FragmentMeshData(Vector3[] vertices, int[] triangles, Vector3[] normals, LodInfos lodInfos)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.lodInfos = lodInfos;
        this.normals = normals;
    }

    public void RecalculateNormalsRight(FragmentMeshData b)
    {
        for (int i = 0; i < rightIndexes.Length; i++)
        {
            int index = rightIndexes[i];
            int tIndex = b.leftIndexes[i];

            Vector3 aNormal = normals[index];
            Vector3 bNormal = b.normals[tIndex];

            Vector3 normal = aNormal + bNormal;
            normal.Normalize();
            normals[index] = normal;
            b.normals[tIndex] = normal;
        }
    }

    public void RecalculateNormalsLeft(FragmentMeshData b)
    {
        for (int i = 0; i < leftIndexes.Length; i++)
        {
            int index = leftIndexes[i];
            int tIndex = b.rightIndexes[i];

            Vector3 aNormal = normals[index];
            Vector3 bNormal = b.normals[tIndex];

            Vector3 normal = aNormal + bNormal;
            normal.Normalize();
            normals[index] = normal;
            b.normals[tIndex] = normal;
        }
    }

    public void RecalculateNormalsUp(FragmentMeshData b)
    {
        for (int i = 0; i < upIndexes.Length; i++)
        {
            int index = upIndexes[i];
            int tIndex = b.downIndexes[i];

            Vector3 aNormal = normals[index];
            Vector3 bNormal = b.normals[tIndex];

            Vector3 normal = aNormal + bNormal;
            normal.Normalize();
            normals[index] = normal;
            b.normals[tIndex] = normal;
        }

    }

    public void RecalculateNormalsDown(FragmentMeshData b)
    {
        for (int i = 0; i < downIndexes.Length; i++)
        {
            int index = downIndexes[i];
            int tIndex = b.upIndexes[i];

            Vector3 aNormal = normals[index];
            Vector3 bNormal = b.normals[tIndex];

            Vector3 normal = aNormal + bNormal;
            normal.Normalize();
            normals[index] = normal;
            b.normals[tIndex] = normal;
        }

    }

    public void RecalculateNormalUpRight(FragmentMeshData t)
    {

    }

    public void RecalculateNormalUpLeft(FragmentMeshData t)
    {

    }

    public void RecalculateNormalDownRight(FragmentMeshData t)
    {

    }
    public void RecalculateNormalDownLeft(FragmentMeshData t)
    {

    }
}
