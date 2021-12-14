using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private EventTrigger startButton;
    [SerializeField] private GridEditor editor;
    [SerializeField] private InfluenceMap influenceMap;
    [SerializeField] private AI ai;

    [Header("Bars")]
    [SerializeField] private GameObject mapBar;
    [SerializeField] private GameObject decisionBar;
    [SerializeField] private GameObject sideBar;

    [Header("Map Variables")]
    [SerializeField] private EventTrigger aiMap;
    [SerializeField] private EventTrigger foodMap;
    [SerializeField] private EventTrigger oreMap;
    [SerializeField] private EventTrigger logMap;
    [SerializeField] private EventTrigger closeButton;

    [Header("Decision Variables")]
    [SerializeField] private EventTrigger aiButton;
    [SerializeField] private EventTrigger foodButton;
    [SerializeField] private EventTrigger oreButton;
    [SerializeField] private EventTrigger logButton;

    private void Awake()
    {
        // Start button setup
        EventTrigger.Entry startOnClicked = new EventTrigger.Entry();
        startOnClicked.eventID = EventTriggerType.PointerClick;
        startOnClicked.callback.AddListener((baseEventData) => StartGame());
        startButton.triggers.Add(startOnClicked);

        // Disable bars
        mapBar.SetActive(false);
        decisionBar.SetActive(false);

        // Ai map setup
        EventTrigger.Entry AIOnClicked = new EventTrigger.Entry();
        AIOnClicked.eventID = EventTriggerType.PointerClick;
        AIOnClicked.callback.AddListener((baseEventData) => influenceMap.ShowInfluenceMapByName("ai"));
        aiMap.triggers.Add(AIOnClicked);

        // Ai button setup
        EventTrigger.Entry AIButtonOnClicked = new EventTrigger.Entry();
        AIButtonOnClicked.eventID = EventTriggerType.PointerClick;
        AIButtonOnClicked.callback.AddListener((baseEventData) => ai.FindBestLocation("ai", false));
        aiButton.triggers.Add(AIButtonOnClicked);

        // Food map setup
        EventTrigger.Entry foodOnClicked = new EventTrigger.Entry();
        foodOnClicked.eventID = EventTriggerType.PointerClick;
        foodOnClicked.callback.AddListener((baseEventData) => influenceMap.ShowInfluenceMapByName("food"));
        foodMap.triggers.Add(foodOnClicked);

        // Food button setup
        EventTrigger.Entry foodButtonOnClicked = new EventTrigger.Entry();
        foodButtonOnClicked.eventID = EventTriggerType.PointerClick;
        foodButtonOnClicked.callback.AddListener((baseEventData) => ai.FindBestLocation("food"));
        foodButton.triggers.Add(foodButtonOnClicked);

        // Log map setup
        EventTrigger.Entry logOnClicked = new EventTrigger.Entry();
        logOnClicked.eventID = EventTriggerType.PointerClick;
        logOnClicked.callback.AddListener((baseEventData) => influenceMap.ShowInfluenceMapByName("logs"));
        logMap.triggers.Add(logOnClicked);

        // Log button setup
        EventTrigger.Entry logButtonOnClicked = new EventTrigger.Entry();
        logButtonOnClicked.eventID = EventTriggerType.PointerClick;
        logButtonOnClicked.callback.AddListener((baseEventData) => ai.FindBestLocation("logs"));
        logButton.triggers.Add(logButtonOnClicked);

        // Ore map setup
        EventTrigger.Entry oreOnClicked = new EventTrigger.Entry();
        oreOnClicked.eventID = EventTriggerType.PointerClick;
        oreOnClicked.callback.AddListener((baseEventData) => influenceMap.ShowInfluenceMapByName("ore"));
        oreMap.triggers.Add(oreOnClicked);

        // Ore button setup
        EventTrigger.Entry oreButtonOnClicked = new EventTrigger.Entry();
        oreButtonOnClicked.eventID = EventTriggerType.PointerClick;
        oreButtonOnClicked.callback.AddListener((baseEventData) => ai.FindBestLocation("ore"));
        oreButton.triggers.Add(oreButtonOnClicked);

        // Close button set up
        EventTrigger.Entry closeOnClicked = new EventTrigger.Entry();
        closeOnClicked.eventID = EventTriggerType.PointerClick;
        closeOnClicked.callback.AddListener((baseEventData) => influenceMap.HideInfluenceCubes());
        closeButton.triggers.Add(closeOnClicked);
    }

    private void StartGame()
    {
        // Stop editing
        editor.TurnOffEditing();
        // Close the side bar
        sideBar.SetActive(false);
        // Create the influence cubes
        influenceMap.CreateInfluenceCubes();

        // Enable the other two bars
        mapBar.SetActive(true);
        decisionBar.SetActive(true);
        // Start up the AI agent
        ai.Initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}
