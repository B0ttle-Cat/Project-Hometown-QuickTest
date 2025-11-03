using System.Collections.Generic;

using Sirenix.OdinInspector;

using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Advanced Cell Count Layout Group")]
public class AdvancedCellCountLayoutGroup : LayoutGroup
{
	public enum CellAxis { Horizontal, Vertical }
	public enum CellFitMode {
		[LabelText("고정 크기")]
		FixedSize,
		[LabelText("비율 맞춤")]
		AspectRatio, 
		[LabelText("자동 맞춤")]
		AutoFit }
	public enum Alignment { Start, Center, End, Evenly }
	public enum AutoFitSource { PreferredSize, MinSize }
	public enum LineDirection {
		[LabelText("오른쪽/아래로")]
		Forward,
		[LabelText("왼쪽/위로")]
		Reverse }

	[PropertyOrder(-1)]
	[SerializeField] private Vector2 spacing = Vector2.zero;

	[TitleGroup("고정 축 설정")]
	[HorizontalGroup("고정 축 설정/H"), LabelText("축 선택"), LabelWidth(50)]
	[SerializeField] private CellAxis axis = CellAxis.Horizontal;

	[HorizontalGroup("고정 축 설정/H"), LabelText("셀 개수"), LabelWidth(50), MinValue(1)]
	[SerializeField] private int cellCount = 3;

	[TitleGroup("변형 축 설정")]
	[HorizontalGroup("변형 축 설정/H"), SerializeField] private CellFitMode fitMode = CellFitMode.FixedSize;
	[HorizontalGroup("변형 축 설정/H"), HideLabel, SerializeField, ShowIf("IsFixedSize")] private float fixedSize = 100f;
	[HorizontalGroup("변형 축 설정/H"), HideLabel, SerializeField, ShowIf("IsAspectRatio")] private float aspectRatio = 1.0f;
	[HorizontalGroup("변형 축 설정/H"), HideLabel, SerializeField, ShowIf("IsAutoFit")] private AutoFitSource autoFitSource = AutoFitSource.PreferredSize;

	[TitleGroup("정렬 설정")]
	[SerializeField, HorizontalGroup("정렬 설정/H"), LabelText("새 줄 배치")]
	private LineDirection lineDirection = LineDirection.Forward;
	[SerializeField, HorizontalGroup("정렬 설정/H"), LabelText("끝 줄 정렬")]
	private Alignment alignment = Alignment.Start;


	private readonly List<RectTransform> currentLine = new();

    public override float minWidth => base.minWidth;

	private bool IsHorizontal() => axis == CellAxis.Horizontal;
	private bool IsFixedSize() => fitMode == CellFitMode.FixedSize;
	private bool IsAspectRatio() => fitMode == CellFitMode.AspectRatio;
	private bool IsAutoFit() => fitMode == CellFitMode.AutoFit;

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		CalculatePreferredSizes();
	}

	public override void CalculateLayoutInputVertical() => CalculatePreferredSizes();
	public override void SetLayoutHorizontal() => LayoutChildren();
	public override void SetLayoutVertical() => LayoutChildren();

	private void LayoutChildren()
	{
		if (rectChildren.Count == 0) return;
		if (cellCount < 1) cellCount = 1;

		float containerWidth = rectTransform.rect.width - padding.left - padding.right;
		float containerHeight = rectTransform.rect.height - padding.top - padding.bottom;

		
		int totalChildren = rectChildren.Count;

		float primaryCellSize = axis == CellAxis.Horizontal
			? (containerWidth - spacing.x * (cellCount - 1)) / cellCount
			: (containerHeight - spacing.y * (cellCount - 1)) / cellCount;

		float secondaryCellSize = GetOtherAxisSize(primaryCellSize);
		Vector2 cellSize = axis == CellAxis.Horizontal
			? new Vector2(primaryCellSize, secondaryCellSize)
			: new Vector2(secondaryCellSize, primaryCellSize);

		float posPrimary = (axis == CellAxis.Horizontal)
			? (lineDirection == LineDirection.Forward ? padding.top : rectTransform.rect.height - padding.bottom - primaryCellSize)
			: (lineDirection == LineDirection.Forward ? padding.left : rectTransform.rect.width - padding.right - primaryCellSize);

		float posSecondary = (axis == CellAxis.Horizontal) ? padding.left : padding.top;

		int currentCount = 0;
		currentLine.Clear();

		for (int i = 0 ; i < totalChildren ; i++)
		{
			var child = rectChildren[i];
			currentLine.Add(child);
			currentCount++;

			bool newLine = (currentCount >= cellCount) || (i == totalChildren - 1);
			if (newLine)
			{
				float autoFitSize = secondaryCellSize;
				if (fitMode == CellFitMode.AutoFit)
				{
					float maxValue = 0f;
					foreach (var c in currentLine)
					{
						float size = GetAutoFitSize(c);
						if (size > maxValue) maxValue = size;
					}
					autoFitSize = maxValue > 0 ? maxValue : fixedSize;

					if (axis == CellAxis.Horizontal)
						cellSize.y = autoFitSize;
					else
						cellSize.x = autoFitSize;
				}

				LayoutLine(currentLine, ref posPrimary, ref posSecondary, cellSize, containerWidth, containerHeight);
				currentLine.Clear();
				currentCount = 0;
			}
		}
	}

	private float GetOtherAxisSize(float primarySize)
	{
		return fitMode switch
		{
			CellFitMode.FixedSize => fixedSize,
			CellFitMode.AspectRatio => axis == CellAxis.Horizontal ? primarySize * aspectRatio : primarySize / aspectRatio,
			CellFitMode.AutoFit => fixedSize,
			_ => fixedSize
		};
	}

	private float GetAutoFitSize(RectTransform child)
	{
		if (axis == CellAxis.Horizontal)
		{
			return autoFitSource switch
			{
				AutoFitSource.PreferredSize => LayoutUtility.GetPreferredHeight(child),
				AutoFitSource.MinSize => LayoutUtility.GetMinHeight(child),
				_ => LayoutUtility.GetPreferredHeight(child)
			};
		}
		else
		{
			return autoFitSource switch
			{
				AutoFitSource.PreferredSize => LayoutUtility.GetPreferredWidth(child),
				AutoFitSource.MinSize => LayoutUtility.GetMinWidth(child),
				_ => LayoutUtility.GetPreferredWidth(child)
			};
		}
	}

	private void LayoutLine(List<RectTransform> line, ref float posPrimary, ref float posSecondary, Vector2 cellSize, float containerWidth, float containerHeight)
	{
		if (line.Count == 0) return;

		float spacingPrimary = axis == CellAxis.Horizontal ? spacing.y : spacing.x;
		float spacingSecondary = axis == CellAxis.Horizontal ? spacing.x : spacing.y;

		float totalSecondary = (axis == CellAxis.Horizontal ? cellSize.x : cellSize.y) * line.Count + spacingSecondary * (line.Count - 1);
		Alignment _alignment = alignment;
		if (line.Count == 1 && _alignment == Alignment.Evenly)
			_alignment = Alignment.Center;

		float startSecondary;
		switch (axis)
		{
			case CellAxis.Horizontal:
			switch (_alignment)
			{
				case Alignment.Start: startSecondary = padding.left; break;
				case Alignment.End: startSecondary = padding.left + containerWidth - totalSecondary; break;
				case Alignment.Center: startSecondary = padding.left + (containerWidth - totalSecondary) / 2f; break;
				case Alignment.Evenly: spacingSecondary = (containerWidth - (cellSize.x * line.Count)) / (line.Count - 1); startSecondary = padding.left; break;
				default: startSecondary = padding.left; break;
			}
			break;
			case CellAxis.Vertical:
			switch (_alignment)
			{
				case Alignment.Start: startSecondary = padding.top; break;
				case Alignment.End: startSecondary = padding.top + containerHeight - totalSecondary; break;
				case Alignment.Center: startSecondary = padding.top + (containerHeight - totalSecondary) / 2f; break;
				case Alignment.Evenly: spacingSecondary = (containerHeight - (cellSize.y * line.Count)) / (line.Count - 1); startSecondary = padding.top; break;
				default: startSecondary = padding.top; break;
			}
			break;
			default:
			startSecondary = padding.left;
			break;
		}

		for (int i = 0 ; i < line.Count ; i++)
		{
			var child = line[i];
			float x, y;

			if (axis == CellAxis.Horizontal)
			{
				x = startSecondary + i * (cellSize.x + spacingSecondary);
				y = posPrimary;
			}
			else
			{
				x = posPrimary;
				y = startSecondary + i * (cellSize.y + spacingSecondary);
			}

			SetChildAlongAxis(child, 0, x, cellSize.x);
			SetChildAlongAxis(child, 1, y, cellSize.y);
		}

		float delta = axis == CellAxis.Horizontal ? cellSize.y + spacingPrimary : cellSize.x + spacingPrimary;
		posPrimary += lineDirection == LineDirection.Forward ? delta : -delta;
	}

	private void CalculatePreferredSizes()
	{
		if (rectChildren.Count == 0) return;
		if (cellCount < 1) cellCount = 1;

		float containerPrimary = axis == CellAxis.Horizontal
			? rectTransform.rect.width - padding.left - padding.right
			: rectTransform.rect.height - padding.top - padding.bottom;

		float primaryCellSize = (containerPrimary - (cellCount - 1) * (axis == CellAxis.Horizontal ? spacing.x : spacing.y)) / cellCount;

		float maxSecondary = 0f;

		int currentLineCount = 0;
		List<RectTransform> lineChildren = new List<RectTransform>();

		foreach (var child in rectChildren)
		{
			lineChildren.Add(child);
			currentLineCount++;

			bool isLastInLine = currentLineCount >= cellCount || child == rectChildren[rectChildren.Count - 1];

			if (isLastInLine)
			{
				// 한 줄의 secondary 크기 계산
				float lineSecondary = 0f;
				foreach (var c in lineChildren)
				{
					float secondary = fitMode switch
					{
						CellFitMode.FixedSize => fixedSize,
						CellFitMode.AspectRatio => primaryCellSize * aspectRatio,
						CellFitMode.AutoFit => axis == CellAxis.Horizontal
						? (autoFitSource == AutoFitSource.PreferredSize
							? LayoutUtility.GetPreferredHeight(c)
							: LayoutUtility.GetMinHeight(c))
						: (autoFitSource == AutoFitSource.PreferredSize
							? LayoutUtility.GetPreferredWidth(c)
							: LayoutUtility.GetMinWidth(c)),
						_ => fixedSize
					};
					lineSecondary = Mathf.Max(lineSecondary, secondary);
				}

				maxSecondary += lineSecondary;
				if (child != rectChildren[rectChildren.Count - 1])
					maxSecondary += axis == CellAxis.Horizontal ? spacing.y : spacing.x;

				currentLineCount = 0;
				lineChildren.Clear();
			}
		}

		// ContentSizeFitter가 동작할 수 있게 LayoutGroup에 알림
		if (axis == CellAxis.Horizontal)
		{
			// 가로는 컨테이너 그대로, 세로는 maxSecondary
			SetLayoutInputForAxis(0, rectTransform.rect.width, 0, 0);   // Horizontal
			SetLayoutInputForAxis(maxSecondary + padding.vertical, maxSecondary + padding.vertical, 0, 1); // Vertical
		}
		else
		{
			// 세로는 컨테이너 그대로, 가로는 maxSecondary
			SetLayoutInputForAxis(maxSecondary + padding.horizontal, maxSecondary + padding.horizontal, 0, 0); // Horizontal
			SetLayoutInputForAxis(0, rectTransform.rect.height, 0, 1); // Vertical
		}
	}



#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		SetDirty();
	}
#endif

	protected void SetDirty()
	{
		if (!IsActive()) return;
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
	}
}
