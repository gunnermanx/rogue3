using System;
using UnityEngine;
using System.Collections.Generic;

public class DoTWeaponSkillData : BaseWeaponSkillData {
	public int DoTDuration;
	public int DoTStackSize;

	public override void PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match ) {

//		float rand = UnityEngine.Random.Range( 0f, 1f );
//		if ( match.Count == 3 && rand <= ThreeTileActivationPercentage || 
//			match.Count >= 4 && rand <= FourTileActivationPercentage ) {
//
//			gameBoard.ReplaceNRandomTiles( 3, match[ 0 ].TileData );
//
//			Debug.Log( "REPLACING" );
//		}
		//battle.ApplyDoT( DoTDuration, DoTStackSize );

		gameBoard.ReplaceNRandomTiles( 3, match[ 0 ].TileData, match );
	}	
}


