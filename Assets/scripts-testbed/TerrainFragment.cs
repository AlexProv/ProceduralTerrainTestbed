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

        //int resolution = terrain.resolutionsLevels[lodInfos.lodLevel];
        //Vector3[] vertices = new Vector3[resolution * resolution];
        //int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        //int triIndex = 0;

        //for(int j = 0; j < resolution; j++) {
        //    for(int i = 0; i < resolution; i++) {
        //        int index = i + j * resolution;

        //        float size = terrain.terrainSettings.fragmentSize;
        //        float xpct = ((i / (resolution - 1.0f)) - 0.5f) * size;
        //        float ypct = ((j / (resolution - 1.0f)) - 0.5f) * size;

        //        float centerOffsetX = lodInfos.center.x;
        //        float centerOffsetY = lodInfos.center.y;

        //        Vector3 coords = new Vector3(xpct + centerOffsetX, 0 , ypct + centerOffsetY);
        //        vertices[index] = coords;

        //        if(i < resolution -1 && j < resolution - 1) {
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
        //mesh.Clear();
        //mesh.vertices = vertices;
        //mesh.triangles = triangles;
        //mesh.RecalculateNormals();

        FragmentMeshData test = BuildMeshData();
        mesh.Clear();
        mesh.vertices = test.vertices;
        mesh.triangles = test.triangles;
        mesh.RecalculateNormals();
    }

    void addTriangle(int a, int b, int c, ref int triIndex, int[] triangles, bool clockwise) {
        if (clockwise)
        {
            triangles[triIndex] = a;
            triangles[triIndex + 2] = b;
            triangles[triIndex + 1] = c;
        }
        else
        {
            triangles[triIndex] = a;
            triangles[triIndex + 1] = b;
            triangles[triIndex + 2] = c;
        }
        triIndex += 3;
    }

    public FragmentMeshData BuildMeshData() {
        int max = 0;
        #region setup
        int resolution = terrain.resolutionsLevels[lodInfos.lodLevel];
        int resolutionB = terrain.resolutionsLevels[lodInfos.bSideLodLevel];
        int resolutionR = terrain.resolutionsLevels[lodInfos.rSideLodLevel];
        //Vector3[] vertices = new Vector3[(resolution-1) * (resolution-1) * 2];
        Vector3[] vertices = new Vector3[(resolution-1) * (resolution-1) + resolutionB + resolutionR];

        //int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6 + (((resolution * 2) + resolutionB + resolutionR - 4) * 3)];
        int[] triangles = new int[((resolution - 2) * (resolution - 2) * 6) + ((resolution + resolutionR - 1) * 3) + ((resolution + resolutionB - 1) * 3)];
        //List<int> triangles = new List<int>();
        int triIndex = 0;
        int index = 0;

        List<int> rightIndexesList = new List<int>();
        List<int> bottomIndexesList = new List<int>();
        #endregion

        #region mainmesh
        for (int j = 0; j < resolution-1; j++){
            for (int i = 0; i < resolution -1; i++){
                float size = terrain.terrainSettings.fragmentSize;
                float xpct = ((i / (resolution - 1.0f)) - 0.5f) * size;
                float ypct = ((j / (resolution - 1.0f)) - 0.5f) * size;

                float centerOffsetX = lodInfos.center.x;
                float centerOffsetY = lodInfos.center.y;

                Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
                vertices[index] = coords;

                if( i == resolution - 2) {
                    //add coords to right
                    rightIndexesList.Add(index);
                }
                if( j == resolution -2)
                {
                    bottomIndexesList.Add(index);
                }

                if (i < resolution - 2 && j < resolution - 2){
                    triangles[triIndex] = index + 1;
                    triangles[triIndex + 1] = index;
                    triangles[triIndex + 2] = index + resolution -1;

                    triangles[triIndex + 3] = index + resolution -1;
                    triangles[triIndex + 4] = index + resolution;
                    triangles[triIndex + 5] = index + 1;
                    triIndex += 6;
                }
                index += 1;
            }
        }
        #endregion

        #region rightStrip 
        int rSideResolution = terrain.resolutionsLevels[lodInfos.rSideLodLevel];
        int[] bigIndexes = rightIndexesList.ToArray();
        int[] smallIndexes = new int[rSideResolution];

        for (int j = 0; j < rSideResolution; j++) {
            float size = terrain.terrainSettings.fragmentSize;
            float ypct = ((j / (rSideResolution - 1.0f)) - 0.5f) * size;
            float xpct = size / 2.0f;

            float centerOffsetX = lodInfos.center.x;
            float centerOffsetY = lodInfos.center.y;

            smallIndexes[j] = index;


            Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
            vertices[index] = coords;
            index += 1;
            //GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere); // will crahs on play
            //dot.transform.position = coords;
            //dot.transform.parent = gameObject.transform;
        }

        bool clockwise = true;
        int tri1 = 2;
        int tri2 = 1;
        if (smallIndexes.Length > bigIndexes.Length)
        {
            int[] swaper = bigIndexes;
            bigIndexes = smallIndexes;
            smallIndexes = swaper;
            clockwise = false;
            tri1 = 1;
            tri2 = 2;
        }

        int step = (bigIndexes.Length+1)  / smallIndexes.Length;
        int smallIndex = 0;
        int currentStepIndex = 0;

        for (int i = 0; i < bigIndexes.Length - 1; i++) {

            //externe vers interne
            triangles[triIndex] = bigIndexes[i];
            triangles[triIndex + tri1] = smallIndexes[smallIndex];
            triangles[triIndex + tri2] = bigIndexes[i + 1];
            triIndex += 3;
            if (i % step == 0 && i != 0 && smallIndex < smallIndexes.Length - 1)
            {
                //triangles[triIndex] = smallIndexes[smallIndex];
                //triangles[triIndex + tri1] = smallIndexes[smallIndex + 1]; //explose si
                //triangles[triIndex + tri2] = bigIndexes[i + 1];
                //triIndex += 3;
                addTriangle(smallIndexes[smallIndex], smallIndexes[smallIndex + 1], bigIndexes[i + 1],
                 ref triIndex, triangles, clockwise);
                smallIndex += 1;
            }
        }
        if (clockwise) {
            addTriangle(smallIndexes[smallIndexes.Length - 2], smallIndexes[smallIndexes.Length - 1], bigIndexes[bigIndexes.Length - 1],
             ref triIndex, triangles, clockwise);
            //triangles[triIndex] = smallIndexes[smallIndexes.Length - 2];
            //triangles[triIndex + tri1] = smallIndexes[smallIndexes.Length-1]; //explose si
            //triangles[triIndex + tri2] = bigIndexes[bigIndexes.Length -1];
            //triIndex += 3;
        }

        #endregion

        #region bottomStrip
        bigIndexes = bottomIndexesList.ToArray();
        smallIndexes = new int[resolutionB];

        for (int j = 0; j < resolutionB; j++)
        {
            float size = terrain.terrainSettings.fragmentSize;
            float ypct = size / 2.0f;
            float xpct = ((j / (resolutionB - 1.0f)) - 0.5f) * size;

            float centerOffsetX = lodInfos.center.x;
            float centerOffsetY = lodInfos.center.y;

            smallIndexes[j] = index;
            Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
            vertices[index] = coords;
            index += 1;
        }

        clockwise = true;
        tri1 = 1;
        tri2 = 2;
        if (smallIndexes.Length > bigIndexes.Length)
        {
            int[] swaper = bigIndexes;
            bigIndexes = smallIndexes;
            smallIndexes = swaper;
            clockwise = false;
            tri1 = 2;
            tri2 = 1;
        }

        step = (bigIndexes.Length +1) / smallIndexes.Length;
        smallIndex = 0;

        for (int i = 0; i < bigIndexes.Length - 1; i++)
        {
            triangles[triIndex] = bigIndexes[i];
            triangles[triIndex + tri1] = smallIndexes[smallIndex];
            triangles[triIndex + tri2] = bigIndexes[i + 1];
            triIndex += 3;
            if (i % step == 0 && i != 0 && smallIndex < smallIndexes.Length - 1)
            {
                triangles[triIndex] = smallIndexes[smallIndex];
                triangles[triIndex + tri1] = smallIndexes[smallIndex + 1];
                triangles[triIndex + tri2] = bigIndexes[i + 1];
                triIndex += 3;
                smallIndex += 1;
            }
        }
        if (clockwise)
        {
            triangles[triIndex] = smallIndexes[smallIndexes.Length - 2];
            triangles[triIndex + tri1] = smallIndexes[smallIndexes.Length - 1]; //explose si
            triangles[triIndex + tri2] = bigIndexes[bigIndexes.Length - 1];
            triIndex += 3;
        }
        #endregion

        #region setheight
        Vector3[] normals = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++) {
            float height = TerrainGenerator.simpleNoise.Evaluate(vertices[i]);
            vertices[i].y = height;
        }
        #endregion


        return new FragmentMeshData(vertices, triangles, normals, lodInfos);
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

        return new FragmentMeshData(vertices, triangles, new Vector3[vertices.Length], lodInfos);
    }
}
