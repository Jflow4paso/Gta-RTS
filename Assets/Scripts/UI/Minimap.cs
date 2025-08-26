using UnityEngine.EventSystems;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
// Handles drag interaction on the minimap UI and converts pointer position
// back to world-space coordinates on the Terrain, then emits an event.
public class Minimap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public RectTransform minimapContainerRectTransform;
	public Vector2 terrainSize;
	private Vector2 _uiSize;

	private Vector2 _offset;
	private Vector2 _lastPointerPosition;
	private bool _dragging = false;

	private void Start()
	{
		// Cache UI and initial offset for coordinate conversions
		_offset = minimapContainerRectTransform.anchoredPosition;
		_uiSize = GetComponent<RectTransform>().sizeDelta;
		_lastPointerPosition = Input.mousePosition;
	}

	private void Update()
	{
		if (!_dragging) return;

		Vector2 delta = (Vector2) Input.mousePosition - _lastPointerPosition;
		_lastPointerPosition = Input.mousePosition;

		if (delta.magnitude > Mathf.Epsilon)
		{
			// Convert screen-space pointer into UI-space, then into world-space
			Vector2 uiPos =
				(new Vector2(Input.mousePosition.x, Input.mousePosition.y) /
				GameManager.instance.canvasScaleFactor) - _offset;
			Vector3 realPos = new Vector3(
				uiPos.x / _uiSize.x * terrainSize.x,
				0f,
				uiPos.y / _uiSize.y * terrainSize.y
			);
			// Raycast down to snap onto the terrain surface
			realPos = Utils.ProjectOnTerrain(realPos);
			EventManager.TriggerEvent("ClickedMinimap", realPos);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_dragging = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_dragging = false;
	}
}
