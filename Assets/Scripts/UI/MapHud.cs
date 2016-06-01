using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapHud : BaseDialog {

	[SerializeField]
	private Button _travelButton;

	[SerializeField]
	private GameObject _lifeCounterPrefab;

	[SerializeField]
	private GridLayoutGroup _lifeCounterGrid;

	private List<LifeCounter> _lifeCounters = new List<LifeCounter>();


	public const string DIALOG_ID = "MAP_HUD";
	public override string GetDialogId() {
		return DIALOG_ID;		
	}

	private Action _travelButtonCallback = null;


	public void UpdateLife( int life, int maxLife ) {

		if ( life > maxLife ) {
			//error
			return;
		}

		if ( _lifeCounters.Count < maxLife ) {
			for ( int i = _lifeCounters.Count; i < maxLife; i++ ) {
				GameObject go = GameObject.Instantiate( _lifeCounterPrefab ) as GameObject;
				LifeCounter lifeCounter = go.GetComponent<LifeCounter>();

				_lifeCounters.Add( lifeCounter );
				lifeCounter.transform.SetParent( _lifeCounterGrid.transform );
				go.transform.localScale = Vector3.one;
			}
		}

		for ( int i = 0; i < life; i++ ) {
			_lifeCounters[ i ].ToggleFilledState( true );
		}
		for ( int i = life; i < maxLife; i++ ) {
			_lifeCounters[ i ].ToggleFilledState( false );
		}
	}

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

