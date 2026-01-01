using UnityEngine;

namespace GameUI
{
    public interface ITargetForLabelPanel : ITargetToPanelAPI
    {
		public string GetLabelName();
		public Sprite GetLabelIcon();
	}
}
