using UnityEngine;
using System.Collections.Generic;

public class Database : MonoBehaviour {
	
#region Path Constants
	private const string STAGES_TEST_PATH = "Database/Stages/Test/";
	private const string WORLD_PATH = "Database/Worlds/World{worldNum}";
	private const string WORLD_STAGES_PATH = "Database/Stages/World{worldNum}/";
	private const string TILES_WEAPONS_PATH = "Database/Tiles_Weapons/";
	private const string TILES_OBSTRUCTIONS_PATH = "Database/Tiles_Obstructions/";
#endregion
	
#region Database Dictionaries
	private Dictionary<string,BattleStageData> _testStageData = new Dictionary<string, BattleStageData>();
	private Dictionary<string, WeaponTileData> _weaponTileData = new Dictionary<string, WeaponTileData>();
	private Dictionary<string,BattleStageData> _worldStageData = new Dictionary<string, BattleStageData>();
	private WorldData _worldData = null;
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
		string worldPath = WORLD_PATH;
		worldPath = worldPath.Replace( "{worldNum}", worldId.ToString() );
		_worldData = Resources.Load( worldPath ) as WorldData;

		string stagesPath = WORLD_STAGES_PATH;
		stagesPath = stagesPath.Replace( "{worldNum}", worldId.ToString() );
		Object[] stages = Resources.LoadAll( stagesPath );
		for ( int i = 0, count = stages.Length; i < count; i++ ) {
			BattleStageData data = stages[ i ] as BattleStageData;
			_worldStageData.Add( data.name, data );
		}
	}

	public void UnloadWorldStageData() {
		if ( _worldData != null ) {
			Resources.UnloadAsset( _worldData );
		}
		_worldData = null;

		foreach( KeyValuePair<string, BattleStageData> kvp in _worldStageData ) {
			Resources.UnloadAsset( kvp.Value );
		}
		_worldStageData.Clear();
	}

	public BattleStageData GetRandomBattleStageData() {
		List<string> keys = new List<string>( _worldStageData.Keys );
		string randomStageId = keys[ UnityEngine.Random.Range( 0, keys.Count ) ];
		return _worldStageData[ randomStageId ];
	}

	public BattleStageData GetBattleStageData( string id ) {
		BattleStageData data = null;
		_worldStageData.TryGetValue( id, out data );
		return data;
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
