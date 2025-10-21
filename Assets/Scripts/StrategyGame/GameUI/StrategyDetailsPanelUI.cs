using System;

using Sirenix.OdinInspector;

using UnityEngine;

public partial class StrategyDetailsPanelUI : DetailsPanelUI
{
#if UNITY_EDITOR
	[ShowInInspector, InlineButton(nameof(_Select),"Open UI"),  HideLabel]
	private StrategyDetailsPanelType _select;
	private void _Select() => OpenUI(_select);
#endif

	public enum StrategyDetailsPanelType
	{
		None,
		Menu = 10,

		[HideInInspector]
		FieldInfo = 100,
		FieldInfo_Overview,
		FieldInfo_MainMission,
		FieldInfo_Statistics,
		FieldInfo_Storyboard,

		[HideInInspector]
		ControlBase = 200,
		ControlBase_Info,
		ControlBase_Support,
		ControlBase_Facilities,
		ControlBase_Garrison,

		[HideInInspector]
		BattleUnit = 300,

		[HideInInspector]
		BattleSkill= 400,
	}


	public StrategyContentController currentDetailsPanelItem;

	public void OpenUI(StrategyDetailsPanelType openContent)
	{
		ClearAllContent();
		Init();

		currentDetailsPanelItem = openContent switch
		{
			StrategyDetailsPanelType.Menu => new MainGameMenuUI(this),
			>= StrategyDetailsPanelType.FieldInfo and < StrategyDetailsPanelType.ControlBase
				=> new FieldInfoViewController(this),
			>= StrategyDetailsPanelType.ControlBase and < StrategyDetailsPanelType.BattleUnit
				=> new ControlBaseUIStruct(this),
			>= StrategyDetailsPanelType.ControlBase and < StrategyDetailsPanelType.BattleSkill
				=> new BattleUnitUIStruct(this),
			>= StrategyDetailsPanelType.BattleSkill
				=> new BattleSkillUIStruct(this),
			_ => null,
		};

		if (currentDetailsPanelItem != null)
		{
			currentDetailsPanelItem.initContent = openContent;
			currentDetailsPanelItem.OnShow();
		}
	}
	public void CloseUI()
	{
		if (currentDetailsPanelItem != null)
		{
			currentDetailsPanelItem.OnHide();
		}

		ClearAllContent();
	}

	[Serializable]
	public abstract class StrategyContentController : ContentController
	{
        public StrategyDetailsPanelUI ThisComponent => component as StrategyDetailsPanelUI;
		public StrategyDetailsPanelType initContent;

        protected StrategyContentController(StrategyDetailsPanelUI component) : base(component)
        {
        }

        public void OnShow(StrategyDetailsPanelType openContent)
		{
			initContent = openContent;
			OnShow();
		}
	}
    [Serializable]
    public abstract class StrategyViewController : ViewController
	{
		public StrategyDetailsPanelUI ThisComponent => component as StrategyDetailsPanelUI;
		public StrategyContentController ThisViewController => viewController  as StrategyContentController;
    }
    public class MainGameMenuUI : StrategyContentController
	{
        public MainGameMenuUI(StrategyDetailsPanelUI component) : base(component)
        {
        }

        public override void OnShow()
		{

		}
		public override void OnHide()
		{
		}
    }
}