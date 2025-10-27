using System;
using System.Collections.Generic;

using UnityEngine;

namespace DockableFloatingUI
{
	public interface IDockable
	{
		public struct Data : IDisposable
		{
			public IDockable Top, Left, Right, Bottom, Parent;
			public DockingDir dockingDir;
			public enum DockingDir { None, Top, Left, Right, Bottom, Parent }
			public RectTransform ParentRect => dockingDir switch
			{
				IDockable.Data.DockingDir.Top => Top?.Rect,
				IDockable.Data.DockingDir.Left => Left?.Rect,
				IDockable.Data.DockingDir.Right => Right?.Rect,
				IDockable.Data.DockingDir.Bottom => Bottom?.Rect,
				IDockable.Data.DockingDir.Parent => Parent?.Rect,
				_ => null,
			};
			public IDockable ParentDockable => dockingDir switch
			{
				IDockable.Data.DockingDir.Top => Top,
				IDockable.Data.DockingDir.Left => Left,
				IDockable.Data.DockingDir.Right => Right,
				IDockable.Data.DockingDir.Bottom => Bottom,
				IDockable.Data.DockingDir.Parent => Parent,
				_ => null,
			};
			public void Dispose()
			{
				dockingDir = DockingDir.None;
				Top = null;
				Left = null;
				Right = null;
				Bottom = null;
				Parent = null;
			}
			public bool FindDock(DockingDir dir, out IDockable dockable)
			{
				dockable = null;
				if (dir == DockingDir.Top) dockable = Top;
				else if (dir == DockingDir.Left) dockable = Left;
				else if (dir == DockingDir.Right) dockable = Right;
				else if (dir == DockingDir.Bottom) dockable = Bottom;
				else if (dir == DockingDir.Parent) dockable = Parent;
				return dockable != null;
			}
			public bool FindDir(IDockable dockable, out DockingDir dir)
			{
				dir = DockingDir.None;
				if (dockable == Top) dir = DockingDir.Top;
				else if (dockable == Left) dir = DockingDir.Left;
				else if (dockable == Right) dir = DockingDir.Right;
				else if (dockable == Bottom) dir = DockingDir.Bottom;
				else if (dockable == Parent) dir = DockingDir.Parent;
				return dir != DockingDir.None;
			}
			public void SetDockable(IDockable dockable, DockingDir dir)
			{
				if (dir == DockingDir.None) return;
				else if (dir == DockingDir.Top) Top = dockable;
				else if (dir == DockingDir.Left) Left = dockable;
				else if (dir == DockingDir.Right) Right = dockable;
				else if (dir == DockingDir.Bottom) Bottom = dockable;
				else if (dir == DockingDir.Parent) Parent = dockable;
			}
			public IDockable[] GetChildDockingUI()
			{
				return dockingDir switch
				{
					Data.DockingDir.Top => new IDockable[3] { Left, Right, Bottom },
					Data.DockingDir.Left => new IDockable[3] { Top, Right, Bottom },
					Data.DockingDir.Right => new IDockable[3] { Left, Top, Bottom },
					Data.DockingDir.Bottom => new IDockable[3] { Left, Right, Top },
					_ => new IDockable[4] { Top, Left, Right, Bottom },
				};
			}
		}
		public RectTransform ParentDockableRect { get; }
		public IDockable ParentDockable { get; }
		public Data.DockingDir ParentDockingDir { get; }
		public RectTransform Rect { get; }
		public RectTransform HandleBar { get; }
		public Data DockableData { get; set; }
		public List<IDockable> MemberList { get; set; }
		public RectTransform MemberContentRoot { get; }

		void MoveChildDockingUI(in Vector2 delta)
		{
			foreach (var child in DockableData.GetChildDockingUI())
			{
				if (child == null || child.Rect == null) continue;
				child.Rect.anchoredPosition += delta;
				if (child is IDockable dockableChild)
					dockableChild.MoveChildDockingUI(in delta);
			}
		}


		public void SetParent(Data.DockingDir parent, IDockable parentDock)
		{
			// 그룹 contentRoot 아래로 이동
			var data = DockableData;
			data.dockingDir = parent;
			switch (parent)
			{
				case Data.DockingDir.Top: data.Top = parentDock; break;
				case Data.DockingDir.Left: data.Left = parentDock; break;
				case Data.DockingDir.Right: data.Right = parentDock; break;
				case Data.DockingDir.Bottom: data.Bottom = parentDock; break;
				case Data.DockingDir.Parent:
				{
					var contentRoot  = parentDock.MemberContentRoot;
					if (contentRoot != null) Rect.SetParent(contentRoot, false);
					data.Parent = parentDock;
				}
				break;
				default:
				break;
			}
			DockableData = data;
		}
		public void RemoveParent()
		{
			var data = DockableData;
			switch (ParentDockingDir)
			{
				case Data.DockingDir.Top: data.Top = null; break;
				case Data.DockingDir.Left: data.Left = null; break;
				case Data.DockingDir.Right: data.Right = null; break;
				case Data.DockingDir.Bottom: data.Bottom = null; break;
				default:
				{
					Rect.SetParent(DockableFloatingUIManager.Instance.transform);
					data.Parent = null;
				}
				break;
			}
			DockableData = data;
		}
		public bool AddMembers(Data.DockingDir thiToMemberDir, params IDockable[] members)
		{
			bool isAnyAdd = false;
			int length=members == null ? 0 : members.Length;
			for (int i = 0 ; i < length ; i++)
			{
				if (AddMember(members[i]))
				{
					isAnyAdd = true;
				}
			}
			return isAnyAdd;
			bool AddMember(IDockable member)
			{
				if (member == null) return false;

				switch (thiToMemberDir)
				{
					case Data.DockingDir.Top:
					{
						var data = DockableData;
						data.SetDockable(member, thiToMemberDir);
						DockableData = data;
						member.SetParent(Data.DockingDir.Bottom, this);
						return true;
					}
					case Data.DockingDir.Left:
					{
						var data = DockableData;
						data.SetDockable(member, thiToMemberDir);
						DockableData = data;
						member.SetParent(Data.DockingDir.Right, this);
						return true;
					}
					case Data.DockingDir.Right:
					{
						var data = DockableData;
						data.SetDockable(member, thiToMemberDir);
						DockableData = data;
						member.SetParent(Data.DockingDir.Left, this);
						return true;
					}
					case Data.DockingDir.Bottom:
					{
						var data = DockableData;
						data.SetDockable(member, thiToMemberDir);
						DockableData = data;
						member.SetParent(Data.DockingDir.Top, this);
						return true;
					}
					default:
					{
						if (MemberList.Contains(member)) return false;
						MemberList.Add(member);
						member.SetParent(IDockable.Data.DockingDir.Parent, this);
						return true;
					}
				}
			}
		}

		public bool RemoveMembers(params IDockable[] members)
		{
			bool isAnyAdd = false;
			int length=members == null ? 0 : members.Length;
			for (int i = 0 ; i < length ; i++)
			{
				if (RemoveMember(members[i]))
				{
					isAnyAdd = true;
				}
			}
			return isAnyAdd;

			bool RemoveMember(IDockable member)
			{
				if (member == null) return false;
				if (DockableData.FindDir(member, out var dir))
				{
					var data = DockableData;
					data.SetDockable(null, dir);
					DockableData = data;
					member.RemoveParent();
					return true;
				}
				else if (MemberList.Contains(member))
				{
					MemberList.Remove(member);
					member.RemoveParent();
					return true;
				}
				return false;
			}
		}
		public void RelaseGroup()
		{
			RemoveMembers(MemberList.ToArray());
			RemoveMembers(DockableData.GetChildDockingUI());
			var parent = DockableData.ParentDockable;
			if (parent != null) parent.RemoveMembers(this);
		}
	}
}
