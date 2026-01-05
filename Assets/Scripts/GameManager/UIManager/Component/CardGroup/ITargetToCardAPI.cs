using UnityEngine;

namespace GameUI
{
    public interface ITargetToCardAPI : ITargetToPanelAPI
	{
		public Sprite GetCardImage();
		public string GetCardName();
	}
}
