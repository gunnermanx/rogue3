using System;
using System.Collections.Generic;
using UnityEngine;

public partial class GameBoard {

#region simple finite state machine class
	private class GameStateMachine {

		// Game states
		public enum State {
			NoState = 0,
			Input, // User input state, may also show hints during this state in the future
			Swap, // User input has occured, checking for a valid swap
			Match, // State to clear out all matches and wait for match tweens
			MatchSkill,
			ExpireObstructions,
			DropAndFill, // Fix data so that we have a complete board once more
			Cleanup, // Check for additional matches, either brings us back to Match or to the TurnEnd
			TurnEnded,
			RecipeSkill
		}

		public enum Transition {
			NoTransition = 0,
			TileDropped,
			InvalidTileDrop,
			ValidTileDrop,
			ActivateMatchSkill,
			CreatedAdditionalMatches,
			CheckObstructions,
			ProcessedAllMatches,
			NewTilesDropped,
			CascadingMatchesFound,
			EndTurn,
			ProceedToInput,
			ActivateRecipeSkill
		}

		private Dictionary<State, BaseGameState> _gameStates = new Dictionary<State, BaseGameState>();

		private BaseGameState _currentState = null;

		private GameBoard _gameBoard = null;

		private void SetupGameStates() {
			_gameStates.Add( State.Input, new InputState( this, _gameBoard) );
			_gameStates.Add( State.Swap, new SwapState( this, _gameBoard) );
			_gameStates.Add( State.Match, new MatchState( this, _gameBoard ) );
			_gameStates.Add( State.ExpireObstructions, new CheckObstructionsState( this, _gameBoard ) );
			_gameStates.Add( State.DropAndFill, new DropAndFillState( this, _gameBoard ) );
			_gameStates.Add( State.Cleanup, new CleanupState( this, _gameBoard ) );
			_gameStates.Add( State.TurnEnded, new TurnEndedState( this, _gameBoard ) );
			_gameStates.Add( State.RecipeSkill, new RecipeSkillState( this, _gameBoard ) );
		}

		public void Initialize( GameBoard gameboard ) {
			_gameBoard = gameboard;
			SetupGameStates();
			_currentState = _gameStates[ State.Input ];
		}

		public void TriggerTransition( Transition transition ) {

			State nextStateId = _currentState.GetNextState( transition );
			if ( nextStateId != State.NoState ) {
				BaseGameState nextState = null;
				if ( _gameStates.TryGetValue( nextStateId, out nextState ) ) {
					_currentState.OnExitState();
					_currentState = nextState;
					_currentState.OnEnterState();
				} else {
					Debug.LogError( "Trying to transition to an uncreated state: " + nextStateId.ToString() );
				}
			}
		}

		public bool CheckCurrentState( State stateId ) {
			BaseGameState state = null;
			_gameStates.TryGetValue( stateId, out state );
			return _currentState == state;
		}
	}
#endregion


#region States

	private class InputState : BaseGameState {

		public InputState( GameStateMachine stateManager, GameBoard gameboard ) : base( stateManager, gameboard ) {}

		protected override void SetupTransitions () {
			_transitions.Add( GameStateMachine.Transition.TileDropped, GameStateMachine.State.Swap );
			_transitions.Add( GameStateMachine.Transition.ActivateRecipeSkill, GameStateMachine.State.RecipeSkill );
		}

		public override void OnEnterState() {}

		public override void OnExitState() {}
	}


	private class SwapState : BaseGameState {

		public SwapState( GameStateMachine stateManager, GameBoard gameboard ) : base( stateManager, gameboard ) {}

		protected override void SetupTransitions () {
			_transitions.Add( GameStateMachine.Transition.InvalidTileDrop, GameStateMachine.State.Input );
			_transitions.Add( GameStateMachine.Transition.ValidTileDrop, GameStateMachine.State.Match );
		}

		public override void OnEnterState() {
			_gameBoard.PerformSwap();

			Swap swap = _gameBoard._swap;
			List<List<Tile>> matches = _gameBoard._matches;

			bool selectedTileMatched = _gameBoard.CheckForMatchAtCoords( swap.SelectedTile, swap.SelectedTile.X, swap.SelectedTile.Y, out swap.SelectedHorizontalMatches, out swap.SelectedVerticalMatches );
			bool targetTileMatched = _gameBoard.CheckForMatchAtCoords( swap.TargetTile, swap.TargetTile.X, swap.TargetTile.Y, out swap.TargetHorizontalMatches, out swap.TargetVerticalMatches );

			// If there are matches at either the target or selected tile locations, we need to swap
			if ( selectedTileMatched || targetTileMatched ) {

				matches.Clear();

				if ( swap.SelectedHorizontalMatches.Count > 0 ) matches.Add( swap.SelectedHorizontalMatches );
				if ( swap.SelectedVerticalMatches.Count > 0 ) matches.Add( swap.SelectedVerticalMatches );
				if ( swap.TargetHorizontalMatches.Count > 0 ) matches.Add( swap.TargetHorizontalMatches );
				if ( swap.TargetVerticalMatches.Count > 0 ) matches.Add( swap.TargetVerticalMatches );

				_gameBoard.PerformSwapAnimation();
			}
			// Otherwise undo the swap
			else {
				_gameBoard.PerformUndoSwap();
				_gameBoard.PerformUndoSwapAnimation();
			}
		}

		public override void OnExitState() {}
	}


	private class MatchState : BaseGameState {

		public MatchState( GameStateMachine stateManager, GameBoard gameboard ) : base( stateManager, gameboard ) {}

		protected override void SetupTransitions () {
			_transitions.Add( GameStateMachine.Transition.CheckObstructions, GameStateMachine.State.ExpireObstructions );
		}

		public override void OnEnterState() {
			_gameBoard.PerformMatch();
		}

		public override void OnExitState() {}
	}


	private class CheckObstructionsState : BaseGameState {

		public CheckObstructionsState( GameStateMachine stateManager, GameBoard gameboard ) : base( stateManager, gameboard ) {}

		protected override void SetupTransitions () {
			_transitions.Add( GameStateMachine.Transition.ProcessedAllMatches, GameStateMachine.State.DropAndFill );
		}

		public override void OnEnterState() {
			_gameBoard.PerformObstructionExpiration();
		}

		public override void OnExitState() {}
	}


	private class DropAndFillState : BaseGameState {

		public DropAndFillState( GameStateMachine stateManager, GameBoard gameboard ) : base( stateManager, gameboard ) {}

		protected override void SetupTransitions () {
			_transitions.Add( GameStateMachine.Transition.NewTilesDropped, GameStateMachine.State.Cleanup );
		}

		public override void OnEnterState() {
			_gameBoard.PerformDrop();
			_gameBoard.PerformFill();
			_gameBoard.PerformDroppingAnimation();
		}

		public override void OnExitState() {}
	}


	private class CleanupState : BaseGameState {

		public CleanupState( GameStateMachine stateManager, GameBoard gameboard ) : base( stateManager, gameboard ) {}

		protected override void SetupTransitions () {
			_transitions.Add( GameStateMachine.Transition.CascadingMatchesFound, GameStateMachine.State.Match );
			_transitions.Add( GameStateMachine.Transition.EndTurn, GameStateMachine.State.TurnEnded );
		}

		public override void OnEnterState() {
			_gameBoard.PerformCleanup();
		}

		public override void OnExitState() {}
	}


	private class TurnEndedState : BaseGameState {

		public TurnEndedState( GameStateMachine stateManager, GameBoard gameboard ) : base( stateManager, gameboard ) {}

		protected override void SetupTransitions () {
			_transitions.Add( GameStateMachine.Transition.ProceedToInput, GameStateMachine.State.Input );
		}

		public override void OnEnterState() {
			_gameBoard.RaiseOnTurnEnded();
		}

		public override void OnExitState() {}
	}


	private class RecipeSkillState : BaseGameState {

		public RecipeSkillState( GameStateMachine stateManager, GameBoard gameboard ) : base( stateManager, gameboard ) {}

		protected override void SetupTransitions () {
			_transitions.Add( GameStateMachine.Transition.CreatedAdditionalMatches, GameStateMachine.State.Match );
		}

		public override void OnEnterState() {
			_gameBoard.ActivateRecipeSkill();
		}

		public override void OnExitState() {}
	}


	private abstract class BaseGameState {
		protected Dictionary<GameStateMachine.Transition, GameStateMachine.State> _transitions = new Dictionary<GameStateMachine.Transition, GameStateMachine.State>();
		protected GameBoard _gameBoard = null;
		protected GameStateMachine _gameStateManager = null;

		protected abstract void SetupTransitions();
		public abstract void OnEnterState();
		public abstract void OnExitState();

		public GameStateMachine.State GetNextState( GameStateMachine.Transition transition ) {
			GameStateMachine.State nextState = GameStateMachine.State.NoState;
			_transitions.TryGetValue( transition, out nextState );
			return nextState;
		}
			
		public BaseGameState( GameStateMachine stateManager, GameBoard gameboard ) {
			_gameStateManager = stateManager;
			_gameBoard = gameboard;
			SetupTransitions();
		}
	}

#endregion
}

