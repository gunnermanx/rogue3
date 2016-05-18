using System;
using UnityEngine;
using UnityEngine.UI;

public class GameResultsDialog : BaseDialog {

	//temp
	public Text ResultLabel;

	public const string DIALOG_ID = "GAME_RESULTS";
	public override string GetDialogId() {
		return DIALOG_ID;
	}

	public void Initialize( Battle.SessionResults results ) {

		// Temp
		if ( results.IsVictory ) {
			ResultLabel.text = "VICTORY";
		} else {
			ResultLabel.text = "DEFEAT";
		}
	}
}


