using System;
using UnityEngine;
using System.Collections.Generic;

public class TileRecipeManager : MonoBehaviour {

	[SerializeField]
	private List<TileRecipe> _recipes;

	public TileRecipe GetRecipe( List<WeaponTileData> weaponData ) {

		TileRecipe recipe = null;

		// IF we are going with the assumption that order matters for the recipe,
		// then we just need to simply iterate the list
		for ( int i = 0, count = _recipes.Count; i < count; i++ ) {
			if ( MatchesRecipe( weaponData, _recipes[ i ] ) ) {
				recipe = _recipes[ i ];
			}	
		}
	
		return recipe;
	}
		
	private bool MatchesRecipe( List<WeaponTileData> chosenWeapons, TileRecipe recipe ) {
		if ( recipe.Tile1 == chosenWeapons[ 0 ].Type &&
			recipe.Tile2 == chosenWeapons[ 1 ].Type &&
			recipe.Tile3 == chosenWeapons[ 2 ].Type &&
			recipe.Tile4 == chosenWeapons[ 3 ].Type ) 
		{
			return true;
		}
		return false;
	}

#region Recipe Skills Functionality

	public void TravellerRecipe( GameBoard board ) {
	}

#endregion
}

