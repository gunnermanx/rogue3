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
		_loadingUI.gameObject.SetActive( true );
		StartCoroutine( LoadingLoop() );
	}

	private IEnumerator LoadingLoop() {
		yield return StartCoroutine( _persistenceManager.LoadPlayerDataFile() );
		yield return StartCoroutine( _lootTableManager.LoadLootTableData() );

		GameObject.Destroy( _loadingUI );

		_gameManager.Startup();
	}
}

