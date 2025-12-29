using System;
using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
    public class KeyPairPanelItemComponent : PanelItemComponent, IFindUIObject
	{
		public IFindUIObject ThisUIFinder => this;
#if UNITY_EDITOR
		[SerializeField, PropertyOrder(-91), ButtonGroup]
		private void CopyKeyPairs()
		{
			CopyKeyPairObjectData copy = new CopyKeyPairObjectData()
			{
				keyPairs = keyPairs
			};
			GUIUtility.systemCopyBuffer = JsonUtility.ToJson(copy);
		}
		[SerializeField, PropertyOrder(-91), ButtonGroup]
		private void PasteKeyPairs()
		{
			try
			{
				CopyKeyPairObjectData data = JsonUtility.FromJson<CopyKeyPairObjectData>(GUIUtility.systemCopyBuffer);
				keyPairs = data.keyPairs.ToList();
			}
			catch { }
		}


        [Serializable]
		private class CopyKeyPairObjectData
		{
			public List<IFindUIObject.KeyPairObject> keyPairs;
		}
#endif
		[SerializeField, PropertyOrder(-90)]
		private List<IFindUIObject.KeyPairObject> keyPairs;
		List<IFindUIObject.KeyPairObject> IFindUIObject.KeyPairs => keyPairs;
	}
}
