using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Battle : MonoBehaviour {

	public class StatusEffect {
		public int DurationRemaining = 0;
	}

	public class DoTStatus : StatusEffect {
		public int StackSize = 0;
	}

	public class StunStatus : StatusEffect {
	}

	public class Session {
		public int TurnsRemaining = -1;
		public int HPMax = -1;
		public int HPRemaining = -1;
		public int EnemyCooldown = -1;
		public int CurrentEnemyCooldown = -1;
		public List<EnemyAttackDataSet> AttackSets = null;
		public BattleStageData.AttackPattern AttackPattern;

		public DoTStatus DoTStatus = null;
		public StunStatus StunStatus = null;

		public SessionResults Results = null;
			
		public Session( BattleStageData stageData ) {
			TurnsRemaining = UnityEngine.Random.Range( stageData.TurnsMin, stageData.TurnsMax+1 );
			HPMax = UnityEngine.Random.Range( stageData.HPMin, stageData.HPMax+1 );
			HPRemaining = HPMax;
			EnemyCooldown = UnityEngine.Random.Range( stageData.CooldownMin, stageData.CooldownMax+1 );
			CurrentEnemyCooldown = EnemyCooldown;
			AttackSets = stageData.AttackSets;
			AttackPattern = stageData.Pattern;
		}
	}

	public class SessionResults {
		public bool IsVictory = false;
	}

	[SerializeField]
	private GameObject _enemyGameObject;

	private Session _session;

	private GameHud _gameHud;

	private GameBoard _gameBoard;

	public void Initialize( BattleStageData data, GameHud gameHud, GameBoard gameBoard ) {
		_session = new Session( data );
		_gameHud = gameHud;
		_gameBoard = gameBoard;

		_gameBoard.OnTilesMatched += HandleOnTilesMatched;
		_gameBoard.OnTurnEnded += HandleOnTurnEnded;

		_enemyGameObject.GetComponent<SpriteRenderer>().sprite = data.EnemySprite;

		UpdateHUD();
	}

	private void OnDestroy()
	{
		_gameBoard.OnTilesMatched -= HandleOnTilesMatched;
		_gameBoard.OnTurnEnded -= HandleOnTurnEnded;
	}

	public bool IsBattleComplete() {
		return ( _session.TurnsRemaining <= 0 || _session.HPRemaining <= 0 );
	}

	public SessionResults GetResults() {
		return _session.Results;
	}

	public List<EnemyAttackDataSet.EnemyAttackData> IncrementTurnAndGetEnemyAttack() {
		
		_session.TurnsRemaining--;

		if ( _session.DoTStatus != null ) {
			int damage = (int)Mathf.Max( 1, _session.HPMax * _session.DoTStatus.StackSize * 0.01f );
			_session.HPRemaining -= damage;
				
			if ( --_session.DoTStatus.DurationRemaining <= 0 ) {
				_session.DoTStatus = null;
			}
		}

		List<EnemyAttackDataSet.EnemyAttackData> attacks = CheckForAttack();

		if ( _session.HPRemaining <= 0 ) {
			_session.Results = new SessionResults() {
				IsVictory = true
			};
		}
		else if ( _session.TurnsRemaining <= 0 ) {
			_session.Results = new SessionResults() {
				IsVictory = false
			};
		}

		UpdateHUD();

		return attacks;
	}



	public void AttackEnemy( List<Tile> matches ) {
		//TODO: doing something simple here for now
		int totalDamage = 0;
		for ( int i = 0, count = matches.Count; i < count; i++ ) {

			Tile matchedTile = matches[ i ];
			totalDamage += matchedTile.GetDamage();

			Debug.Assert( matchedTile != null, "Tile in matches is null, something is very wrong!" );

			SpawnAttackVFX( matchedTile );
		}

		_session.HPRemaining -= totalDamage;



		UpdateHUD();
	}


	public void DealCritDamage( int damage ) {
		_session.HPRemaining -= damage;
	}

	public void ApplyDoT( int duration, int stackSize ) {
		if ( _session.DoTStatus != null ) {
			int currentDuration = _session.DoTStatus.DurationRemaining;
			_session.DoTStatus.DurationRemaining = Mathf.Max( currentDuration, duration );
			_session.DoTStatus.StackSize += stackSize;
		} else {
			_session.DoTStatus = new DoTStatus() { DurationRemaining = duration, StackSize = stackSize };
		}	

		_gameHud.UpdateDoTStatus( duration, stackSize );
	}

	public void ApplyStun( int duration ) {

		// cant restun
		if ( _session.StunStatus != null ) {
			return;
		}

		//TODO: create vfx?
		_session.StunStatus = new StunStatus() { DurationRemaining = duration };

		_gameHud.UpdateStunStatus( duration );
	}

	private void SpawnAttackVFX( Tile matchedTile ) {
	
		GameObject prefab =  matchedTile.GetAttackVFXPrefab();
		if ( prefab == null ) {
			Debug.Log ( "The prefab ref is null? " + matchedTile.ToString() );
			return;
		}

		GameObject vfxGO = GameObject.Instantiate( matchedTile.GetAttackVFXPrefab(), 
		                                          matchedTile.transform.position, 
		                                          Quaternion.identity ) as GameObject;

		Texture tex = matchedTile.GetAttackVFXTexture();
		if ( tex != null ) {
			ParticleSystemRenderer psr = vfxGO.GetComponent<ParticleSystemRenderer>();
			psr.material.mainTexture = tex; 
		}

		iTween.MoveTo( vfxGO, iTween.Hash( "position", _enemyGameObject.transform.position, 
								            "easetype", iTween.EaseType.easeOutQuart, 
								            "speed", 4f,
			                                  "oncomplete", "KillVFX"
								           )
		);
	}

	private List<EnemyAttackDataSet.EnemyAttackData> CheckForAttack() {	

		// TODO: delete vfx?
		if ( _session.StunStatus != null ) {
			if ( --_session.StunStatus.DurationRemaining <= 0 ) {
				_session.StunStatus = null;
			}
			return null;
		}

		// check for stuns/status blah here
		_session.CurrentEnemyCooldown--;

		if ( _session.CurrentEnemyCooldown == 0 ) {
			_session.CurrentEnemyCooldown = _session.EnemyCooldown;

			// create an attack
			return CreateAttack();
		}

		return null;
	}

	private List<EnemyAttackDataSet.EnemyAttackData> CreateAttack() {
		if ( _session.AttackPattern == BattleStageData.AttackPattern.RandomSet ) {
			int index = UnityEngine.Random.Range( 0, _session.AttackSets.Count );
			EnemyAttackDataSet set = _session.AttackSets[ index ];
			return set.AttackData;
		}
		return null;
	}
	
	private void UpdateHUD() {

		if ( _session.DoTStatus == null ) {
			_gameHud.HideDoTStatus();
		} else {
			_gameHud.UpdateDoTStatus( _session.DoTStatus.DurationRemaining, _session.DoTStatus.StackSize );
		}

		if ( _session.StunStatus == null ) {
			_gameHud.HideStunStatus();
		} else {
			_gameHud.UpdateStunStatus( _session.StunStatus.DurationRemaining );
		}

		_gameHud.UpdateHPBar( _session.HPRemaining, _session.HPMax );
		_gameHud.UpdateEnemyTurnCounter( _session.CurrentEnemyCooldown );
		_gameHud.UpdateTurnsRemaining( _session.TurnsRemaining );                              
	}

#region EventHandlers

	public void HandleOnTilesMatched( List<Tile> matches ) {
		// TODO 
		AttackEnemy( matches );
	}

	public void HandleOnTurnEnded() {
		List<EnemyAttackDataSet.EnemyAttackData> attacks = IncrementTurnAndGetEnemyAttack();

		bool gameComplete = IsBattleComplete();
		if ( gameComplete ) {

			_gameBoard.GameComplete();
			GameManager.Instance.GameComplete( _session.Results.IsVictory );

			// Show the results panel
			ShowResults();
		}

		// There is no attack from the enemy, notify the boardmanager to continue to input
		if ( attacks == null ) {
			_gameBoard.ContinueToInput();
		}
		// Otherwise we need to attack the board
		else {
			_gameBoard.ProcessEnemyAttack( attacks );
		}
	}
#endregion

	public void ShowResults() {
		if ( _session.Results != null ) {
			GameResultsDialog dialog = UIManager.Instance.OpenDialog( GameResultsDialog.DIALOG_ID ) as GameResultsDialog;
			dialog.Initialize( _session.Results, OnReturnFromResults );
		} else {
			Debug.LogError( "results are null?" );
		}
	}

	public void OnReturnFromResults() {
		//GameManager.Instance.GameComplete( _session.Results.IsVictory );
		GameManager.Instance.CleanupGame();
	}

#region Debug
	public void SetEnemyRemainingHP( int hp ) {
		_session.HPRemaining = hp;
		UpdateHUD();
	}
#endregion
}

