﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TilePickerDialog : BaseDialog {

	[SerializeField]
	private DynamicInventory _ownedWeaponsInventory;

	[SerializeField]
	private StaticInventory _equippedWeaponsInventory;

	public const string DIALOG_ID = "WEAPON_PICKER";
	public override string GetDialogId() {
		return DIALOG_ID;		
	}

	private Action<List<WeaponTileData>> _startButtonCallback = null;

	public void Initialize( List<string> weaponDataIds, Action<List<WeaponTileData>> startButtonCallback ) {
		// Initialize the owned weapons inventory ( dynamic )
		List<InventoryItemData> inventoryData = new List<InventoryItemData>();
		for ( int i = 0, count = weaponDataIds.Count; i < count; i++ ) {
			inventoryData.Add( 
				new InventoryItemData() {
					DataId = weaponDataIds[i],
					Count = 1
				}
			);
		}	
		_ownedWeaponsInventory.Initialize( inventoryData );

		// Initialize the equipped weapons inventory ( static )
		_equippedWeaponsInventory.Initialize( null );

		_startButtonCallback = startButtonCallback;
	}

	public void StartButtonTapped() {

		// Get weapon data from inventory
		List<WeaponTileData> weaponTileData = new List<WeaponTileData>();
		List<InventoryItemData> itemData = _equippedWeaponsInventory.GetStoredInventoryItemData();
		for ( int i = 0, count = itemData.Count; i < count; i++ ) {
			weaponTileData.Add( Database.Instance.GetWeaponTileData( itemData[i].DataId ) );
		}

		if ( _startButtonCallback != null ) {
			_startButtonCallback( weaponTileData );
		}

		UIManager.Instance.CloseDialog( DIALOG_ID );
	}
}
