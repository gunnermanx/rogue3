using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponPicker : MonoBehaviour {

	[SerializeField]
	private GameObject InventorySlotPrefab;

	[SerializeField]
	private Transform InventorySlotContainer;

	private void Start() {
		
	}

	private void CreateInventorySlots() {
		
	}

	public void StartButtonTapped() {
		
		List<WeaponTileData> data = new List<WeaponTileData>();
		data.Add( Database.Instance.GetWeaponTileData( "WoodenAxe" ) );
		data.Add( Database.Instance.GetWeaponTileData( "WoodenBow" ) );
		data.Add( Database.Instance.GetWeaponTileData( "WoodenSword" ) );
		data.Add( Database.Instance.GetWeaponTileData( "WoodenStaff" ) );
		BattleStageData stageData = Database.Instance.GetRandomTestBattleStageData();

		GameManager.Instance.StartGame( data, stageData );


	}
}
