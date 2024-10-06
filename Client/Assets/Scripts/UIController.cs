using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections;
using System;

public class UIController : MonoBehaviour
{
    private UIDocument document;
    private ListView listView;
    private UnityEngine.UIElements.Button jumpButton;
    private UnityEngine.UIElements.Button lastClicked;
    
    public void Start()
    {
        document = GetComponent<UIDocument>();

        var root = document.rootVisualElement;
        listView = root.Q<ListView>("ExoplanetList");
        jumpButton = root.Q<UnityEngine.UIElements.Button>("JumpButton");


        StartCoroutine(WaitForData(() => !NetworkManager.Instance.areExoplanetsDataPending, () =>
        {
            var data = NetworkManager.Instance.exoplanets.items
                                    .Select(e => e.name)
                                    .ToList();

            listView.itemsSource = data;

            listView.makeItem = () => new UnityEngine.UIElements.Button();
            listView.bindItem = (element, index) =>
            {
                var button = element as UnityEngine.UIElements.Button;
                button.text = data[index];
                button.clicked += () =>
                {
                    lastClicked = button;
                };
            };

            listView.fixedItemHeight = 25;
            listView.selectionType = SelectionType.Single;
        }));

        
        jumpButton.clicked += () =>
        {
            if (lastClicked == null)
                return;

            NetworkManager.Instance.RequestExoplanetStars(lastClicked.text);
            
            StartCoroutine(WaitForData(() => !NetworkManager.Instance.areStarsDataPending, () =>
            {
                ObjectsManager.Instance.ClearAll();
                ObjectsManager.Instance.SpawnCurrent();
            }));
        };
    }


    private IEnumerator WaitForData(Func<bool> check, Action action)
    {
        while(!check())
        {
            yield return new WaitForSeconds(1f);
        }

        action();
    }
}
