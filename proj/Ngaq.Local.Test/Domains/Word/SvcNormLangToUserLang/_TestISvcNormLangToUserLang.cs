using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcNormLangToUserLang: ITester{
	readonly ISvcNormLangToUserLang SvcNormLangToUserLang;
	readonly IRepo<PoNormLangToUserLang, IdNormLangToUserLang> RepoNormLangToUserLang;

	IdUser _ownerA = new();
	IdUser _ownerB = new();
	str _token = "";
	readonly List<IdNormLangToUserLang> _ids = [];

	public TestISvcNormLangToUserLang(
		ISvcNormLangToUserLang SvcNormLangToUserLang,
		IRepo<PoNormLangToUserLang, IdNormLangToUserLang> RepoNormLangToUserLang
	){
		this.SvcNormLangToUserLang = SvcNormLangToUserLang;
		this.RepoNormLangToUserLang = RepoNormLangToUserLang;
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;

		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLangToUserLang),
			[typeof(ISvcNormLangToUserLang), typeof(IRepo<PoNormLangToUserLang, IdNormLangToUserLang>)],
			[],
			nameof(TestISvcNormLangToUserLang)
		);
		var R = register.Register;
		R("SvcNormLangToUserLang_Setup_InsertSeedData", async(o)=>{
			await InsertSeedData();
			return NIL;
		});

		RegisterGetUserLangByNormLang(Node);
		RegisterPageNormLangToUserLang(Node);
		RegisterBatAddNormLangToUserLang(Node);
		RegisterBatUpdNormLangToUserLang(Node);
		RegisterBatSoftDelNormLangToUserLang(Node);

		R("SvcNormLangToUserLang_Cleanup_AllInsertedData", async(o)=>{
			await CleanupData();
			return NIL;
		});

		return Node;
	}

	static void AssertThrowsErrItem(Exception Ex, IErrNode Expected, str CaseName){
		if(Ex is not AppErr appErr){
			throw new Exception($"{CaseName} should throw AppErr, got {Ex.GetType().FullName}");
		}
		if(!ReferenceEquals(appErr.Type, Expected)){
			throw new Exception($"{CaseName} should throw expected err item, got [{appErr.Key}]");
		}
	}

	async Task InsertSeedData(){
		_ownerA = new IdUser();
		_ownerB = new IdUser();
		_token = "ut_norm_lang_map_" + Guid.NewGuid().ToString("N");

		var rows = new[]{
			new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_zh_hant_tw",
				UserLang = _token + "_user_zh",
				Descr = "seed_a1",
				BizUpdatedAt = Tempus.FromUnixMs(1101),
			},
			new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_en_us",
				UserLang = _token + "_user_en",
				Descr = "seed_a2",
				BizUpdatedAt = Tempus.FromUnixMs(1102),
			},
			new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_fr_fr",
				UserLang = _token + "_other_search",
				Descr = "seed_a3",
				BizUpdatedAt = Tempus.FromUnixMs(1103),
			},
			new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerB,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_de_de",
				UserLang = _token + "_other_owner",
				Descr = "seed_b1",
				BizUpdatedAt = Tempus.FromUnixMs(1104),
			},
		};

		await RunNoTxn(async(Ctx)=>{
			await RepoNormLangToUserLang.BatAdd(Ctx, AsyE(rows), CT.None);
			return NIL;
		});
		_ids.AddRange(rows.Select(x=>x.Id));
	}

	async Task CleanupData(){
		await RunNoTxn(async(Ctx)=>{
			var ids = _ids.Distinct().ToArray();
			if(ids.Length > 0){
				await RepoNormLangToUserLang.BatHardDelById(Ctx, AsyE(ids), CT.None);
			}
			return NIL;
		});
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
		return new DbUserCtx(MkUser(UserId));
	}
}
