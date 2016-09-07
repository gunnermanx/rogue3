using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu]
public class DoTWeaponSkillData : BaseWeaponSkillData {

	public override IEnumerator PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match, GameBoard.Swap swap ) {

		float rand = UnityEngine.Random.Range( 0f, 1f );
		if ( match.Count == 3 && rand <= ThreeTileActivationPercentage || 
			match.Count >= 4 && rand <= FourTileActivationPercentage ) {

			BaseTileData matchedTileData = match[ 0 ].TileData;
			gameBoard.ReplaceNRandomTiles( 3, matchedTileData, 
				delegate( Tile tile ) {
					return tile.TileType == matchedTileData.Type && !match.Contains( tile );
				}
			);
		}

		yield break;

	}	
}


