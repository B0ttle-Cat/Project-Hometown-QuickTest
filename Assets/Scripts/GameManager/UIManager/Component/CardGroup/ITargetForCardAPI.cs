using UnityEngine;

namespace GameUI
{
    public interface ITargetForCardAPI : ITargetToPanelAPI
	{
		public Sprite GetCardImage();
		public string GetCardName();
	}
}
