using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class CharacterSaveSlot : MonoBehaviour {

	[SerializeField]
	private Image _portraitImage;

	[SerializeField]
	private Text _nameLabel;

	[SerializeField]
	private GameObject _emptyContainer;

	[SerializeField]
	private GameObject _fullContainer;

	private string _slotName = null;
	private CharacterBlob _blob = null;

	private Action<string> _createCharacterCallback = null;
	private Action<string> _deleteCharacterCallback = null;
	private Action<string> _chooseCharacterCallback = null;

	public void Initialize( string slotName, CharacterBlob blob, Action<string> createCharacterCallback, Action<string> deleteCharacterCallback, Action<string> chooseCharacterCallback ) {
		_slotName = slotName;
		_blob = blob;

		_createCharacterCallback = createCharacterCallback;
		_deleteCharacterCallback = deleteCharacterCallback;
		_chooseCharacterCallback = chooseCharacterCallback;

		if ( _blob != null ) {
			InitializeFullView();
		} else {
			InitializeEmptyView();
		}
	}

	private void InitializeEmptyView() {
		_emptyContainer.SetActive( true );
		_fullContainer.SetActive( false );
	}

	private void InitializeFullView() {
		_emptyContainer.SetActive( false );
		_fullContainer.SetActive( true );

		_nameLabel.text = _blob.Name;
	}

	public void CreateCharacterTapped() {
		_createCharacterCallback( _slotName );
	}

	public void DeleteCharacterTapped() {
		_deleteCharacterCallback( _slotName );
	}

	public void ChooseCharacterTapped() {
		_chooseCharacterCallback( _slotName );
	}
}
