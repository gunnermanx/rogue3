﻿using UnityEngine;
using UnityEditor;
using System.IO;

public class ScriptableObjectUtility {
	
	[MenuItem("Assets/Create/ScriptableObjects/BattleStageData")]
	public static void CreateBattleStageData() {
		CreateScriptableObject<BattleStageData>();
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
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = sObject;
	}
}