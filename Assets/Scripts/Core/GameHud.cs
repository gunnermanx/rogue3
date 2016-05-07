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


	public void UpdateHUD() {
		GameManager.GameSession session = GameManager.Instance.GetCurrentGameSession();

		_turnLimitText.text = session.TurnsRemaining.ToString();
	}
}

