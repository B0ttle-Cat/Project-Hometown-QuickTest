using System;
using System.Threading;

using UnityEngine;

namespace GameUI
{
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
	public interface IShowHide : IPanelItem
	{
		IShowHideController ShowHideController => RootUI;
		IShowHide ThisShowHide { get; }
		bool IsShow { get; set; }
		bool IsHide { get => !IsShow; set => IsShow = !value; }
		void OnShow(Action awaitCallback = null)
		{
			if (ShowHideController.IsNullRef())
			{
				return;
			}
			ShowHideController.OnShow(this, awaitCallback);
		}
		void OnHide(Action awaitCallback = null)
		{
			if (ShowHideController.IsNullRef())
			{
				return;
			}
			ShowHideController.OnHide(this, awaitCallback);
		}

		[Obsolete("특수한 경우가 아니면, 이 메서드를 직접 호출하지 말것")]
		void Show();
		[Obsolete("특수한 경우가 아니면, 이 메서드를 직접 호출하지 말것")]
		void Hide();

		void OnShowImmediate()
		{
			if (ShowHideController.IsNullRef())
			{
				if (ShowHideController.IsNullRef()) return;
			}
			ShowHideController.OnShowImmediate(this);
		}
		void OnHideImmediate()
		{
			if (ShowHideController.IsNullRef())
			{
				if (ShowHideController.IsNullRef()) return;
			}
			ShowHideController.OnHideImmediate(this);
		}

		void PairingShowHide() => ShowHideController.PairingShowHide(this);
		void UnpairingShowHide() => ShowHideController.UnpairingShowHide(this);
	}
	public interface IShowHideAsync : IShowHide
	{
		[Obsolete("특수한 경우가 아니면, 이 메서드를 직접 호출하지 말것")]
		Awaitable Show(CancellationToken cancellationToken);
		[Obsolete("특수한 경우가 아니면, 이 메서드를 직접 호출하지 말것")]
		Awaitable Hide(CancellationToken cancellationToken);
	}
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.
}
