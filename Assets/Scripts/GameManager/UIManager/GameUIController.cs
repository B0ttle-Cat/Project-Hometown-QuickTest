using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	
	public abstract class GameUIController : MonoBehaviour
		, IShowStackController
	{
		public IShowStackController StackController => this;
		public virtual IShowStackController.GroupShowStack ShowStack { get; protected set; }

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



		[ButtonGroup]
		public void OnShow()
		{
			Show();
		}
		[ButtonGroup]
		public void OnHide()
		{
			Hide();
		}

		protected abstract void Show();
		protected abstract void Hide();
    }
}
