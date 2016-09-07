using System;
using UnityEngine;
using System.Collections.Generic;

public class ShopDialog : BaseDialog {

	[SerializeField]
	private StaticInventory _shopCashier;

	[SerializeField]
	private DynamicInventory _weaponInventory;

	[SerializeField]
	private DynamicInventory _itemInventory;

	public const string DIALOG_ID = "ShopDialog";
	public override string GetDialogId() {
		return DIALOG_ID;		
	}

	public void Initialize( ShopStageData shopData ) {

		List<InventoryItemData> shopWeaponInventory = new List<InventoryItemData>();
		for ( int i = 0, count = shopData.Weapons.Count; i < count; i++ ) {
			shopWeaponInventory.Add( 
				new InventoryItemData() {
					DataId = shopData.Weapons[ i ].name,
					Count = 1
				}
			);
		}

		_weaponInventory.Initialize( shopWeaponInventory );
	}

}

