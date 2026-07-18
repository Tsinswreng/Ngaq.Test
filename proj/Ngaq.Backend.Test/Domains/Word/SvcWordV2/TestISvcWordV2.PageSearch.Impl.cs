using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsPage;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2 {
	/// <summary>
	/// 將 PageSearch 行為測試掛入測試樹。
	/// </summary>
	partial void RegisterPageSearch(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		register.TesteeFnNames = [nameof(ISvcWordV2.PageSearch)];
		var R = register.Register;

		R(nameof(PageSearchWhenRawStrIsWordIdShouldReturnExactWordHit), PageSearchWhenRawStrIsWordIdShouldReturnExactWordHit!);

		R(nameof(PageSearchWhenRawStrIsPropIdShouldReturnExactPropHitAndRoot), PageSearchWhenRawStrIsPropIdShouldReturnExactPropHitAndRoot!);

		R(nameof(PageSearchWhenRawStrIsLearnIdShouldReturnExactLearnHitAndRoot), PageSearchWhenRawStrIsLearnIdShouldReturnExactLearnHitAndRoot!);

		R(nameof(PageSearchWhenPrefixMatchedShouldOrderByHeadAndSliceByPageQry), PageSearchWhenPrefixMatchedShouldOrderByHeadAndSliceByPageQry!);

		R(nameof(PageSearchWhenRawStrMatchesExactHeadAndHeadPrefixShouldOrderExactFirst), PageSearchWhenRawStrMatchesExactHeadAndHeadPrefixShouldOrderExactFirst!);

		R(nameof(PageSearchWhenRawStrMatchesIdAndHeadShouldReturnOnlyIdTier), PageSearchWhenRawStrMatchesIdAndHeadShouldReturnOnlyIdTier!);

		R(nameof(PageSearchWhenExactPropHitReturnedJnWordShouldExcludeOtherSoftDeletedAssets), PageSearchWhenExactPropHitReturnedJnWordShouldExcludeOtherSoftDeletedAssets!);

		R(nameof(PageSearchWhenPrefixMatchedShouldExcludeSoftDeletedAssets), PageSearchWhenPrefixMatchedShouldExcludeSoftDeletedAssets!);
	}

	/// 驗證詞 ID 的精確命中。
	public async partial Task<nil> PageSearchWhenRawStrIsWordIdShouldReturnExactWordHit(obj? O) {
		var Owner = new IdUser();
		var Word = new PoWord{
			Id = new IdWord(),
			Owner = Owner,
			Head = "ut_pagesearch_" + Guid.NewGuid().ToString("N"),
			Lang = "en",
		};
		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Word), CT.None);
				return NIL;
			});
			var Page = await SvcWordV2.PageSearch(
				MkUserCtx(Owner),
				MkPageQry(0, 10, true),
				new ReqSearchWord{RawStr = Word.Id + ""},
				CT.None
			);
			Assert.IsTrue(
				Page.Data?.Count == 1
				&& Page.Data[0].HitKind == EWordSearchHitKind.Word
				&& Page.Data[0].JnWord.Id == Word.Id
			);
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdHardDelById(Ctx, AsyE(Word.Id), CT.None);
				return NIL;
			});
		}
	}

	/// 驗證屬性 ID 命中保留屬性本身及其完整聚合根。
	public async partial Task<nil> PageSearchWhenRawStrIsPropIdShouldReturnExactPropHitAndRoot(obj? O) {
		var Owner = new IdUser();
		var Token = "ut_pagesearch_prop_" + Guid.NewGuid().ToString("N");
		var Word = new PoWord{
			Id = new IdWord(),
			Owner = Owner,
			Head = Token,
			Lang = "en",
		};
		var Prop = new PoWordProp{
			Id = new IdWordProp(),
			WordId = Word.Id,
			KStr = KeysProp.Inst.note,
			VType = EKvType.Str,
			VStr = Token,
		};
		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Word), CT.None);
				await RepoProp.OrdAdd(Ctx, AsyE(Prop), CT.None);
				return NIL;
			});
			var Page = await SvcWordV2.PageSearch(
				MkUserCtx(Owner),
				MkPageQry(0, 10, true),
				new ReqSearchWord{RawStr = Prop.Id + ""},
				CT.None
			);
			var Hit = Page.Data?.SingleOrDefault();
			Assert.IsTrue(
				Hit?.HitKind == EWordSearchHitKind.WordProp
				&& Hit.WordProp?.Id == Prop.Id
				&& Hit.JnWord.Id == Word.Id
				&& Hit.JnWord.Props.Any(x=>x.Id == Prop.Id)
			);
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoProp.OrdHardDelById(Ctx, AsyE(Prop.Id), CT.None);
				await RepoWord.OrdHardDelById(Ctx, AsyE(Word.Id), CT.None);
				return NIL;
			});
		}
	}

	/// 驗證學習記錄 ID 命中保留記錄本身及其完整聚合根。
	public async partial Task<nil> PageSearchWhenRawStrIsLearnIdShouldReturnExactLearnHitAndRoot(obj? O) {
		var Owner = new IdUser();
		var Token = "ut_pagesearch_learn_" + Guid.NewGuid().ToString("N");
		var Word = new PoWord{
			Id = new IdWord(),
			Owner = Owner,
			Head = Token,
			Lang = "en",
		};
		var Learn = new PoWordLearn{
			Id = new IdWordLearn(),
			WordId = Word.Id,
			LearnResult = ELearn.Rmb,
		};
		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Word), CT.None);
				await RepoLearn.OrdAdd(Ctx, AsyE(Learn), CT.None);
				return NIL;
			});
			var Page = await SvcWordV2.PageSearch(
				MkUserCtx(Owner),
				MkPageQry(0, 10, true),
				new ReqSearchWord{RawStr = Learn.Id + ""},
				CT.None
			);
			var Hit = Page.Data?.SingleOrDefault();
			Assert.IsTrue(
				Hit?.HitKind == EWordSearchHitKind.WordLearn
				&& Hit.WordLearn?.Id == Learn.Id
				&& Hit.JnWord.Id == Word.Id
				&& Hit.JnWord.Learns.Any(x=>x.Id == Learn.Id)
			);
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoLearn.OrdHardDelById(Ctx, AsyE(Learn.Id), CT.None);
				await RepoWord.OrdHardDelById(Ctx, AsyE(Word.Id), CT.None);
				return NIL;
			});
		}
	}

	/// 驗證前綴搜尋在資料庫層完成所有者過濾、穩定排序及分頁。
	public async partial Task<nil> PageSearchWhenPrefixMatchedShouldOrderByHeadAndSliceByPageQry(obj? O) {
		var Owner = new IdUser();
		var OtherOwner = new IdUser();
		var Token = "ut_pagesearch_prefix_" + Guid.NewGuid().ToString("N");
		var Words = new[]{
			new PoWord{Id = new IdWord(), Owner = Owner, Head = Token + "_c", Lang = "en"},
			new PoWord{Id = new IdWord(), Owner = Owner, Head = Token + "_a", Lang = "en"},
			new PoWord{Id = new IdWord(), Owner = Owner, Head = Token + "_b", Lang = "en"},
			new PoWord{Id = new IdWord(), Owner = OtherOwner, Head = Token + "_0", Lang = "en"},
		};
		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Words), CT.None);
				return NIL;
			});
			var Page = await SvcWordV2.PageSearch(
				MkUserCtx(Owner),
				MkPageQry(1, 1, true),
				new ReqSearchWord{RawStr = Token},
				CT.None
			);
			Assert.IsTrue(
				Page.HasTotCnt
				&& Page.TotCnt == 3
				&& Page.Data?.Count == 1
				&& Page.Data[0].JnWord.Head == Token + "_b"
			);
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdHardDelById(
					Ctx,
					AsyE(Words.Select(x=>x.Id).ToArray()),
					CT.None
				);
				return NIL;
			});
		}
	}

	/// 驗證完全相同的詞頭在同前綴結果中按排序規則位於首位。
	public async partial Task<nil> PageSearchWhenRawStrMatchesExactHeadAndHeadPrefixShouldOrderExactFirst(obj? O) {
		var Owner = new IdUser();
		var Token = "ut_pagesearch_head_" + Guid.NewGuid().ToString("N");
		var Words = new[]{
			new PoWord{Id = new IdWord(), Owner = Owner, Head = Token + "_b", Lang = "en"},
			new PoWord{Id = new IdWord(), Owner = Owner, Head = Token, Lang = "en"},
			new PoWord{Id = new IdWord(), Owner = Owner, Head = Token + "_a", Lang = "en"},
		};
		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Words), CT.None);
				return NIL;
			});
			var Page = await SvcWordV2.PageSearch(
				MkUserCtx(Owner),
				MkPageQry(0, 10, true),
				new ReqSearchWord{RawStr = Token},
				CT.None
			);
			Assert.IsTrue(
				Page.Data?.Count == 3
				&& Page.Data[0].JnWord.Head == Token
				&& Page.Data[1].JnWord.Head == Token + "_a"
				&& Page.Data[2].JnWord.Head == Token + "_b"
			);
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdHardDelById(
					Ctx,
					AsyE(Words.Select(x=>x.Id).ToArray()),
					CT.None
				);
				return NIL;
			});
		}
	}

	/// 驗證可同時作爲詞頭的 ID 字串仍按 ID 優先級短路。
	public async partial Task<nil> PageSearchWhenRawStrMatchesIdAndHeadShouldReturnOnlyIdTier(obj? O) {
		var Owner = new IdUser();
		var IdWord = new PoWord{
			Id = new IdWord(),
			Owner = Owner,
			Head = "ut_pagesearch_id_" + Guid.NewGuid().ToString("N"),
			Lang = "en",
		};
		var RawId = IdWord.Id + "";
		var HeadWords = new[]{
			new PoWord{Id = new IdWord(), Owner = Owner, Head = RawId, Lang = "en"},
			new PoWord{Id = new IdWord(), Owner = Owner, Head = RawId + "_suffix", Lang = "en"},
		};
		var Words = new[]{IdWord, HeadWords[0], HeadWords[1]};
		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Words), CT.None);
				return NIL;
			});
			var Page = await SvcWordV2.PageSearch(
				MkUserCtx(Owner),
				MkPageQry(0, 10, true),
				new ReqSearchWord{RawStr = RawId},
				CT.None
			);
			Assert.IsTrue(
				Page.Data?.Count == 1
				&& Page.Data[0].HitKind == EWordSearchHitKind.Word
				&& Page.Data[0].JnWord.Id == IdWord.Id
			);
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdHardDelById(
					Ctx,
					AsyE(Words.Select(x=>x.Id).ToArray()),
					CT.None
				);
				return NIL;
			});
		}
	}

	/// 驗證精確命中資產時，返回聚合只包含未軟刪除的其他資產。
	public async partial Task<nil> PageSearchWhenExactPropHitReturnedJnWordShouldExcludeOtherSoftDeletedAssets(obj? O) {
		var Owner = new IdUser();
		var Token = "ut_pagesearch_deleted_" + Guid.NewGuid().ToString("N");
		var Word = new PoWord{
			Id = new IdWord(),
			Owner = Owner,
			Head = Token,
			Lang = "en",
		};
		var ActiveProp = new PoWordProp{
			Id = new IdWordProp(),
			WordId = Word.Id,
			KStr = KeysProp.Inst.note,
			VType = EKvType.Str,
			VStr = Token,
		};
		var DeletedProp = new PoWordProp{
			Id = new IdWordProp(),
			WordId = Word.Id,
			KStr = KeysProp.Inst.tag,
			VType = EKvType.Str,
			VStr = Token,
		};
		var ActiveLearn = new PoWordLearn{
			Id = new IdWordLearn(),
			WordId = Word.Id,
			LearnResult = ELearn.Add,
		};
		var DeletedLearn = new PoWordLearn{
			Id = new IdWordLearn(),
			WordId = Word.Id,
			LearnResult = ELearn.Fgt,
		};
		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Word), CT.None);
				await RepoProp.OrdAdd(Ctx, AsyE(ActiveProp, DeletedProp), CT.None);
				await RepoLearn.OrdAdd(Ctx, AsyE(ActiveLearn, DeletedLearn), CT.None);
				await RepoProp.SoftDelInId(Ctx, AsyE(DeletedProp.Id), CT.None);
				await RepoLearn.SoftDelInId(Ctx, AsyE(DeletedLearn.Id), CT.None);
				return NIL;
			});
			var Page = await SvcWordV2.PageSearch(
				MkUserCtx(Owner),
				MkPageQry(0, 10, true),
				new ReqSearchWord{RawStr = ActiveProp.Id + ""},
				CT.None
			);
			var Hit = Page.Data?.SingleOrDefault();
			Assert.IsTrue(
				Hit is not null
				&& Hit.JnWord.Props.Any(x=>x.Id == ActiveProp.Id)
				&& Hit.JnWord.Props.All(x=>x.Id != DeletedProp.Id)
				&& Hit.JnWord.Learns.Any(x=>x.Id == ActiveLearn.Id)
				&& Hit.JnWord.Learns.All(x=>x.Id != DeletedLearn.Id)
			);
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoProp.OrdHardDelById(
					Ctx,
					AsyE(ActiveProp.Id, DeletedProp.Id),
					CT.None
				);
				await RepoLearn.OrdHardDelById(
					Ctx,
					AsyE(ActiveLearn.Id, DeletedLearn.Id),
					CT.None
				);
				await RepoWord.OrdHardDelById(Ctx, AsyE(Word.Id), CT.None);
				return NIL;
			});
		}
	}

	/// 驗證詞頭分頁裝配聚合時，IncludeDeleted=false 會落實到資產查詢 SQL。
	public async partial Task<nil> PageSearchWhenPrefixMatchedShouldExcludeSoftDeletedAssets(obj? O){
		var Owner = new IdUser();
		var Token = "ut_pagesearch_prefix_deleted_" + Guid.NewGuid().ToString("N");
		var Word = new PoWord{
			Id = new IdWord(),
			Owner = Owner,
			Head = Token,
			Lang = "en",
		};
		var ActiveProp = new PoWordProp{
			Id = new IdWordProp(),
			WordId = Word.Id,
			KStr = KeysProp.Inst.note,
			VType = EKvType.Str,
			VStr = Token,
		};
		var DeletedProp = new PoWordProp{
			Id = new IdWordProp(),
			WordId = Word.Id,
			KStr = KeysProp.Inst.tag,
			VType = EKvType.Str,
			VStr = Token,
		};
		var ActiveLearn = new PoWordLearn{
			Id = new IdWordLearn(),
			WordId = Word.Id,
			LearnResult = ELearn.Add,
		};
		var DeletedLearn = new PoWordLearn{
			Id = new IdWordLearn(),
			WordId = Word.Id,
			LearnResult = ELearn.Fgt,
		};

		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Word), CT.None);
				await RepoProp.OrdAdd(Ctx, AsyE(ActiveProp, DeletedProp), CT.None);
				await RepoLearn.OrdAdd(Ctx, AsyE(ActiveLearn, DeletedLearn), CT.None);
				await RepoProp.SoftDelInId(Ctx, AsyE(DeletedProp.Id), CT.None);
				await RepoLearn.SoftDelInId(Ctx, AsyE(DeletedLearn.Id), CT.None);
				return NIL;
			});

			var Page = await SvcWordV2.PageSearch(
				MkUserCtx(Owner),
				MkPageQry(0, 10, true),
				new ReqSearchWord{RawStr = Token},
				CT.None
			);
			var Hit = Page.Data?.SingleOrDefault();
			Assert.IsTrue(
				Hit is not null
				&& Hit.JnWord.Props.Any(x=>x.Id == ActiveProp.Id)
				&& Hit.JnWord.Props.All(x=>x.Id != DeletedProp.Id)
				&& Hit.JnWord.Learns.Any(x=>x.Id == ActiveLearn.Id)
				&& Hit.JnWord.Learns.All(x=>x.Id != DeletedLearn.Id)
			);
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoProp.OrdHardDelById(
					Ctx,
					AsyE(ActiveProp.Id, DeletedProp.Id),
					CT.None
				);
				await RepoLearn.OrdHardDelById(
					Ctx,
					AsyE(ActiveLearn.Id, DeletedLearn.Id),
					CT.None
				);
				await RepoWord.OrdHardDelById(Ctx, AsyE(Word.Id), CT.None);
				return NIL;
			});
		}
	}

	/// <summary>
	/// 以指定頁碼、頁長與總數需求建立查詢物件。
	/// </summary>
	private static partial IPageQry MkPageQry(u64 PageIdx, u64 PageSize, bool WantTotCnt) {
		return new PageQry {
			PageIdx = PageIdx,
			PageSize = PageSize,
			WantTotCnt = WantTotCnt,
		};
	}
}
