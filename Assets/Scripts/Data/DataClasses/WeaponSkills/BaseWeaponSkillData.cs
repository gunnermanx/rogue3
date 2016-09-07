using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BaseWeaponSkillData : ScriptableObject {
	public float ThreeTileActivationPercentage;
	public float FourTileActivationPercentage;

//	public class WeaponSkillActivation {
//	}

	public virtual IEnumerator PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match, GameBoard.Swap swap ) {
		yield break;
	}

	//public virtual void 
}

