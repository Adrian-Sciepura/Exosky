using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Star
{
    public string GAIA_id;
    public double x;
    public double y;
    public double z;
}

[System.Serializable]
public class Exoplanet
{
    public string name;
    public double x;
    public double y;
    public double z;
}

[System.Serializable]
public class JsonWrapper<T>
{
    public T[] items;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [HideInInspector] public JsonWrapper<Star> stars;
    [HideInInspector] public JsonWrapper<Exoplanet> exoplanets;

    public bool areStarsDataPending = true;
    public bool areExoplanetsDataPending = true;

    public void Awake()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
        NetworkManager.Instance.RequestExoplanets();
    }

    public void RequestExoplanets()
    {
        areExoplanetsDataPending = true;
        string apiUrl = @"https://localhost:7054/api/Exosky/getExoplanets";

        Debug.Log("Requested for exoplanets data");

        StartCoroutine(GetRequest(apiUrl, (UnityWebRequest req) =>
        {
            var result = req.downloadHandler.text;
            exoplanets = JsonUtility.FromJson<JsonWrapper<Exoplanet>>(result);
            Debug.Log($"Loaded {exoplanets.items.Length} exoplanets");
            areExoplanetsDataPending = false;
        }));
    }

    public void RequestExoplanetStars(string exoplanetName)
    {
        areStarsDataPending = true;
        string apiUrl = $@"https://localhost:7054/api/Exosky/getExoplanetStars/{Uri.EscapeDataString(exoplanetName)}";

        Debug.Log($"Requested for {exoplanetName} exoplanet stars data");

        StartCoroutine(GetRequest(apiUrl, (UnityWebRequest req) =>
        {
            var result = req.downloadHandler.text;
            stars = JsonUtility.FromJson<JsonWrapper<Star>>(result);

            Debug.Log($"Loaded {stars.items.Length} stars");
            areStarsDataPending = false;
        }));
    }

    IEnumerator GetRequest(string url, Action<UnityWebRequest> action)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("B³¹d zapytania: " + webRequest.error);
            }
            else
            {
                action(webRequest);
            }
        }
    }

}
