using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Models.Req;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Local.Db.TswG;
using Ngaq.Local.Domains.StudyPlan.Dao;
using Ngaq.Local.Domains.StudyPlan.Svc;
using Tsinswreng.CsPage;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public class TestISvcStudyPlan:ITester{
	ISqlCmdMkr SqlCmdMkr;
	SvcStudyPlan SvcStudyPlan;
	IRepo<PoStudyPlan, IdStudyPlan> RepoStudyPlan;
	IRepo<PoWeightArg, IdWeightArg> RepoWeightArg;
	IRepo<PoWeightCalculator, IdWeightCalculator> RepoWeightCalculator;
	IRepo<PoPreFilter, IdPreFilter> RepoPreFilter;

	IdUser _ownerA = IdUser.Zero;
	IdUser _ownerB = IdUser.Zero;
	str _token = "";

	readonly List<IdStudyPlan> _studyPlanIds = [];
	readonly List<IdWeightArg> _weightArgIds = [];
	readonly List<IdWeightCalculator> _weightCalculatorIds = [];
	readonly List<IdPreFilter> _preFilterIds = [];

	public TestISvcStudyPlan(
		ISqlCmdMkr SqlCmdMkr
		,ISvcKv SvcKv
		,TxnWrapper TxnWrapper
		,ITblMgr TblMgr
		,IRepo<PoStudyPlan, IdStudyPlan> RepoStudyPlan
		,IRepo<PoWeightArg, IdWeightArg> RepoWeightArg
		,IRepo<PoWeightCalculator, IdWeightCalculator> RepoWeightCalculator
		,IRepo<PoPreFilter, IdPreFilter> RepoPreFilter
	){
		this.SqlCmdMkr = SqlCmdMkr;
		this.RepoStudyPlan = RepoStudyPlan;
		this.RepoWeightArg = RepoWeightArg;
		this.RepoWeightCalculator = RepoWeightCalculator;
		this.RepoPreFilter = RepoPreFilter;
		var DaoStudyPlan = new DaoStudyPlan(
			SqlCmdMkr
			,TblMgr
			,RepoStudyPlan
			,RepoWeightArg
			,RepoWeightCalculator
			,RepoPreFilter
		);
		this.SvcStudyPlan = new SvcStudyPlan(
			SvcKv
			,DaoStudyPlan
			,SqlCmdMkr
			,TxnWrapper
			,RepoStudyPlan
			,RepoWeightArg
			,RepoWeightCalculator
			,RepoPreFilter
		);
	}

	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test ??= new TestNode();
		Test.Ordered = true;
		var register = Test.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[
				typeof(SvcStudyPlan)
				,typeof(ITblMgr)
				,typeof(ISvcKv)
				,typeof(IRepo<PoStudyPlan, IdStudyPlan>)
				,typeof(IRepo<PoWeightArg, IdWeightArg>)
				,typeof(IRepo<PoWeightCalculator, IdWeightCalculator>)
				,typeof(IRepo<PoPreFilter, IdPreFilter>)
			]
			,[]
			,nameof(TestISvcStudyPlan)
		);
		var R = register.Register;

		register.TesteeFnNames = [
			nameof(IRepo<PoStudyPlan, IdStudyPlan>.BatAdd)
			,nameof(IRepo<PoWeightArg, IdWeightArg>.BatAdd)
			,nameof(IRepo<PoWeightCalculator, IdWeightCalculator>.BatAdd)
			,nameof(IRepo<PoPreFilter, IdPreFilter>.BatAdd)
			,nameof(SvcStudyPlan.PageStudyPlan)
			,nameof(SvcStudyPlan.PageWeightArg)
			,nameof(SvcStudyPlan.PageWeightCalculator)
			,nameof(SvcStudyPlan.PagePreFilter)
			,nameof(IRepo<PoStudyPlan, IdStudyPlan>.BatHardDelById)
			,nameof(IRepo<PoWeightArg, IdWeightArg>.BatHardDelById)
			,nameof(IRepo<PoWeightCalculator, IdWeightCalculator>.BatHardDelById)
			,nameof(IRepo<PoPreFilter, IdPreFilter>.BatHardDelById)
		];
		R("SvcStudyPlan_Page_AllPo_Insert_Query_Cleanup", async(o)=>{
			await InsertData();
			try{
				await AssertPageStudyPlan();
				await AssertPageWeightArg();
				await AssertPageWeightCalculator();
				await AssertPagePreFilter();
			}
			finally{
				await CleanupData();
			}
			return NIL;
		});

		return Test;
	}

	Task<TRtn> RunNoTxn<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn){
		IDbFnCtx Ctx = new DbFnCtx();
		return Fn(Ctx);
	}

	static async IAsyncEnumerable<T> AsyE<T>(params T[] Items){
		foreach(var I in Items){
			yield return I;
		}
	}

	static async Task<List<T>> ToList<T>(IAsyncEnumerable<T>? Asy){
		if(Asy is null){
			return [];
		}
		var R = new List<T>();
		await foreach(var x in Asy){
			R.Add(x);
		}
		return R;
	}

	async Task InsertData(){
		_ownerA = new IdUser();
		_ownerB = new IdUser();
		_token = "ut_sp_page_" + Guid.NewGuid().ToString("N");

		await RunNoTxn(async(Ctx)=>{
			var studyPlans = new[]{
				new PoStudyPlan{Id = new IdStudyPlan(), Owner = _ownerA, UniqName = _token + "_sp_a_1", Descr = "a1", BizUpdatedAt = 1001},
				new PoStudyPlan{Id = new IdStudyPlan(), Owner = _ownerA, UniqName = _token + "_sp_a_2", Descr = "a2", BizUpdatedAt = 1002},
				new PoStudyPlan{Id = new IdStudyPlan(), Owner = _ownerA, UniqName = "sp_a_3_" + _token, Descr = "a3", BizUpdatedAt = 1003},
				new PoStudyPlan{Id = new IdStudyPlan(), Owner = _ownerB, UniqName = _token + "_sp_b_1", Descr = "b1", BizUpdatedAt = 1004},
			};
			var weightArgs = new[]{
				new PoWeightArg{Id = new IdWeightArg(), Owner = _ownerA, UniqName = _token + "_wa_a_1", Descr = "a1", BizUpdatedAt = 2001},
				new PoWeightArg{Id = new IdWeightArg(), Owner = _ownerA, UniqName = _token + "_wa_a_2", Descr = "a2", BizUpdatedAt = 2002},
				new PoWeightArg{Id = new IdWeightArg(), Owner = _ownerA, UniqName = "wa_a_3_" + _token, Descr = "a3", BizUpdatedAt = 2003},
				new PoWeightArg{Id = new IdWeightArg(), Owner = _ownerB, UniqName = _token + "_wa_b_1", Descr = "b1", BizUpdatedAt = 2004},
			};
			var weightCalculators = new[]{
				new PoWeightCalculator{Id = new IdWeightCalculator(), Owner = _ownerA, UniqName = _token + "_wc_a_1", Descr = "a1"},
				new PoWeightCalculator{Id = new IdWeightCalculator(), Owner = _ownerA, UniqName = _token + "_wc_a_2", Descr = "a2"},
				new PoWeightCalculator{Id = new IdWeightCalculator(), Owner = _ownerA, UniqName = "wc_a_3_" + _token, Descr = "a3"},
				new PoWeightCalculator{Id = new IdWeightCalculator(), Owner = _ownerB, UniqName = _token + "_wc_b_1", Descr = "b1"},
			};
			var preFilters = new[]{
				new PoPreFilter{Id = new IdPreFilter(), Owner = _ownerA, UniqName = _token + "_pf_a_1", Descr = "a1", BizUpdatedAt = 3001},
				new PoPreFilter{Id = new IdPreFilter(), Owner = _ownerA, UniqName = _token + "_pf_a_2", Descr = "a2", BizUpdatedAt = 3002},
				new PoPreFilter{Id = new IdPreFilter(), Owner = _ownerA, UniqName = "pf_a_3_" + _token, Descr = "a3", BizUpdatedAt = 3003},
				new PoPreFilter{Id = new IdPreFilter(), Owner = _ownerB, UniqName = _token + "_pf_b_1", Descr = "b1", BizUpdatedAt = 3004},
			};

			await RepoStudyPlan.BatAdd(Ctx, AsyE(studyPlans), CT.None);
			await RepoWeightArg.BatAdd(Ctx, AsyE(weightArgs), CT.None);
			await RepoWeightCalculator.BatAdd(Ctx, AsyE(weightCalculators), CT.None);
			await RepoPreFilter.BatAdd(Ctx, AsyE(preFilters), CT.None);

			_studyPlanIds.Clear();
			_studyPlanIds.AddRange(studyPlans.Select(x=>x.Id));
			_weightArgIds.Clear();
			_weightArgIds.AddRange(weightArgs.Select(x=>x.Id));
			_weightCalculatorIds.Clear();
			_weightCalculatorIds.AddRange(weightCalculators.Select(x=>x.Id));
			_preFilterIds.Clear();
			_preFilterIds.AddRange(preFilters.Select(x=>x.Id));
			return NIL;
		});
	}

	async Task AssertPageStudyPlan(){
		await RunNoTxn(async(Ctx)=>{
			var qrySearch = new PageQry{PageIdx = 0, PageSize = 10, WantTotCnt = false};
			var reqSearch = new ReqPageStudyPlan{
				Owner = _ownerA,
				PageQry = qrySearch,
				UniqNameSearch = _token + "_sp_a_",
			};
			var pageSearch = await SvcStudyPlan.PageStudyPlan(Ctx, reqSearch, CT.None);
			var dataSearch = await ToList(pageSearch.DataAsyE);
			if(dataSearch.Count != 2){
				throw new Exception($"PageStudyPlan search expected 2, got {dataSearch.Count}");
			}
			if(dataSearch.Any(x=>x.Owner != _ownerA)){
				throw new Exception("PageStudyPlan search contains wrong owner data");
			}

			var page0 = await SvcStudyPlan.PageStudyPlan(Ctx, new ReqPageStudyPlan{
				Owner = _ownerA,
				PageQry = new PageQry{PageIdx = 0, PageSize = 2, WantTotCnt = false},
			}, CT.None);
			var page0Data = await ToList(page0.DataAsyE);
			if(page0Data.Count != 2){
				throw new Exception($"PageStudyPlan page0 expected 2, got {page0Data.Count}");
			}

			var page1 = await SvcStudyPlan.PageStudyPlan(Ctx, new ReqPageStudyPlan{
				Owner = _ownerA,
				PageQry = new PageQry{PageIdx = 1, PageSize = 2, WantTotCnt = false},
			}, CT.None);
			var page1Data = await ToList(page1.DataAsyE);
			if(page1Data.Count != 1){
				throw new Exception($"PageStudyPlan page1 expected 1, got {page1Data.Count}");
			}
			return NIL;
		});
	}

	async Task AssertPageWeightArg(){
		await RunNoTxn(async(Ctx)=>{
			var qrySearch = new PageQry{PageIdx = 0, PageSize = 10, WantTotCnt = false};
			var reqSearch = new ReqPageWeightArg{
				Owner = _ownerA,
				PageQry = qrySearch,
				UniqNameSearch = _token + "_wa_a_",
			};
			var pageSearch = await SvcStudyPlan.PageWeightArg(Ctx, reqSearch, CT.None);
			var dataSearch = await ToList(pageSearch.DataAsyE);
			if(dataSearch.Count != 2){
				throw new Exception($"PageWeightArg search expected 2, got {dataSearch.Count}");
			}
			if(dataSearch.Any(x=>x.Owner != _ownerA)){
				throw new Exception("PageWeightArg search contains wrong owner data");
			}

			var page0 = await SvcStudyPlan.PageWeightArg(Ctx, new ReqPageWeightArg{
				Owner = _ownerA,
				PageQry = new PageQry{PageIdx = 0, PageSize = 2, WantTotCnt = false},
			}, CT.None);
			var page0Data = await ToList(page0.DataAsyE);
			if(page0Data.Count != 2){
				throw new Exception($"PageWeightArg page0 expected 2, got {page0Data.Count}");
			}

			var page1 = await SvcStudyPlan.PageWeightArg(Ctx, new ReqPageWeightArg{
				Owner = _ownerA,
				PageQry = new PageQry{PageIdx = 1, PageSize = 2, WantTotCnt = false},
			}, CT.None);
			var page1Data = await ToList(page1.DataAsyE);
			if(page1Data.Count != 1){
				throw new Exception($"PageWeightArg page1 expected 1, got {page1Data.Count}");
			}
			return NIL;
		});
	}

	async Task AssertPageWeightCalculator(){
		await RunNoTxn(async(Ctx)=>{
			var qrySearch = new PageQry{PageIdx = 0, PageSize = 10, WantTotCnt = false};
			var reqSearch = new ReqPageWeightCalculator{
				Owner = _ownerA,
				PageQry = qrySearch,
				UniqNameSearch = _token + "_wc_a_",
			};
			var pageSearch = await SvcStudyPlan.PageWeightCalculator(Ctx, reqSearch, CT.None);
			var dataSearch = await ToList(pageSearch.DataAsyE);
			if(dataSearch.Count != 2){
				throw new Exception($"PageWeightCalculator search expected 2, got {dataSearch.Count}");
			}
			if(dataSearch.Any(x=>x.Owner != _ownerA)){
				throw new Exception("PageWeightCalculator search contains wrong owner data");
			}

			var page0 = await SvcStudyPlan.PageWeightCalculator(Ctx, new ReqPageWeightCalculator{
				Owner = _ownerA,
				PageQry = new PageQry{PageIdx = 0, PageSize = 2, WantTotCnt = false},
			}, CT.None);
			var page0Data = await ToList(page0.DataAsyE);
			if(page0Data.Count != 2){
				throw new Exception($"PageWeightCalculator page0 expected 2, got {page0Data.Count}");
			}

			var page1 = await SvcStudyPlan.PageWeightCalculator(Ctx, new ReqPageWeightCalculator{
				Owner = _ownerA,
				PageQry = new PageQry{PageIdx = 1, PageSize = 2, WantTotCnt = false},
			}, CT.None);
			var page1Data = await ToList(page1.DataAsyE);
			if(page1Data.Count != 1){
				throw new Exception($"PageWeightCalculator page1 expected 1, got {page1Data.Count}");
			}
			return NIL;
		});
	}

	async Task AssertPagePreFilter(){
		await RunNoTxn(async(Ctx)=>{
			var qrySearch = new PageQry{PageIdx = 0, PageSize = 10, WantTotCnt = false};
			var reqSearch = new ReqPagePreFilter{
				Owner = _ownerA,
				PageQry = qrySearch,
				UniqNameSearch = _token + "_pf_a_",
			};
			var pageSearch = await SvcStudyPlan.PagePreFilter(Ctx, reqSearch, CT.None);
			var dataSearch = await ToList(pageSearch.DataAsyE);
			if(dataSearch.Count != 2){
				throw new Exception($"PagePreFilter search expected 2, got {dataSearch.Count}");
			}
			if(dataSearch.Any(x=>x.Owner != _ownerA)){
				throw new Exception("PagePreFilter search contains wrong owner data");
			}

			var page0 = await SvcStudyPlan.PagePreFilter(Ctx, new ReqPagePreFilter{
				Owner = _ownerA,
				PageQry = new PageQry{PageIdx = 0, PageSize = 2, WantTotCnt = false},
			}, CT.None);
			var page0Data = await ToList(page0.DataAsyE);
			if(page0Data.Count != 2){
				throw new Exception($"PagePreFilter page0 expected 2, got {page0Data.Count}");
			}

			var page1 = await SvcStudyPlan.PagePreFilter(Ctx, new ReqPagePreFilter{
				Owner = _ownerA,
				PageQry = new PageQry{PageIdx = 1, PageSize = 2, WantTotCnt = false},
			}, CT.None);
			var page1Data = await ToList(page1.DataAsyE);
			if(page1Data.Count != 1){
				throw new Exception($"PagePreFilter page1 expected 1, got {page1Data.Count}");
			}
			return NIL;
		});
	}

	async Task CleanupData(){
		await RunNoTxn(async(Ctx)=>{
			if(_studyPlanIds.Count > 0){
				await RepoStudyPlan.BatHardDelById(Ctx, AsyE(_studyPlanIds.ToArray()), CT.None);
			}
			if(_weightArgIds.Count > 0){
				await RepoWeightArg.BatHardDelById(Ctx, AsyE(_weightArgIds.ToArray()), CT.None);
			}
			if(_weightCalculatorIds.Count > 0){
				await RepoWeightCalculator.BatHardDelById(Ctx, AsyE(_weightCalculatorIds.ToArray()), CT.None);
			}
			if(_preFilterIds.Count > 0){
				await RepoPreFilter.BatHardDelById(Ctx, AsyE(_preFilterIds.ToArray()), CT.None);
			}
			return NIL;
		});
	}
}
