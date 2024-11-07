using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
   public void OnDrop(PointerEventData eventData)
   {
       Debug.Log("Dropped");
       if (transform.childCount > 0)
       {
           return;
       }
       GameObject droppedItem = eventData.pointerDrag;
       DraggableItem draggableitem = droppedItem.GetComponent<DraggableItem>();
       draggableitem.parentAfterDrag = transform;
   }
}
