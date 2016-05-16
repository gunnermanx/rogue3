using System;
using UnityEngine;

public class BaseWeaponSkillData : ScriptableObject {
	public float ActivationPercentage;

	public virtual void PerformWeaponSkill( BoardManager boardManager, BattleManager battleManager ) {
	}	
}

