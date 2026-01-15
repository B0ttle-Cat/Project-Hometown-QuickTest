using System;
using System.Collections;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;

namespace GameUI
{
	[RequireComponent(typeof(RectTransform))]
	public abstract class PanelGroupComponent : PanelItemComponent, IShowHideAsync
	{
		public abstract void Add(IPanelItem item);
		public abstract void Insert(int index, IPanelItem item);
		public abstract bool Remove(IPanelItem item);
		public abstract bool Contains(IPanelItem item);
		internal abstract void AddItem(ITargetToPanelAPI item, RectTransform slot = null);
		internal abstract bool RemoveItem(ITargetToPanelAPI item, RectTransform slot = null);
	}
	public abstract class PanelGroupComponent<T> : PanelGroupComponent, IPanelGroup<T>, IShowHideAsync
		where T : class, IPanelItem
	{

		#region IPanelGroup<T>
		[HideIf("@true")]
		private List<T> items;
		[ShowInInspector]
		protected List<T> Items
		{
			get { if (items == null) New(); return items; }
			set { items = value; }
		}
		public int Count => items == null ? 0 : items.Count;
		public bool IsReadOnly => ((ICollection<T>)Items).IsReadOnly;
		public virtual T this[int index] { get => Items[index]; set => Items[index] = value; }
		public int IndexOf(T item)
		{
			return Items.IndexOf(item);
		}
		sealed public override void Insert(int index, IPanelItem item) => Insert(index, item as T);
		public virtual void Insert(int index, T item)
		{
			Items.Insert(index, item);
		}
		public virtual void RemoveAt(int index)
		{
			if (index >= 0 && index < Count)
			{
				Remove(this[index]);
			}
		}
		sealed public override void Add(IPanelItem item) => Add(item as T);
		sealed public override bool Remove(IPanelItem item) => Remove(item as T);
		public virtual bool Remove(T item)
		{
			if (Items.Remove(item))
			{
				if (!PoolingUIObject)
				{
					GameObject.Destroy(item.ThisRect.gameObject);
				}
				return true;
			}
			return false;
		}
		public virtual void Add(T item)
		{
			Items.Add(item);
		}
		public virtual void New()
		{
			if (items == null) items = new List<T>();
			else items.Clear();
		}
		public virtual void Clear()
		{
			if (items == null) return;
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				var item = this[i];
				var itemRect = item.ThisRect;

				if (item is IDisposable disposable) disposable.Dispose();

				if (itemRect.IsNotNullRef())
				{
					GameObject.Destroy(itemRect.gameObject);
				}
			}
			if (PoolingUIObject)
			{
				ClearPoolStack();
			}
			Items.Clear();
		}
		sealed public override bool Contains(IPanelItem item) => Contains(item as T);
		public virtual bool Contains(T item)
		{
			return Items.Contains(item);
		}
		public void CopyTo(T[] array, int arrayIndex)
		{
			Items.CopyTo(array, arrayIndex);
		}
		public IEnumerator<T> GetEnumerator()
		{
			return Items.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return Items.GetEnumerator();
		}
		#endregion

		protected virtual void Reset()
		{
			items = new List<T>();
		}
		protected override void Awake()
		{
			items ??= new List<T>();
			base.Awake();
		}
		protected override void OnDestroy()
		{
			Clear();
			base.OnDestroy();
		}

		#region PoolingDataContainer


		[SerializeField]
		[InfoBox("@TypeErrorMessage", VisibleIf ="NotPrefabIsUsable", InfoMessageType = InfoMessageType.Error)]
		protected T PanelPrefab;
		[SerializeField]
		protected RectTransform PanelParent;
		private bool PrefabIsUsable => PanelPrefab.IsNotNullRef() && PanelPrefab is PanelItemComponent and ISetTargetPanel and T;
#if UNITY_EDITOR
		private bool NotPrefabIsUsable => !PrefabIsUsable;
		private string TypeErrorMessage =>
			PanelPrefab.IsNullRef() ? "PanelPrefab 이 Null 입니다. 정상동작을 위해 값을 세팅해 주세요." :
			$"사용 가능한 컴퍼넌트를 가지고 있지 않습니다.\n\"{nameof(T)}\"티압을 상속받은 컴퍼넌트가 필요합니다.";
		private bool ShowPoolingField => PrefabIsUsable && PanelParent.IsNotNullRef();
#endif

		[SerializeField, ShowIf("ShowPoolingField")]
		[Tooltip("선택사항: PanelPrefab를 풀링하여 사용하고 싶은 경우 할당할 것.")]
		private PoolingDataContainer poolingDataContainer;
		public PoolingDataContainer PoolingData
		{
			get
			{
				if (poolingDataContainer.IsNullRef())
				{
					TryGetComponent<PoolingDataContainer>(out poolingDataContainer);
				}
				return poolingDataContainer;
			}
		}
		protected bool PoolingUIObject => poolingDataContainer.IsNotNullRef() && PrefabIsUsable;

		[ShowInInspector, ShowIf("PoolingUIObject")]
		[HideInEditorMode]
		protected Stack<PanelItemComponent> PoolStack;
		[SerializeField, ShowIf("PoolingUIObject"), Tooltip("0보다 작으면 무제한, 0 과 일치할 경우 Stack 에 저장 안함")]
		private int limitPoolStackCount = -1;
		protected virtual void InitObjects(IEnumerable<ITargetToPanelAPI> targets)
		{
			Clear();
			poolingDataContainer = PoolingData;
			if (PoolingUIObject)
			{
				poolingDataContainer.InitData(targets);
				return;
			}

			foreach (var item in targets)
			{
				if (item.IsNullRef()) continue;
				AddItem(item);
			}
		}
		public void AddObject(ITargetToPanelAPI item)
		{
			poolingDataContainer = PoolingData;
			if (PoolingUIObject)
			{
				poolingDataContainer.AddData(item);
				return;
			}
		}
		public void RemoveObject(ITargetToPanelAPI item)
		{
			poolingDataContainer = PoolingData;
			if (PoolingUIObject)
			{
				poolingDataContainer.RemoveData(item);
				return;
			}
		}
		internal override void AddItem(ITargetToPanelAPI item, RectTransform slot = null)
		{
			if (!PrefabIsUsable) return;
			if (PanelPrefab is not PanelItemComponent PanelComponentPrefab) return;

			if (item.IsNullRef() || Contains(item)) return;

			Transform parent = slot.IsNotNullRef() ? slot :  PanelParent.IsNotNullRef() ? PanelParent : transform;

			while (PoolStack != null && PoolStack.Count > 0 && PoolStack.TryPop(out PanelItemComponent pop))
			{

				if (pop.IsNullRef()) continue;
				if (pop is not T tPanel) return;
				if (pop.ThisPanel.IsNullRef()) continue;
				pop.gameObject.SetActive(true);
				var thisRect = pop.ThisPanel.ThisRect;
				if (thisRect.IsNullRef()) continue;
				if (slot.IsNotNullRef())
				{
					thisRect.parent = parent;
					thisRect.anchoredPosition = Vector2.zero;
					thisRect.anchorMin = Vector2.zero;
					thisRect.anchorMax = Vector2.one;
					thisRect.sizeDelta = Vector2.zero;
					thisRect.pivot = Vector2.one * 0.5f;
				}
				//Transform popSiblingTarget = slot.IsNotNullRef() ? slot : thisRect;
				//if (addLast) popSiblingTarget.SetAsLastSibling();
				//else popSiblingTarget.SetAsFirstSibling();
				if (pop is ISetTargetPanel setPop && setPop.SetTarget(item))
				{
					Add(tPanel);
				}
				else
				{
					Remove(tPanel);
				}
				return;
			}

			var newPanel = Instantiate(PanelComponentPrefab, parent);
			if (newPanel.IsNullRef()) return;
			if (slot.IsNotNullRef())
			{
				var thisRect = newPanel.ThisPanel.ThisRect;
				thisRect.parent = parent;
				thisRect.anchoredPosition = Vector2.zero;
				thisRect.anchorMin = Vector2.zero;
				thisRect.anchorMax = Vector2.one;
				thisRect.sizeDelta = Vector2.zero;
				thisRect.pivot = Vector2.one * 0.5f;
			}
			if (newPanel is not T tNewPanel)
			{
				GameObject.Destroy(newPanel.gameObject);
				return;
			}

			var isInit =  SetPanelObject(tNewPanel, item);
			if (!isInit)
			{
				GameObject.Destroy(newPanel.gameObject);
				return;
			}

			Add(tNewPanel);
		}
		internal override bool RemoveItem(ITargetToPanelAPI item, RectTransform slot = null)
		{
			if (!PrefabIsUsable) return false;
			if (item.IsNullRef()) return false;

			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				var panel = this[i];
				if (panel.IsNullRef()) continue;
				if (panel is not ISetTargetPanel setPanel) continue;
				if (setPanel.Target != item) continue;
			
				if (panel is PanelItemComponent panelComponent)
				{
					if (Remove(panel) && limitPoolStackCount != 0)
					{
						PoolStack ??= new Stack<PanelItemComponent>();
						if (limitPoolStackCount < 0 || PoolStack.Count < limitPoolStackCount)
						{
							PoolStack.Push(panelComponent);
							panelComponent.transform.parent = transform;
							panelComponent.gameObject.SetActive(false);
						}
						else
						{
							GameObject.Destroy(panelComponent.gameObject);
						}
					}
					else
					{
						GameObject.Destroy(panelComponent.gameObject);
					}
				}
				else
				{
					Remove(panel);
				}
				length = Count;
				i--;
			}
			return false;
		}
		protected virtual bool SetPanelObject(T newPanel, ITargetToPanelAPI item)
		{
			if (newPanel is ISetTargetPanel setPanel)
			{
				return setPanel.SetTarget(item);
			}
			return false;
		}
		public virtual bool Contains(ITargetToPanelAPI item)
		{
			int length = Count;
			for (int i = 0 ; i < length ; i++)
			{
				var panel = this[i];
				if (panel.IsNullRef()) continue;
				if (panel is not ISetTargetPanel setPanel) continue;
				if (setPanel.Target == item)
				{
					return true;
				}
			}
			return false;
		}

		private void ClearPoolStack()
		{
			if (PoolStack == null) return;
			foreach (var item in PoolStack)
			{
				if (item.IsNullRef()) continue;
				GameObject.Destroy(item.gameObject);
			}
			PoolStack.Clear();
			if (poolingDataContainer != null)
			{
				poolingDataContainer.ClearData();
			}
		}
		#endregion
	}
}
