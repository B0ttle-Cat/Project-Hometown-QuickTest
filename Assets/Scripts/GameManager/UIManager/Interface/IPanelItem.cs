using System.Collections.Generic;

using UnityEngine;

namespace GameUI
{
	public interface IPanelItem
	{
		IPanelItem ThisPanel { get; }
		RectTransform ThisRect { get; }
	}
	public interface IPanelGroup<T> : IPanelItem, IList<T> where T : IPanelItem
	{
		new IPanelGroup<T> ThisPanel { get; }
	}
}
