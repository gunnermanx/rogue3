using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu]
public class StunWeaponSkillData : BaseWeaponSkillData {
	public int TurnsStunned;

	public override IEnumerator PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match, GameBoard.Swap swap ) {
		
		float rand = UnityEngine.Random.Range( 0f, 1f );
		if ( match.Count == 3 && rand <= ThreeTileActivationPercentage || 
			match.Count >= 4 && rand <= FourTileActivationPercentage ) {

			battle.ApplyStun( TurnsStunned );

			int numReplaced = UnityEngine.Random.Range( 3, 5 );
			gameBoard.ClearNRandomTiles( numReplaced, delegate( Tile tile ) {
				return !match.Contains( tile );
			});
		}

		yield break;
	}	
}


