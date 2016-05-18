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
	private Text _dotStatusDurationText;
	[SerializeField]
	private Text _dotStatusStackSizeText;
	[SerializeField]
	private Text _stunStatusDurationText;
	[SerializeField]
	private GameObject _dotStatus;
	[SerializeField]
	private GameObject _stunStatus;

	public const string DIALOG_ID = "GAME_HUD";
	public override string GetDialogId() {
		return DIALOG_ID;		
	}

	public void HideDoTStatus() {
		_dotStatus.SetActive( false );
	}

	public void UpdateDoTStatus( int duration, int stacksize ) {
		if ( !_dotStatus.activeSelf ) {
			_dotStatus.SetActive( true );
		}
		
		_dotStatusDurationText.text = duration.ToString();
		_dotStatusStackSizeText.text = stacksize.ToString();
	}

	public void HideStunStatus() {
		_stunStatus.SetActive( false );
	}

	public void UpdateStunStatus( int duration ) {
		if ( !_stunStatus.activeSelf ) {
			_stunStatus.SetActive( true );
		}

		_stunStatusDurationText.text = duration.ToString();
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

	public void ShowResults( Battle.SessionResults results ) {
		if ( results != null ) {
			GameResultsDialog dialog = UIManager.Instance.OpenDialog( GameResultsDialog.DIALOG_ID ) as GameResultsDialog;
			dialog.Initialize( results );
		} else {
			Debug.LogError( "results are null?" );
		}
	}

	public void ContinueButtonTapped() {
		GameManager.Instance.CompleteGame();
	}
}

