using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridEditor : MonoBehaviour
{
    [System.Serializable]
    private struct GridTile
    {
        public GridType gridType;
        public Sprite displayImage;
    }

    [SerializeField] private List<GridTile> gridTiles;
    [SerializeField] private Image displayUI;
    [SerializeField] private GameObject editPanel;
    [SerializeField] private EventTrigger editButtonEventTrigger;
    [SerializeField] private EventTrigger upArrowEventTrigger;
    [SerializeField] private EventTrigger downArrowEventTrigger;
    private int selectedIndex = 0;
    private bool inEditMode = false;

    public void TurnOffEditing()
    {
        editPanel.SetActive(false);
        inEditMode = false; 
    }

    private void Awake()
    {
        // Set up event triggers for the UI
        editPanel.SetActive(false);
        EventTrigger.Entry editClicked = new EventTrigger.Entry();
        editClicked.eventID = EventTriggerType.PointerClick;
        editClicked.callback.AddListener((eventData) => ToggleDisplay());
        editButtonEventTrigger.triggers.Add(editClicked);

        EventTrigger.Entry upArrowClicked = new EventTrigger.Entry();
        upArrowClicked.eventID = EventTriggerType.PointerClick;
        upArrowClicked.callback.AddListener((eventData) => changedSelectedTile(-1));
        upArrowEventTrigger.triggers.Add(upArrowClicked);

        EventTrigger.Entry downArrowClicked = new EventTrigger.Entry();
        downArrowClicked.eventID = EventTriggerType.PointerClick;
        downArrowClicked.callback.AddListener((eventData) => changedSelectedTile(1));
        downArrowEventTrigger.triggers.Add(downArrowClicked);

    }

    private void Update()
    {
        if(inEditMode)
        {
            if(Input.GetMouseButtonDown(0))
            {
                WorldGrid.Instance.PlaceGridTile(gridTiles[selectedIndex].gridType);
            }
        }
    }

    private void changedSelectedTile(int amount)
    {
        // Update the index
        selectedIndex += amount;
        if (selectedIndex < 0) selectedIndex = gridTiles.Count - 1;
        else if (selectedIndex >= gridTiles.Count) selectedIndex = 0;

        // Change the corresponding display
        displayUI.sprite = gridTiles[selectedIndex].displayImage;
    }

    private void ToggleDisplay()
    {
        editPanel.SetActive(!editPanel.activeSelf);
        inEditMode = editPanel.activeSelf;
    }
}
