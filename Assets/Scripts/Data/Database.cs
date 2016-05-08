using UnityEngine;
using System.Collections.Generic;

public class Database : MonoBehaviour {
	
	#region Path Constants
	private const string STAGES_TEST_PATH = "Database/Stages/Test/";
	#endregion
	
	#region Database Dictionaries
	private Dictionary<string,BattleStageData> _testStageData = new Dictionary<string, BattleStageData>();
	#endregion

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
}
