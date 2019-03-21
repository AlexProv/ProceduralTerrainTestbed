using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEditor;
using System.Threading.Tasks;

public class TerrainGenerator : MonoBehaviour
{
    public TerrainSettings terrainSettings;
    [HideInInspector]
    public bool foldout = false;
    public bool autoLodIncrement = true; //will need to be implemented somehow. 

    static Pool<Mesh> meshPool = new Pool<Mesh>();
    public static FragmentObjectPool fragmentObjPool;
    static Queue<ThreadTaskCallback<FragmentMeshData>> fragmentTaskCallbacksQueue = new Queue<ThreadTaskCallback<FragmentMeshData>>();

    public GameObject viewer;
    public Vector3 viewerPosition;
    Vector3 oldViewerPosition;
    public Material material;

    [HideInInspector]
    //public readonly int[] resolutionsLevels = {240, 120, 60, 30, 15};
    public readonly int[] resolutionsLevels = { 10, 5, 5, 5};
    //public readonly int[] resolutionsLevels = { 60, 30, 15, 5 };
    public float[] lodThresholdsLevels;

    Dictionary<Vector2, TerrainFragment> visibleLastframeFragments;
    Dictionary<Vector2, TerrainFragment> visibleFragments;

    int fragmentVisibleInViewDistance;

    void Update() {

        viewerPosition = viewer.transform.position;
        if (Vector3.Distance(oldViewerPosition, viewerPosition) > terrainSettings.fragmentSize / 2.0f){
            oldViewerPosition = viewerPosition;

            GenerateFragements();

            foreach (KeyValuePair<Vector2, TerrainFragment> entry in visibleLastframeFragments) {
                fragmentObjPool.Recover(entry.Value.gameObject);
            }
        }

        while (fragmentTaskCallbacksQueue.Count > 0){
            ThreadTaskCallback<FragmentMeshData> fragmentMeshDataCallback = fragmentTaskCallbacksQueue.Dequeue();
            fragmentMeshDataCallback.callback(fragmentMeshDataCallback.parameter);
        }
    }

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        fragmentObjPool = new FragmentObjectPool(material, transform);
        viewerPosition = viewer.transform.position;
        visibleFragments = new Dictionary<Vector2, TerrainFragment>();
        for(int i = 0; i < transform.childCount; i++) //destroy editor terrain fragments
            Destroy(transform.GetChild(i).gameObject);
            
        oldViewerPosition = viewer.transform.position;
        if (viewerPosition == null)
            viewerPosition = viewer.transform.position;

        visibleLastframeFragments = new Dictionary<Vector2, TerrainFragment>();

        setResolutionLevels();

        fragmentVisibleInViewDistance = Mathf.RoundToInt(terrainSettings.maxViewDistance / (float)terrainSettings.fragmentSize);
        GenerateFragements();
    }

    void GenerateFragements()
    {
        visibleLastframeFragments = visibleFragments;
        visibleFragments = new Dictionary<Vector2, TerrainFragment>();
        int currentFragmentCoordX = Mathf.RoundToInt(viewerPosition.x / terrainSettings.fragmentSize);
        int currentFragmentCoordY = Mathf.RoundToInt(viewerPosition.z / terrainSettings.fragmentSize); // to get the right player plane 


        for (int yOffset = -fragmentVisibleInViewDistance; yOffset <= fragmentVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -fragmentVisibleInViewDistance; xOffset <= fragmentVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedFragmentCoord = new Vector2(currentFragmentCoordX + xOffset, currentFragmentCoordY + yOffset);
                TerrainFragment existingFragment;
                //need to check for lod level 
                LodInfos lodInfo = new LodInfos(viewedFragmentCoord, terrainSettings, lodThresholdsLevels, viewerPosition);

                if (visibleLastframeFragments.TryGetValue(viewedFragmentCoord, out existingFragment)){
                    if (existingFragment.lodInfos == lodInfo) {
                        visibleFragments.Add(viewedFragmentCoord, existingFragment);
                        visibleLastframeFragments.Remove(viewedFragmentCoord);
                    }
                    else {
                        TerrainFragment fragment = CreateNewFragment(lodInfo, Vector3.up);
                        visibleFragments.Add(viewedFragmentCoord, fragment);
                    }
                }
                else {
                    TerrainFragment fragment = CreateNewFragment(lodInfo, Vector3.up);
                    visibleFragments.Add(viewedFragmentCoord, fragment);
                }
            }
        }
        
    }

    TerrainFragment CreateNewFragment(LodInfos lodInfo, Vector3 localUp) {
        TerrainFragment fragment = new TerrainFragment(lodInfo, Vector3.up, this);
        if (EditorApplication.isPlaying){

            ThreadPool.QueueUserWorkItem(a => FragmentDataThread(fragment, OnFragmentDataRecived));
        }
        else
        {
            GenerateEditorFragment(fragment, lodInfo);
        }
        return fragment;
    }

    void setResolutionLevels() {
        int maxLods = Mathf.RoundToInt(terrainSettings.maxViewDistance / terrainSettings.fragmentSize);
        if(terrainSettings.fragmentSize * resolutionsLevels.Length > terrainSettings.maxViewDistance)
        {
            throw new Exception("maxViewDistance too short for fragment size");
        }

        lodThresholdsLevels = new float[resolutionsLevels.Length];
        for(int i = 0; i < resolutionsLevels.Length; i++) {
            float step = terrainSettings.maxViewDistance / resolutionsLevels.Length;
            float percentDistance = (i+1) * step;
            lodThresholdsLevels[i] = percentDistance;
        }
    }

    void GenerateEditorFragment(TerrainFragment fragment, LodInfos lodInfo) {
        //in editor stuff
        fragment.BuildMesh();
        GameObject fragmentObj = new GameObject();

        fragmentObj.name = "Fragment lod " + lodInfo.lodLevel + " center " + lodInfo.center;
        MeshRenderer meshRenderer = fragmentObj.AddComponent<MeshRenderer>();

        if (material == null){
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        }
        else{
            meshRenderer.sharedMaterial = material;
        }

        fragmentObj.transform.parent = transform;
        MeshFilter meshFilter = fragmentObj.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = fragment.mesh;
    }

    public void GenerateTerrain() {
        Initialize();
    }

    public void OnTerrainSettingsChanged()
    {

    }

    //public void RequesTerrainFragmentData(TerrainFragment fragment, Action<FragmentMeshData> callback)
    //{
    //    ThreadStart start = delegate
    //    {
    //        FragmentDataThread(fragment, callback);
    //    };

    //    new Thread(start).Start();
    //}


    void FragmentDataThread(TerrainFragment fragment, Action<FragmentMeshData> callback)
    {
        FragmentMeshData data = fragment.BuildMeshData();
        lock (fragmentTaskCallbacksQueue)
        {
            ThreadTaskCallback<FragmentMeshData> task = new ThreadTaskCallback<FragmentMeshData>(callback,data);
            fragmentTaskCallbacksQueue.Enqueue(task);
        }
    }

    private void OnFragmentDataRecived(FragmentMeshData fragmentMeshData)
    {
        GameObject fragmentObj = fragmentObjPool.Acquire();
        fragmentObj.name = "Fragment lod " +fragmentMeshData.lodInfos.lodLevel + " center " + fragmentMeshData.lodInfos.center;

        Mesh mesh = fragmentObjPool.meshPool.Acquire();

        MeshFilter meshFilter = fragmentObj.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        mesh.vertices = fragmentMeshData.vertices;
        mesh.triangles = fragmentMeshData.triangles;
        mesh.RecalculateNormals();

        TerrainFragment terrainFragment;
        visibleFragments.TryGetValue(fragmentMeshData.lodInfos.coords, out terrainFragment);
        terrainFragment.gameObject = fragmentObj;
    }

    public struct ThreadTaskCallback<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public ThreadTaskCallback(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

public class LodInfos{
    public Vector2 center { get; private set; } //position
    public Vector2 coords;

    TerrainSettings terrainSettings;
    Vector3 viewerPosition;
    float[] lodThresholdsLevels;

    public int lodLevel { private set; get; }
    public int bSideLodLevel { private set; get; }
    public int rSideLodLevel { private set; get; }


    public LodInfos(Vector2 coords, TerrainSettings terrainSettings, float[] lodThresholdsLevels, Vector3 viewerPosition) {
        this.viewerPosition = viewerPosition;
        this.lodThresholdsLevels = lodThresholdsLevels;
        this.coords = coords;
        this.terrainSettings = terrainSettings;

        center = coords * terrainSettings.fragmentSize;

        lodLevel = getLodLevel(center);
        rSideLodLevel = getLodLevel(center + Vector2.right * terrainSettings.fragmentSize); //refactor needed for planet axisA & axsisB 
        bSideLodLevel = getLodLevel(center + Vector2.down * terrainSettings.fragmentSize);
    }

    public int getLodLevel(Vector2 center) {

        //probleme with how lod is calculated and updated for fragments
        Bounds bounds = new Bounds(new Vector3(center.x, 0, center.y), Vector3.one * terrainSettings.fragmentSize);

        float distance = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
        //Debug.Log("Bounds " + center + " postion " + viewerPosition);

        int lodLevel = 0;
        for (int i = 0; i < lodThresholdsLevels.Length; i++) {
            if (distance > lodThresholdsLevels[i]){
                lodLevel = i;
            }
        }
        return lodLevel;
    }

    public static bool operator == (LodInfos a, LodInfos b) {
        return a.lodLevel == b.lodLevel && a.rSideLodLevel == b.rSideLodLevel && a.bSideLodLevel == b.bSideLodLevel;
    }

    public static bool operator != (LodInfos a , LodInfos b) {
        return !(a == b);
     }
}


