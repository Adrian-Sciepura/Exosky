using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public void GetData()
    {
        NetworkManager network = NetworkManager.Instance;
        network.RequestExoplanets();
    }

    public void SpawnElements()
    {
        ObjectsManager objects = ObjectsManager.Instance;
        objects.ClearAll();
        objects.SpawnCurrent();
    }
}
