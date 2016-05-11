using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponPicker : MonoBehaviour {

	[SerializeField]
	private DynamicInventory _ownedWeaponsInventory;

	[SerializeField]
	private StaticInventory _equippedWeaponsInventory;

	private void Awake() {
		GameManager.Instance.RegisterWeaponPicker( this );
	}

	private void OnDestroy() {
		GameManager.Instance.UnregisterWeaponPicker();
	}

	public void Initialize( List<string> weaponDataIds ) {
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
	}

	public void StartButtonTapped() {

		// Get weapon data from inventory
		List<WeaponTileData> weaponTileData = new List<WeaponTileData>();
		List<InventoryItemData> itemData = _equippedWeaponsInventory.GetStoredInventoryItemData();
		for ( int i = 0, count = itemData.Count; i < count; i++ ) {
			weaponTileData.Add( Database.Instance.GetWeaponTileData( itemData[i].DataId ) );
		}

		// Get stage data
		BattleStageData stageData = Database.Instance.GetRandomTestBattleStageData();

		GameManager.Instance.StartGame( weaponTileData, stageData );


	}
}
