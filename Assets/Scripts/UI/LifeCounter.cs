using System;
using UnityEngine;

public class LifeCounter : MonoBehaviour {

	[SerializeField]
	private GameObject _filledState;

	public void ToggleFilledState( bool toggle ) {
		_filledState.SetActive( toggle );
	}
}

