using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfluenceMap : MonoBehaviour
{
    [System.Serializable]
    private struct InfluenceData
    {
        public GridType[] tileTypes;
        public float startingInfluence;
        public int radius;
        public Color maxInfluenceColor;
        public Dictionary<int, float> influence;
    }

    [Header("Dictionary for influence maps")]
    [SerializeField] private string[] influenceMapNames;
    [SerializeField] private InfluenceData[] influenceMapTypes;
    private Dictionary<string, InfluenceData> influenceMapDictionary;
    [SerializeField] private GameObject influenceCubePrefab;
    [SerializeField] private Color baseColor;
    private GameObject[] influenceCubes;

    public void CreateInfluenceCubes()
    {
        GridSquare[] squares = WorldGrid.Instance.GridSpaces;
        influenceCubes = new GameObject[squares.Length];
        for(int i = 0; i < squares.Length; ++i)
        {
            GameObject cube = Instantiate(influenceCubePrefab, squares[i].position, Quaternion.identity);
            cube.transform.position = new Vector3(cube.transform.position.x, cube.transform.position.y + (0.5f * WorldGrid.Instance.Spacing), cube.transform.position.z);
            cube.transform.localScale = new Vector3(cube.transform.localScale.x * WorldGrid.Instance.Spacing, cube.transform.localScale.y * WorldGrid.Instance.Spacing, cube.transform.localScale.z * WorldGrid.Instance.Spacing);
            cube.SetActive(false);
            influenceCubes[i] = cube;
        }
    }

    public void ShowInfluenceMapByName(string name)
    {
        if (influenceMapDictionary.TryGetValue(name, out InfluenceData data))
        {
            ShowInfluenceMapByType(data);
        }
    }

    public Dictionary<int, float> CalculateInfluenceMapByName(string name)
    {
        if(influenceMapDictionary.TryGetValue(name, out InfluenceData data))
        {
            return CalculateInfluenceMap(data);
        }
        return new Dictionary<int, float>();
    }

    public float CheckIndexOnInfluenceMap(string name, int index)
    {
        if (influenceMapDictionary.TryGetValue(name, out InfluenceData data))
        {
            Dictionary<int, float> influenceMap = CalculateInfluenceMap(data);
            if(influenceMap.TryGetValue(index, out float influence))
            {
                return influence;
            }
        }
        return -1f;
    }

    public void HideInfluenceCubes()
    {
        for (int i = 0; i < influenceCubes.Length; ++i)
        {
            influenceCubes[i].SetActive(false);
        }
    }

    public Dictionary<int, float> GetInfluenceMapByName(string name)
    {
        if(influenceMapDictionary.TryGetValue(name, out InfluenceData data))
        {
            return data.influence;
        }
        return null;
    }

    private void Awake()
    {
        influenceMapDictionary = new Dictionary<string, InfluenceData>();
        for(int i = 0; i < influenceMapNames.Length; ++i)
        {
            influenceMapDictionary.Add(influenceMapNames[i], influenceMapTypes[i]);
        }
    }

    private void ShowInfluenceMapByType(InfluenceData data)
    {
        // Hide old influence cubes
        HideInfluenceCubes();
        // Create the new one and show it
        data.influence = CalculateInfluenceMap(data);
        CalculateColors(data);
    }

    private Dictionary<int, float> CalculateInfluenceMap(InfluenceData data)
    {
        // Clear the old influence map
        Dictionary<int, float> map = new Dictionary<int, float>();
        // Get all the directly influenced indices (no matter how many types are a part of this influence map)
        foreach (GridType type in data.tileTypes)
        {
            foreach (int cubeIndex in WorldGrid.Instance.GetGridIndicesFromType(type))
            {
                // Calculate the influence for directly influenced cubes
                if (map.TryGetValue(cubeIndex, out float influence))
                {
                    map[cubeIndex] = influence + data.startingInfluence;
                }
                else
                {
                    map[cubeIndex] = data.startingInfluence;
                }

                Vector2Int mainPoint = WorldGrid.Instance.GetPointFromIndex(cubeIndex);
                // Calculate the influence for each neighbor to directly affected cubes
                foreach (int neighbor in WorldGrid.Instance.GetNeighbors(data.radius, cubeIndex))
                {
                    Vector2Int neighborPoint = WorldGrid.Instance.GetPointFromIndex(neighbor);
                    // Calculate the distance
                    Vector2Int distance = neighborPoint - mainPoint;
                    // Get the max and min
                    // Max - distance traveled cardinally (distance = 1)
                    // Min - distance traveled diagonally (distance = 1.2)
                    distance.Set(Mathf.Abs(distance.x), Mathf.Abs(distance.y));
                    int max, min;
                    if (distance.x >= distance.y)
                    {
                        max = distance.x;
                        min = distance.y;
                    }
                    else
                    {
                        max = distance.y;
                        min = distance.x;
                    }
                    // Calculate the new influence using the distance
                    float newInfluence = data.startingInfluence / Mathf.Sqrt(1f + max + (1.2f * min));

                    if (map.TryGetValue(neighbor, out float neighborInfluence))
                    {
                        map[neighbor] = neighborInfluence + newInfluence;
                    }
                    else
                    {
                        map[neighbor] = newInfluence;
                    }
                }
            }
        }
        // Set the dictionary to the data
        return map;
    }

    private void CalculateColors(InfluenceData data)
    {
        float maxVal = 0;
        foreach (KeyValuePair<int, float> entry in data.influence)
        {
            if (maxVal < entry.Value) maxVal = entry.Value;
        }
        if (maxVal == 0) return;

        foreach (KeyValuePair<int, float> entry in data.influence)
        {
            influenceCubes[entry.Key].GetComponent<MeshRenderer>().material.color = Color.Lerp(baseColor, data.maxInfluenceColor, entry.Value / maxVal);
            influenceCubes[entry.Key].SetActive(true);
        }

    }
}
