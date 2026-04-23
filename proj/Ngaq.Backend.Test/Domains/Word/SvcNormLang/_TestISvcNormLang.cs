using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcNormLang : ITester{
	readonly ISvcNormLang SvcNormLang;
	readonly IRepo<PoNormLang, IdNormLang> RepoNormLang;

	IdUser _ownerA = new();
	IdUser _ownerB = new();
	IdUser _ownerInit = new();
	str _token = "";
	readonly List<IdNormLang> _ids = [];

	public TestISvcNormLang(
		ISvcNormLang SvcNormLang,
		IRepo<PoNormLang, IdNormLang> RepoNormLang
	){
		this.SvcNormLang = SvcNormLang;
		this.RepoNormLang = RepoNormLang;
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;

		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLang),
			[typeof(ISvcNormLang), typeof(IRepo<PoNormLang, IdNormLang>)],
			[],
			nameof(TestISvcNormLang)
		);
		var R = register.Register;
		R("SvcNormLang_Setup_InsertSeedData", async(o)=>{
			await InsertSeedData();
			return NIL;
		});

		RegisterBatGetNormLangByTypeCode(Node);
		RegisterPageNormLang(Node);
		RegisterBatAddNormLang(Node);
		RegisterBatUpdNormLang(Node);
		RegisterBatSoftDelNormLang(Node);
		RegisterInitBuiltinNormLang(Node);

		R("SvcNormLang_Cleanup_AllInsertedData", async(o)=>{
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
		_ownerInit = new IdUser();
		_token = "ut_norm_lang_" + Guid.NewGuid().ToString("N");

		var rows = new[]{
			new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_zh_hant_tw",
				NativeName = "seed_a1",
				BizUpdatedAt = UnixMs.FromUnixMs(1101),
			},
			new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_en_us",
				NativeName = "seed_a2",
				BizUpdatedAt = UnixMs.FromUnixMs(1102),
			},
			new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_other_search",
				NativeName = "seed_a3",
				BizUpdatedAt = UnixMs.FromUnixMs(1103),
			},
			new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerB,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_de_de",
				NativeName = "seed_b1",
				BizUpdatedAt = UnixMs.FromUnixMs(1104),
			},
		};

		await RunNoTxn(async(Ctx)=>{
			await RepoNormLang.BatAdd(Ctx, AsyE(rows), CT.None);
			return NIL;
		});
		_ids.AddRange(rows.Select(x=>x.Id));
	}

	async Task CleanupData(){
		await RunNoTxn(async(Ctx)=>{
			var ids = _ids.Distinct().ToArray();
			if(ids.Length > 0){
				await RepoNormLang.BatHardDelById(Ctx, AsyE(ids), CT.None);
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
