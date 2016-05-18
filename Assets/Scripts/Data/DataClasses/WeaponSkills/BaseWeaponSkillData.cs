using System;
using UnityEngine;
using System.Collections.Generic;

public class BaseWeaponSkillData : ScriptableObject {
	public float ThreeTileActivationPercentage;
	public float FourTileActivationPercentage;

	public virtual void PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match ) {
	}	
}

