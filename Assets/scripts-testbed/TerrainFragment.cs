using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class TerrainFragment{
    public LodInfos lodInfos;
    TerrainGenerator terrain;

    //ShapeGenerator shapeGenerator;
    Vector3 axisA, axisB, localUp;
    public Mesh mesh;
    public MeshFilter meshFilter;
    public bool isVisible = true;
    public GameObject gameObject;

    public TerrainFragment(LodInfos lodInfos, Vector3 localUp, TerrainGenerator terrain){
        this.lodInfos = lodInfos;
        this.localUp = localUp;
        this.terrain = terrain;
    }

    public void BuildMesh() {
        //mesh = TerrainGenerator.fragmentObjPoll.meshPool.Acquire();
        mesh = new Mesh();

        int resolution = terrain.resolutionsLevels[lodInfos.lodLevel];
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for(int j = 0; j < resolution; j++) {
            for(int i = 0; i < resolution; i++) {
                int index = i + j * resolution;

                float size = terrain.terrainSettings.fragmentSize;
                float xpct = ((i / (resolution - 1.0f)) - 0.5f) * size;
                float ypct = ((j / (resolution - 1.0f)) - 0.5f) * size;

                float centerOffsetX = lodInfos.center.x;
                float centerOffsetY = lodInfos.center.y;

                Vector3 coords = new Vector3(xpct + centerOffsetX, 0 , ypct + centerOffsetY);
                vertices[index] = coords;

                if(i < resolution -1 && j < resolution - 1) {
                    triangles[triIndex] = index;
                    triangles[triIndex + 2] = index + resolution + 1;
                    triangles[triIndex + 1] = index + resolution;

                    triangles[triIndex + 3] = index;
                    triangles[triIndex + 5] = index + 1;
                    triangles[triIndex + 4] = index + resolution + 1;
                    triIndex += 6;
                }
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void addTriangle(int a, int b, int c, int index, int[] triangles) {
        triangles[index] = a;
        triangles[index+1] = b;
        triangles[index+2] = c;
    }

    public FragmentMeshData BuildMeshData() {
        int resolution = terrain.resolutionsLevels[lodInfos.lodLevel];

        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;
        int index = 0;
        for (int j = 0; j < resolution-1; j++){
            for (int i = 0; i < resolution -1; i++){
                index = i + j * resolution;

                float size = terrain.terrainSettings.fragmentSize;
                float xpct = ((i / (resolution - 1.0f)) - 0.5f) * size;
                float ypct = ((j / (resolution - 1.0f)) - 0.5f) * size;

                float centerOffsetX = lodInfos.center.x;
                float centerOffsetY = lodInfos.center.y;

                Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
                vertices[index] = coords;

                if (i < resolution - 2 && j < resolution - 2){
                    triangles[triIndex] = index;
                    triangles[triIndex + 2] = index + resolution + 1;
                    triangles[triIndex + 1] = index + resolution;

                    triangles[triIndex + 3] = index;
                    triangles[triIndex + 5] = index + 1;
                    triangles[triIndex + 4] = index + resolution + 1;
                    triIndex += 6;
                }
            }
        }

        //GET RIGHT vertex 
        int rSideResolution = terrain.resolutionsLevels[lodInfos.rSideLodLevel];
        for (int j = 0; j < rSideResolution; j++) {
            //int index = (resolution - 1) * j;
            float size = terrain.terrainSettings.fragmentSize;
            float ypct = ((j / (rSideResolution - 1.0f)) - 0.5f) * size;
            float xpct = 1;

            float centerOffsetX = lodInfos.center.x;
            float centerOffsetY = lodInfos.center.y;

            Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
            vertices[index] = coords;
        }
        int bSideResolution = terrain.resolutionsLevels[lodInfos.bSideLodLevel];
        for (int j = 0; j < bSideResolution; j++)
        {
            //int index = (resolution - 1) * j;
            float size = terrain.terrainSettings.fragmentSize;
            float xpct = ((j / (bSideResolution - 1.0f)) - 0.5f) * size;
            float ypct = 1;

            float centerOffsetX = lodInfos.center.x;
            float centerOffsetY = lodInfos.center.y;

            Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
            vertices[index] = coords;
        }


        bool clockwise = true;
        int resolutionBig = resolution;

        if( resolution < rSideResolution) {
            resolutionBig = rSideResolution;
            clockwise = false;
        }


        //for(int j = resolution -1; j < resolution; j++){
        //    for(int i = resolution -1; i < resolution; i++){
        //        int index = i + j * resolution;

        //        float size = terrain.terrainSettings.fragmentSize;
        //        float xpct = ((i / (resolution - 1.0f)) - 0.5f) * size;
        //        float ypct = ((j / (resolution - 1.0f)) - 0.5f) * size;

        //        float centerOffsetX = lodInfos.center.x;
        //        float centerOffsetY = lodInfos.center.y;

        //        Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
        //        vertices[index] = coords;

        //        if (i < resolution - 1 && j < resolution - 1){
        //            triangles[triIndex] = index;
        //            triangles[triIndex + 2] = index + resolution + 1;
        //            triangles[triIndex + 1] = index + resolution;

        //            triangles[triIndex + 3] = index;
        //            triangles[triIndex + 5] = index + 1;
        //            triangles[triIndex + 4] = index + resolution + 1;
        //            triIndex += 6;
        //        }
        //    }
        //}

        return new FragmentMeshData(vertices, triangles, lodInfos);
    }

    public FragmentMeshData BuildMeshData_()
    {
        int resolution = terrain.resolutionsLevels[lodInfos.lodLevel];

        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for (int j = 0; j < resolution; j++)
        {
            for (int i = 0; i < resolution; i++)
            {
                int index = i + j * resolution;

                float size = terrain.terrainSettings.fragmentSize;
                float xpct = ((i / (resolution - 1.0f)) - 0.5f) * size;
                float ypct = ((j / (resolution - 1.0f)) - 0.5f) * size;

                float centerOffsetX = lodInfos.center.x;
                float centerOffsetY = lodInfos.center.y;

                Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
                vertices[index] = coords;

                if (i < resolution - 1 && j < resolution - 1)
                {
                    triangles[triIndex] = index;
                    triangles[triIndex + 2] = index + resolution + 1;
                    triangles[triIndex + 1] = index + resolution;

                    triangles[triIndex + 3] = index;
                    triangles[triIndex + 5] = index + 1;
                    triangles[triIndex + 4] = index + resolution + 1;
                    triIndex += 6;
                }
            }
        }

        return new FragmentMeshData(vertices, triangles, lodInfos);
    }
}
