using UnityEngine;

namespace GameUI
{
    public interface ITargetForCardPanel : ITargetToPanelAPI
	{
		public Sprite GetCardImage();
		public string GetCardName();
	}
}
