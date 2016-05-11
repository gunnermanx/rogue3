using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DynamicInventory : MonoBehaviour {

	[SerializeField]
	protected GameObject InventorySlotPrefab;

	[SerializeField]
	protected GameObject InventoryItemPrefab;

	[SerializeField]
	protected Transform InventorySlotContainer;

	private List<InventorySlot> _inventorySlots = new List<InventorySlot>();

	public void Initialize( List<InventoryItemData> inventoryData ) {
		CreateInventorySlotsAndItems( inventoryData );
	}

	private void CreateInventorySlotsAndItems( List<InventoryItemData> inventoryData ) {
		// Create as many slots as needed, round up if odd
		int numSlotsCreated = inventoryData.Count % 2 == 0 ? inventoryData.Count : inventoryData.Count+1;
		for ( int i = 0; i < numSlotsCreated; i++ ) {

			// Skip dummy slots
			if ( i >= inventoryData.Count ) {
				break;
			}

			// Create the inventory slot
			GameObject slotGO = GameObject.Instantiate( InventorySlotPrefab );
			slotGO.transform.SetParent( InventorySlotContainer );
			slotGO.transform.localScale = Vector3.one;
			InventorySlot slot = slotGO.GetComponent<InventorySlot>();
			_inventorySlots.Add( slot );

			// Create the inventory item
			GameObject itemGO = GameObject.Instantiate( InventoryItemPrefab );		
			// Initialize the item
			InventoryItem item = itemGO.GetComponent<InventoryItem>();
			item.Initialize( inventoryData[ i ] );
			// Accept the item into the slot
			slot.AcceptInventoryItem( item );
		}
	}

	public List<InventoryItemData> GetStoredInventoryItemData() {
		List<InventoryItemData> data = new List<InventoryItemData>();
		for ( int i = 0, count = _inventorySlots.Count; i < count; i++ ) {
			data.Add( _inventorySlots[ i ].Item.InventoryItemData );
		}
		return data;
	}
}
