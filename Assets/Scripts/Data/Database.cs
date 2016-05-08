using UnityEngine;
using System.Collections.Generic;

public class Database : MonoBehaviour {
	
#region Path Constants
	private const string STAGES_TEST_PATH = "Database/Stages/Test/";
	private const string TILES_WEAPONS_PATH = "Database/Tiles_Weapons/";
	private const string TILES_OBSTRUCTIONS_PATH = "Database/Tiles_Obstructions/";
#endregion
	
#region Database Dictionaries
	private Dictionary<string,BattleStageData> _testStageData = new Dictionary<string, BattleStageData>();
	private Dictionary<string, WeaponTileData> _weaponTileData = new Dictionary<string, WeaponTileData>();
	private Dictionary<string, ObstructionTileData> _obstructionTileData = new Dictionary<string, ObstructionTileData>();	
#endregion

	// Singleton Accessor
	private static Database _instance = null;
	public static Database Instance {
		get { return _instance; }
	}

	private void Awake() {
		_instance = this;
	}

	public BattleStageData GetRandomTestBattleStageData() {
		Object[] allTestStages = Resources.LoadAll( STAGES_TEST_PATH );
		for ( int i = 0, count = allTestStages.Length; i < count; i++ ) {
			BattleStageData data = allTestStages[ i ] as BattleStageData;
			_testStageData.Add( data.name, data );
		}
		return (BattleStageData)allTestStages[ UnityEngine.Random.Range( 0, allTestStages.Length )];
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
