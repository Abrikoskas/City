using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class Generator : MonoBehaviour
{
    [SerializeField] public int citySize = 21;
    public int roadFreq = 3;
    public GameObject roadHorizontal;
    public GameObject roadVertical;
    public GameObject crossroad;

    public GameObject[] buildings;
    public GameObject playerPrefab;

    [SerializeField] public Vector3 roadScale = new Vector3(1f, 1f, 1f);
    [SerializeField] public Vector3 buildingScale = new Vector3(1f, 1f, 1f);

    public System.Random rnd = new System.Random();
    private Tile[,] cityMatrix;
    private NavMeshSurface navMeshSurface;

    void Start()
    {
        GameObject cityRoot = new GameObject("CityRoot");
        cityMatrix = new Tile[citySize, citySize];

        // Add NavMeshSurface to the city root
        navMeshSurface = cityRoot.AddComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.Children;

        // Generate the city and bake NavMesh
        CreateCity(cityRoot);
        AddNavMeshLinks();
        BakeNavMesh();
        SpawnPlayer();
    }

    public void CreateCity(GameObject cityRoot)
    {
        for (int i = 0; i < citySize; i++)
        {
            for (int j = 0; j < citySize; j++)
            {
                string streetName = GenerateStreetName(i, j);

                if (IsCrossroad(i, j))
                {
                    CreateTile(crossroad, new Vector3(i * 5, 0, j * 5), "CrossRoad", i, j, streetName, cityRoot);
                }
                else if (IsHorizontalRoad(i, j))
                {
                    CreateTile(roadHorizontal, new Vector3(i * 5, 0, j * 5), "HorizontalRoad", i, j, streetName, cityRoot);
                }
                else if (IsVerticalRoad(i, j))
                {
                    CreateTile(roadVertical, new Vector3(i * 5, 0, j * 5), "VerticalRoad", i, j, streetName, cityRoot);
                }
                else
                {
                    int bIndex = rnd.Next(buildings.Length);
                    GameObject building = buildings[bIndex];
                    string houseAddress = GenerateHouseAddress(i, j);
                    CreateTile(building, new Vector3(i * 5, 0, j * 5), "Building", i, j, houseAddress, cityRoot);
                }
            }
        }
    }

    public void CreateTile(GameObject tileObject, Vector3 position, string tileType, int i, int j, string label, GameObject parent)
    {
        GameObject tileInstance = Instantiate(tileObject, position, Quaternion.identity, parent.transform);
        tileInstance.tag = "Ground";

        // Scale and orientation adjustments
        ScaleTile(tileInstance, tileType);

        if (tileType == "Building")
        {
            AdjustBuildingOrientation(tileInstance, i, j);
        }

        // Add to city matrix
        Tile tile = new Tile(tileInstance, position, label);
        cityMatrix[i, j] = tile;

        // Add NavMeshModifier for buildings
        if (tileType == "Building")
        {
            var navModifier = tileInstance.AddComponent<NavMeshModifier>();
            navModifier.overrideArea = true;
            navModifier.area = 1; // Default walkable area
        }
    }

    private void AddNavMeshLinks()
    {
        foreach (var tile in cityMatrix)
        {
            if (tile == null || tile.building == null) continue;

            // Add links for buildings adjacent to roads
            if (tile.building.name.Contains("Building"))
            {
                CreateNavMeshLink(tile, tile.position, tile.position + Vector3.forward * 2.5f); // Example forward link
                CreateNavMeshLink(tile, tile.position, tile.position + Vector3.back * 2.5f);   // Example backward link
            }
        }
    }

    private void CreateNavMeshLink(Tile tile, Vector3 start, Vector3 end)
    {
        GameObject navLinkObject = new GameObject("NavMeshLink");
        var navMeshLink = navLinkObject.AddComponent<NavMeshLink>();

        navLinkObject.transform.SetParent(tile.building.transform);

        navMeshLink.startPoint = start - tile.building.transform.position;
        navMeshLink.endPoint = end - tile.building.transform.position;
        navMeshLink.width = 2f;
        navMeshLink.costModifier = -1; // Default cost
        navMeshLink.bidirectional = true; // Allow movement both ways
    }

    private void BakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }

    private void SpawnPlayer()
    {
        Vector3 playerStartPosition = new Vector3(5, 1, 5);
        Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
    }

    // Other helper methods remain unchanged...
}