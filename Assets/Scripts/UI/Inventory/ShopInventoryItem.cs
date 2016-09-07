using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopInventoryItem : InventoryItem {

	public override void OnBeginDrag( PointerEventData eventData ) {
		base.OnBeginDrag( eventData );

		ShopInventorySlot parentSlot = ParentInventorySlot as ShopInventorySlot;
		if ( parentSlot != null ) {
			parentSlot.ShowFadedSprite( _image.sprite );
		} else {
			Debug.LogError( "error!" );
		}
	} 

	public override void OnEndDrag( PointerEventData eventData ) {
		base.OnEndDrag( eventData );

		ShopInventorySlot parentSlot = ParentInventorySlot as ShopInventorySlot;
		if ( parentSlot != null ) {
			parentSlot.HideFadedSprite();
		} else {
			Debug.LogError( "error!" );
		}
	}
}

