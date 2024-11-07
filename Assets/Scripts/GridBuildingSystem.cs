using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Empty, TileGrassy, TileGrassyGreen, TileGrassyRed, TileDirty, TileDirtyGreen, TileDirtyRed }

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem current;
    public GridLayout gridLayout;
    public Tilemap MainTilemap, TempTilemap;

    private static Dictionary<TileType, TileBase> tileBases;

    public BoundsInt PrevArea { get; private set; }

    private void Awake() => current = this;

    private void Start()
    {
        string tilePath = @"Tiles/";
        tileBases = new Dictionary<TileType, TileBase>
        {
            { TileType.Empty, null },
            { TileType.TileGrassy, Resources.Load<TileBase>(tilePath + "TileGrassy") },
            { TileType.TileGrassyGreen, Resources.Load<TileBase>(tilePath + "TileGrassyGreen") },
            { TileType.TileGrassyRed, Resources.Load<TileBase>(tilePath + "TileGrassyRed") },
            { TileType.TileDirty, Resources.Load<TileBase>(tilePath + "TileDirty") },
            { TileType.TileDirtyGreen, Resources.Load<TileBase>(tilePath + "TileDirtyGreen") },
            { TileType.TileDirtyRed, Resources.Load<TileBase>(tilePath + "TileDirtyRed") }
        };

        Debug.Log(MainTilemap == null ? "MainTilemap is null" : "MainTilemap is properly assigned");
        Debug.Log(TempTilemap == null ? "TempTilemap is null" : "TempTilemap is properly assigned");

        foreach (var entry in tileBases) { Debug.Log($"{entry.Key}: {(entry.Value == null ? "Tile not loaded" : "Tile loaded")}"); }
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        var array = new TileBase[area.size.x * area.size.y * area.size.z];
        var counter = 0;
        foreach (var pos in area.allPositionsWithin)
        {
            array[counter++] = tilemap.HasTile(pos) ? tilemap.GetTile(pos) : null;
        }
        return array;
    }

    private static void FillTiles(TileBase[] arr, TileType type)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = tileBases[type];
        }
    }

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        var size = area.size.x * area.size.y * area.size.z;
        var tileArray = new TileBase[size];
        for (int i = 0; i < size; i++) tileArray[i] = tileBases[type];
        tilemap.SetTilesBlock(area, tileArray);
    }

    public void ClearArea(BoundsInt area) => TempTilemap.SetTilesBlock(area, new TileBase[area.size.x * area.size.y * area.size.z]);

    public void FollowBuilding(DraggableItem draggableItem)
    {
        ClearArea(PrevArea);
        var baseArray = GetTilesBlock(draggableItem.area, MainTilemap);
        var tileArray = new TileBase[baseArray.Length];

        bool canPlace = true;
        for (int i = 0; i < baseArray.Length; i++)
        {
            Debug.Log($"Currently processing tile: {baseArray[i]} against {tileBases[TileType.TileGrassy]} and {tileBases[TileType.TileDirty]}");
            if (baseArray[i] == tileBases[TileType.TileGrassy])
            {
                tileArray[i] = tileBases[TileType.TileGrassyGreen];
            }
            else if (baseArray[i] == tileBases[TileType.TileDirty])
            {
                tileArray[i] = tileBases[TileType.TileDirtyGreen];
            }
            else
            {
                canPlace = false;
                break;
            }
        }

        if (!canPlace)
        {
            FillTiles(tileArray, TileType.TileGrassyRed);
        }

        TempTilemap.SetTilesBlock(draggableItem.area, tileArray);
        PrevArea = draggableItem.area;
    }


    public bool CanTakeArea(BoundsInt area)
    {
        foreach (var pos in area.allPositionsWithin)
        {
            var tile = MainTilemap.GetTile(pos);
            if (!(tile == tileBases[TileType.TileGrassy] || tile == tileBases[TileType.TileDirty]))
            {
                Debug.Log("Can't take area");
                return false;
            }
        }
        Debug.Log($"Can take area, tiles are: {MainTilemap.GetTilesBlock(area)}");
        return true;
    }


    public void TakeArea(BoundsInt area)
    {
        Debug.Log($"Taking area: {area.position}, Size: {area.size}");

        SetTilesBlock(area, TileType.Empty, TempTilemap);

        foreach (var pos in area.allPositionsWithin)
        {
            Debug.Log($"Processing position: {pos}");
            var tile = MainTilemap.GetTile(pos);
            if (tile == tileBases[TileType.TileGrassy])
            {
                SetTilesBlock(area, TileType.TileGrassy, MainTilemap);
                Debug.Log("Set grassy tile.");
            }
            else if (tile == tileBases[TileType.TileDirty])
            {
                SetTilesBlock(area, TileType.TileDirty, MainTilemap);
                Debug.Log("Set dirty tile.");
            }
        }
    }

}
