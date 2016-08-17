using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameResultsItemDrop : MonoBehaviour {

	[SerializeField]
	private Image _itemIcon;

	[SerializeField]
	private Text _itemQuantity;

	public void Initialize( LootTableDrop drop ) {
		if ( drop.Type == LootTableDrop.DropType.WEAPON ) {
			WeaponTileData data = Database.Instance.GetWeaponTileData( drop.ItemId );
			_itemIcon.sprite = data.Sprite;
			_itemQuantity.text = drop.Amount.ToString();
		} 
		else if ( drop.Type == LootTableDrop.DropType.CURRENCY ) {
			BaseItemData data = Database.Instance.GetItemData( drop.ItemId );
			_itemIcon.sprite = data.Sprite;
			_itemQuantity.text = drop.Amount.ToString();
		}
	}
}
