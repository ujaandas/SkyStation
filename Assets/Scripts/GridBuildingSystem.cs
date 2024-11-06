using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Quaternion = UnityEngine.Quaternion; 

// Implements singleton pattern
public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem current;
    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public Tilemap TempTilemap;

    private static Dictionary<TileType, TileBase> tileBases = new Dictionary<TileType, TileBase>();

    private Building temp;
    private Vector3 prevPos;
    private BoundsInt prevArea;
    private bool isDragging = false;

    #region Unity Methods
    private void Awake()
    {
        // Set current to this instance
        current = this;
    }

    private void Start()
    {
        // Initialize grid
        string tilePath = @"Tiles/";
        tileBases.Add(TileType.Empty, null);
        tileBases.Add(TileType.White, Resources.Load<TileBase>(tilePath + "TileWhite"));
        tileBases.Add(TileType.Green, Resources.Load<TileBase>(tilePath + "TileGreen"));
        tileBases.Add(TileType.Red, Resources.Load<TileBase>(tilePath + "TileRed"));
    }

    private void Update()
    {
        if (!temp) return;

        // Start dragging
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject(0)) {
                return;
            }

            isDragging = true;
        }
        // End dragging and place object
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            if (!temp.Placed)
            {
                if (temp.CanBePlaced())
                {
                    temp.Place();
                }
            }
            isDragging = false; // Stop dragging
        }
        // Cancel dragging
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearArea();
            Destroy(temp.gameObject);
            isDragging = false;
        }

        // Update position while dragging
        if (isDragging && !temp.Placed)
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = gridLayout.LocalToCell(touchPos);

            if (prevPos != cellPos)
            {
                temp.transform.localPosition = gridLayout.CellToLocalInterpolated(cellPos + new Vector3(.5f, .5f, .5f));
                prevPos = cellPos;
                FollowBuilding();
            }
        }
    }
    #endregion

    #region Tilemap Management
    private static void FillTiles(TileBase[] arr, TileType type) {
        for (int i = 0; i < arr.Length; i++) {
            arr[i] = tileBases[type];
        }
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap) {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach (var v in area.allPositionsWithin) {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            
            if (tilemap.HasTile(pos)) {
                array[counter] = tilemap.GetTile(pos);
            } else {
                array[counter] = null; // Handle out-of-bounds or uninitialized tiles
            }
            
            counter++;
        }

        return array;
    }

    private static void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap) {
        int size = area.size.x * area.size.y * area.size.z;
        TileBase[] tileArray = new TileBase[size];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }
    #endregion

    #region Building Placement
    public void InitializeWithBuilding(GameObject building) {
        temp = Instantiate(building, Vector3.zero, Quaternion.identity).GetComponent<Building>();
        FollowBuilding();
    }

    private void ClearArea() {
        TileBase[] toClear = new TileBase[prevArea.size.x * prevArea.size.y * prevArea.size.z];
        FillTiles(toClear, TileType.Empty);
        TempTilemap.SetTilesBlock(prevArea, toClear);
    }

    private void FollowBuilding() {
    ClearArea();
    temp.area.position = gridLayout.WorldToCell(temp.gameObject.transform.position);
    BoundsInt buildingArea = temp.area;

    TileBase[] baseArray = GetTilesBlock(buildingArea, MainTilemap);
    
    int size = baseArray.Length;
    TileBase[] tileArray = new TileBase[size];

    for (int i = 0; i < size; i++)
    {
        Debug.Log(baseArray[i]);
        if (baseArray[i] == tileBases[TileType.White])
        {
            tileArray[i] = tileBases[TileType.Green];
        }
        else
        {
            FillTiles(tileArray, TileType.Red);
            break;
        }
    }

    TempTilemap.SetTilesBlock(buildingArea, tileArray);
    prevArea = buildingArea;
    }


    public bool CanTakeArea(BoundsInt area) {
        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);
        foreach (var tile in baseArray) {
            if (tile != tileBases[TileType.White]) {
                Debug.Log("Can't take area");
                return false;
            }
        }

        return true;
    }

    public void TakeArea(BoundsInt area) {
        SetTilesBlock(area, TileType.Empty, TempTilemap);
        SetTilesBlock(area, TileType.Green, MainTilemap);
    }

    #endregion
}

public enum TileType {
    Empty,
    White,
    Green,
    Red,
}