using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Target Graphic Group", 12)]
public class TargetGraphicGroup : MaskableGraphic
{
	[Serializable]
	public class GraphicEntry
	{
		[HorizontalGroup]
		public Graphic graphic;
		[HorizontalGroup]
		public bool enableTransition = true;
	}

	[SerializeField]
	private List<GraphicEntry> graphics = new List<GraphicEntry>();

    protected override void Reset()
    {
		AutoCollectChildren();
	}
    protected override void Awake()
	{
		base.Awake();
		raycastTarget = false; // 실제 렌더링하지 않음
		maskable = false;
	}

	[Button("Auto Collect Children")]
	private void AutoCollectChildren()
	{
		if (TryGetComponent<Selectable>(out var selectable))
		{
			selectable.targetGraphic = this;
		}

		graphics.Clear();

		foreach (var g in GetComponentsInChildren<Graphic>(true))
		{
			if (g == this) continue;
			graphics.Add(new GraphicEntry { graphic = g, enableTransition = true });
		}
		UnityEditor.EditorUtility.SetDirty(this);
	}
	public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
	{
		foreach (var entry in graphics)
		{
			if (entry.enableTransition && entry.graphic != null)
				entry.graphic.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
		}
	}
	public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
	{
		foreach (var entry in graphics)
		{
			if (entry.enableTransition && entry.graphic != null)
				entry.graphic.CrossFadeAlpha(alpha, duration, ignoreTimeScale);
		}
	}

	public override void SetMaterialDirty() { }
	public override void SetVerticesDirty() { }
}
