using System;
using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
	
	[SerializeField]
	private List<GameObject> _dialogPrefabs = new List<GameObject>();

	private Dictionary<string, GameObject> _dialogPrefabMap = new Dictionary<string, GameObject>();

	private Dictionary<string, BaseDialog> _openedDialogs = new Dictionary<string, BaseDialog>();

	// Singleton Accessor
	private static UIManager _instance = null;
	public static UIManager Instance {
		get { return _instance; }
	}


	private void Awake() {
		_instance = this;
		DontDestroyOnLoad( gameObject );
	}

	public BaseDialog OpenDialog( string dialogId ) {

		GameObject dialogPrefab = null;
		BaseDialog dialog = null;


		// Check if the dialog is opened
		if ( _openedDialogs.TryGetValue( dialogId, out dialog ) ) {

			// unhide/initialize dialog?

			return dialog;
		}

		// First check the map to see if the mapping is cached
		if ( !_dialogPrefabMap.TryGetValue( dialogId, out dialogPrefab ) ) {
			// Check the list of dialog prefabs	
			for ( int i = 0, count = _dialogPrefabs.Count; i < count; i++ ) {
				dialog = _dialogPrefabs[ i ].GetComponent<BaseDialog>();
				if ( dialog.GetDialogId() == dialogId ) {
					dialogPrefab = _dialogPrefabs[ i ];
					_dialogPrefabMap.Add( dialogId, dialogPrefab );
					break;
				}
			}
		}

		// Instantiate the prefab
		GameObject dialogGO = GameObject.Instantiate( dialogPrefab, Vector3.zero, Quaternion.identity ) as GameObject;
		dialogGO.transform.SetParent( transform, false );
		dialog = dialogGO.GetComponent<BaseDialog>();

		// Initialize the dialog

		_openedDialogs.Add( dialogId, dialog );

		return dialog;
	}

	public void CloseDialog( string dialogId ) {
		
		BaseDialog dialog = null;
		if ( _openedDialogs.TryGetValue( dialogId, out dialog ) ){

			// Cleanup the dialog

			_openedDialogs.Remove( dialogId );
			GameObject.Destroy( dialog.gameObject );
		}
	}
}

