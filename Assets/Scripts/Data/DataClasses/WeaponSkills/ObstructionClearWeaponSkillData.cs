using System;
using UnityEngine;

public class ObstructionClearWeaponSkillData : BaseWeaponSkillData {
	public BaseTileData.TileType[] TypesCleared;

	public override void PerformWeaponSkill( GameBoard gameBoard, Battle battle, int matchSize, object contextData = null ) {

		if ( !gameBoard.IsObstructionsOnGameBoard() ) {
			return;
		}

		float rand = UnityEngine.Random.Range( 0f, 1f );
		if ( matchSize == 3 && rand <= ThreeTileActivationPercentage || 
			 matchSize >= 4 && rand <= FourTileActivationPercentage ) {

			// tell the gameboard to clear an obs
			gameBoard.DestroyRandomObstruction();
		}
	}	
}

