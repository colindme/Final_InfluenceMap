using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public struct GridData
{
    [SerializeField] private int spacing;
    [SerializeField] private int rows;
    [SerializeField] private int columns;
    [SerializeField] private Vector3 startingLocation;
    [SerializeField] private GridSquare[] gridSquares;

    public int Spacing { get => spacing; }
    public int Rows { get => rows; }
    public int Columns { get => columns; }
    public Vector3 StartingLocation { get => startingLocation; }
    public GridSquare[] GridSquares { get => gridSquares; }

    public static GridData CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<GridData>(jsonString);
    }

    public void Instantiate(int spacing, int rows, int columns, Vector3 startingLocation, GridSquare[] gridSquares)
    {
        this.spacing = spacing;
        this.rows = rows;
        this.columns = columns;
        this.startingLocation = startingLocation;
        this.gridSquares = gridSquares;
    }
}

public class GridLoader : MonoBehaviour
{
    private GameObject openMenu;

    [SerializeField] private GridEditor editor;

    [Header("Save Variables")]
    [SerializeField] private GameObject saveMenu;
    [SerializeField] private EventTrigger saveMenuOpenTrigger;
    [SerializeField] private EventTrigger saveMenuCloseTrigger;
    [SerializeField] private Button saveButton;
    [SerializeField] private InputField saveNameInput;

    [Header("Load Variables")]
    [SerializeField] private GameObject loadMenu;
    [SerializeField] private EventTrigger loadMenuOpenTrigger;
    [SerializeField] private EventTrigger loadMenuCloseTrigger;
    [SerializeField] private Button loadButton;
    [SerializeField] private InputField loadNameInput;

    [Header("New Grid Variables")]
    [SerializeField] private GameObject newGridMenu;
    [SerializeField] private EventTrigger newGridOpenTrigger;
    [SerializeField] private EventTrigger newGridCloseTrigger;
    [SerializeField] private Button createButton;
    [SerializeField] private Text rowCount;
    [SerializeField] private Slider rowSlider;
    [SerializeField] private Text columnCount;
    [SerializeField] private Slider columnSlider;
    [SerializeField] private Text spacingCount;
    [SerializeField] private Slider spacingSlider;

    private void Awake()
    {
        AssignNewGridMenuListeners();
        AssignSaveMenuListeners();
        AssignLoadMenuListeners();
    }

    private void AssignNewGridMenuListeners()
    {
        EventTrigger.Entry openClicked = new EventTrigger.Entry();
        openClicked.eventID = EventTriggerType.PointerClick;
        openClicked.callback.AddListener((baseEventData) => OpenMenu(newGridMenu));
        newGridOpenTrigger.triggers.Add(openClicked);

        EventTrigger.Entry closeClicked = new EventTrigger.Entry();
        closeClicked.eventID = EventTriggerType.PointerClick;
        closeClicked.callback.AddListener((baseEventData) => CloseMenu());
        newGridCloseTrigger.triggers.Add(closeClicked);

        rowSlider.onValueChanged.AddListener((baseEventData) => UpdateText(rowCount, rowSlider.value));
        UpdateText(rowCount, rowSlider.value);
        columnSlider.onValueChanged.AddListener((baseEventData) => UpdateText(columnCount, columnSlider.value));
        UpdateText(columnCount, columnSlider.value);
        spacingSlider.onValueChanged.AddListener((baseEventData) => UpdateText(spacingCount, spacingSlider.value));
        UpdateText(spacingCount, spacingSlider.value);

        createButton.onClick.AddListener(CreateGrid);
    }

    private void AssignSaveMenuListeners()
    {
        EventTrigger.Entry openClicked = new EventTrigger.Entry();
        openClicked.eventID = EventTriggerType.PointerClick;
        openClicked.callback.AddListener((baseEventData) => OpenMenu(saveMenu));
        saveMenuOpenTrigger.triggers.Add(openClicked);

        EventTrigger.Entry closeClicked = new EventTrigger.Entry();
        closeClicked.eventID = EventTriggerType.PointerClick;
        closeClicked.callback.AddListener((baseEventData) => CloseMenu());
        saveMenuCloseTrigger.triggers.Add(closeClicked);

        saveButton.onClick.AddListener(Save);
    }

    private void AssignLoadMenuListeners()
    {
        EventTrigger.Entry openClicked = new EventTrigger.Entry();
        openClicked.eventID = EventTriggerType.PointerClick;
        openClicked.callback.AddListener((baseEventData) => OpenMenu(loadMenu));
        loadMenuOpenTrigger.triggers.Add(openClicked);

        EventTrigger.Entry closeClicked = new EventTrigger.Entry();
        closeClicked.eventID = EventTriggerType.PointerClick;
        closeClicked.callback.AddListener((baseEventData) => CloseMenu());
        loadMenuCloseTrigger.triggers.Add(closeClicked);

        loadButton.onClick.AddListener(Load);
    }

    private void CreateGrid()
    {
        // Create grid
        GridData newGrid = new GridData();
        newGrid.Instantiate((int)spacingSlider.value, (int)rowSlider.value, (int)columnSlider.value, new Vector3(-5, 0.25f, -5), null);
        WorldGrid.Instance.CreateGrid(newGrid);
        
        CloseMenu();
    }

    private void Save()
    {
        if (saveNameInput.text.Length == 0) return;
        // Save to folder
        string grid = JsonUtility.ToJson(WorldGrid.Instance.GetGridData());
        StreamWriter sw = new StreamWriter(Application.dataPath + "/Data/" + saveNameInput.text + ".json");
        sw.Write(grid);
        sw.Close();
        CloseMenu();
    }

    private void Load()
    {
        if (loadNameInput.text.Length == 0) return;
        // Load from data folder and create
        if (!File.Exists(Application.dataPath + "/Data/" + loadNameInput.text + ".json")) return;
        StreamReader sr = new StreamReader(Application.dataPath + "/Data/" + loadNameInput.text + ".json");
        string data = sr.ReadToEnd();
        GridData grid = JsonUtility.FromJson<GridData>(data);
        WorldGrid.Instance.CreateGrid(grid);
        sr.Close();
        CloseMenu();
    }

    private void OpenMenu(GameObject menu)
    {
        if (openMenu != null) CloseMenu();
        menu.SetActive(true);
        openMenu = menu;
        editor.TurnOffEditing();
    }

    private void CloseMenu()
    {
        openMenu.SetActive(false);
        openMenu = null;
    }

    private void UpdateText(Text textToUpdate, float value)
    {
        textToUpdate.text = ((int)value).ToString();
    }
}
