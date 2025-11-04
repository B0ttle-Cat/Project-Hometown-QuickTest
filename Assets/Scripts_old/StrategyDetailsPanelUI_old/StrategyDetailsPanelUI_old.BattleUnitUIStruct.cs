using System;

using Sirenix.OdinInspector;

using UnityEngine;

public partial class StrategyDetailsPanelUI_old // BattleUnit UI
{
	[Serializable]
	public struct BattleUnitUIPrefab
	{
		public RectTransform overview;
	}
	[FoldoutGroup("유닛 정보 UI"), InlineProperty,PropertyOrder(9), HideLabel]
	public BattleUnitUIPrefab battleUnitUIPrefab;
	public class BattleUnitUIStruct : StrategyContentController
	{
        public BattleUnitUIStruct(StrategyDetailsPanelUI_old component) : base(component) {}
		BattleUnitUIPrefab UIPrefab => ThisComponent.battleUnitUIPrefab;
		public override void OnShow()
		{

		}
		public override void OnHide()
		{

		}
	}
}
