using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu]
public class BattleStageData : ScriptableObject {

	public enum AttackPattern {
		RandomSet,
		All
	}

	public int HPMin;
	public int HPMax;
	public int CooldownMin;
	public int CooldownMax;
	public int TurnsMin;
	public int TurnsMax;
	public Sprite EnemySprite;

	public AttackPattern Pattern;
	public List<EnemyAttackDataSet> AttackSets;
}

