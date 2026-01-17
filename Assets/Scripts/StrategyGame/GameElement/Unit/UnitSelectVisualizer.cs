using Shapes;

using UnityEngine;

public class UnitSelectVisualizer : SelectVisualizer
{
	private UnitObject unit;

	private Vector3 targetPosition;
	private Vector3 targetUpDir;

	private float currentRadius;
	private float currentThickness;
	private Color currentColor;
	private float currentBlinkAlpha;

	[SerializeField]
	private Vector3 targetPositionOffset;
	[SerializeField]
	private float targetThickness;
	[SerializeField]
	private float pointRadiusOffset;
	[SerializeField]
	private float selectRadiusOffset;

	public override void OnInit(ISelectable target)
	{
		if (target is not UnitObject unit) return;
		this.unit = unit;
		base.OnInit(target);

		currentRadius = default;
		currentThickness = default;
		currentColor = default;
		currentBlinkAlpha = default;
	}


	public override void Deinit()
	{
		unit = null;
		base.Deinit();

		currentRadius = default;
		currentThickness = default;
		currentColor = default;
	}
	protected override void EnterShowState()
	{
	}

	protected override void OnComputeShapes()
	{
		targetPosition = unit.ThisMovement.CurrentPosition + targetPositionOffset;
		targetUpDir = unit.transform.up;
		currentRadius = unit.ThisMovement.CurrentRadius;
		currentThickness = targetThickness;

		if (visualizerState == VisualizerState.None)
		{
			currentColor = Color.clear;
		}
		else
		{
			currentThickness = targetThickness;
			currentColor = unit.Faction.FactionColor;
		}
	}

	protected override async Awaitable OnBlinkShapes()
	{
		float pi2 = 2f * Mathf.PI;
		int blinkCount = 2;
		float blinkTime = .5f;
		float currBlinkTime = 0f;
		if (blinkTime < 0f || blinkCount == 0) return;
		currentBlinkAlpha = 0;
		while (currBlinkTime < blinkTime)
		{
			currBlinkTime += Time.deltaTime;
			float blickRatio = currBlinkTime / blinkTime;

			float cos = -Mathf.Cos(blickRatio * pi2 * blinkCount);
			currentBlinkAlpha = (cos + 1) * 0.5f;
			await Awaitable.NextFrameAsync();
		}
		currentBlinkAlpha = 0;
	}
	protected override void OnDrawShapes(Camera cam)
	{
		using (Draw.Command(StrategyManager.MainCamera))
		{
			DiscColors discColors = DiscColors.Flat(currentColor);
			Draw.ThicknessSpace = ThicknessSpace.Pixels;
			if (visualizerState.HasFlag(VisualizerState.Select))
			{
				Draw.Ring(targetPosition, targetUpDir, currentRadius + selectRadiusOffset, currentThickness, discColors);
			}
			if (visualizerState.HasFlag(VisualizerState.PointUp))
			{
				DashStyle dashStyle = new DashStyle()
				{
					type = DashType.Basic,
					space = DashSpace.FixedCount,
					snap = DashSnapping.Tiling,
					size = 10,
					offset = Time.time * 2f,
					spacing = 0.5f,
					shapeModifier = 0
				};
				Draw.UseDashes = true;
				Draw.DashStyle = dashStyle;
				Draw.Ring(targetPosition, targetUpDir, currentRadius + pointRadiusOffset, currentThickness, discColors);
				Draw.UseDashes = false;
			}
			if (visualizerState.HasFlag(VisualizerState.Blink))
			{
				DashStyle dashStyle = DashStyle.defaultDashStyleRing;
				dashStyle.offset = Time.time * 2f;
				Draw.DashStyle = dashStyle;

				Color blickColor = currentColor;
				blickColor.a = currentBlinkAlpha;
				Draw.Disc(targetPosition, targetUpDir, currentRadius + selectRadiusOffset, DiscColors.Flat(blickColor));
			}
		}
	}
}
