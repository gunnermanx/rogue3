using System;
using UnityEngine;
using UnityEngine.UI;

public class GameResultsDialog : BaseDialog {

	//temp
	public Text ResultLabel;

	[SerializeField]
	private Transform _dropsParentTransform;

	[SerializeField]
	private GameResultsItemDrop _itemDropPrefab;

	private Action _continueButtonCallback = null;

	public const string DIALOG_ID = "GAME_RESULTS";
	public override string GetDialogId() {
		return DIALOG_ID;
	}

	public void Initialize( Battle.SessionResults results, Action continueButtonCallback ) {

		// Temp
		if ( results.IsVictory ) {
			ResultLabel.text = "VICTORY";

			for ( int i = 0, count = results.Drops.Count; i < count; i++ ) {
				LootTableDrop drop = results.Drops[ i ];
				GameObject go = GameObject.Instantiate( _itemDropPrefab.gameObject ) as GameObject;
				go.transform.SetParent( _dropsParentTransform );
				go.transform.localScale = Vector3.one;

				go.GetComponent<GameResultsItemDrop>().Initialize( drop );
			}
		} else {
			ResultLabel.text = "DEFEAT";
		}



		_continueButtonCallback = continueButtonCallback;
	}

	public void ContinueButtonTapped() {
		if ( _continueButtonCallback != null ) {
			_continueButtonCallback();
		}
		UIManager.Instance.CloseDialog( DIALOG_ID );
	}
}


