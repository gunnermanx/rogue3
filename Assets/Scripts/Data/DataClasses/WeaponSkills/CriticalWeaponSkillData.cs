using System;
using UnityEngine;
using System.Collections.Generic;

public class CriticalWeaponSkillData : BaseWeaponSkillData {
	public float CriticalMultiplier;

	public override void PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match ) {
		
		float rand = UnityEngine.Random.Range( 0f, 1f );
		if ( match.Count == 3 && rand <= ThreeTileActivationPercentage || 
			match.Count >= 4 && rand <= FourTileActivationPercentage ) {

			gameBoard.ExtendMatch( match );
		}
	}
}

