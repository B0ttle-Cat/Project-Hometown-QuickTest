using System;

using Sirenix.OdinInspector;

using UnityEngine;

public partial class StrategyDetailsPanelUI // BattleSkill UI
{
	[Serializable]
	public struct BattleSkillUIPrefab
	{
		public RectTransform overview;
	}
	[FoldoutGroup("기술 정보 UI"), InlineProperty, HideLabel]
	public BattleSkillUIPrefab battleSkillUIPrefab;
	public class BattleSkillUIStruct : StrategyContentController
	{
		public BattleSkillUIStruct(StrategyDetailsPanelUI component) : base(component) { }
		BattleSkillUIPrefab UIPrefab => ThisComponent.battleSkillUIPrefab;
		public override void OnShow()
		{

		}
		public override void OnHide()
		{

		}
	}
}