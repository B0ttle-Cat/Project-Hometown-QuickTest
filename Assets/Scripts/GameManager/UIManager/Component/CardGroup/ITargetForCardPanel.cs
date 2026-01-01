using UnityEngine;

namespace GameUI
{
    public interface ITargetForCardPanel : IObjectForPanel
	{
		public Sprite GetTitleImage();
		public string GetTitleName();
		public string GetDescription();
		public string GetFactionName();
	}
}
