using UnityEngine;

public class ObjectsManager : MonoBehaviour
{
    public static ObjectsManager Instance;

    [SerializeField] private GameObject spawnedExoplanetsRoot;
    [SerializeField] private GameObject spawnedStarsRoot;

    [SerializeField] private GameObject exoplanetPrefab;
    [SerializeField] private GameObject starPrefab;


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
        exoplanetPos /= 10.0f;
        Instantiate(exoplanetPrefab, exoplanetPos, Quaternion.identity, spawnedExoplanetsRoot.transform);

        // Stars

        Vector3 startPos;
        foreach (var star in NetworkManager.Instance.stars.items)
        {
            startPos = new Vector3((float)star.x, (float)star.y, (float)star.z);
            startPos /= 10.0f;
            Instantiate(starPrefab, startPos, Quaternion.identity, spawnedStarsRoot.transform);
        }
    }
}
