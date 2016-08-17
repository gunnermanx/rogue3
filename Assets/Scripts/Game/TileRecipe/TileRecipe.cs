using System;
using UnityEngine;
using UnityEngine.Events;

// Given a tile recipe, it should contain data like... 
// - how many matches it takes to charge
// - a function that modifies the state of the gameboard
// - name?
[Serializable]
public class TileRecipe {

	public string Name;

	public BaseTileData.TileType Tile1;
	public BaseTileData.TileType Tile2;
	public BaseTileData.TileType Tile3;
	public BaseTileData.TileType Tile4;

	public int RequiredMatches;

	[SerializeField]
	private TileRecipeEvent RecipeFunction;

	public void ActivateRecipeSkill( GameBoard board ) {
		RecipeFunction.Invoke( board );
	}

}

[Serializable]
public class TileRecipeEvent : UnityEvent<GameBoard> {}
