using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Empty, White, Green, Red }

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
        tileBases = new Dictionary<TileType, TileBase>
        {
            { TileType.Empty, null },
            { TileType.White, Resources.Load<TileBase>("Tiles/TileWhite") },
            { TileType.Green, Resources.Load<TileBase>("Tiles/TileGreen") },
            { TileType.Red, Resources.Load<TileBase>("Tiles/TileRed") }
        };

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

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        var size = area.size.x * area.size.y * area.size.z;
        var tileArray = new TileBase[size];
        for (int i = 0; i < size; i++) tileArray[i] = tileBases[type];
        tilemap.SetTilesBlock(area, tileArray);
    }

    public void PlaceBuilding(Vector3Int cellPos, DraggableItem item)
    {
        var worldPosition = gridLayout.CellToWorld(cellPos);
        var newBuilding = Instantiate(item.gameObject, worldPosition, Quaternion.identity).GetComponent<DraggableItem>();
        newBuilding.Place();
        Debug.Log($"Placed building at cell position: {cellPos}, world position: {worldPosition}");
    }

    public void ClearArea(BoundsInt area) => TempTilemap.SetTilesBlock(area, new TileBase[area.size.x * area.size.y * area.size.z]);

    public void FollowBuilding(DraggableItem draggableItem)
    {
        ClearArea(PrevArea);
        var baseArray = GetTilesBlock(draggableItem.area, MainTilemap);
        var tileArray = new TileBase[baseArray.Length];
        var canPlace = true;

        for (int i = 0; i < baseArray.Length; i++)
        {
            tileArray[i] = baseArray[i] == tileBases[TileType.White] ? tileBases[TileType.Green] : null;
            if (baseArray[i] != tileBases[TileType.White]) canPlace = false;
        }

        if (!canPlace) for (int i = 0; i < tileArray.Length; i++) tileArray[i] = tileBases[TileType.Red];

        TempTilemap.SetTilesBlock(draggableItem.area, tileArray);
        PrevArea = draggableItem.area;
    }

    public bool CanTakeArea(BoundsInt area)
    {
        foreach (var tile in GetTilesBlock(area, MainTilemap))
            if (tile != tileBases[TileType.White]) return false;
        return true;
    }

    public void TakeArea(BoundsInt area)
    {
        SetTilesBlock(area, TileType.Empty, TempTilemap);
        SetTilesBlock(area, TileType.Green, MainTilemap);
    }
}
