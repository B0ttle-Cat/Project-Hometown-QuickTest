using UnityEngine;

namespace GameUI
{
	[RequireComponent(typeof(RectTransform))]
	public class LabelPositionItem : MonoBehaviour
	{
		[Header("Settings")]
		public int Priority = 0;
		public Rigidbody2D labelRigidbody2D;
		public CapsuleCollider2D labelCapsuleCollider2D;
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
			labelRigidbody2D = GetComponent<Rigidbody2D>();
			labelCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
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
			var size = RectTransform.rect.size;
			size = RectTransform.rect.size;
			Vector2 pivot =  RectTransform.pivot;
			Vector2 offset = size * 0.5f;
			offset -= size * pivot;
			labelCapsuleCollider2D.offset = offset;
			labelCapsuleCollider2D.size = size;
		}

		public void ApplySmoothPosition()
		{
			if (!gameObject.activeInHierarchy) return;

			_appliedOffset = Vector2.Lerp(_appliedOffset, CurrentOffset, Time.deltaTime * SmoothSpeed);
			RectTransform.anchoredPosition = OriginalScreenPos + _appliedOffset;
		}
	}
}
