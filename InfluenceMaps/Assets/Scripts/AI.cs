using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    private HashSet<int> buildingLocations = new HashSet<int>();
    private bool initialized = false;
    [SerializeField] private InfluenceMap influenceMap;

    public void Initialize()
    {
        if (initialized) return;
        initialized = true;
        foreach (int index in WorldGrid.Instance.GetGridIndicesFromType(GridType.OCCUPIED))
            buildingLocations.Add(index);
        
    }

    public void FindBestLocation(string name, bool highest = true)
    {
        int index = FindBestIndexForType(name, highest);
        if(index == -1)
        {
            Debug.Log("could not find a reasonable location");
            return;
        }
        else
        {
            WorldGrid.Instance.PlaceGridTileAtIndex(GridType.OCCUPIED, index);
            // Place label based on what is passed in
            switch (name)
            {
                case "ai":
                    break;
                case "logs":
                    break;
                case "food":
                    break;
                case "ore":
                    break;
                default:
                    break;
            }
        }
    }

    private int FindBestIndexForType(string name, bool highest = true)
    {
        int result = -1;
        // Have the influence map calculate AI influence map
        Dictionary <int, float> aiMap = influenceMap.CalculateInfluenceMapByName("ai");
        if (aiMap.Count == 0) return result;
        // Loop through AI affected indices
        float influence;
        if (highest)
            influence = -1f;
        else
            influence = float.MaxValue;
        List<int> candidates = new List<int>();
        foreach(KeyValuePair<int, float> aiIndex in aiMap)
        {
            // If they are a field tile
            // AND if they are in the influence map for the passed in type
            if (WorldGrid.Instance.GetTileType(aiIndex.Key) == GridType.FIELD && influenceMap.CheckIndexOnInfluenceMap(name, aiIndex.Key) != -1)
            {
                if(highest)
                {
                    if(aiIndex.Value > influence)
                    {
                        candidates.Clear();
                        candidates.Add(aiIndex.Key);
                        influence = aiIndex.Value;
                    }
                    else if (aiIndex.Value == influence)
                    {
                        candidates.Add(aiIndex.Key);
                    }
                }
                else
                {
                    if(aiIndex.Value < influence)
                    {
                        candidates.Clear();
                        candidates.Add(aiIndex.Key);
                        influence = aiIndex.Value;
                    }
                    else if (aiIndex.Value == influence)
                    {
                        candidates.Add(aiIndex.Key);
                    }
                }
            }
        }
        // If there are no valid indices, return early
        if (influence == 0f || influence == -1f) return result;
        // Randomly choose one from the best candidates
        int randomIndex = Random.Range(0, candidates.Count);
        result = candidates[randomIndex];

        return result;
    }
}
