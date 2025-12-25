using System;
using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

using static GameUI.FillRectMultiPanelUI;
using static GameUI.FillRectPanelUI;

namespace GameUI
{
	public class FillRectMultiPanelUI : PanelGroupComponent<FillRectMultiItem>, IFillGroup
	{
		#region PanelGroupComponent
		[PropertyOrder(0), BoxGroup("FillRext")]
		[ShowInInspector, ListDrawerSettings(ShowFoldout = false)]
		protected override List<FillRectMultiItem> Items
		{
			get { return items; }
			set { items = value; }
		}
		#endregion

		[BoxGroup("Text"), PropertyOrder(1), SerializeField]
		protected TMP_Text textUI;


		public enum GroupFillMethodType { 누적, 분배, }
		[SerializeField, HideIf("@true")]
		private Vector2 minMax;
		[SerializeField, HideIf("@true")]
		private FillMethodType fillMethod;
		[SerializeField, HideIf("@true")]
		private FloatToIntType floatToInt;
		[SerializeField, HideIf("@true")]
		private GroupFillMethodType groupFillMethod;
		[SerializeField, HideIf("@true")]
		protected string textFormat;

		public Vector2 Value { 
			get => new Vector2(0, TotalValue()); 
			set { this[0].FillValue = value.y; }
		}
		float IFillGroup.this[int index] {
			get => this[0].FillValue; 
			set => this[0].FillValue = value; 
		}

		[BoxGroup("FillRext"), ShowInInspector]
		public Vector2 MinMax
		{
			get => minMax;
			set { minMax = value; ChangeMinMax(); }
		}
		[BoxGroup("FillRext"), ShowInInspector, EnumToggleButtons]
		public FillMethodType FillMethod
		{
			get => fillMethod;
			set { fillMethod = value; ChangeFillMethod(); }
		}
		[BoxGroup("FillRext"), ShowInInspector, EnumToggleButtons]
		public FloatToIntType FloatToInt
		{
			get => floatToInt;
			set { floatToInt = value; ChangeFillMethod(); }
		}
		[BoxGroup("FillRext"), ShowInInspector, EnumToggleButtons]
		public GroupFillMethodType GroupFillMethod
		{
			get => groupFillMethod;
			set { groupFillMethod = value; GruopFillUpdate(); }
		}
		[BoxGroup("Text"), ShowIf("@textUI != null"), ShowInInspector]
		public string TextFormat
		{
			get => textFormat;
			set { textFormat = value; ChangeText(); }
		}

		public override IPanelGroup<FillRectMultiItem> ThisPanel => this;
		public override IShowHideAsync ThisShowHide => this;

        protected override void Reset()
		{
			InitListItem();

			var texts = GetComponentsInChildren<TMP_Text>(true);
			foreach (var text in texts)
			{
				if (text.transform.parent == this.transform)
				{
					this.textUI = text; break;
				}
			}
		}
		protected void OnValidate()
		{
			InitListItem();
		}
		protected void Awake()
		{
			InitListItem();
		}
		[ButtonGroup, PropertyOrder(-1)]
		private void ResetListItem()
		{
			Items.Clear();
			InitListItem();
		}
		[ButtonGroup, PropertyOrder(-1)]
		private void InitListItem()
		{
			if (Count == 0)
			{
				Items = new List<FillRectMultiItem>();
				var fillRects = GetComponentsInChildren<FillRectPanelUI>(true);
				foreach (var fill in fillRects)
				{
					Items.Add(new FillRectMultiItem(fill, GruopFillUpdate));
				}
			}
			else
			{
				foreach (var item in Items)
				{
					item.SetFillAction(GruopFillUpdate);
				}
			}
			ChangeMinMax();
			ChangeFillMethod();
			GruopFillUpdate();
		}

		protected override void Hide()
		{
		}
		protected override void Show()
		{
		}
		[HideReferenceObjectPicker, Serializable]
		public class FillRectMultiItem : IPanelItem, IShowHide
		{
			[LabelWidth(100),LabelText("Fill Rect UI")]
			public FillRectPanelUI fillRectPanelUI;
			private bool isBG;
			private float fillValue;
			private Action fillUpdate;
#if UNITY_EDITOR
			private float Min => fillRectPanelUI == null ? fillValue : fillRectPanelUI.MinMax.x;
			private float Max => fillRectPanelUI == null ? fillValue : fillRectPanelUI.MinMax.y;
#endif
			[HorizontalGroup("Value", width: 20, VisibleIf = "@fillRectPanelUI != null"), HideLabel, ShowInInspector, ToggleLeft]
			public bool IsBackGround
			{
				get => isBG;
				set { isBG = value; fillUpdate?.Invoke(); }
			}
			[HorizontalGroup("Value"), ShowInInspector, DisableIf("IsBackGround"), LabelWidth(80)]
			[PropertyRange("Min", "Max")]
			public float FillValue
			{
				get => fillValue;
				set { fillValue = value; fillUpdate?.Invoke(); }
			}
			[ShowInInspector, HorizontalGroup("Color", VisibleIf = "@fillRectPanelUI != null")]
			[ColorUsage(false), LabelWidth(100)]
			public Color FillColor
			{
				get => fillRectPanelUI == null ? Color.clear : fillRectPanelUI.FillColor;
				set { if (fillRectPanelUI == null) return; else fillRectPanelUI.FillColor = value; }
			}
			[ShowInInspector, HorizontalGroup("Color")]
			[Range(0, 1), LabelWidth(60)]
			public float FillAlpha
			{
				get => fillRectPanelUI == null ? 0 : fillRectPanelUI.FillColor.a;
				set
				{
					if (fillRectPanelUI == null) return;
					else
					{
						Color color = fillRectPanelUI.FillColor;
						color.a = value;
						fillRectPanelUI.FillColor = color;
					}
				}
			}

			public FillRectMultiItem(FillRectPanelUI fill, Action fillUpdate)
			{
				fillRectPanelUI = fill;
				fillValue = 0;
				this.fillUpdate = fillUpdate;
			}
			public void SetFillAction(Action fillUpdate)
			{
				this.fillUpdate = fillUpdate;
			}


			public IPanelItem ThisPanel => fillRectPanelUI.ThisPanel;
			public IShowHide ThisShowHide => fillRectPanelUI.ThisShowHide;
			public bool IsShow { get => fillRectPanelUI.ThisShowHide.IsShow; set => fillRectPanelUI.ThisShowHide.IsShow = value; }
			RectTransform IPanelItem.ThisRect => fillRectPanelUI.ThisPanel.ThisRect;
#pragma warning disable CS0618 // 형식 또는 멤버는 사용되지 않습니다.
			void IShowHide.Hide() => fillRectPanelUI.ThisShowHide.Hide();
			void IShowHide.Show() => fillRectPanelUI.ThisShowHide.Show();
#pragma warning restore CS0618 // 형식 또는 멤버는 사용되지 않습니다.

			internal void ChangeMinMax(Vector2 minMax)
			{
				fillRectPanelUI.MinMax = minMax;
			}
			internal void ChangeValue(Vector2 value)
			{
				fillRectPanelUI.Value = value;
			}
			internal void ChangeFillMode(FillMethodType fillMethod)
			{
				fillRectPanelUI.FillMethod = fillMethod;
			}

			internal void ChangeFloatToInt(FloatToIntType floatToInt)
			{
				fillRectPanelUI.FloatToInt = floatToInt;
			}

			internal bool IsNull()
			{
				return fillRectPanelUI.IsNullRef();

			}
		}


		private void ChangeMinMax()
		{
			int count = Count;
			for (int i = 0 ; i < count ; i++)
			{
				var item = this[i];
				if (item.IsNull()) continue;
				item.ChangeMinMax(MinMax);
			}
			GruopFillUpdate();
		}
		private void ChangeFillMethod()
		{
			int count = Count;
			for (int i = 0 ; i < count ; i++)
			{
				var item = this[i];
				if (item.IsNull()) continue;
				item.ChangeFillMode(FillMethod);
				item.ChangeFloatToInt(FloatToInt);
			}
			GruopFillUpdate();
		}
		private void GruopFillUpdate()
		{
			int count = Count;
			if (count == 0) return;

			switch (GroupFillMethod)
			{
				case GroupFillMethodType.누적: FillRectUpdate_누적(); break;
				case GroupFillMethodType.분배: FillRectUpdate_분배(); break;
			}
			ChangeText();
		}
		private void FillRectUpdate_누적()
		{
			int count = Count;
			float min = MinMax.x;
			float max = MinMax.y;

			float currentPos = min;
			if (FloatToInt != FloatToIntType.Float)
			{
				currentPos = ConvertFloatToInt(currentPos);
			}
			for (int i = 0 ; i < count ; i++)
			{
				var item = this[i];
				if (item.IsNull()) continue;

				if (item.IsBackGround)
				{
					// isBG가 true인 경우 현재 위치부터 전체 범위의 끝(MinMax.y)까지 채움
					if (FloatToInt != FloatToIntType.Float)
					{
						item.ChangeValue(new Vector2(ConvertFloatToInt(currentPos), ConvertFloatToInt(max)));
					}
					else
					{
						item.ChangeValue(new Vector2(currentPos, max));
					}
				}
				else
				{
					if (FloatToInt != FloatToIntType.Float)
					{
						float nextPos = ConvertFloatToInt(currentPos + item.FillValue - min);
						item.ChangeValue(new Vector2(ConvertFloatToInt(currentPos), nextPos));
						currentPos = nextPos;
					}
					else
					{
						float nextPos = currentPos + item.FillValue - min;
						item.ChangeValue(new Vector2(currentPos, nextPos));
						currentPos = nextPos;
					}
				}
			}
		}
		private void FillRectUpdate_분배()
		{
			int count = Count;
			float min = MinMax.x;
			float max = MinMax.y;
			float totalFillValue = min;
			float totalRange = max - min;
			float currentRangeStart = min;

			for (int i = 0 ; i < count ; i++)
			{
				// isBG는 합계에서 제외
				var item = this[i];
				if (item.IsNull()) continue;
				if (!item.IsBackGround)
				{
					totalFillValue += this[i].FillValue - min;
				}
			}

			if (FloatToInt != FloatToIntType.Float)
			{
				min = ConvertFloatToInt(min);
				max = ConvertFloatToInt(max);
				totalRange = ConvertFloatToInt(totalRange);
				currentRangeStart = ConvertFloatToInt(currentRangeStart);
			}
			for (int i = 0 ; i < count ; i++)
			{
				var item = this[i];
				if (item.IsNull()) continue;
				if (item.IsBackGround || totalFillValue <= min)
				{
					// isBG이거나 유효한 값이 없는 경우 빈 영역 처리
					if (FloatToInt != FloatToIntType.Float)
					{
						item.ChangeValue(new Vector2(0, 0));
					}
					else
					{
						item.ChangeValue(new Vector2(0, 0));
					}
					continue;
				}

				float ratio = (item.FillValue-min) / (totalFillValue-min);
				float rangeSize = totalRange * ratio;
				float nextRangeEnd = currentRangeStart + rangeSize;

				if (FloatToInt != FloatToIntType.Float)
				{
					nextRangeEnd = ConvertFloatToInt(nextRangeEnd);
					item.ChangeValue(new Vector2(ConvertFloatToInt(currentRangeStart), nextRangeEnd));
					currentRangeStart = nextRangeEnd;
				}
				else
				{
					item.ChangeValue(new Vector2(currentRangeStart, nextRangeEnd));
					currentRangeStart = nextRangeEnd;
				}
			}
		}
		private void ChangeText()
		{
			if (textUI.IsNullRef()) return;
			if (string.IsNullOrWhiteSpace(textFormat))
			{
				textUI.text = "";
				return;
			}

			float min = MinMax.x;
			float max = MinMax.y;
			if (min > max)
			{
				max = MinMax.x;
				min = MinMax.y;
			}
			int count = Count;
			float total = min;
			for (int i = 0 ; i < count ; i++)
			{
				var item = this[i];
				if (item.IsNull()) continue;
				total += item.FillValue - min;
			}

			try
			{
				if (floatToInt != FloatToIntType.Float)
				{
					textUI.text = string.Format(textFormat,
						ConvertFloatToInt(total),
						ConvertFloatToInt(max),
						ConvertFloatToInt(min),
						ConvertFloatToInt(min));
				}
				else
				{
					textUI.text = string.Format(textFormat, total, max, min, min);
				}
			}
			catch (Exception ex)
			{
				textUI.text = textFormat;
				Debug.LogWarning(ex);
			}
		}
		private int ConvertFloatToInt(float value)
		{
			return floatToInt switch
			{
				FloatToIntType.FloorToInt => Mathf.FloorToInt(value),
				FloatToIntType.RountToInt => Mathf.RoundToInt(value),
				FloatToIntType.CeilToInt => Mathf.CeilToInt(value),
				_ => (int)value,
			};
		}

		private float TotalValue()
		{
			float min = MinMax.x;
			float max = MinMax.y;
			if (min > max)
			{
				max = MinMax.x;
				min = MinMax.y;
			}

			int count = Count;
			float total = min;
			for (int i = 0 ; i < count ; i++)
			{
				var item = this[i];
				if (item.IsNull()) continue;
				total += item.FillValue - min;
			}

			return total;
		}
	}
}
