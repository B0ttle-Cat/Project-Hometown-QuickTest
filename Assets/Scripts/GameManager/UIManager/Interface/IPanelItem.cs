using System.Collections.Generic;

using UnityEngine;

namespace GameUI
{
	public interface IPanelItem
	{
		//private RectTransform thisRect;
		//private GameUIController root;
		//public IPanelItem ThisPanel => this;
		//RectTransform IPanelItem.ThisRect => thisRect.IsNotNullRef() ? thisRect : thisRect = GetComponent<RectTransform>();
		//GameUIController IPanelItem.RootUI => root.IsNotNullRef() ? root : root = ThisPanel.FindRoot();
		GameUIController RootUI { get; }
		IPanelItem ThisPanel { get; }
		RectTransform ThisRect { get; }

		GameUIController FindRoot()
		{
			if (ThisRect.IsNullRef())
			{
				if(this is MonoBehaviour thisTr)
				{
					var result = thisTr.GetComponentInParent<GameUIController>();
					if (result.IsNotNullRef()) return result;
				}
				return GameObject.FindAnyObjectByType<GameUIController>();
			}
			return ThisRect.gameObject.GetComponentInParent<GameUIController>();
		}
	}
	public interface IPanelGroup<T> : IPanelItem, IList<T> where T : IPanelItem
	{

	}
}
