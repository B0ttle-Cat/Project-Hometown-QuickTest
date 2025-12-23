using System.Collections.Generic;
using System.Threading;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	
	public abstract class GameUIController : MonoBehaviour
		, IShowStackController
		, IFundUIObject
		, IShowHideAsync
	{
		public IShowStackController StackController => this;
		public IFundUIObject ThisUIFinder => this;
		public IShowHideAsync ThisShowHide => this;
		IShowHide IShowHide.ThisShowHide => ThisShowHide;

		[SerializeField, PropertyOrder(-10000)]
		private bool isShow = false;
		[SerializeField, PropertyOrder(-9998)]
		private List<IFundUIObject.KeyPairObject> keyPairs;
#if UNITY_EDITOR
		[ShowInInspector, InlineButton("TestKeyPair"), PropertyOrder(-9999)]
		private string testKeyPair { get; set; }
		void TestKeyPair(string key)
		{
			int count = ThisUIFinder.KeyPairs == null ? 0 : ThisUIFinder.KeyPairs.Count;
			for (int i = 0 ; i < count ; i++)
			{
				ThisUIFinder.KeyPairs[i].testFindit = ThisUIFinder.IsPathMatch(key, ThisUIFinder.KeyPairs[i].Key);
			}
		}
#endif
		public virtual IShowStackController.GroupShowStack ShowStack { get; protected set; }
		List<IFundUIObject.KeyPairObject> IFundUIObject.KeyPairs => keyPairs;
		bool IShowHide.IsShow { get => isShow; set => isShow = value; }
        CancellationTokenSource IShowHideAsync.ShowHideCancellationTokenSource { get; set; }
        protected virtual void Awake()
		{
			ShowStack = new IShowStackController.GroupShowStack();
		}

		protected virtual void OnDestroy()
		{
			Dispose();
		}
        public void Dispose()
        {
			ShowStack?.Clear();
			ShowStack = null;
		}

#if UNITY_EDITOR
		[ButtonGroup, PropertyOrder(-9997)]
		private void TestShow()
		{
			if (this is not IShowHide showHide) return;
			showHide.OnShow();
		}
		[ButtonGroup, PropertyOrder(-9997)]
		private void TestHide()
		{
			if (this is not IShowHide showHide) return;
			showHide.OnHide();
		}
#endif

		void IShowHide.Show()
		{
			Show();
		}
		void IShowHide.Hide()
		{
			Hide();
		}
		async Awaitable IShowHideAsync.Show(CancellationToken cancellationToken)
		{
			await Show(cancellationToken);
		}
		async Awaitable IShowHideAsync.Hide(CancellationToken cancellationToken)
		{
			await Hide(cancellationToken);
		}
		protected abstract void Show();
		protected abstract void Hide();
		protected virtual async Awaitable Show(CancellationToken cancellationToken){ return; }
		protected virtual async Awaitable Hide(CancellationToken cancellationToken){ return; }



    }
}
