using UnityEngine;
using UnityEditor;
using System.IO;

public class ScriptableObjectUtility {
	
	[MenuItem("Assets/Create/ScriptableObjects/BattleStageData")]
	public static void CreateBattleStageData() {
		CreateScriptableObject<BattleStageData>();
	}

	[MenuItem("Assets/Create/ScriptableObjects/WeaponTileData")]
	public static void CreateWeaponTileData() {
		CreateScriptableObject<WeaponTileData>();
	}

	[MenuItem("Assets/Create/ScriptableObjects/ObstructionTileData")]
	public static void CreateObstructionTileData() {
		CreateScriptableObject<ObstructionTileData>();
	}

	[MenuItem("Assets/Create/ScriptableObjects/EnemyAttackDataSet")]
	public static void CreateEnemyAttackDataSet() {
		CreateScriptableObject<EnemyAttackDataSet>();
	}

	
	private static void CreateScriptableObject<T> () where T: ScriptableObject {
		T sObject = ScriptableObject.CreateInstance<T>();
		
		string path = "Assets";
		foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
		{
			path = AssetDatabase.GetAssetPath(obj);
			if (File.Exists(path))
			{
				path = Path.GetDirectoryName(path);
			}
			break;
		}
		
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath( path + "/New " + typeof(T).ToString() + ".asset" );
		
		AssetDatabase.CreateAsset( sObject, assetPathAndName );
		AssetDatabase.SaveAssets();
		Selection.activeObject = sObject;
	}
}