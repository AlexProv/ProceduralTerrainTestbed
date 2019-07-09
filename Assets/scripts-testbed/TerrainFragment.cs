
using System.Collections.Generic;
using UnityEngine;


public class TerrainFragment{
    public LodInfos lodInfos;
    TerrainGenerator terrain;

    Vector3 axisA, axisB, localUp;
    public Mesh mesh;
    public MeshFilter meshFilter;
    public bool isVisible = true;
    public GameObject gameObject;

    int triIndex = 0;
    public Vector3[] normals;
    public Vector3 vertices;
    public int[] triangles;
    public int resolutionUp;
    public int resolutionRight;
    public int resolution;

    int[] upIndexes;
    int[] downIndexes;
    int[] rightIndexes;
    int[] leftIndexes;

    public TerrainFragment(LodInfos lodInfos, Vector3 localUp, TerrainGenerator terrain){
        this.lodInfos = lodInfos;
        this.localUp = localUp;
        this.terrain = terrain;
    }

    public void BuildMesh() {
        mesh = new Mesh();
        FragmentMeshData test = BuildMeshData();
        mesh.Clear();
        mesh.vertices = test.vertices;
        mesh.triangles = test.triangles;
        mesh.RecalculateNormals();
    }

    void addTriangle(int a, int b, int c, int[] triangles, bool clockwise) {
        if (clockwise){
            triangles[triIndex] = a;
            triangles[triIndex + 2] = b;
            triangles[triIndex + 1] = c;
        }
        else{
            triangles[triIndex] = a;
            triangles[triIndex + 1] = b;
            triangles[triIndex + 2] = c;
        }
        triIndex += 3;
    }


    public FragmentMeshData BuildMeshData() {
        #region setup
        resolution = terrain.resolutionsLevels[lodInfos.lodLevel];
        resolutionUp = terrain.resolutionsLevels[lodInfos.upSideLodLevel];
        resolutionRight = terrain.resolutionsLevels[lodInfos.rightSideLodLevel];
        List<int> upIndexesList = new List<int>();
        List<int> rightIndexesList = new List<int>();
        List<int> downIndexesList = new List<int>();
        List<int> leftIndexesList = new List<int>();
        //int resolutionDown = terrain.resolutionsLevels[lodInfos.downSideLodLevel];
        //int resolutionLeft = terrain.resolutionsLevels[lodInfos.leftSideLodLevel];

        Vector3[] vertices = new Vector3[(resolution-1) * (resolution-1) + resolutionUp + resolutionRight];
        int[] triangles = new int[((resolution - 2) * (resolution - 2) * 6) + ((resolution + resolutionRight - 1) * 3) + ((resolution + resolutionUp - 1) * 3)];
        int index = 0;

        List<int> rightInnerIndexesList = new List<int>();
        List<int> upInnerIndexesList = new List<int>();

        float size = terrain.terrainSettings.fragmentSize;
        #endregion

        #region mainmesh
        for (int j = 0; j < resolution-1; j++){
            for (int i = 0; i < resolution -1; i++){
                float xpct = ((i / (resolution - 1.0f)) - 0.5f) * size;
                float ypct = ((j / (resolution - 1.0f)) - 0.5f) * size;

                float centerOffsetX = lodInfos.center.x;
                float centerOffsetY = lodInfos.center.y;


                Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
                float height = TerrainGenerator.simpleNoise.Evaluate(coords);
                coords.y = height;

                vertices[index] = coords;

                if ( i == resolution - 2) {
                    rightInnerIndexesList.Add(index);
                }
                if( j == resolution -2)
                {
                    upInnerIndexesList.Add(index);
                }
                if (i == 0) {
                    leftIndexesList.Add(index);
                }
                if (j == 0) {
                    downIndexesList.Add(index);
                }

                if (i < resolution - 2 && j < resolution - 2){
                    addTriangle(index,
                                index + resolution,
                                index + 1,
                        triangles, false);
                    addTriangle(index + resolution,
                                index,
                                index + resolution - 1,
                        triangles, false);
                }
                index += 1;
            }
        }

        downIndexesList.Add(index); //add last one to the down verts 
        #endregion

        #region rightStrip 
        int[] bigIndexes = rightInnerIndexesList.ToArray();
        int[] smallIndexes = new int[resolutionRight];

        for (int j = 0; j < resolutionRight; j++) {
            float ypct = ((j / (resolutionRight - 1.0f)) - 0.5f) * size;
            float xpct = size / 2.0f;

            float centerOffsetX = lodInfos.center.x;
            float centerOffsetY = lodInfos.center.y;

            smallIndexes[j] = index;

            Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
            float height = TerrainGenerator.simpleNoise.Evaluate(coords);
            coords.y = height;

            vertices[index] = coords;
            rightIndexesList.Add(index);
            index += 1;
        }

        bool clockwise = true;
        if (smallIndexes.Length > bigIndexes.Length)
        {
            int[] swaper = bigIndexes;
            bigIndexes = smallIndexes;
            smallIndexes = swaper;
            clockwise = false;
        }

        int step = (bigIndexes.Length+1)  / smallIndexes.Length;
        int smallIndex = 0;

        for (int i = 0; i < bigIndexes.Length - 1; i++) {
            addTriangle(bigIndexes[i],
                        smallIndexes[smallIndex],
                        bigIndexes[i + 1], 
                triangles, clockwise);
            if (i % step == 0 && i != 0 && smallIndex < smallIndexes.Length - 1)
            {
                addTriangle(smallIndexes[smallIndex],
                            smallIndexes[smallIndex + 1],
                            bigIndexes[i + 1],
                 triangles, clockwise);
                smallIndex += 1;
            }
        }
        if (clockwise) {
            addTriangle(smallIndexes[smallIndexes.Length - 2],
                        smallIndexes[smallIndexes.Length - 1],
                        bigIndexes[bigIndexes.Length - 1],
             triangles, clockwise);
        
        }

        leftIndexesList.Add(index);
        #endregion

        #region upStrip
        bigIndexes = upInnerIndexesList.ToArray();
        smallIndexes = new int[resolutionUp];

        for (int j = 0; j < resolutionUp; j++)
        {

            float ypct = size / 2.0f;
            float xpct = ((j / (resolutionUp - 1.0f)) - 0.5f) * size;

            float centerOffsetX = lodInfos.center.x;
            float centerOffsetY = lodInfos.center.y;

            smallIndexes[j] = index;
            Vector3 coords = new Vector3(xpct + centerOffsetX, 0, ypct + centerOffsetY);
            float height = TerrainGenerator.simpleNoise.Evaluate(coords);
            coords.y = height;

            vertices[index] = coords;
            upIndexesList.Add(index);
            index += 1;
        }

        clockwise = true;
        if (smallIndexes.Length > bigIndexes.Length){
            int[] swaper = bigIndexes;
            bigIndexes = smallIndexes;
            smallIndexes = swaper;
            clockwise = false;
        }

        step = (bigIndexes.Length +1) / smallIndexes.Length;
        smallIndex = 0;

        for (int i = 0; i < bigIndexes.Length - 1; i++)
        {
            addTriangle(bigIndexes[i],
                        smallIndexes[smallIndex],
                        bigIndexes[i + 1],
                 triangles, !clockwise);
            if (i % step == 0 && i != 0 && smallIndex < smallIndexes.Length - 1)
            {
                addTriangle(smallIndexes[smallIndex],
                            smallIndexes[smallIndex + 1],
                            bigIndexes[i + 1],
                     triangles, !clockwise);
                smallIndex += 1;
            }
        }
        if (clockwise)
        {
            addTriangle(smallIndexes[smallIndexes.Length - 2],
                        smallIndexes[smallIndexes.Length - 1],
                        bigIndexes[bigIndexes.Length - 1],
             triangles, !clockwise);         
        }
        #endregion

        upIndexes = upIndexesList.ToArray();
        downIndexes = downIndexesList.ToArray();
        rightIndexes = rightIndexesList.ToArray();
        leftIndexes = leftIndexesList.ToArray();

        normals = RecalculateNormals(vertices, triangles); 

        return new FragmentMeshData(vertices, triangles, normals, lodInfos);
    }

    Vector3[] RecalculateNormals(Vector3[] vertices, int[] triangles) {
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            int ia = triangles[i];
            int ib = triangles[i + 1];
            int ic = triangles[i + 2];

            Vector3 a = vertices[ia];
            Vector3 b = vertices[ib];
            Vector3 c = vertices[ic];

            Vector3 ab = a - b;
            Vector3 ac = a - c;
            Vector3 normal = Vector3.Cross(ab, ac);

            normals[ia] += normal;
            normals[ib] += normal;
            normals[ic] += normal;

            normals[ia].Normalize();
            normals[ib].Normalize();
            normals[ic].Normalize();
        }
        return normals;
    }


    public void RecalculateNormalsRight(TerrainFragment b)
    {
        for (int i = 0; i < rightIndexes.Length; i++) {
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

    public void RecalculateNormalsLeft(TerrainFragment b)
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

    public void RecalculateNormalsUp(TerrainFragment b)
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

    public void RecalculateNormalsDown(TerrainFragment b)
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

    public void RecalculateNormalUpRight(TerrainFragment t) 
    { 

    }

    public void RecalculateNormalUpLeft(TerrainFragment t)
    {

    }

    public void RecalculateNormalDownRight(TerrainFragment t)
    {
    
    }

    public void RecalculateNormalDownLeft(TerrainFragment t)
    {

    }

}
