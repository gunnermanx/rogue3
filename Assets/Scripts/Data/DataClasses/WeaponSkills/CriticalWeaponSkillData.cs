using System;
using UnityEngine;
using System.Collections.Generic;

public class CriticalWeaponSkillData : BaseWeaponSkillData {
	public float CriticalMultiplier;

	public override void PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match ) {
		// todo: lazy simple thing for now

		int totalDamage = 0;
		for ( int i = 0, count = match.Count; i < count; i++ ) {
			Tile matchedTile = match[ i ];
			totalDamage += matchedTile.GetDamage();
		}

		battle.DealCritDamage( (int)( totalDamage * CriticalMultiplier ) );
	}
}

