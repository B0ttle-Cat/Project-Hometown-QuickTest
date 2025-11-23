using System.Collections.Generic;

using Sirenix.OdinInspector;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static StrategyMapPanelUI.OperationLabelGroup;

public partial class StrategyMapPanelUI // OperationLabelGroup
{
	[SerializeField, FoldoutGroup("OperationLabel")]
	private GameObject operationLabelPreafab;
	[SerializeField, FoldoutGroup("OperationLabel")]
	private Transform operationLabelRoot;
	[SerializeField, FoldoutGroup("OperationLabel"), InlineProperty, HideLabel]
	private OperationLabelGroup operationLabelGroup;

	private void ShowOperationLabelGroup()
	{
		operationLabelGroup = new OperationLabelGroup(operationLabelPreafab, operationLabelRoot, this);
		operationLabelGroup.Show();
	}
	private void HideOperationLabelGroup()
	{
		if (operationLabelGroup == null) return;
		operationLabelGroup.Hide();
		operationLabelGroup.Dispose();
		operationLabelGroup = null;
	}
	private void OperationLabelGroupUpdate()
	{
		if (operationLabelGroup == null) return;
		operationLabelGroup.Update();
	}

	public class OperationLabelGroup : MapLabelGroup<OperationLabel>
	{
		HashSet<OperationObject> aliveOperation;
		public OperationLabelGroup(GameObject preafab, Transform root, StrategyMapPanelUI panel) : base(preafab, root, panel)
		{
			aliveOperation = new HashSet<OperationObject>();
		}

		protected override void OnDispose()
		{
			if (aliveOperation != null)
			{
				foreach (var item in aliveOperation)
				{
					if (item == null) continue;
					if (item is not IVisibilityEvent<OperationObject> visibility) continue;
					visibility.OnChangeVisible -= Operation_OnChangeVisible;
					visibility.OnChangeInvisible -= Operation_OnChangeInvisible;
				}
				aliveOperation.Clear();
				aliveOperation = null;
			}
			base.OnDispose();
		}

		protected override void OnHide()
		{
			StrategyManager.Collector.RemoveChangeListener<OperationObject>(OnChangeList);

		}

		protected override void OnShow()
		{
			StrategyManager.Collector.AddChangeListener<OperationObject>(OnChangeList, out List<OperationObject> operationList);
			if (operationList == null || operationList.Count == 0) return;

			int length = operationList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var operation = operationList[i];
				if (operation == null) continue;
				OnChangeList(operation, true);
			}
		}
		private void OnChangeList(IStrategyElement element, bool isAdded)
		{
			if (element == null || element is not OperationObject operation || operation == null) return;

			if (isAdded) OnChangeList_Add();
			else OnChangeList_Remove();
			void OnChangeList_Add()
			{
				if (operation is not IVisibilityEvent<OperationObject> visibility) return;
				if (!aliveOperation.Add(operation)) return;
				visibility.OnChangeVisible += Operation_OnChangeVisible;
				visibility.OnChangeInvisible += Operation_OnChangeInvisible;
				if (visibility.IsVisible) Operation_OnChangeVisible(operation);
				else Operation_OnChangeInvisible(operation);
			}
			void OnChangeList_Remove()
			{
				if (operation is not IVisibilityEvent<OperationObject> visibility) return;
				if (!aliveOperation.Remove(operation)) return;
				visibility.OnChangeVisible -= Operation_OnChangeVisible;
				visibility.OnChangeInvisible -= Operation_OnChangeInvisible;
				Operation_OnChangeInvisible(operation);
			}
		}

		private void Operation_OnChangeVisible(OperationObject operation)
		{
			if (operation == null) return;

			int length = LabelList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var label = LabelList[i];
				if (label != null && label.Operation == operation)
				{
					if (!label.IsShow && label is IViewItemUI view)
					{
						view.Visible();
						return;
					}
				}
			}
			{
				var newLabel = new OperationLabel(operation, PopLabelUiObject(), this);
				LabelList.Add(newLabel);
				if (newLabel is IViewItemUI view)
				{
					view.Visible();
				}
			}
		}

		private void Operation_OnChangeInvisible(OperationObject operation)
		{
			if (operation == null) return;

			int length = LabelList.Count;
			for (int i = 0 ; i < length ; i++)
			{
				var label = LabelList[i];
				if (label.Operation == operation)
				{
					if (label.IsShow && label is IViewItemUI view)
					{
						view.Invisible();
						return;
					}
				}
			}
		}

		public class OperationLabel : MapLabel
		{
			private readonly OperationObject operation;
			public OperationObject Operation => operation;

			private readonly TMP_Text nameText;
			private readonly Button select;
			private readonly Button showCloser;
			private readonly Button delete;
			private readonly Button goBack;
			private readonly Button play;
			private readonly Button pause;
			private readonly Button edit;
			private readonly Button merge;
			private readonly Button divide;
			private readonly GameObject tooltipUI;
			private readonly TMP_Text tooltipText;

			private readonly EventTrigger tooltipSelect;
			private readonly EventTrigger tooltipShowCloser;
			private readonly EventTrigger tooltipDelete;
			private readonly EventTrigger tooltipGoBack;
			private readonly EventTrigger tooltipPlay;
			private readonly EventTrigger tooltipPause;
			private readonly EventTrigger tooltipEdit;
			private readonly EventTrigger tooltipMerge;
			private readonly EventTrigger tooltipDivide;

			public OperationLabel(OperationObject operation, GameObject uiObject, MapLabelGroup<OperationLabel> group) : base(uiObject, group)
			{
				this.operation = operation;

				KeyPair
					.FindPairChain<TMP_Text>("NameText", out nameText)
					.FindPairChain<Button, EventTrigger>("Select", out select, out tooltipSelect)
					.FindPairChain<Button, EventTrigger>("ShowCloser", out showCloser, out tooltipShowCloser)
					.FindPairChain<Button, EventTrigger>("Delete", out delete, out tooltipDelete)
					.FindPairChain<Button, EventTrigger>("Back", out goBack, out tooltipGoBack)
					.FindPairChain<Button, EventTrigger>("Play", out play, out tooltipPlay)
					.FindPairChain<Button, EventTrigger>("Pause", out pause, out tooltipPause)
					.FindPairChain<Button, EventTrigger>("Edit", out edit, out tooltipEdit)
					.FindPairChain<Button, EventTrigger>("Merge", out merge, out tooltipMerge)
					.FindPairChain<Button, EventTrigger>("Divide", out divide, out tooltipDivide)
					.FindPairChain("TooltipUI", out tooltipUI)
					.FindPairChain<TMP_Text>("TooltipText", out tooltipText);
			}

			protected override void OnDispose()
			{
			}

			protected override void Invisible()
			{
				if (nameText != null) nameText.text = "";

				if (select != null) select.onClick.RemoveAllListeners();
				if (showCloser != null) showCloser.onClick.RemoveAllListeners();
				if (delete != null) delete.onClick.RemoveAllListeners();
				if (goBack != null) goBack.onClick.RemoveAllListeners();
				if (play != null) play.onClick.RemoveAllListeners();
				if (pause != null) pause.onClick.RemoveAllListeners();
				if (edit != null) edit.onClick.RemoveAllListeners();
				if (merge != null) merge.onClick.RemoveAllListeners();
				if (divide != null) divide.onClick.RemoveAllListeners();

				if (tooltipSelect != null) tooltipSelect.RemoveAllListeners();
				if (tooltipShowCloser != null) tooltipShowCloser.RemoveAllListeners();
				if (tooltipDelete != null) tooltipDelete.RemoveAllListeners();
				if (tooltipGoBack != null) tooltipGoBack.RemoveAllListeners();
				if (tooltipPlay != null) tooltipPlay.RemoveAllListeners();
				if (tooltipPause != null) tooltipPause.RemoveAllListeners();
				if (tooltipEdit != null) tooltipEdit.RemoveAllListeners();
				if (tooltipMerge != null) tooltipMerge.RemoveAllListeners();
				if (tooltipDivide != null) tooltipDivide.RemoveAllListeners();

				if (tooltipUI != null) tooltipUI.SetActive(false);

				if (operation != null)
				{
					operation.OnChangeUnitList -= Operation_OnChangeUnitList;
					operation.OnChangeMovementOrderState -= Operation_OnChangeMovementOrderState; 
				}
			}

			protected override void Visible()
			{
				if (nameText != null) nameText.text = operation.TeamName;

				if (select != null) select.onClick.AddListener(OnClick_Select);
				if (showCloser != null) showCloser.onClick.AddListener(OnClick_ShowCloser);
				if (delete != null) delete.onClick.AddListener(OnClick_Delete);
				if (goBack != null) goBack.onClick.AddListener(OnClick_GoBack);
				if (play != null) play.onClick.AddListener(OnClick_Play);
				if (pause != null) pause.onClick.AddListener(OnClick_Pause);
				if (edit != null) edit.onClick.AddListener(OnClick_Edit);
				if (merge != null) merge.onClick.AddListener(OnClick_Merge);
				if (divide != null) divide.onClick.AddListener(OnClick_Divide);

				if(tooltipSelect != null) tooltipSelect.AddListener((EventTriggerType.PointerEnter, (data) => ShowTooltip("부대 선택")),
					(EventTriggerType.PointerExit, (data) => HideTooltip()));

				if (tooltipShowCloser != null) tooltipShowCloser.AddListener((EventTriggerType.PointerEnter, (data) => ShowTooltip("확대하기")),
					(EventTriggerType.PointerExit, (data) => HideTooltip()));

				if (tooltipDelete != null) tooltipDelete.AddListener((EventTriggerType.PointerEnter, (data) => ShowTooltip("삭제")),
					(EventTriggerType.PointerExit, (data) => HideTooltip()));

				if (tooltipGoBack != null) tooltipGoBack.AddListener((EventTriggerType.PointerEnter, (data) => ShowTooltip("뒤로가기")),
					(EventTriggerType.PointerExit, (data) => HideTooltip()));

				if (tooltipPlay != null) tooltipPlay.AddListener((EventTriggerType.PointerEnter, (data) => ShowTooltip("재생")),
					(EventTriggerType.PointerExit, (data) => HideTooltip()));

				if (tooltipPause != null) tooltipPause.AddListener((EventTriggerType.PointerEnter, (data) => ShowTooltip("일시정지")),
					(EventTriggerType.PointerExit, (data) => HideTooltip()));

				if (tooltipEdit != null) tooltipEdit.AddListener((EventTriggerType.PointerEnter, (data) => ShowTooltip("편집")),
					(EventTriggerType.PointerExit, (data) => HideTooltip()));

				if (tooltipMerge != null) tooltipMerge.AddListener((EventTriggerType.PointerEnter, (data) => ShowTooltip("병합")),
					(EventTriggerType.PointerExit, (data) => HideTooltip()));

				if (tooltipDivide != null) tooltipDivide.AddListener((EventTriggerType.PointerEnter, (data) => ShowTooltip("분할")),
					(EventTriggerType.PointerExit, (data) => HideTooltip()));

				if (operation != null)
				{
					operation.OnChangeUnitList += Operation_OnChangeUnitList;
                    operation.OnChangeMovementOrderState += Operation_OnChangeMovementOrderState;
					Operation_OnChangeUnitList(operation);
					Operation_OnChangeMovementOrderState(operation);
				}
			}
		
			private void Operation_OnChangeUnitList(OperationObject operation)
			{
				if (operation == null) return;
				if (FloatingPanelUI == null || FloatingPanelUI is not CenterFloatingPanelItemUI centerFloating) return;
				
				centerFloating.SetTargetInMap(operation.GetAllUnitTr.ToArray());
			}
			private void Operation_OnChangeMovementOrderState(OperationObject operation)
			{
				operation.ThisController.OnMovementOrder_AvailableType(out bool play, out bool pause, out bool goBack);

				if (this.goBack != null) this.goBack.gameObject.SetActive(goBack);
				if (this.play != null) this.play.gameObject.SetActive(play);
				if (this.pause != null) this.pause.gameObject.SetActive(pause);
			}
			private void OnClick_Delete()
			{
				// 팝업 메니저에서 먼저 띄워주고 정말 삭제할 건지 물어본다.
				//그뒤 확인되면 호출한다.
				Operation.ThisController.DeleteThis();
			}
			private void OnClick_GoBack() => Operation.ThisController.OnMovementOrder_Cancel();
			private void OnClick_Play() => Operation.ThisController.OnMovementOrder_Execute();
			private void OnClick_Pause() => Operation.ThisController.OnMovementOrder_Pause();
			private void OnClick_Edit()
			{
				// 우선 SpawnTroopsInfo 를 설정 할수 있는 UI 를 띄운다.
				// 그리고 그 결과를 SpawnTroopsInfo 로 받으면 
				// SpawnTroopsInfo 만큼 편제를 변경.
				SpawnTroopsInfo edit = default;
				if (Operation.ThisController.OnOrganization_CheckValid(in edit))
					if (Operation.ThisController.OnOrganization_Edit(in edit))
					{
						// 편집 성공
					}
			}
			private void OnClick_Merge()
			{
				// 합치기 가능한 (동일한 위치에 있는 부대) 작전을먼저 보여주고 
				// UI 를 통해 합쳐질 편제를 계산한다.
				// 선택된 부대를 제외한 다른 부재는 삭제한다.
				SpawnTroopsInfo merge = default;
				if (Operation.ThisController.OnOrganization_CheckValid(in merge))
					if (Operation.ThisController.OnOrganization_Merge(in merge))
					{
						// 합치기 성공
					}
			}
			private void OnClick_Divide()
			{
				// 우선 SpawnTroopsInfo 를 설정 할수 있는 UI 를 띄운다.
				// 그리고 그 결과를 SpawnTroopsInfo 로 받으면 
				// SpawnTroopsInfo 만큼 분리된 새로운 부대를 생성.
				SpawnTroopsInfo divide = default;
				if (Operation.ThisController.OnOrganization_CheckValid(in divide))
					if (Operation.ThisController.OnOrganization_Divide(in divide))
					{
						// 나누기 성공
					}
			}
			private void OnClick_ShowCloser() => Operation.ThisController.OnShowCloser();
			private void OnClick_Select() => Operation.ThisController.On_SelectLabel();
			// 툴팁 이벤트
			private void ShowTooltip(string text)
			{
				if (tooltipUI == null || tooltipText == null) return;
				tooltipUI.SetActive(true);
				tooltipText.text = text;
			}

			private void HideTooltip()
			{
				if (tooltipUI == null || tooltipText == null) return;
				tooltipUI.SetActive(false);
				tooltipText.text = "";
			}
		}

	}
}