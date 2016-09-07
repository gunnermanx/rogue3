using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopCashierInventorySlot : InventorySlot {

	public override void OnDrop( PointerEventData eventData ) {
		InventoryItem draggedItem = InventoryItem.DraggedInventoryItem;
		if ( draggedItem == null ) {
			return;
		}

//		// Just accept into the empty slot
//		if ( _acceptedItem == null ) {			
//			draggedItem.RemoveFromParentInventorySlot();
//			AcceptInventoryItem( draggedItem );
//		}
//		// We need to swap
//		else {
//			// First accept our item into the dragged item's parent slot
//			draggedItem.ParentInventorySlot.AcceptInventoryItem( _acceptedItem );
//			// Next accept the dragged item into our slot
//			AcceptInventoryItem( draggedItem );
//		}
	}
}


