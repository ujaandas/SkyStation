using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    [HideInInspector] public Transform parentAfterDrag;
    private GridBuildingSystem gridBuildingSystem;

    public bool Placed { get; private set; }
    public BoundsInt area;

    private GameObject tilemapRepresentation;

    private void Start()
    {
        gridBuildingSystem = GridBuildingSystem.current;
    }

    public bool CanBePlaced()
    {
        Vector3Int positionInt = GridBuildingSystem.current.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;

        if (GridBuildingSystem.current.CanTakeArea(areaTemp))
        {
            return true;
        }

        return false;
    }

    public void Place()
    {
        Vector3Int positionInt = GridBuildingSystem.current.gridLayout.LocalToCell(transform.position);
        BoundsInt areaTemp = area;
        areaTemp.position = positionInt;

        Placed = true;
        GridBuildingSystem.current.TakeArea(areaTemp);

        // Log position and placement for debugging
        Debug.Log($"Placed building at: {positionInt}, Bounds: {areaTemp}");

        // Instantiate a tilemap representation
        if (tilemapRepresentation == null)
        {
            tilemapRepresentation = new GameObject("TilemapRepresentation");
            SpriteRenderer sr = tilemapRepresentation.AddComponent<SpriteRenderer>();
            sr.sprite = image.sprite;
            tilemapRepresentation.transform.position = GridBuildingSystem.current.gridLayout.CellToWorld(areaTemp.position);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Convert the mouse position to a world position
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0; // Ensure z-position is set correctly

        // Convert the world position to a cell position
        Vector3Int cellPosition = gridBuildingSystem.gridLayout.WorldToCell(worldPosition);

        // Calculate the new position with the offset
        Vector3 newPosition = gridBuildingSystem.gridLayout.CellToWorld(cellPosition);

        transform.position = newPosition;
        area.position = cellPosition;

        // Update the position of the associated image
        image.transform.position = Camera.main.WorldToScreenPoint(newPosition);

        Debug.Log($"Mouse position: {Input.mousePosition}, World position: {worldPosition}, Cell position: {cellPosition}, New position: {newPosition}");

        // Follow with TempTiles
        gridBuildingSystem.FollowBuilding(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");

        if (EventSystem.current.IsPointerOverGameObject() && IsPointerOverGrid())
        {
            Vector2 touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPos = gridBuildingSystem.gridLayout.LocalToCell(touchPos);

            Debug.Log($"Touch Position: {touchPos}, Cell Position: {cellPos}, Area Position: {area.position}");

            if (gridBuildingSystem.CanTakeArea(area))
            {
                gridBuildingSystem.PlaceBuilding(cellPos, this);
                Destroy(gameObject);

                // Instantiate a new GameObject to represent the item in the Tilemap
                if (tilemapRepresentation == null)
                {
                    tilemapRepresentation = new GameObject("TilemapRepresentation");
                    SpriteRenderer sr = tilemapRepresentation.AddComponent<SpriteRenderer>();
                    sr.sprite = image.sprite;

                    // Center the tilemap representation as well
                    tilemapRepresentation.transform.position = gridBuildingSystem.gridLayout.CellToWorld(area.position) +
                                                              gridBuildingSystem.gridLayout.cellSize / 2;
                }
            }
        }

        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;
        image.raycastTarget = true;

        // Clear the current draggable item in the grid building system
        gridBuildingSystem.ClearArea(gridBuildingSystem.PrevArea);
    }


    private bool IsPointerOverGrid()
    {
        return true;
    }
}
