using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler {

	private InventoryItem _acceptedItem = null;
	public InventoryItem Item { get { return _acceptedItem; } } 

	public void AcceptInventoryItem( InventoryItem item ) {
		_acceptedItem = item;
		item.transform.SetParent( transform );
		item.transform.localScale = Vector3.one;
		item.ParentInventorySlot = this;
	}

	public void ClearSlot() {
		_acceptedItem = null;
	}

	public void OnDrop( PointerEventData eventData ) {
		InventoryItem draggedItem = InventoryItem.DraggedInventoryItem;
		if ( draggedItem == null ) {
			return;
		}

		// Just accept into the empty slot
		if ( _acceptedItem == null ) {			
			draggedItem.RemoveFromParentInventorySlot();
			AcceptInventoryItem( draggedItem );
		}
		// We need to swap
		else {
			// First accept our item into the dragged item's parent slot
			draggedItem.ParentInventorySlot.AcceptInventoryItem( _acceptedItem );
			// Next accept the dragged item into our slot
			AcceptInventoryItem( draggedItem );
		}
	}
}


