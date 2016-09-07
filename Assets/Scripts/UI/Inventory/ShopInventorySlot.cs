using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopInventorySlot : InventorySlot {

	[SerializeField]
	private Image _fadedSprite;

	public void ShowFadedSprite( Sprite sprite ) {
		_fadedSprite.gameObject.SetActive( true );
		_fadedSprite.sprite = sprite;
	}

	public void HideFadedSprite() {
		_fadedSprite.gameObject.SetActive( false );
	}
}

