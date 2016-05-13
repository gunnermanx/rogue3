using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseDialog : MonoBehaviour, IPointerClickHandler {

	[SerializeField]
	protected GameObject _touchToExitObject; 

	public abstract string GetDialogId();

	#region IPointerClickHandler implementation
	public void OnPointerClick( PointerEventData eventData ) {
		GameObject go = eventData.pointerCurrentRaycast.gameObject;
		if ( _touchToExitObject != null && _touchToExitObject == go ) {
			UIManager.Instance.CloseDialog( GetDialogId() );
		}
	}
	#endregion
}

