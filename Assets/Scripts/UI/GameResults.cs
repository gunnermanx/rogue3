using System;
using UnityEngine;
using UnityEngine.UI;

public class GameResults : MonoBehaviour {
	//temp
	public Text ResultLabel;

	public void Initialize( Battle.SessionResults results ) {

		// Temp
		if ( results.IsVictory ) {
			ResultLabel.text = "VICTORY";
		} else {
			ResultLabel.text = "DEFEAT";
		}
	}
}


