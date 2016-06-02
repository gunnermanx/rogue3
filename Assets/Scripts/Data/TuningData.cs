using UnityEngine;
using System.Collections;

public class TuningData : MonoBehaviour
{

	private static TuningData _instance = null;
	public static TuningData Instance {
		get { return _instance; }
	}

	private void Awake() {
		_instance = this;
	}

	public float SwapAnimationTime;

	public float TileDropSpeed;
	public iTween.EaseType TileDropEaseType;

	public float TileClearTime;
	public iTween.EaseType TileClearEaseType;

	public float AttackVFXTime;
	public iTween.EaseType AttackVFXEaseType;

	public float MatchTimeDelay;
}

