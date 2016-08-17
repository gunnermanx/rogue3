using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class PersistenceManager : MonoBehaviour {

	// FOR NOW NO NETWORK STUFF, STORED LOCALLY, 
	//TODO HUGE TODO

	public delegate void PlayerBlobLoadedDelegate( PlayerBlob playerBlob );

	private WWW _fileWWW = null;
	private PlayerBlobLoadedDelegate _callback;


	private PlayerBlob _playerBlob;
	public PlayerBlob PlayerBlob{ get { return _playerBlob; } }

	private CharacterBlob _characterBlob = null;
	public CharacterBlob CharacterBlob { get { return _characterBlob; } }


	public IEnumerator LoadPlayerDataFile(){
		Debug.Log ( "LoadPlayerData start" );

		string playerDataText = null;

		// loading from persistent data
		string filePath = Application.persistentDataPath + "/PlayerBlob.json";
		if( File.Exists( filePath ) ) {
			_fileWWW = new WWW( "file://" + filePath );
			yield return _fileWWW;
			if( _fileWWW.bytes.Length > 0 ){
				playerDataText = _fileWWW.text;
			}

			_playerBlob = (PlayerBlob) Serializer.Deserialize( typeof(PlayerBlob), playerDataText );
		}
		// first time load
		else {
			_playerBlob = PlayerBlob.NewPlayerBlob();
		}

		Debug.Log ( "LoadPlayerData done!" );
	}

	public void CreateNewCharacter( string slotName ) {
		CharacterBlob blob;
		if ( _playerBlob.CharacterBlobSlots.TryGetValue( slotName, out blob ) ) {
			if ( blob != null ) {
				Debug.Log( "Can't create new character on existing slot, Delete the character first" );
			} else {
				CharacterBlob newChar = CharacterBlob.NewCharacterBlob( "testName" );
				_playerBlob.CharacterBlobSlots[ slotName ] = newChar;
			}
		}

		SavePlayerData();
	}

	public void DeleteCharacter( string slotName ) {
		CharacterBlob blob;
		if ( _playerBlob.CharacterBlobSlots.TryGetValue( slotName, out blob ) ) {
			if ( blob != null ) {
				_playerBlob.CharacterBlobSlots[ slotName ] = null;
			} else {
				Debug.Log( "Slot " + slotName + " is empty, nothing to delete" );
			}
		}

		SavePlayerData();
	}

	public void PickCharacter( string slotName ) {
		CharacterBlob blob = null;
		if ( !_playerBlob.CharacterBlobSlots.TryGetValue( slotName, out blob ) ) {
			Debug.LogError( "Tried to pick character in slot: " + slotName + " and it does not exist in the blob" );
		}
		_characterBlob = blob;
	}

	public void SavePlayerData() {
		string playerDataText = Serializer.Serialize( typeof(PlayerBlob), _playerBlob );
		System.IO.File.WriteAllText( Application.persistentDataPath + "/PlayerBlob.json", playerDataText );
	}

	public void DeletePlayerData() {
		System.IO.File.Delete( Application.persistentDataPath + "/PlayerBlob.json" );
	}

	public void SaveMapData( MapBlob blob ) {
		_characterBlob.MapBlob = blob;
		SavePlayerData();
	}

	public void SaveCompletedNode( string completedNodeId ) {
		Debug.Assert( !_characterBlob.MapBlob.CompletedNotes.Contains( completedNodeId ), "Trying to complete already completed stage" );

		_characterBlob.MapBlob.CompletedNotes.Add( completedNodeId );
		SavePlayerData();
	}

	public void UpdateCurrentLives( int currentLives ) {
		_characterBlob.CurrentLives = currentLives;
		SavePlayerData();
	}

	public void UpdateMaxLives( int maxLives ) {
		_characterBlob.MaxLives = maxLives;
		SavePlayerData();
	}

	public void AddWeapon( string weaponId ) {
		if ( !_characterBlob.OwnedTileIds.Contains( weaponId ) ) {			
			_characterBlob.OwnedTileIds.Add( weaponId );
		}
		SavePlayerData();
	}

	public void AddGold( int amount ) {
		_characterBlob.Gold += amount;
		SavePlayerData();
	}
}