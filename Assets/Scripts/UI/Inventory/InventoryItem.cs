using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemData {
	public string DataId;
	public int Count;
}

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	protected Image _image;

	public static InventoryItem DraggedInventoryItem = null;

	private Vector3 _originalPosition;
	private InventorySlot _originalParentInventorySlot;

	public InventorySlot ParentInventorySlot { get; set; }
	public InventoryItemData InventoryItemData { get; set; }

	public virtual void OnBeginDrag( PointerEventData eventData ) {
		DraggedInventoryItem = this;
		_originalPosition = transform.position;
		_originalParentInventorySlot = ParentInventorySlot;
		_canvasGroup.blocksRaycasts = false;
	}

	public void OnDrag( PointerEventData eventData ) {
		transform.position = eventData.position;
	}

	public virtual void OnEndDrag( PointerEventData eventData ) {
		DraggedInventoryItem = null;
		_canvasGroup.blocksRaycasts = true;
		if ( ParentInventorySlot == _originalParentInventorySlot ) {
			transform.position = _originalPosition;
		}
	}

	public void Initialize( InventoryItemData data ) {
		// TODO fix this to be generic
		InventoryItemData = data;
		WeaponTileData weaponTileData = Database.Instance.GetWeaponTileData( data.DataId );
		_image.sprite = weaponTileData.Sprite;
	}

	public void RemoveFromParentInventorySlot() {
		ParentInventorySlot.ClearSlot();
	}
}
