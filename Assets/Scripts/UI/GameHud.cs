using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameHud : BaseDialog {

	[SerializeField]
	private Text _turnLimitText;
	[SerializeField]
	private Text _enemyTurnCounterText;
	[SerializeField]
	private Slider _hpSlider;
	[SerializeField]
	private GameResults _resultsPanel;

	public const string DIALOG_ID = "GAME_HUD";
	public override string GetDialogId() {
		return DIALOG_ID;		
	}

	public void UpdateTurnsRemaining( int turnsRemaining ) {
		_turnLimitText.text = turnsRemaining.ToString();
	}

	public void UpdateEnemyTurnCounter( int enemyTurnCounter ) {
		_enemyTurnCounterText.text = enemyTurnCounter.ToString();
	}

	public void UpdateHPBar( int current, int max ) {
		_hpSlider.value = (float)current / (float) max;
	}

	public void ShowResults( BattleManager.SessionResults results ) {
		if ( results != null ) {
			_resultsPanel.gameObject.SetActive( true );
			_resultsPanel.Initialize( results );
		} else {
			Debug.LogError( "results are null?" );
		}
	}

	public void ContinueButtonTapped() {
		GameManager.Instance.CompleteGame();
	}
}

