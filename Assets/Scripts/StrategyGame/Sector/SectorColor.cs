using UnityEngine;

public class SectorColor : MonoBehaviour
{
	private Color targetColor;
	private Gradient targetGradient;
	private float targetProgress = 0;

	public bool changeColor = false;
	private Color currentColor;
	private Renderer thisRenderer;
	private MaterialPropertyBlock thisMaterial;


	private void Awake()
	{
		changeColor = true;
		currentColor = Color.white;
		thisRenderer = null;
		thisMaterial = null;
		InitBlock();
	}
	private void InitBlock()
	{
		if(thisRenderer != null && thisMaterial != null)
		{
			thisRenderer.GetPropertyBlock(thisMaterial);
			return;
		}

		if(thisRenderer == null) thisRenderer = GetComponentInChildren<Renderer>();
		if(thisMaterial == null) thisMaterial = new MaterialPropertyBlock();

		thisRenderer.GetPropertyBlock(thisMaterial);
	}

	internal void UpdateColor(Faction faction, float progress)
	{
		var nextColor = faction == null ? Color.white : faction.FactionColor;
		bool changeTarget = targetColor != nextColor;
		if (targetColor == null || changeTarget)
		{
			targetColor = nextColor;
			targetGradient = new Gradient()
			{
				colorKeys = new GradientColorKey[]
			{
				new GradientColorKey(Color.white, 0f),
				new GradientColorKey(targetColor, 1f)
			},
				alphaKeys = new GradientAlphaKey[]
			{
				new GradientAlphaKey(1f, 0f),
				new GradientAlphaKey(1f, 1f)
			},
				mode = GradientMode.PerceptualBlend
			};
		}

		var _progress = Mathf.Clamp01(progress);
		if (!changeTarget && Mathf.Approximately(targetProgress, _progress))
		{
			return;
		}
		targetProgress = _progress;

		InitBlock();
		currentColor = targetGradient.Evaluate(targetProgress * 0.9f);

		changeColor = true;
		enabled = true;
	}

	private void LateUpdate()
	{
		if (changeColor == false)
		{
			enabled = false;
			return;
		}
		changeColor = false;
		RenderColor();
	}

	private void RenderColor()
	{
		InitBlock();
		thisMaterial.SetColor("_BaseColor", currentColor);
		thisRenderer.SetPropertyBlock(thisMaterial);
	}
}
