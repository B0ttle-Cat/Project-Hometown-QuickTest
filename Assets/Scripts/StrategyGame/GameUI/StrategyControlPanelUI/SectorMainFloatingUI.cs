using System;

using Sirenix.OdinInspector;

using UnityEngine;

public class SectorMainFloatingUI : MonoBehaviour, SimpleFloatingUI
{
	private SimpleFloatingUI thisFloating;
	[SerializeField, InlineProperty, HideLabel, FoldoutGroup("SectorMainFloating")]
	private SectorMainFloating sectorMainFloating;
	public SimpleFloatingUI ThisFloating => thisFloating ??= this;
	SimpleFloatingUI.Positioning SimpleFloatingUI.ThisPsitioning { get => sectorMainFloating; set => sectorMainFloating = value as SectorMainFloating; }
	[Serializable]
	public class SectorMainFloating : SimpleFloatingUI.Positioning
	{
		public SimpleFloatingUI ThisUI { get; set; }
		public RectTransform TargetRect { get; set; }

		private Camera camera;
		[SerializeField]
		private Transform anchor;
		[SerializeField]
		private Vector3 offset;
		public void OnInit()
		{
			camera = Camera.main;
		}
		public void SetAnchorPosition(Transform anchor)
		{
			this.anchor = anchor;
		}
		public void UpdatePosition()
		{
			if (camera == null || anchor == null)
			{
				return;
			}

			var screenPoint = camera.WorldToScreenPoint(anchor.position);
			TargetRect.position = screenPoint + offset;
		}
		public void OnDispose()
		{
			camera = null; 
			anchor = null;
		}
	}

	private Transform floatingAnchor;
	public Transform Anchor => floatingAnchor;

	public void Awake()
	{
		ThisFloating.NewPsitioning(sectorMainFloating);
	}
	public void Update()
	{
		ThisFloating.UpdatePosition();
	}
	public void OnDestroy()
	{
		ThisFloating.Dispose();
		thisFloating = null;
		sectorMainFloating = null;
		if (floatingAnchor != null)
		{
			Destroy(floatingAnchor.gameObject);
			floatingAnchor = null;
		}
	}
	public void SetAnchor(Transform anchor)
	{

		if (sectorMainFloating != null)
		{
			sectorMainFloating.SetAnchorPosition(anchor);
			if (floatingAnchor != null)
			{
				Destroy(floatingAnchor.gameObject);
				floatingAnchor = null;
			}
			floatingAnchor = anchor;
		}
	}
}
