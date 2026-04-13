using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Infra;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan{
	void RegisterSyncPreFilter(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcStudyPlan),
			[typeof(ISvcStudyPlan)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcStudyPlan.SyncPreFilter)];

		R("SyncPreFilter_Should_InsertAndUpdate_ById", async(o)=>{
			var ctx = MkUserCtx(_ownerA);
			var token = _token + "_sync_pf_" + Guid.NewGuid().ToString("N");
			var add = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = token + "_add",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};
			var upd = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = _ownerA,
				UniqName = token + "_upd",
				Type = EPreFilterType.Json,
				DataSchemaVer = new Version(1, 0),
				Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[],\"PropFilter\":[]}",
			};

			await RunNoTxn(async(db)=>{
				await RepoPreFilter.BatAdd(db, AsyE(upd), CT.None);
				return NIL;
			});
			_preFilterIds.Add(add.Id);
			_preFilterIds.Add(upd.Id);

			upd.Descr = "updated";
			upd.Text = "{\"Version\":\"1.0.0.0\",\"CoreFilter\":[{\"Fields\":[\"Lang\"],\"Filters\":[]}],\"PropFilter\":[]}";
			upd.BizUpdatedAt = Tempus.Now();
			await SvcStudyPlan.SyncPreFilter(ctx, AsyE(add, upd), CT.None);

			await RunNoTxn(async(db)=>{
				var got = await ToList(RepoPreFilter.BatGetByIdWithDel(db, AsyE(add.Id, upd.Id), CT.None));
				if(got.Count != 2 || got.Any(x=>x is null)){
					throw new Exception("SyncPreFilter should keep both insert and update targets");
				}
				var addGot = got.First(x=>x!.Id == add.Id)!;
				var updGot = got.First(x=>x!.Id == upd.Id)!;
				if(addGot.Text != add.Text){
					throw new Exception("SyncPreFilter should insert missing row");
				}
				if(updGot.Text != upd.Text || updGot.Descr != upd.Descr){
					throw new Exception("SyncPreFilter should update existing row");
				}
				return NIL;
			});
			return NIL;
		});
	}
}

