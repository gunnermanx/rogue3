using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

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
	[SerializeField]
	private Slider _recipeSkillChargeSlider;
	[SerializeField]
	private Button _recipeSkillButton;

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

	private int _recipeMatches = 0;
	private int _requiredMatches = 0;
	private Action _recipeChargeButtonCallback = null;
	public void SetupRecipeChargeHud( GameBoard board, TileRecipe recipe, Action callback ) {
		if ( recipe == null ) {
			_recipeSkillButton.gameObject.SetActive( false );
			_recipeSkillChargeSlider.gameObject.SetActive( false );
		} else {
			_recipeMatches = 0;
			_requiredMatches = recipe.RequiredMatches;
			_recipeSkillChargeSlider.value = 0f;
			_recipeSkillButton.interactable = false;
			_recipeChargeButtonCallback = callback;

			board.OnTilesMatched += delegate( List<Tile> matches ) {
				_recipeMatches = Mathf.Min( _recipeMatches + 1, _requiredMatches );
				_recipeSkillChargeSlider.value = (float) _recipeMatches / (float) _requiredMatches;
				_recipeSkillButton.interactable = ( _recipeMatches == _requiredMatches );
			};
		}	
	}

	public void RecipeSkillButtonPressed() {
		if ( _recipeChargeButtonCallback != null ) {

			_recipeMatches = 0;
			_recipeSkillChargeSlider.value = 0;
			_recipeSkillButton.interactable = false;

			_recipeChargeButtonCallback();
		}
	}
}

