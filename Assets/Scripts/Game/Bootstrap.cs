using System;
using System.Collections;
using UnityEngine;

public class Bootstrap : MonoBehaviour {

	[SerializeField]
	private GameObject _loadingUI;

	[SerializeField]
	private LootTableManager _lootTableManager;

	[SerializeField]
	private PersistenceManager _persistenceManager;

	[SerializeField]
	private GameManager _gameManager;

	private void Start() {
		StartCoroutine( LoadingLoop() );
	}

	private IEnumerator LoadingLoop() {
		yield return StartCoroutine( _persistenceManager.LoadPlayerDataFile() );
		yield return StartCoroutine( _lootTableManager.LoadLootTableData() );

		GameObject.Destroy( _loadingUI );

		_lootTableManager.RollFromTable( "testTableId" );

		_gameManager.Startup();
	}
}

