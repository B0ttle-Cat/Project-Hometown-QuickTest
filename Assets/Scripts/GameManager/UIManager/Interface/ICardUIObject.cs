using UnityEngine;

namespace GameUI
{
    public interface ICardUIObject
    {
		public Sprite GetTitleImage();
		public string GetTitleName();
		public string GetDescription();
		public string GetFactionName();
	}
}
