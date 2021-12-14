using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GridSquare
{
    [SerializeField] public Vector3 position;
    [SerializeField] public GridType type;
    [SerializeField] public GameObject visualRepresentation;
}
[System.Serializable]
public enum GridType
{
    INVALID_TYPE = -1,
    FIELD = 0,
    FOREST = 1,
    LAKE = 2,
    MOUNTAIN = 3,
    OCCUPIED = 4
}

[RequireComponent(typeof(LineRenderer))]
public class WorldGrid : MonoBehaviour
{
    // Singleton
    private static WorldGrid instance;
    public static WorldGrid Instance { get => instance; }

    [SerializeField] private GameObject testTileSpawn;
    [SerializeField] private GameObject floorObject;
    [SerializeField] private InfluenceMap influenceMap;
    [Header("Debug/Visualization")]
    [SerializeField] private bool debugVisualize = true;
    [SerializeField] private GameObject visualizedObject;
    [SerializeField] private Color firstIndexColor;
    [SerializeField] private Color lastIndexColor;
    [SerializeField] private LineRenderer hoveredTileOutline;
    [SerializeField] private float lineRendererWidth;
    private int hitIndex = -1;

    [Header("Grid information")]
    [SerializeField] private Vector3 startingLocation;
    [SerializeField] private int rows;
    [SerializeField] private int columns;
    [SerializeField] private int spacing;
    private GridSquare[] gridSpaces;
    private float inverseSize;

    public int Spacing { get => spacing; }
    public GridSquare[] GridSpaces { get => gridSpaces; }

    [Header("Tile information")]
    [SerializeField] private List<GridType> keys;
    [SerializeField] private List<GameObject> values;
    Dictionary<GridType, GameObject> gridSquareDictionary;

    public GridData GetGridData()
    {
        GridData data = new GridData();
        data.Instantiate(spacing, rows, columns, startingLocation, gridSpaces);
        return data;
    }

    public void PlaceGridTile(GridType type)
    {
        // Test to see if there is a selected tile
        if (hitIndex == -1) return;

        GridSquare square = gridSpaces[hitIndex];
        // Set the grid square type
        if (square.type == type) return;
        square.type = type;
        if(square.visualRepresentation != null)
        {
            Destroy(square.visualRepresentation);
            square.visualRepresentation = null;
        }
        gridSquareDictionary.TryGetValue(type, out GameObject visRep);
        if(visRep != null)
        {
            visRep = Instantiate(visRep, square.position, Quaternion.identity);
            visRep.transform.localScale = new Vector3(visRep.transform.localScale.x * spacing, visRep.transform.localScale.y * spacing, visRep.transform.localScale.z * spacing);
            square.visualRepresentation = visRep;
        }
        gridSpaces[hitIndex] = square;
    }

    public void PlaceGridTileAtIndex(GridType type, int index)
    {
        GridSquare square = gridSpaces[index];
        square.type = type;
        if (square.visualRepresentation != null)
        {
            Destroy(square.visualRepresentation);
            square.visualRepresentation = null;
        }
        gridSquareDictionary.TryGetValue(type, out GameObject visRep);
        if (visRep != null)
        {
            visRep = Instantiate(visRep, square.position, Quaternion.identity);
            visRep.transform.localScale = new Vector3(visRep.transform.localScale.x * spacing, visRep.transform.localScale.y * spacing, visRep.transform.localScale.z * spacing);
            square.visualRepresentation = visRep;
        }
        gridSpaces[index] = square;
    }

    public List<int> GetGridIndicesFromType(GridType type)
    {
        List<int> result = new List<int>();
        for(int i = 0; i < gridSpaces.Length; ++i)
        {
            if (gridSpaces[i].type == type)
                result.Add(i);
        }
        return result;
    }

    public List<Vector2Int> GetNeighbors(int radius, Vector2Int point)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        if (radius <= 0) return result;

        for(int i = point.x - radius; i <= point.x + radius; ++i)
        {
            for(int j = point.y - radius; j <= point.y + radius; ++j)
            {
                Vector2Int newPoint = new Vector2Int(point.x + i, point.y + j);
                if(IsValidPoint(newPoint))
                {
                    result.Add(newPoint);
                }
            }
        }

        return result;
    }

    public List<int> GetNeighbors(int radius, int index)
    {
        List<int> result = new List<int>();
        if (radius <= 0) return result;
        Vector2Int point = GetPointFromIndex(index);
        for (int i = point.x - radius; i <= point.x + radius; ++i)
        {
            for (int j = point.y - radius; j <= point.y + radius; ++j)
            {
                Vector2Int newPoint = new Vector2Int(i, j);
                if (i == point.x && j == point.y) continue;
                if (IsValidPoint(newPoint))
                {
                    result.Add(GetGridIndexFromPoint(newPoint));
                }
            }
        }

        return result;
    }

    public void CreateGrid(GridData data)
    {
        for (int i = 0; i < gridSpaces.Length; ++i)
        {
            if (gridSpaces[i].visualRepresentation != null)
            {
                Destroy(gridSpaces[i].visualRepresentation);
            }
        }
        if (data.GridSquares == null)
        {
            gridSpaces = new GridSquare[data.Rows * data.Columns];
            Vector3 gridStart = data.StartingLocation;
            gridStart.Set(gridStart.x + 0.5f * data.Spacing, gridStart.y, gridStart.z + 0.5f * data.Spacing);
            // Create a default grid
            for (int i = 0; i < data.Columns; ++i)
            {
                for (int j = 0; j < data.Rows; ++j)
                {
                    Vector3 position = new Vector3(gridStart.x + (j * data.Spacing), gridStart.y, gridStart.z + (i * data.Spacing));
                    gridSpaces[(i * data.Rows) + j].position = position;
                }
            }
        }
        else
        {
            gridSpaces = data.GridSquares;
            // Place any visual indicators necessary
            for(int i = 0; i < gridSpaces.Length; ++i)
            {
                GridSquare square = gridSpaces[i];
                gridSquareDictionary.TryGetValue(square.type, out GameObject visRep);
                if (visRep != null)
                {
                    visRep = Instantiate(visRep, square.position, Quaternion.identity);
                    visRep.transform.localScale = new Vector3(visRep.transform.localScale.x * spacing, visRep.transform.localScale.y * spacing, visRep.transform.localScale.z * spacing);
                    square.visualRepresentation = visRep;
                }
                gridSpaces[i] = square;
            }
        }

        // Assign the inverse size
        inverseSize = 1f / data.Spacing;
        spacing = data.Spacing;
        rows = data.Rows;
        columns = data.Columns;        

        floorObject.transform.localScale = new Vector3(columns * spacing, floorObject.transform.localScale.y, rows * spacing);
        floorObject.transform.position = new Vector3(0 - (10 - columns * spacing) * 0.5f, 0 , 0 - (10 - rows * spacing) * 0.5f);
    }

    public GridType GetTileType(int index)
    {
        return gridSpaces[index].type;
    }

    private void Awake()
    {
        // Singleton set-up
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        
        // Initialize the line renderer
        if(hoveredTileOutline == null)
        {
            hoveredTileOutline = GetComponent<LineRenderer>();
        }
        hoveredTileOutline.positionCount = 5;
        hoveredTileOutline.startWidth = lineRendererWidth;
        hoveredTileOutline.endWidth = lineRendererWidth;

        // Initialize the square dictionary
        gridSquareDictionary = new Dictionary<GridType, GameObject>();
        for (int i = 0; i < keys.Count; ++i)
        {
            gridSquareDictionary.Add(keys[i], values[i]);
        }
        gridSpaces = new GridSquare[rows * columns];
        // Adjust the grid start based on whether it is centered or not
        Vector3 gridStart = startingLocation;
        gridStart.Set(gridStart.x + 0.5f * spacing, gridStart.y, gridStart.z + 0.5f * spacing);
        // Create a default grid
        for (int i = 0; i < columns; ++i)
        {
            for (int j = 0; j < rows; ++j)
            {
                Vector3 position = new Vector3(gridStart.x + (j * spacing), gridStart.y, gridStart.z + (i * spacing));
                gridSpaces[(i * rows) + j].position = position;
            }
        }

        GridData initialGrid = new GridData();
        initialGrid.Instantiate(spacing, rows, columns, startingLocation, gridSpaces);
        CreateGrid(initialGrid);

        if (debugVisualize)
            VisualizeGrid();
    }

    private void VisualizeGrid()
    {
        for(int i = 0; i < gridSpaces.Length; ++i)
        {
            GameObject gridSquare = Instantiate(visualizedObject, gridSpaces[i].position, Quaternion.identity);
            gridSquare.transform.localScale = new Vector3(spacing, gridSquare.transform.localScale.y, spacing);
            gridSquare.GetComponent<MeshRenderer>().material.color = Color.Lerp(firstIndexColor, lastIndexColor, (float)i / gridSpaces.Length);
        }
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit hit);
        if (hit.collider != null && hit.point.y == startingLocation.y)
        {
            hitIndex = GetGridIndexFromLocation(hit.point);
            if (hitIndex < 0 || hitIndex >= gridSpaces.Length)
                hitIndex = -1;
            else
                DrawSquareOutline(hitIndex);
        }
        else if (hitIndex != -1)
        {
            Vector3[] positions = new Vector3[5];
            hoveredTileOutline.SetPositions(positions);
            hitIndex = -1;
        }

    }

    private int GetGridIndexFromLocation(Vector3 position)
    {
        Vector3 pointOnGrid = position - startingLocation;
        pointOnGrid *= inverseSize;
        int index = (int)pointOnGrid.x + (int)pointOnGrid.z * rows;
        return index;
    }

    private int GetGridIndexFromPoint(Vector2Int point)
    {
        int index = point.x + point.y * rows;
        return index;
    }

    public Vector2Int GetPointFromIndex(int index)
    {
        Vector3 position = gridSpaces[index].position;
        position -= startingLocation;
        position *= inverseSize;
        Vector2Int point = new Vector2Int((int)position.x, (int)position.z);
        return point;
    }

    private void DrawSquareOutline(int squareIndex)
    {
        Vector3 centerPosition = gridSpaces[squareIndex].position;
        Vector3[] positions = new Vector3[5];
        float halfSpacingIncludingWidth = (spacing * 0.5f) - lineRendererWidth;
        positions[0] = new Vector3(centerPosition.x - halfSpacingIncludingWidth, centerPosition.y, centerPosition.z - halfSpacingIncludingWidth);
        positions[1] = new Vector3(centerPosition.x - halfSpacingIncludingWidth, centerPosition.y, centerPosition.z + halfSpacingIncludingWidth);
        positions[2] = new Vector3(centerPosition.x + halfSpacingIncludingWidth, centerPosition.y, centerPosition.z + halfSpacingIncludingWidth);
        positions[3] = new Vector3(centerPosition.x + halfSpacingIncludingWidth, centerPosition.y, centerPosition.z - halfSpacingIncludingWidth);
        positions[4] = new Vector3(centerPosition.x - halfSpacingIncludingWidth, centerPosition.y, centerPosition.z - halfSpacingIncludingWidth);
        hoveredTileOutline.SetPositions(positions);
    }

    private bool IsValidPoint(Vector2Int point)
    {
        // Can't have negative grid indices
        if (point.x < 0 || point.y < 0) return false;
        if (point.x >= rows || point.y >= columns) return false;
        return true;
    }

}
