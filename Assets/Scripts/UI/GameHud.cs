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


	private static GameHud _instance = null;
	public static GameHud Instance {
		get { return _instance; }
	}

	private void Awake() {
		_instance = this;
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
}

