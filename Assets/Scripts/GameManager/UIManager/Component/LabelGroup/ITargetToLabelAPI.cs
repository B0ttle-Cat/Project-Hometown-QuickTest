using UnityEngine;

namespace GameUI
{
    public interface ITargetToLabelAPI : ITargetToPanelAPI
    {
		public string GetLabelName();
		public Sprite GetLabelIcon();
		public Color GetLabelAccentColor() => Color.white;
		public Color GetLabelTextColor() => Color.black;
		public Vector3 LabelWorldPosition();
	}
}
