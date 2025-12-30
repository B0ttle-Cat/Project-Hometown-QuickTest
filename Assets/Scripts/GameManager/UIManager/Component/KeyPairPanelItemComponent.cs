using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	[Obsolete("IFindUIObject 를 직접 상속 받을 것", true)]
    public class KeyPairPanelItemComponent : PanelItemComponent, IFindUIObject
	{
		public IFindUIObject ThisUIFinder => this;
		[SerializeField, PropertyOrder(-90)] private List<IFindUIObject.KeyPairObject> keyPairs;
		List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs { get => keyPairs; set => keyPairs = value; }
	}
}
