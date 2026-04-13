using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcUserLang: ITester{
	readonly ISvcUserLang SvcUserLang;
	readonly IRepo<PoUserLang, IdUserLang> RepoUserLang;
	readonly IRepo<PoWord, IdWord> RepoWord;

	IdUser _ownerA = new();
	IdUser _ownerB = new();
	str _token = "";
	readonly List<IdUserLang> _userLangIds = [];
	readonly List<IdWord> _wordIds = [];

	public TestISvcUserLang(
		ISvcUserLang SvcUserLang
		,IRepo<PoUserLang, IdUserLang> RepoUserLang
		,IRepo<PoWord, IdWord> RepoWord
	){
		this.SvcUserLang = SvcUserLang;
		this.RepoUserLang = RepoUserLang;
		this.RepoWord = RepoWord;
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;

		var register = Node.MkTestFnRegister(
			typeof(TestISvcUserLang)
			,[typeof(ISvcUserLang), typeof(IRepo<PoUserLang, IdUserLang>), typeof(IRepo<PoWord, IdWord>)]
			,[]
			,nameof(TestISvcUserLang)
		);
		var R = register.Register;

		R("SvcUserLang_Setup_InsertSeedData", async(o)=>{
			await InsertSeedData();
			return NIL;
		});
		RegisterPageUserLang(Node);
		RegisterBatAddUserLang(Node);
		RegisterBatUpdUserLang(Node);
		RegisterGetUnregisteredUserLangs(Node);
		RegisterAddAllUnregisteredUserLangs(Node);

		R("SvcUserLang_Cleanup_AllInsertedData", async(o)=>{
			await CleanupData();
			return NIL;
		});

		return Node;
	}

	static void AssertThrowsErrItem(
		Exception Ex
		,IErrNode Expected
		,str CaseName
	){
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
		_token = "ut_user_lang_" + Guid.NewGuid().ToString("N");

		var rows = new[]{
			new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = _token + "_page_a_1",
				Descr = "a1",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_page_a_1",
				BizUpdatedAt = Tempus.FromUnixMs(1001),
			},
			new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = _token + "_page_a_2",
				Descr = "a2",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_page_a_2",
				BizUpdatedAt = Tempus.FromUnixMs(1002),
			},
			new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = _token + "_page_a_3_other_search",
				Descr = "a3",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_page_a_3_other_search",
				BizUpdatedAt = Tempus.FromUnixMs(1003),
			},
			new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerB,
				UniqName = _token + "_page_a_4_other_owner",
				Descr = "b1",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_page_a_4_other_owner",
				BizUpdatedAt = Tempus.FromUnixMs(1004),
			},
		};
		await RunNoTxn(async(Ctx)=>{
			await RepoUserLang.BatAdd(Ctx, AsyE(rows), CT.None);
			return NIL;
		});
		_userLangIds.AddRange(rows.Select(x=>x.Id));
	}

	async Task CleanupData(){
		await RunNoTxn(async(Ctx)=>{
			var userLangIds = _userLangIds.Distinct().ToArray();
			var wordIds = _wordIds.Distinct().ToArray();
			if(userLangIds.Length > 0){
				await RepoUserLang.BatHardDelById(Ctx, AsyE(userLangIds), CT.None);
			}
			if(wordIds.Length > 0){
				await RepoWord.BatHardDelById(Ctx, AsyE(wordIds), CT.None);
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
