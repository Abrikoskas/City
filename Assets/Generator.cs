using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Generator : MonoBehaviour
{
    [SerializeField]
    public int citySize = 21; // Size of the city
    public int roadFreq = 3;
    public GameObject roadHorizontal;
    public GameObject roadVertical;
    public GameObject crossroad;

    public GameObject[] buildings;
    public GameObject playerPrefab; // Player prefab to be assigned in the inspector

    // Scales for roads and buildings
    [SerializeField]
    public Vector3 roadScale = new Vector3(1f, 1f, 1f);
    [SerializeField]
    public Vector3 buildingScale = new Vector3(1f, 1f, 1f);

    public System.Random rnd = new System.Random();

    // 2D array representing the city
    private Tile[,] cityMatrix;

    void Start()
    {
        // Validate prefab assignments
        if (roadHorizontal == null || roadVertical == null || crossroad == null)
        {
            Debug.LogError("Road, crossroad, or tile prefabs are not assigned in the Generator script.");
            return;
        }

        if (buildings == null || buildings.Length == 0)
        {
            Debug.LogError("Building prefabs are not assigned or are empty in the Generator script.");
            return;
        }

        cityMatrix = new Tile[citySize, citySize];
        CreateCity();
        SpawnPlayer();
    }

    public void CreateCity()
    {
        for (int i = 0; i < citySize; i++)
        {
            for (int j = 0; j < citySize; j++)
            {
                string streetName = GenerateStreetName(i, j);

                // Determine where to place roads, crossroads, and buildings
                if (IsCrossroad(i, j))
                {     
                    CreateTile(crossroad, new Vector3(i * 5, 0, j * 5), "CrossRoad", i, j, streetName);
                }
                else if (IsHorizontalRoad(i, j))
                {
                    CreateTile(roadHorizontal, new Vector3(i * 5, 0, j * 5), "HorizontalRoad", i, j, streetName);
                }
                else if (IsVerticalRoad(i, j))
                {
                    CreateTile(roadVertical, new Vector3(i * 5, 0, j * 5), "VerticalRoad", i, j, streetName);
                }
                else
                {
                    // Choose a random building prefab
                    int bIndex = rnd.Next(buildings.Length);
                    GameObject building = buildings[bIndex];

                    // Generate address for the house
                    string houseAddress = GenerateHouseAddress(i, j);
                    CreateTile(building, new Vector3(i * 5, 0, j * 5), "Building", i, j, houseAddress);
                }
            }
        }
    }

    // Method to create and place a tile in the scene
    public void CreateTile(GameObject tileObject, Vector3 position, string tileType, int i, int j, string label)
    {
        GameObject tileInstance = Instantiate(tileObject, position, Quaternion.identity);
        tileInstance.tag = "Ground";
        ScaleTile(tileInstance, tileType);

        string tileName = $"{tileType}_{Guid.NewGuid()}";
        tileInstance.name = tileName;

        if (tileType == "Building")
        {
            AdjustBuildingOrientation(tileInstance, i, j);
        }

        // Create Tile object and add it to the city matrix
        Tile tile = new Tile(tileInstance, position, label);
        cityMatrix[i, j] = tile;
    }

    // Method to adjust the orientation of the building
    // Method to adjust the orientation of the building
private void AdjustBuildingOrientation(GameObject building, int row, int column)
{
    Debug.Log("Adjusting Building Orientation");

    // Ensure indices are within the matrix bounds
    if (row < 0 || row >= citySize || column < 0 || column >= citySize)
        return; // Out of bounds, return early

    // Determine the nearest road direction
    if (row - 1 >=0 && cityMatrix[row-1, column] != null && cityMatrix[row-1, column].building.name.Contains("Road")) // Check down
    {
        Debug.Log("Building facing up");
        building.transform.rotation = Quaternion.Euler(0, 270, 0); // Facing up
    }
    else if (row + 1 < citySize && cityMatrix[row+1, column] != null && cityMatrix[row+1, column].building.name.Contains("Road")) // Check up
    {
        Debug.Log("Building facing down");
        building.transform.rotation = Quaternion.Euler(0, 90, 0); // Facing down
    }
    else if (column - 1 >= 0 && cityMatrix[row, column - 1] != null && cityMatrix[row, column - 1].building.name.Contains("Road")) // Check left
    {
        Debug.Log("Building facing right");
        building.transform.rotation = Quaternion.Euler(0, 180, 0); // Facing right
    }
    else if (column + 1 < citySize && cityMatrix[row, column + 1] != null && cityMatrix[row, column + 1].building.name.Contains("Road")) // Check right
    {
        Debug.Log("Building facing left");
        building.transform.rotation = Quaternion.Euler(0, 0, 0); // Facing left
    }
    // If there's no adjacent road, keep the default orientation
}

    // Method to scale tiles based on their type
    private void ScaleTile(GameObject tileInstance, string tileType)
    {
        tileInstance.transform.localScale = tileType.Contains("Road") ? roadScale : buildingScale;
    }

    // Method to generate a street name
    private string GenerateStreetName(int row, int column)
    {
        string name = "";
        
        // If it's a row (horizontal street)
        if ((row - 1) % roadFreq == 0)
        {
            name = $"Street {row}";
        }
        
        // If it's a column (vertical street)
        if ((column - 1) % roadFreq == 0)
        {
            name += string.IsNullOrEmpty(name) ? $"Avenue {column}" : $" & Avenue {column}";
        }

        return string.IsNullOrEmpty(name) ? "Unnamed" : name;
    }

    // Method to generate a house address
    private string GenerateHouseAddress(int row, int column)
    {
        // Find the nearest street and avenue
        int nearestStreet = FindNearestRoad(row);
        int nearestAvenue = FindNearestRoad(column);

        // Form the house address
        string address = $"House near Street {nearestStreet} & Avenue {nearestAvenue}";
        return address;
    }

    // Method to find the nearest street or avenue
    private int FindNearestRoad(int index)
    {
        // Check if the index is already a road
        if ((index - 1) % roadFreq == 0)
        {
            return index; // Current row/column is a road
        }
        else
        {
            // Return the nearest previous road
            return ((index - 1) / roadFreq) * roadFreq + 1;
        }
    }

    // Method to determine if the current position is a crossroad
    private bool IsCrossroad(int row, int column)
    {
        return (row - 1) % roadFreq == 0 && (column - 1) % roadFreq == 0;
    }

    // Method to determine if the current position is a horizontal road
    private bool IsHorizontalRoad(int row, int column)
    {
        return (row - 1) % roadFreq == 0 && (column - 1) % roadFreq != 0;
    }

    // Method to determine if the current position is a vertical road
    private bool IsVerticalRoad(int row, int column)
    {
        return (row - 1) % roadFreq != 0 && (column - 1) % roadFreq == 0;
    }
    private void SpawnPlayer()
    {
        // Set the player's position to a predefined location (adjust as necessary)
        Vector3 playerStartPosition = new Vector3(5, 0, 5); // Example spawn position
        Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
    }
}

// Class Tile to represent a city tile
public class Tile
{
    public Guid id { get; private set; }
    public string name { get; private set; }
    public GameObject building { get; private set; }
    public Vector3 position { get; private set; }
    public string label { get; private set; } // For streets this is the street name, for houses - address

    public Tile(GameObject building, Vector3 position, string label)
    {
        this.id = Guid.NewGuid();
        this.name = building.name;
        this.building = building;
        this.position = position;
        this.label = label;
    }
}