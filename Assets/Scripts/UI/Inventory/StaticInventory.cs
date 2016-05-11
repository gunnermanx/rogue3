using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StaticInventory: MonoBehaviour {

	[SerializeField]
	protected GameObject InventoryItemPrefab;

	[SerializeField]
	protected List<InventorySlot> InventorySlots;

	public void Initialize( List<InventoryItemData> inventoryData ) {
		CreateInventoryItems( inventoryData );
	}

	private void CreateInventoryItems( List<InventoryItemData> inventoryData ) {
		for ( int i = 0, count = InventorySlots.Count; i < count; i++ ) {
			// If there is data to populate with
			if ( inventoryData != null && i < inventoryData.Count ) {
				// do something with the data here

				// Create the inventory item inside
				GameObject itemGO = GameObject.Instantiate( InventoryItemPrefab );
				// Initialize the item
				InventoryItem item = itemGO.GetComponent<InventoryItem>();
				item.Initialize( inventoryData[ i ] );
				// Accept the item into the slot
				InventorySlots[i].AcceptInventoryItem( item );
			}
			else {
				break;
			}
		}
	}

	public List<InventoryItemData> GetStoredInventoryItemData() {
		List<InventoryItemData> data = new List<InventoryItemData>();
		for ( int i = 0, count = InventorySlots.Count; i < count; i++ ) {
			data.Add( InventorySlots[ i ].Item.InventoryItemData );
		}
		return data;
	}
}
