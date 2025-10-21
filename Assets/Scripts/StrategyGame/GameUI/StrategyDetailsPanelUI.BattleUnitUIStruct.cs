using System;

using Sirenix.OdinInspector;

using UnityEngine;

public partial class StrategyDetailsPanelUI // BattleUnit UI
{
	[Serializable]
	public struct BattleUnitUIPrefab
	{
		public RectTransform overview;
	}
	[FoldoutGroup("유닛 정보 UI"), InlineProperty, HideLabel]
	public BattleUnitUIPrefab battleUnitUIPrefab;
	public class BattleUnitUIStruct : StrategyContentController
	{
        public BattleUnitUIStruct(StrategyDetailsPanelUI component) : base(component) {}
		BattleUnitUIPrefab UIPrefab => ThisComponent.battleUnitUIPrefab;
		public override void OnShow()
		{

		}
		public override void OnHide()
		{

		}
	}
}
