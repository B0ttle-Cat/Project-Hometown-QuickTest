using UnityEngine;

namespace GameUI
{
    public interface ITargetToLabelAPI : ITargetToPanelAPI
    {
		public string GetLabelName();
		public Sprite GetLabelIcon();
		public Vector3 LabelWorldPosition();
	}
}
