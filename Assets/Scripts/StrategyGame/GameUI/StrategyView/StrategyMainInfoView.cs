using System;

using GameUI;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;

using static StrategyMainInfoView;

[RequireComponent(typeof(CanvasGroupUI))]
public partial class StrategyMainInfoView : PanelGroupComponent<MainInfoView>
{

	protected override void Reset()
	{
		InitContents();
	}
	protected override void Awake()
	{
		base.Awake();
		InitContents();
	}

	void InitContents()
	{
		this.New();
		this.Clear();

		if (ShowMainMission)
		{
			this.Add(mainScenario);
			InitMainScenario();
		}
		if (ShowMainMission)
		{
			this.Add(mainMission);
			InitMainMission();
		}
		if (ShowSubMission)
		{
			this.Add(subMissions);
			InitSubMissions();
		}
	}
	protected override void Hide()
	{

	}
	protected override void Show()
	{

	}

	partial void InitMainScenario();
	partial void InitMainMission();
	partial void InitSubMissions();

	[Serializable]
	public abstract class MainInfoView : IPanelItem, IDisposable
	{
		[SerializeField]
		private RectTransform thisRect;
		private GameUIController root;
		public IPanelItem ThisPanel => this;
		RectTransform IPanelItem.ThisRect => thisRect;
		GameUIController IPanelItem.RootUI => root.IsNotNullRef() ? root : root = ThisPanel.FindRoot();

		public void OnUpdateView<T>(T data) where T : class
		{
			if (ThisPanel.ThisRect.IsNullRef()) return;
			if (data.IsNullRef()) return;
			UpdateView(data);
		}
		protected abstract void UpdateView<T>(T data) where T : class;
		public virtual void Dispose()
		{
			thisRect = null;
			root = null;
		}
	}
}
#region ScenarioView
public partial class StrategyMainInfoView // ScenarioView
{
	[Serializable]
	public class ScenarioView : MainInfoView
	{



		[SerializeField] private TMP_Text titleTextUI;
		[SerializeField] private TMP_Text bodyTextUI;

		public override void Dispose()
		{
		}

		protected override void UpdateView<T>(T data) where T : class
		{

		}
	}

	[SerializeField, ToggleGroup("ShowMainScenario", "MainScenario", CollapseOthersOnExpand = false)]
	private bool ShowMainScenario;
	[SerializeField, ToggleGroup("ShowMainScenario"), InlineProperty, HideLabel]
	private ScenarioView mainScenario;
	partial void InitMainScenario()
	{
		//StrategyManager.Scenario
	}
}
#endregion

#region ScenarioView
public partial class StrategyMainInfoView // MainMission
{
	[Serializable]
	public class MissionView : MainInfoView
	{
		[SerializeField] private TMP_Text bodyTextUI;
		protected override void UpdateView<T>(T data) where T : class
		{

		}
	}
	[SerializeField, ToggleGroup("ShowMainMission", "MainMission", CollapseOthersOnExpand = false)]
	private bool ShowMainMission;
	[SerializeField, ToggleGroup("ShowMainMission"), InlineProperty, HideLabel]
	private MissionView mainMission;

	[SerializeField, ToggleGroup("ShowSubMission", "SubMissions", CollapseOthersOnExpand = false)]
	private bool ShowSubMission;
	[SerializeField, ToggleGroup("ShowSubMission"), InlineProperty, HideLabel]
	private MissionView subMissions;



	partial void InitMainMission()
	{

	}
	partial void InitSubMissions()
	{

	}
}
#endregion