namespace Ngaq.Test.Nodes;

using Tsinswreng.CsTest;

/// <summary>
/// 根测试节点，包含所有测试树的入口
/// </summary>
public class RootTestNode : TestNodeBase {
	public override str Name => "All Tests";
	
	public RootTestNode() {
		// 组织结构：按功能模块分组
		var CsSql = new TestGroupNode("CsSql");
		CsSql.AddChild(new RepoTestNode());
		AddChild(CsSql);
		
		AddChild(new WordTestNode());
		AddChild(new JsonTestNode());
		AddChild(new CsLangTestNode());
		AddChild(new ToolTestNode());
		AddChild(new TryTestNode());
		AddChild(new SqlTestNode());
	}
}
