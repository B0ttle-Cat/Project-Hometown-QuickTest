using UnityEngine;

namespace GameUI
{
	[RequireComponent(typeof(RectTransform))]
	public class LabelPositionItem : MonoBehaviour
	{
		[Header("Settings")]
		public int Priority = 0;
		// x: Left, y: Right, z: Top, w: Bottom 추가 여백
		public Vector4 MarginOffset = new Vector4(5f, 5f, 5f, 5f);
		public float SmoothSpeed = 15f;

		public RectTransform RectTransform { get; private set; }
		private LabelOverlapController _controller;

		public Vector2 OriginalScreenPos { get; private set; }
		public Vector2 CurrentOffset { get; set; }
		private Vector2 _appliedOffset;

		private void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
			_controller = GetComponentInParent<LabelOverlapController>();
		}

		private void OnEnable()
		{
			if (_controller != null) _controller.AddItem(this);
		}

		private void OnDisable()
		{
			if (_controller != null) _controller.RemoveItem(this);
		}

		private void OnDestroy()
		{
			if (_controller != null) _controller.RemoveItem(this);
		}

		public void SetOriginalPosition(Vector2 pos)
		{
			OriginalScreenPos = pos;
			CurrentOffset = Vector2.zero;
		}

		public void ApplySmoothPosition()
		{
			if (!gameObject.activeInHierarchy) return;

			_appliedOffset = Vector2.Lerp(_appliedOffset, CurrentOffset, Time.deltaTime * SmoothSpeed);
			RectTransform.anchoredPosition = OriginalScreenPos + _appliedOffset;
		}
	}
}