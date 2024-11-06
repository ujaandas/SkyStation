using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Quaternion = UnityEngine.Quaternion;

public enum TileType
{
    Empty,
    White,
    Green,
    Red,
}

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem current;
    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public Tilemap TempTilemap;

    private static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    public BoundsInt PrevArea
    {
        get; private set;
    }

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        string tilePath = @"Tiles/";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "TileWhite"));
        tileBases.Add(TileType.Green, Resources.Load<TileBase>(tilePath + "TileGreen"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "TileRed"));

        Debug.Log(MainTilemap == null ? "MainTilemap is null" : "MainTilemap is properly assigned");
        Debug.Log(TempTilemap == null ? "TempTilemap is null" : "TempTilemap is properly assigned");

        if (MainTilemap != null)
        {
            foreach (var pos in MainTilemap.cellBounds.allPositionsWithin)
            {
                if (MainTilemap.HasTile(pos))
                {
                    Debug.Log($"Tile at {pos}: {MainTilemap.GetTile(pos)}");
                }
            }
        }
    }

    private static void FillTiles(TileBase[] arr, TileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = tileBases[type];
        }
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);

            if (tilemap.HasTile(pos))
            {
                // Debug.Log($"Tile at {pos} is {tilemap.GetTile(pos)}");
                array[counter] = tilemap.GetTile(pos);
            }
            else
            {
                // Debug.Log($"Tile at {pos} is null");
                array[counter] = null;
            }

            counter++;
        }

        return array;
    }

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }

    public void PlaceBuilding(Vector3Int cellPos, DraggableItem item)
    {
        // Ensure the position is correct and log it
        Vector3 worldPosition = gridLayout.CellToWorld(cellPos);
        Debug.Log($"Placing building at cell position: {cellPos}, world position: {worldPosition}");

        GameObject buildingPrefab = item.gameObject;

        // Instantiate the building and log its position
        DraggableItem newBuilding = Instantiate(buildingPrefab, worldPosition, Quaternion.identity).GetComponent<DraggableItem>();
        Debug.Log($"New building instantiated at: {newBuilding.transform.position} with area: {newBuilding.area} and image position: {newBuilding.image.transform.position}");

        newBuilding.Place();
    }


    public void ClearArea(BoundsInt area)
    {
        TileBase[] toClear = new TileBase[area.size.x * area.size.y * area.size.z];
        FillTiles(toClear, TileType.Empty);
        TempTilemap.SetTilesBlock(area, toClear);
    }

    public void FollowBuilding(DraggableItem draggableItem)
    {
        ClearArea(PrevArea);

        BoundsInt buildingArea = draggableItem.area;
        TileBase[] baseArray = GetTilesBlock(buildingArea, MainTilemap);
        int size = baseArray.Length;
        TileBase[] tileArray = new TileBase[size];

        bool canPlace = true;
        for (int i = 0; i < size; i++)
        {
            if (baseArray[i] == tileBases[TileType.White])
            {
                tileArray[i] = tileBases[TileType.Green];
            }
            else
            {
                canPlace = false;
                break;
            }
        }

        if (!canPlace)
        {
            FillTiles(tileArray, TileType.Red);
        }

        TempTilemap.SetTilesBlock(buildingArea, tileArray);
        PrevArea = buildingArea;
    }

    public bool CanTakeArea(BoundsInt area)
    {
        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);
        foreach (var tile in baseArray)
        {
            if (tile != tileBases[TileType.White])
            {
                Debug.Log($"Can't take area, {tile} is not white");
                return false;
            }
        }
        Debug.Log("Can take area");
        return true;
    }

    public void TakeArea(BoundsInt area)
    {
        SetTilesBlock(area, TileType.Empty, TempTilemap);
        SetTilesBlock(area, TileType.Green, MainTilemap);
    }
}