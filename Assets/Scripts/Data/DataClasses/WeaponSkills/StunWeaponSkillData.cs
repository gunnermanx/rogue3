using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class StunWeaponSkillData : BaseWeaponSkillData {
	public int TurnsStunned;

	public override void PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match ) {
		battle.ApplyStun( TurnsStunned );
	}	
}


