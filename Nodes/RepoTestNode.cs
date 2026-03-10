namespace Ngaq.Test.Nodes;

using Ngaq.Test.CsSqlHelper.Integration.Repo;
using Tsinswreng.CsTest;

/// <summary>
/// Repo 相关测试节点
/// </summary>
public class RepoTestNode : TestNodeBase {
	public override str Name => "Repo Tests";
	
	protected override async Task RegisterOwnTests(TestFixture Fixture, CT Ct) {
		var testRepo = NgaqTest.GetRSvc<TestRepo>();
		Fixture.Register(
			$"{Name}::{nameof(TestRepo.TestBatSlctAggById)}",
			(Obj) => testRepo.TestBatSlctAggById(Obj, Ct)
		);
		
		// TODO: 添加更多 Repo 相关的测试
		// Fixture.Register(...);
		
		await Task.CompletedTask;
	}
}
