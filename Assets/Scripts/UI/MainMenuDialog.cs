using UnityEngine;
using System.Collections.Generic;

public class MainMenuDialog : BaseDialog {
	
	[SerializeField]
	private CharacterSaveSlot _slotPrefab;

	[SerializeField]
	private Transform _slotParent;

	public const string DIALOG_ID = "MAIN_MENU";
	public override string GetDialogId() {
		return DIALOG_ID;		
	}

	public void Initialize( PersistenceManager persistenceManager ) {

		for ( int i = _slotParent.childCount-1; i >= 0; i-- ) {
			GameObject.Destroy( _slotParent.GetChild( i ).gameObject );
		}

		foreach( KeyValuePair<string, CharacterBlob> kvp in persistenceManager.PlayerBlob.CharacterBlobSlots ) {
			GameObject slotGO = GameObject.Instantiate( _slotPrefab.gameObject, Vector3.zero, Quaternion.identity ) as GameObject;
			slotGO.transform.SetParent( _slotParent );
			slotGO.transform.localScale = Vector3.one;

			CharacterSaveSlot slot = slotGO.GetComponent<CharacterSaveSlot>();
			slot.Initialize( kvp.Key, kvp.Value, 
				delegate( string slotName ) {
					persistenceManager.CreateNewCharacter( slotName );
					Initialize( persistenceManager );
				},
				delegate( string slotName ) {
					persistenceManager.DeleteCharacter( slotName );
					Initialize( persistenceManager );
				},
				delegate( string slotName ) {
					persistenceManager.PickCharacter( slotName );
					GameManager.Instance.LoadGameMap();
				}
			);
		}
	}
}
