using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;

using static StrategyGamePlayData;
using static StrategyGamePlayData.MissionTreeData;

public partial class StrategyMissionTree : MonoBehaviour, IDisposable
{
	#region Structs
	public interface ITreeBuilder
	{
		public GroupNode Root { get; set; }
		public GroupNode CurrentGroup => GroupStack.Peek();
		public Stack<GroupNode> GroupStack { get; set; }

		public ITreeBuilder AddItem(int indent, ItemStruct missionStruct, bool alwaysCheck = false, bool enable = true)
		{
			MissionComputer missionComputer = missionStruct.missionType switch
			{
				MissionType.Kill => new Kill_Mission(),
				MissionType.Protect => new Protect_Mission(),
				MissionType.ControlBase_Count => new ControlBase_Count_Mission(),
				MissionType.CaptureAndSecureBase  => new CaptureAndSecureBase_Mission(),
				_ => null,
			};

			CurrentGroup.children.Add(new ItemNode(indent, "", missionStruct, missionComputer)
			{
				enable = enable,
				isAlwaysCheck = alwaysCheck,
			});

			return this;
		}
		public ITreeBuilder AddItem(int indent, string description, ItemStruct missionStruct, Func<ItemStruct, ResultTyoe> condition, bool alwaysCheck = false, bool enable = true)
		{
			var nextNode = new ItemNode(indent, description, missionStruct, new CustomFunction_Mission(description, condition))
			{
				enable = enable,
				isAlwaysCheck = alwaysCheck,
			};
			CurrentGroup.children.Add(nextNode);

			return this;
		}

		public ITreeBuilder EnterGroup(int indent, GroupStruct groupStruct, bool alwaysCheck = false, bool enable = true)
		{
			var groupNode = new GroupNode(indent,groupStruct)
			{
				enable = enable,
				isAlwaysCheck = alwaysCheck,
			};
			CurrentGroup.children.Add(groupNode);
			GroupStack.Push(groupNode);
			return this;
		}
		public ITreeBuilder ExitGroup()
		{
			if (GroupStack.Count > 1)
			{
				GroupStack.Pop();
			}
			return this;
		}

		public ITreeBuilder MoveRoot()
		{
			GroupStack.Clear();
			GroupStack.Push(Root);
			return this;
		}
	}
	[Serializable]
	public class MissionTreeBuilder : ITreeBuilder
	{
		private GroupNode root;
		private Stack<GroupNode> groupStack;
		public GroupNode Root { get => root; set => root = value; }
		public Stack<GroupNode> GroupStack { get => groupStack; set => groupStack = value; }

		public MissionTreeBuilder()
		{
			root = null;
			groupStack = new Stack<GroupNode>();
		}
		public ITreeBuilder Start(GroupStruct groupStruct)
		{
			root = new GroupNode(0, groupStruct);
			groupStack.Push(root);
			return this;
		}
		public MissionTree Build(string id, string name, string description)
		{
			var tree = new MissionTree(id, name, description, Root);
			root = null;
			groupStack.Clear();
			return tree;
		}
	}
	[Serializable]
	public class MissionTree : IDisposable
	{
		public string id;
		public string name;
		public string description;
		public Node node;

		public MissionTree(string id, string name, string description, Node node)
		{
			this.id = id;
			this.name = name;
			this.description = description;
			this.node = node;
		}

		public void Dispose()
		{
			if (node != null)
			{
				node.Dispose();
				node = null;
			}
		}

		public void Foreach(Action<Node> callbackNode, bool onylEnable = true)
		{
			ForeachNode(node);
			void ForeachNode(Node _node)
			{
				if (onylEnable && !_node.enable) return;

				if (_node is ItemNode itemNode)
				{
					callbackNode(itemNode);
				}
				else if (_node is GroupNode groupNode)
				{
					callbackNode.Invoke(groupNode);
					foreach (var item in groupNode.children)
					{
						ForeachNode(item);
					}
				}
			}
		}
	}
	[Serializable]
	public abstract class Node : IDisposable
	{
		public virtual string Description => description;

		public bool enable;
		public ResultTyoe resultTyoe;
		public bool isAlwaysCheck;

		public int indent;
		public string description;

		protected Node()
		{
			enable = true;
			resultTyoe = ResultTyoe.Wait;
			isAlwaysCheck = false;
		}

		public abstract ResultTyoe IsCmplete();
		public abstract void Dispose();
	}
	[Serializable]
	public class ItemNode : Node
	{
		public ItemStruct missionStruct;
		public MissionComputer missionComputer;
		public ItemNode(int indent, string description, ItemStruct missionStruct, MissionComputer missionComputer) : base()
		{
			this.indent = indent;
			this.description = description;
			this.missionStruct = missionStruct;
			this.missionComputer = missionComputer;
		}
		public override string Description => string.IsNullOrWhiteSpace(description)
			? missionComputer.ConverToText(in missionStruct)
			: description;
		public override ResultTyoe IsCmplete()
		{
			if (enable || missionComputer == null) return ResultTyoe.Succeed;
			if (!isAlwaysCheck && resultTyoe != ResultTyoe.Wait) return resultTyoe;

			return missionComputer.Compute(missionStruct);
		}
		public override void Dispose()
		{
			missionStruct.Dispose();

			if (missionComputer != null)
			{
				missionComputer.Dispose();
				missionComputer = null;
			}
		}
	}
	[Serializable]
	public class GroupNode : Node
	{
		public GroupNode(int indent, GroupStruct missionStruct) : base()
		{
			this.indent = indent;
			this.description = "";
			this.missionStruct = missionStruct;
			children = new List<Node>();
		}
		public GroupNode(int indent, string description, GroupStruct missionStruct) : base()
		{
			this.indent = indent;
			this.description = description;
			this.missionStruct = missionStruct;
			children = new List<Node>();
		}

		public GroupStruct missionStruct;
		public List<Node> children;

		public override string Description => string.IsNullOrWhiteSpace(description)
			? missionStruct.logicType switch
			{
				LogicType.All => "하위의 모든 조건을 완료하세요.",
				LogicType.Any => $"하위의 아무 조건을 {missionStruct.anyCount}개 완료하세요.",
				_ => ""
			}
			: description;

		public override ResultTyoe IsCmplete()
		{
			if (enable) return ResultTyoe.Succeed;
			if (!isAlwaysCheck && resultTyoe != ResultTyoe.Wait) return resultTyoe;

			return resultTyoe = missionStruct.logicType switch
			{
				LogicType.All => All(),
				LogicType.Any => Any(missionStruct.anyCount),
				_ => Any(1),
			};
		}
		private void CountChildResult(out int total, out int wait, out int succeed, out int failed)
		{
			int length = children.Count;
			total = 0;
			wait = 0;
			succeed = 0;
			failed = 0;

			for (int i = 0 ; i < length ; i++)
			{
				var child = children[i];
				if (child == null)
				{
					continue;
				}

				ResultTyoe childResult = ResultTyoe.Wait;
				try
				{
					childResult = child.IsCmplete();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					continue;
				}
				total++;
				if (childResult != ResultTyoe.Succeed) succeed++;
				else if (childResult != ResultTyoe.Failed) failed++;
				else wait++;
			}
		}
		private ResultTyoe All()
		{
			CountChildResult(out int total, out int wait, out int succeed, out int failed);
			return succeed == total ? ResultTyoe.Succeed : failed > 0 ? ResultTyoe.Failed : ResultTyoe.Wait;
		}
		private ResultTyoe Any(int count)
		{
			CountChildResult(out int total, out int wait, out int succeed, out int failed);
			if (total < count) count = total;
			return succeed >= count ? ResultTyoe.Succeed : failed > total - count ? ResultTyoe.Failed : ResultTyoe.Wait;
		}

		public override void Dispose()
		{
			missionStruct.Dispose();

			if (children != null)
			{
				foreach (var item in children)
				{
					item.Dispose();
				}
				children.Clear();
				children = null;
			}
		}
	}
	public struct MissionParser
	{
		public enum CommandType
		{
			StartGroup,
			EnterGroup,
			ExitGroup,
			AddItem
		}

		[Serializable]
		public struct Command
		{
			public CommandType type;
			public int indent;

			public MissionTreeData.ItemStruct itemStruct;
			public MissionTreeData.GroupStruct groupStruct;

			public Command(int indent, MissionTreeData.ItemStruct itemStruct)
			{
				this.type = CommandType.AddItem;
				this.indent = indent;
				this.itemStruct = itemStruct;
				this.groupStruct = default;
			}
			public Command(CommandType type, int indent, MissionTreeData.GroupStruct groupStruct)
			{
				this.type = type;
				this.indent = indent;
				this.itemStruct = default;
				this.groupStruct = groupStruct;
			}
			public static Command ExitGroup => new Command()
			{
				type = CommandType.ExitGroup,
				indent = 0,
				groupStruct = default,
				itemStruct = default,
			};
		}

		private static readonly Regex lineRegex = new Regex(
				@"^(?<indent>\t*)(?<keyword>\w+)(?:,\s*(?:(?<capture>\d+)\s*(?<compare><=|>=|==|!=|<|>)\s*,\s*)?(?<args>.*))?$",
				RegexOptions.Compiled);

		public static List<Command> ParseLines(string text)
		{
			var commands = new List<Command>();

			if (string.IsNullOrWhiteSpace(text))
				return commands;

			var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			int lastIndent = 0;

			foreach (var rawLine in lines)
			{
				var match = lineRegex.Match(rawLine);
				if (!match.Success) continue;

				int indent = match.Groups["indent"].Value.Length;
				string keyword = match.Groups["keyword"].Value.Trim();
				string valueStr = match.Groups["capture"].Value;
				string compareStr = match.Groups["compare"].Value;
				string argsRaw = match.Groups["args"].Value?.Trim() ?? "";
				string[] args = SplitArgs(argsRaw);

				if (!int.TryParse(valueStr, out int value))
				{
					value = 1;
				}
				var conditionType = ParseComparisonType(compareStr);

				// 들여쓰기 깊이가 바뀌면 그룹의 진입/종료를 알림
				if (indent > lastIndent)
				{
					// deeper = enter group
					// handled implicitly when next group starts
				}
				else if (indent < lastIndent)
				{
					// closing groups
					int diff = lastIndent - indent;
					for (int i = 0 ; i < diff ; i++)
						commands.Add(Command.ExitGroup);
				}

				if (IsGroup(keyword, out var logicType))
				{
					if (commands.Count == 0)
					{
						commands.Add(new Command(CommandType.StartGroup, indent, new MissionTreeData.GroupStruct()
						{
							logicType = logicType,
							anyCount = value,
							anyComparisonType = conditionType,
						}));
					}
					else
					{
						commands.Add(new Command(CommandType.EnterGroup, indent, new MissionTreeData.GroupStruct()
						{
							logicType = logicType,
							anyCount = value,
							anyComparisonType = conditionType,
						}));
					}
				}
				else if (IsItem(keyword, out var missiontype))
				{
					commands.Add(new Command(indent, new MissionTreeData.ItemStruct()
					{
						missionType = missiontype,
						targets = args,
						count = value,
						comparisonType = conditionType,
					}));
				}

				lastIndent = indent;
			}

			// 파일 끝난 후 남은 그룹 닫기
			for (int i = 0 ; i < lastIndent ; i++)
				commands.Add(Command.ExitGroup);

			return commands;
		}

		private static MissionTreeData.ComparisonType ParseComparisonType(string op)
		{
			// 10 op N
			return op switch
			{
				"==" => MissionTreeData.ComparisonType.동등,
				">=" => MissionTreeData.ComparisonType.이하,
				"<=" => MissionTreeData.ComparisonType.이상,
				_ => MissionTreeData.ComparisonType.이상
			};
		}
		private static string[] SplitArgs(string input)
		{
			if (string.IsNullOrEmpty(input)) return Array.Empty<string>();
			return input.Split(',').Select(s => s.Trim().Trim('"')).ToArray();
		}
		private static bool IsGroup(string keyword, out MissionTreeData.LogicType logicType)
		{
			if (keyword.StartsWith(nameof(MissionTreeData.LogicType.All), StringComparison.OrdinalIgnoreCase))
			{
				logicType = MissionTreeData.LogicType.All;
				return true;
			}
			else if (keyword.StartsWith(nameof(MissionTreeData.LogicType.Any), StringComparison.OrdinalIgnoreCase))
			{
				logicType = MissionTreeData.LogicType.Any;
				return true;
			}
			else
			{
				logicType = default;
				return false;
			}
		}
		private static bool IsItem(string keyword, out MissionTreeData.MissionType missionType)
		{
			if (keyword.Equals(nameof(MissionTreeData.MissionType.Kill), StringComparison.OrdinalIgnoreCase))
			{
				missionType = MissionTreeData.MissionType.Kill;
				return true;
			}
			else if (keyword.Equals(nameof(MissionTreeData.MissionType.Protect), StringComparison.OrdinalIgnoreCase))
			{
				missionType = MissionTreeData.MissionType.Protect;
				return true;
			}
			else if (keyword.Equals(nameof(MissionTreeData.MissionType.ControlBase_Count), StringComparison.OrdinalIgnoreCase))
			{
				missionType = MissionTreeData.MissionType.ControlBase_Count;
				return true;
			}
			else if (keyword.Equals(nameof(MissionTreeData.MissionType.CaptureAndSecureBase), StringComparison.OrdinalIgnoreCase))
			{
				missionType = MissionTreeData.MissionType.CaptureAndSecureBase;
				return true;
			}
			else
			{
				missionType = default;
				return false;
			}
		}

		public const string testParserData = @"
All
	Kill, 10 <=, ""UnitA"",""UnitB"",""UnitC"",""UnitD""
	Any
		Kill, ""Unit_99""
		All
			Kill, ""Unit_01""
			Kill, ""Unit_02""
			Kill, 3 ==, ""Unit_03""
	Any, 2 <=
		Kill, ""Unit_05""
		Kill, ""Unit_06""
		Kill, ""Unit_07""
";
	}
	#endregion

	private MissionTree victoryMission;
	private MissionTree defeatMission;
	private Dictionary<string, MissionTree> missionTreeList;

	internal void Init()
	{
		missionTreeList = new Dictionary<string, MissionTree>();
	}
	public MissionTree NewMissionTree(string id, List<MissionParser.Command> commands, Action<MissionTreeBuilder, List<MissionParser.Command>> builder)
	{
		return NewMissionTree(id, "", "", commands, builder);
	}
	public MissionTree NewMissionTree(string id, string name, string description, List<MissionParser.Command> commands, Action<MissionTreeBuilder, List<MissionParser.Command>> builder)
	{
		if (builder == null) return null;

		var treeBuilder = new MissionTreeBuilder();
		builder.Invoke(treeBuilder, commands);

		MissionTree missionTree = treeBuilder.Build(id, name, description);
		missionTreeList.Add(id, missionTree);

		return missionTree;
	}
	public MissionTree FindMission(string missionID)
	{
		return missionTreeList.TryGetValue(missionID, out var missionTree) ? missionTree : null;
	}
	public List<MissionTree> GetMissionList(bool withoutMain = true)
	{
		var newMissionTreeList= new List<MissionTree>();
		if (!withoutMain)
		{
			if (victoryMission != null) newMissionTreeList.Add(victoryMission);
			if (defeatMission != null) newMissionTreeList.Add(defeatMission);
		}
		newMissionTreeList.AddRange(missionTreeList.Values.ToArray());
		return newMissionTreeList;
	}

	public void Dispose()
	{
		if (victoryMission != null)
		{
			victoryMission.Dispose();
			victoryMission = null;
		}
		if (defeatMission != null)
		{
			defeatMission.Dispose();
			defeatMission = null;
		}
		if (missionTreeList != null)
		{
			foreach (var item in missionTreeList)
			{
				if (item.Value == null) continue;
				item.Value.Dispose(); 
			}
			missionTreeList.Clear();
			missionTreeList = null;
		}
	}
}
public partial class StrategyMissionTree // _Init GamePlay
{
	public MissionTree VictoryMission => victoryMission;
	public MissionTree DefeatMission => defeatMission;

	public void InitMainMission()
	{
		var Data = PreparedData.GetData();
		var missionData = Data.mission;
		var victoryCommandList = MissionParser.ParseLines(missionData.victoryScript);
		var defeatCommandList = MissionParser.ParseLines(missionData.defeatScript);

		string MainMissionID = missionData.id;
		string victoryId = $"{MainMissionID}_Victory";
		string defeatId = $"{MainMissionID}_Defeat";
		victoryMission = StrategyManager.Mission.NewMissionTree(victoryId, missionData.title, missionData.description, victoryCommandList, MissionBuild);
		defeatMission = StrategyManager.Mission.NewMissionTree(defeatId, missionData.title, missionData.description, defeatCommandList, MissionBuild);

		void MissionBuild(MissionTreeBuilder builder, List<MissionParser.Command> commandList)
		{
			int length = commandList.Count;
			ITreeBuilder treeBuilder = null;
			for (int i = 0 ; i < length ; i++)
			{
				var command = commandList[i];
				int indent = command.indent;
				treeBuilder = command.type switch
				{
					MissionParser.CommandType.StartGroup => builder.Start(command.groupStruct),
					MissionParser.CommandType.EnterGroup => treeBuilder?.EnterGroup(indent, command.groupStruct),
					MissionParser.CommandType.ExitGroup => treeBuilder?.ExitGroup(),
					MissionParser.CommandType.AddItem => treeBuilder?.AddItem(indent, command.itemStruct),
					_ => treeBuilder?.ExitGroup(),
				};
			}
		}
	}

	public void InitSubMission()
	{
		var Data = PreparedData.GetData();
		var missionData = Data.mission;
		var subMissions = missionData.enableSubMissions;

		int length = subMissions.Length;
		for (int i = 0 ; i < length ; i++)
		{
			var subMission = subMissions[i];
			string id = subMission.id;
			if (string.IsNullOrWhiteSpace(id)) continue;

			var commands = MissionParser.ParseLines(subMission.missionScript);
			var missionTree = StrategyManager.Mission.NewMissionTree(id, commands , MissionBuild);
			if (missionTree == null) continue;

			missionTreeList.Add(id, missionTree);
		}

		void MissionBuild(MissionTreeBuilder builder, List<MissionParser.Command> commandList)
		{
			int length = commandList.Count;
			ITreeBuilder treeBuilder = null;
			for (int i = 0 ; i < length ; i++)
			{
				var command = commandList[i];
				int indent = command.indent;
				treeBuilder = command.type switch
				{
					MissionParser.CommandType.StartGroup => builder.Start(command.groupStruct),
					MissionParser.CommandType.EnterGroup => treeBuilder?.EnterGroup(indent, command.groupStruct),
					MissionParser.CommandType.ExitGroup => treeBuilder?.ExitGroup(),
					MissionParser.CommandType.AddItem => treeBuilder?.AddItem(indent, command.itemStruct),
					_ => treeBuilder?.ExitGroup(),
				};
			}
		}
	}
}