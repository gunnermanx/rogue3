using System;
using UnityEngine;
using UnityEngine.UI;

public class MapHud : BaseDialog {

	[SerializeField]
	private Button _travelButton;

	public const string DIALOG_ID = "MAP_HUD";
	public override string GetDialogId() {
		return DIALOG_ID;		
	}

	private Action _travelButtonCallback = null;

	public void Initialize( Action travelButtonCallback ) {
		_travelButtonCallback = travelButtonCallback;
	}

	public void ToggleTravelButton( bool toggle ) {
		_travelButton.gameObject.SetActive( toggle );
	}

	public void TravelButtonTapped() {
		if ( _travelButtonCallback != null ) {
			_travelButtonCallback();
		}
	}
}

