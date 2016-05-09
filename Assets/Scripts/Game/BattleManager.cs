using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour {

	public class Session {
		public int TurnsRemaining = -1;
		public int HPMax = -1;
		public int HPRemaining = -1;
		public int EnemyCooldown = -1;
		public int CurrentEnemyCooldown = -1;
		public List<EnemyAttackDataSet> AttackSets = null;
		public BattleStageData.AttackPattern AttackPattern;
			
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

	public class EnemyAttack {

	}

	private Session _session;

	public void Initialize( BattleStageData data ) {
		_session = new Session( data );

		UpdateHUD();
	}

	public bool IsBattleComplete() {
		return ( _session.TurnsRemaining <= 0 || _session.HPRemaining <= 0 );
	}

	public List<EnemyAttackDataSet.EnemyAttackData> IncrementTurnAndGetEnemyAttack() {
		_session.TurnsRemaining--;

		List<EnemyAttackDataSet.EnemyAttackData> attacks = CheckForAttack();

		UpdateHUD();

		return attacks;
	}

	private List<EnemyAttackDataSet.EnemyAttackData> CheckForAttack() {	
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
		GameHud gameHud = GameManager.Instance.GetGameHUD();
		gameHud.UpdateHPBar( _session.HPRemaining, _session.HPMax );
		gameHud.UpdateEnemyTurnCounter( _session.CurrentEnemyCooldown );
		gameHud.UpdateTurnsRemaining( _session.TurnsRemaining );                              
	}
}

