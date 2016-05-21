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


	public void LoadPlayerData( PlayerBlobLoadedDelegate playerDataLoadedCallback ) {
		PlayerBlob playerData = null;
		_callback = playerDataLoadedCallback;

		StartCoroutine( LoadPlayerDataFile( playerData ) );
	}


	private IEnumerator LoadPlayerDataFile( PlayerBlob playerData ){

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
		}
		// first time load
		else {
			TextAsset textData = (TextAsset) Resources.Load("NewPlayerBlob");
			playerDataText = textData.text;
		}

		if ( !string.IsNullOrEmpty( playerDataText ) ) {
			_playerBlob = (PlayerBlob) Serializer.Deserialize( typeof(PlayerBlob), playerDataText );
		}
			
		_callback( _playerBlob );

		Debug.Log ( "LoadPlayerData done!" );
	}

	public void SavePlayerData() {
		string playerDataText = Serializer.Serialize( typeof(PlayerBlob), _playerBlob );
		System.IO.File.WriteAllText( Application.persistentDataPath + "/PlayerBlob.json", playerDataText );
	}

	public void DeletePlayerData() {
		System.IO.File.Delete( Application.persistentDataPath + "/PlayerBlob.json" );
	}

	public void SaveMapData( MapBlob blob ) {
		_playerBlob.MapBlob = blob;
		SavePlayerData();
	}
}