using System;
using UnityEngine;
using System.Collections.Generic;

public class ObstructionClearWeaponSkillData : BaseWeaponSkillData {
	public BaseTileData.TileType[] TypesCleared;

	public override void PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match ) {

		if ( !gameBoard.IsObstructionsOnGameBoard() ) {
			return;
		}

		float rand = UnityEngine.Random.Range( 0f, 1f );
		if ( match.Count == 3 && rand <= ThreeTileActivationPercentage || 
			 match.Count >= 4 && rand <= FourTileActivationPercentage ) {

			// tell the gameboard to clear an obs
			gameBoard.DestroyRandomObstruction();
		}
	}	
}

