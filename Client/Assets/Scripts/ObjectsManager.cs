using UnityEngine;

public class ObjectsManager : MonoBehaviour
{
    public static ObjectsManager Instance;

    [SerializeField] private GameObject spawnedExoplanetsRoot;
    [SerializeField] private GameObject spawnedStarsRoot;

    [SerializeField] private GameObject exoplanetPrefab;
    [SerializeField] private GameObject starPrefab;

    [SerializeField] private GameObject mainCameraObject;

    Matrix4x4[] matrices;
    int noOfStars;

    Mesh mesh;
    Material material;

    bool isRendering;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public void ClearAll()
    {
        foreach(Transform child in spawnedExoplanetsRoot.transform)
            Destroy(child.gameObject);

        foreach(Transform child in spawnedStarsRoot.transform)
            Destroy(child.gameObject);
    }

    public void SpawnCurrent()
    {
        // Exoplanet

        Exoplanet currExoplanet = NetworkManager.Instance.exoplanets.items[0];
        Vector3 exoplanetPos = new Vector3((float)currExoplanet.x, (float)currExoplanet.y, (float)currExoplanet.z);
        //exoplanetPos /= 5.0f;
        var newObj = Instantiate(exoplanetPrefab, exoplanetPos, Quaternion.identity, spawnedExoplanetsRoot.transform);
        mainCameraObject.transform.position = newObj.transform.position;
        newObj.SetActive(false);

        // Stars

        Vector3 startPos;
        mesh = starPrefab.GetComponent<MeshFilter>().sharedMesh;
        material = starPrefab.GetComponent<MeshRenderer>().sharedMaterial;

        if (mesh == null || material == null)
            return;

        noOfStars = NetworkManager.Instance.stars.items.Length;
        matrices = new Matrix4x4[noOfStars];

        int i = 0;
        foreach (var star in NetworkManager.Instance.stars.items)
        {
            startPos = new Vector3((float)star.x, (float)star.y, (float)star.z);
            //startPos /= 5.0f;
            matrices[i++] = Matrix4x4.TRS(startPos, Quaternion.identity, Vector3.one);
        }

        isRendering = true;
    }

    public void Update()
    {
        if(isRendering)
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices, noOfStars);
    }
}
