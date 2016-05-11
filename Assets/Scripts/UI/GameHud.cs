using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameHud : MonoBehaviour {

	[SerializeField]
	private Text _turnLimitText;
	[SerializeField]
	private Text _enemyTurnCounterText;
	[SerializeField]
	private Slider _hpSlider;
	[SerializeField]
	private GameResults _resultsPanel;


	private static GameHud _instance = null;
	public static GameHud Instance {
		get { return _instance; }
	}

	private void Awake() {
		_instance = this;
		GameManager.Instance.RegisterGameHud( this );
	}

	private void OnDestroy() {
		GameManager.Instance.UnregisterGameHud();
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

