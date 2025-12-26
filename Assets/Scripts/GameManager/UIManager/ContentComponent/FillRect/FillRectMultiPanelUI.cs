using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

		[BoxGroup("Text"), PropertyOrder(2), SerializeField]
		protected TMP_Text textUI;


		public enum GroupFillMethodType { 누적, 분배, [InspectorName("누적 후 분배")]누적분배 }
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
		private bool skipChange;
	
		public Vector2 Value
		{
			get => new Vector2(0, TotalValue());
			set { this[0].FillValue = value.y; }
		}
		float IFillGroup.this[int index]
		{
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


		[BoxGroup("BackGround"), PropertyOrder(1), SerializeField]
		private FillRectPanelUI backGroundFillRect;
		[ShowInInspector, HorizontalGroup("BackGround/Color", VisibleIf = "@backGroundFillRect != null"), PropertyOrder(1)]
		[ColorUsage(false), LabelWidth(100)]
		public Color BgColor
		{
			get => backGroundFillRect == null ? Color.clear : backGroundFillRect.FillColor;
			set { if (backGroundFillRect == null) return; else backGroundFillRect.FillColor = value; }
		}
		[ShowInInspector, HorizontalGroup("BackGround/Color"), PropertyOrder(1)]
		[Range(0, 1), LabelWidth(60)]
		public float BgAlpha
		{
			get => backGroundFillRect == null ? 0 : backGroundFillRect.FillColor.a;
			set
			{
				if (backGroundFillRect == null) return;
				else
				{
					Color color = backGroundFillRect.FillColor;
					color.a = value;
					backGroundFillRect.FillColor = color;
				}
			}
		}

#if UNITY_EDITOR
		[BoxGroup("Text"), ShowIf("@textUI != null"), PropertyOrder(2), ShowInInspector]
		private bool ShowTextFormatHelp { get; set; }
#endif
		[InfoBox(@"TextFormat Hint
	{i}:Items 의 i 번째 값 (Fill Value 가 비활성 상테에서는 인덱스에서 무시된다.)
		연산은 다음 + - 와 Range 를 지원한다. 	
		{a+b} = Items[a] + Items[b]의 값
		{a-b} = Items[a] + Items[b]의 값
			ex) {a+b-c} = Items[a] + Items[b] - Items[c] 의 깂
		{..}: 모든 Items 의 총합
			그외 모든 System.Range를 사용 가능하며 모두 합하여 계산된다.
			ex) {a..} = Items[a] + Items[a+1] + Items[a+2] .... Items[count-1]
			ex) {a..b} = Items[a] + Items[a+1] + Items[a+2] .... Items[b]
			ex) {a..^b} = Items[a] + Items[a+1] + Items[a+2] .... Items[count-b]
			ex) {^b} = Items[0] + Items[1] + Items[2] .... Items[count-b]
	{max}: 최대값
	{min}: 최소값
	자리수를 정하기 위해 value:#.## 또는 value:0.00 사용 가능.", VisibleIf = "ShowTextFormatHelp")]
		[BoxGroup("Text"), ShowIf("@textUI != null"), PropertyOrder(2), ShowInInspector]
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
					Items.Add(new FillRectMultiItem(fill, OnChangeValue));
				}
			}
			else
			{
				foreach (var item in Items)
				{
					item.SetFillAction(OnChangeValue);
				}
			}
			ChangeMinMax();
			ChangeFillMethod();
			GruopFillUpdate();
			skipChange = false;
			void OnChangeValue()
			{
				if (skipChange) return;
				GruopFillUpdate();
			}
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
			[SerializeField, HideIf("@true")]
			private bool isBG;
			[SerializeField, HideIf("@true")]
			private float fillValue;
			private Action fillUpdate;
#if UNITY_EDITOR
			private float Min => fillRectPanelUI == null ? fillValue : fillRectPanelUI.MinMax.x;
			private float Max => fillRectPanelUI == null ? fillValue : fillRectPanelUI.MinMax.y;
#endif
			[HorizontalGroup("Value"), ShowInInspector, LabelWidth(100)]
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
		public void SetValue(params int[] values)
		{
			skipChange = true;
			int length = values.Length;
			for (int i = 0 ; i < length && i < Count ; i++)
			{
				this[i].FillValue = values[i];
			}
			skipChange = false;
			GruopFillUpdate();
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
			if(backGroundFillRect.IsNotNullRef())
					backGroundFillRect.MinMax = MinMax;
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
			if (backGroundFillRect.IsNotNullRef())
			{
				backGroundFillRect.FillMethod = FillMethod;
				backGroundFillRect.FloatToInt = FloatToInt;
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
				case GroupFillMethodType.누적분배: FillRectUpdate_누적분배(); break;
				case GroupFillMethodType.분배: FillRectUpdate_분배(); break;
			}
			ChangeText();
		}
		private void FillRectUpdate_누적분배()
		{
			int count = Count;
			float min = MinMax.x;
			float max = MinMax.y;
			float totalFillValue = min;
			float totalRange = max - min;
			for (int i = 0 ; i < count ; i++)
			{
				// isBG는 합계에서 제외
				var item = this[i];
				if (item.IsNull()) continue;
				totalFillValue += this[i].FillValue - min;
			}
			if (totalRange > totalFillValue)
			{
				FillRectUpdate_누적();
			}
			else
			{
				FillRectUpdate_분배();
			}
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
			if (backGroundFillRect.IsNotNullRef())
			{
				// isBG가 true인 경우 현재 위치부터 전체 범위의 끝(MinMax.y)까지 채움
				backGroundFillRect.MinMax = minMax;
				if (FloatToInt != FloatToIntType.Float)
				{
					backGroundFillRect.Value = new Vector2(ConvertFloatToInt(currentPos), ConvertFloatToInt(max));
				}
				else
				{
					backGroundFillRect.Value = new Vector2(currentPos, max);
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

			int lastIndex = -1;
			for (int i = 0 ; i < count ; i++)
			{
				// isBG는 합계에서 제외
				var item = this[i];
				if (item.IsNull()) continue;
				totalFillValue += this[i].FillValue - min;
				lastIndex = i;
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
				if (totalFillValue <= min)
				{
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
					if (i == lastIndex)
					{
						nextRangeEnd = max;
					}
					item.ChangeValue(new Vector2(ConvertFloatToInt(currentRangeStart), nextRangeEnd));
					currentRangeStart = nextRangeEnd;
				}
				else
				{
					item.ChangeValue(new Vector2(currentRangeStart, nextRangeEnd));
					currentRangeStart = nextRangeEnd;
				}
			}
			backGroundFillRect.Value = Vector2.zero;
		}
		private void ChangeText()
		{
			if (textUI.IsNullRef()) return;
			if (string.IsNullOrWhiteSpace(textFormat))
			{
				textUI.text = "";
				return;
			}

			// 인덱스 참조({0}, {^1} 등)는 이 리스트를 기준으로 동작함
			var validItems = Items;
			int vCount = Count;

			string processedText = textFormat;
			float min = MinMax.x;
			float max = MinMax.y;

			// 정규식 패턴: {query:format}
			processedText = Regex.Replace(processedText, @"\{(?<query>[^:{}]+)(?::(?<format>[^}]+))?\}", m =>
			{
				string query = m.Groups["query"].Value.Trim();
				string format = m.Groups["format"].Success ? m.Groups["format"].Value : null;

				try
				{
					float resultValue = 0f;
					bool isMatched = false;

					// A. 키워드 처리
					if (query == ".." || query == "total")
					{
						resultValue = TotalValue(); // TotalValue는 내부적으로 이미 min/max 및 BG 제외 로직을 따름
						isMatched = true;
					}
					else if (query == "max")
					{
						resultValue = max;
						isMatched = true;
					}
					else if (query == "min")
					{
						resultValue = min;
						isMatched = true;
					}
					// B. 범위 합산 처리: {a..b}, {a..^b} (BG 제외 카운트 기준)
					else if (query.Contains(".."))
					{
						string[] parts = query.Split(new[] { ".." }, StringSplitOptions.None);

						int start = string.IsNullOrEmpty(parts[0]) ? 0 : ParseIndex(parts[0], vCount);
						int end = (parts.Length < 2 || string.IsNullOrEmpty(parts[1])) ? vCount : ParseIndex(parts[1], vCount);

						start = Mathf.Clamp(start, 0, vCount);
						end = Mathf.Clamp(end, 0, vCount);

						float sum = 0;
						for (int i = start ; i < end ; i++)
						{
							sum += validItems[i].FillValue - min;
						}
						resultValue = sum;
						isMatched = true;
					}
					// C. 산술 연산 처리: {0+1-^1} (BG 제외 카운트 기준)
					else if (query.Contains("+") || query.Contains("-"))
					{
						var matches = Regex.Matches(query, @"([+-]?\s*\^?\d+)");
						float calcResult = 0;
						foreach (Match match in matches)
						{
							string part = match.Value.Replace(" ", "");
							bool isNegative = part.StartsWith("-");

							string indexStr = part.TrimStart('+', '-');
							int index = ParseIndex(indexStr, vCount);

							if (index >= 0 && index < vCount)
							{
								calcResult += isNegative ? -(validItems[index].FillValue - min) : (validItems[index].FillValue - min);
							}
						}
						resultValue = calcResult;
						isMatched = true;
					}
					// D. 단일 인덱스 참조: {i} 또는 {^i} (BG 제외 카운트 기준)
					else
					{
						if (Regex.IsMatch(query, @"^\^?\d+$"))
						{
							int idx = ParseIndex(query, vCount);
							if (idx >= 0 && idx < vCount)
							{
								resultValue = validItems[idx].FillValue - min;
								isMatched = true;
							}
						}
					}

					if (isMatched)
					{
						return GetFormattedValue(resultValue, format);
					}
				}
				catch (Exception ex)
				{
					Debug.LogWarning($"Format Error in '{query}': {ex.Message}");
				}

				return m.Value;
			});

			textUI.text = processedText;
		}
		/// <summary>
		/// 문자열 인덱스를 실제 배열 인덱스로 변환합니다. ^ 연산자를 지원합니다.
		/// </summary>
		private int ParseIndex(string input, int length)
		{
			input = input.Trim();
			if (input.StartsWith("^"))
			{
				if (int.TryParse(input.Substring(1), out int val))
				{
					return length - val;
				}
			}
			else
			{
				if (int.TryParse(input, out int val))
				{
					return val;
				}
			}
			return 0;
		}
		private string GetFormattedValue(float value, string format)
		{
			if (floatToInt != FloatToIntType.Float)
			{
				int intVal = ConvertFloatToInt(value);
				return !string.IsNullOrEmpty(format) ? intVal.ToString(format) : intVal.ToString();
			}

			if (!string.IsNullOrEmpty(format))
			{
				return value.ToString(format);
			}

			return value.ToString();
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
