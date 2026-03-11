namespace Ngaq.Test.Nodes;

using Ngaq.Test.Word;
using Tsinswreng.CsTest;

/// <summary>
/// Word 相关测试节点
/// </summary>
public class WordTestNode : TestNodeBase {
	public override str Name => "Word Tests";
	
	protected override async Task RegisterOwnTests(TestFixture Fixture, CT Ct) {
		// TODO: 添加 Word 相关的测试
		// var testWord = Program.GetRSvc<TestWord>();
		// Fixture.Register(...);
		
		await Task.CompletedTask;
	}
}
