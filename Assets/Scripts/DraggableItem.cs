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
        image.transform.position = Camera.main.WorldToScreenPoint(GridBuildingSystem.current.gridLayout.CellToWorld(areaTemp.position));

        Placed = true;
        GridBuildingSystem.current.TakeArea(areaTemp);

        // Log position and placement for debugging
        Debug.Log($"Placed building at: {positionInt}, Bounds: {areaTemp}");
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

        // Snap the draggable item to the cell position
        Vector3 snappedPosition = gridBuildingSystem.gridLayout.CellToWorld(cellPosition);
        snappedPosition.z = 0; // Ensure z-position is set correctly
        transform.position = snappedPosition;

        // Update the area position
        area.position = cellPosition;

        // Update the position of the associated image
        image.transform.position = Camera.main.WorldToScreenPoint(snappedPosition);

        Debug.Log($"Mouse position: {Input.mousePosition}, World position: {worldPosition}, Cell position: {cellPosition}, Snapped position: {snappedPosition}");

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
