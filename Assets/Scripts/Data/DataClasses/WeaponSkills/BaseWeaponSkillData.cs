using System;
using UnityEngine;

public class BaseWeaponSkillData : ScriptableObject {
	public float ThreeTileActivationPercentage;
	public float FourTileActivationPercentage;

	public virtual void PerformWeaponSkill( GameBoard gameBoard, Battle battle, int matchSize, object contextData = null ) {
	}	
}

