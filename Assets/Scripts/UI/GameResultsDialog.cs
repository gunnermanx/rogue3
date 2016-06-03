using System;
using UnityEngine;
using UnityEngine.UI;

public class GameResultsDialog : BaseDialog {

	//temp
	public Text ResultLabel;

	private Action _continueButtonCallback = null;

	public const string DIALOG_ID = "GAME_RESULTS";
	public override string GetDialogId() {
		return DIALOG_ID;
	}

	public void Initialize( Battle.SessionResults results, Action continueButtonCallback ) {

		// Temp
		if ( results.IsVictory ) {
			ResultLabel.text = "VICTORY";


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


