using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Infra;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.StudyPlan;

public partial class TestISvcStudyPlan: ITester{
	ISvcStudyPlan SvcStudyPlan;
	IRepo<PoStudyPlan, IdStudyPlan> RepoStudyPlan;
	IRepo<PoWeightArg, IdWeightArg> RepoWeightArg;
	IRepo<PoWeightCalculator, IdWeightCalculator> RepoWeightCalculator;
	IRepo<PoPreFilter, IdPreFilter> RepoPreFilter;

	IdUser _ownerA = new IdUser();
	IdUser _ownerB = new IdUser();
	str _token = "";

	readonly List<IdStudyPlan> _studyPlanIds = [];
	readonly List<IdWeightArg> _weightArgIds = [];
	readonly List<IdWeightCalculator> _weightCalculatorIds = [];
	readonly List<IdPreFilter> _preFilterIds = [];

	public TestISvcStudyPlan(
		ISvcStudyPlan SvcStudyPlan
		,IRepo<PoStudyPlan, IdStudyPlan> RepoStudyPlan
		,IRepo<PoWeightArg, IdWeightArg> RepoWeightArg
		,IRepo<PoWeightCalculator, IdWeightCalculator> RepoWeightCalculator
		,IRepo<PoPreFilter, IdPreFilter> RepoPreFilter
	){
		this.SvcStudyPlan = SvcStudyPlan;
		this.RepoStudyPlan = RepoStudyPlan;
		this.RepoWeightArg = RepoWeightArg;
		this.RepoWeightCalculator = RepoWeightCalculator;
		this.RepoPreFilter = RepoPreFilter;
	}

	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test ??= new TestNode();
		Test.Ordered = true;

		var register = Test.MkTestFnRegister(
			typeof(TestISvcStudyPlan)
			,[
				typeof(ISvcStudyPlan)
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
			nameof(ISvcStudyPlan.PageStudyPlan)
			,nameof(ISvcStudyPlan.PageWeightArg)
			,nameof(ISvcStudyPlan.PageWeightCalculator)
			,nameof(ISvcStudyPlan.PagePreFilter)
			,nameof(ISvcStudyPlan.BatAddPreFilter)
			,nameof(ISvcStudyPlan.BatAddWeightArg)
			,nameof(ISvcStudyPlan.BatAddWeightCalculator)
			,nameof(ISvcStudyPlan.SetCurStudyPlanId)
			,nameof(ISvcStudyPlan.GetCurStudyPlanId)
			,nameof(ISvcStudyPlan.GetCurJnStudyPlan)
			,nameof(ISvcStudyPlan.GetCurBoStudyPlan)
			,nameof(ISvcStudyPlan.GetCurWeightCalctr)
			,nameof(ISvcStudyPlan.GetDfltStudyPlan)
			,nameof(ISvcStudyPlan.EnsureCurStudyPlan)
		];

		R("StudyPlan_Setup_InsertSeedData", async(o)=>{
			await InsertSeedData();
			return NIL;
		});

		RegisterPageStudyPlan(Test);
		RegisterPagePreFilter(Test);
		RegisterPageWeightArg(Test);
		RegisterPageWeightCalculator(Test);
		RegisterBatAddPreFilter(Test);
		RegisterBatAddWeightArg(Test);
		RegisterBatAddWeightCalculator(Test);
		RegisterCurStudyPlanApis(Test);
		RegisterBuiltinAndEnsureApis(Test);
		RegisterDirectImplementedMethods(Test);
		R("StudyPlan_Cleanup_AllInsertedData", async(o)=>{
			await CleanupData();
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

	IUserCtx MkUser(IdUser UserId){
		return new UserCtx{UserId = UserId};
	}

	IDbUserCtx MkUserCtx(IdUser UserId){
		return new DbUserCtx(
			MkUser(UserId)
			,new DbFnCtx()
		);
	}

	async Task InsertSeedData(){
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
