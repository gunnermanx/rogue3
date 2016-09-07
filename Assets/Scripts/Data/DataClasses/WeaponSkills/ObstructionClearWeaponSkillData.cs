using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu]
public class ObstructionClearWeaponSkillData : BaseWeaponSkillData {
	public BaseTileData.TileType[] TypesCleared;

	public override IEnumerator PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match, GameBoard.Swap swap ) {



		float rand = UnityEngine.Random.Range( 0f, 1f );
		if ( match.Count == 3 && rand <= ThreeTileActivationPercentage || 
			 match.Count >= 4 && rand <= FourTileActivationPercentage ) {

			// tell the gameboard to clear an obs
			if ( gameBoard.IsObstructionsOnGameBoard() ) {
				gameBoard.DestroyRandomObstruction();
			}



			gameBoard.ClearAdjacentTiles( swap.SelectedTile, 
				delegate(Tile tile) {
					return !match.Contains( tile );
				} 
			);
		}

		//yield return new WaitForSeconds( 3.0f );

		yield break;
	}	
}

