using UnityEngine;
using System.Collections.Generic;

public class Database : MonoBehaviour {
	
#region Path Constants
	private const string STAGES_TEST_PATH = "Database/Stages/Test/";
	private const string STAGES_PATH = "Database/Stages/World{worldNum}/";
	private const string TILES_WEAPONS_PATH = "Database/Tiles_Weapons/";
	private const string TILES_OBSTRUCTIONS_PATH = "Database/Tiles_Obstructions/";
#endregion
	
#region Database Dictionaries
	private Dictionary<string,BattleStageData> _testStageData = new Dictionary<string, BattleStageData>();
	private Dictionary<string, WeaponTileData> _weaponTileData = new Dictionary<string, WeaponTileData>();

	private Dictionary<string, BattleStageData> _worldStageData = new Dictionary<string, BattleStageData>();
#endregion

	// Singleton Accessor
	private static Database _instance = null;
	public static Database Instance {
		get { return _instance; }
	}

	private void Awake() {
		_instance = this;
	}

	public void LoadWorldStageData( int worldId ) {
		if ( _worldStageData.Count == 0 ) {
			string path = STAGES_PATH;
			path.Replace( "{worldNum}", worldId.ToString() );
			Object[] allWorldStages = Resources.LoadAll( path );
			for ( int i = 0, count = allWorldStages.Length; i < count; i++ ) {
				BattleStageData data = allWorldStages[ i ] as BattleStageData;
				_worldStageData.Add( data.name, data );
			}
		}
	}

	public void UnloadWorldStageData() {		
		foreach( KeyValuePair<string, BattleStageData> kvp in _worldStageData ) {
			Resources.UnloadAsset( kvp.Value );
		}
		_worldStageData.Clear();
	}

	public BattleStageData GetRandomTestBattleStageData() {
		if ( _testStageData.Count == 0 ) {
			Object[] allTestStages = Resources.LoadAll( STAGES_TEST_PATH );
			for ( int i = 0, count = allTestStages.Length; i < count; i++ ) {
				BattleStageData data = allTestStages[ i ] as BattleStageData;
				_testStageData.Add( data.name, data );
			}
			return (BattleStageData)allTestStages[ UnityEngine.Random.Range( 0, allTestStages.Length )];
		}
		else {
			int index = UnityEngine.Random.Range( 0, _testStageData.Count );
			string[] keys = new string[ _testStageData.Keys.Count ];
			_testStageData.Keys.CopyTo( keys, 0 );
			return _testStageData[ keys[ index ] ];
		}
	}
		
	public BattleStageData GetBattleStageData( string id ) {
		BattleStageData data = null;
		string path = null;
		if ( !_testStageData.TryGetValue( id, out data ) ) {
			path = STAGES_TEST_PATH + id;
			data = Resources.Load( path ) as BattleStageData;
		}
		#if DEBUG		
		Debug.Assert( data != null, "No BattleStageData found with id " + id + " at path " + path );
		#endif
		return data;
	}

	public WeaponTileData GetWeaponTileData( string id ) {
		WeaponTileData data = null;
		string path = null;
		if ( !_weaponTileData.TryGetValue( id, out data ) ) {
			path = TILES_WEAPONS_PATH + id;
			data = Resources.Load( path ) as WeaponTileData;
		}
		#if DEBUG		
		Debug.Assert( data != null, "No WeaponTileData found with id " + id + " at path " + path );
		#endif
		return data;
	}
}
