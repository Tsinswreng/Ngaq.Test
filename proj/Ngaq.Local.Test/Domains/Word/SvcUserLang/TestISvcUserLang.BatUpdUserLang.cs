using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcUserLang{
	void RegisterBatUpdUserLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcUserLang)
			,[typeof(ISvcUserLang)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcUserLang.BatUpdUserLang)];

		R("BatUpdUserLang_Should_UpdateOwnedRow_ForceOwner_AndTouchBizUpdatedAt", async(o)=>{
			var row = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = _token + "_upd_a_1",
				Descr = "before_upd",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_upd_a_1",
				BizUpdatedAt = Tempus.FromUnixMs(1000),
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoUserLang.BatAdd(Ctx, AsyE(row), CT.None);
				return NIL;
			});
			_userLangIds.Add(row.Id);

			var upd = new PoUserLang{
				Id = row.Id,
				Owner = _ownerA,
				UniqName = row.UniqName,
				Descr = "after_upd",
				RelLangType = row.RelLangType,
				RelLang = row.RelLang,
				BizUpdatedAt = Tempus.FromUnixMs(1),
			};
			await SvcUserLang.BatUpdUserLang(MkUserCtx(_ownerA), AsyE(upd), CT.None);

			await RunNoTxn(async(Ctx)=>{
				var got = await RepoUserLang.BatGetByIdWithDel(Ctx, AsyE(row.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(got is null){
					throw new Exception("BatUpdUserLang should keep row");
				}
				if(got.Owner != _ownerA){
					throw new Exception("BatUpdUserLang should keep checked owner");
				}
				if(got.Descr != "after_upd"){
					throw new Exception("BatUpdUserLang should update mutable fields");
				}
				if(got.BizUpdatedAt <= Tempus.FromUnixMs(1)){
					throw new Exception("BatUpdUserLang should refresh BizUpdatedAt");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatUpdUserLang_Should_ThrowPermissionDenied_WhenContainsNonOwnedRows", async(o)=>{
			var mine = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = _token + "_perm_mine",
				Descr = "before_mine",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_perm_mine",
				BizUpdatedAt = Tempus.FromUnixMs(1000),
			};
			var other = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerB,
				UniqName = _token + "_perm_other",
				Descr = "before_other",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_perm_other",
				BizUpdatedAt = Tempus.FromUnixMs(1000),
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoUserLang.BatAdd(Ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_userLangIds.Add(mine.Id);
			_userLangIds.Add(other.Id);

			mine.Descr = "after_mine_should_not_apply";
			other.Owner = _ownerA;
			other.Descr = "after_other_should_not_apply";
			try{
				await SvcUserLang.BatUpdUserLang(MkUserCtx(_ownerA), AsyE(mine, other), CT.None);
				throw new Exception("BatUpdUserLang should throw permission denied");
			}
			catch(Exception ex){
				AssertThrowsErrItem(ex, ItemsErr.Common.PermissionDenied, nameof(ISvcUserLang.BatUpdUserLang));
			}

			await RunNoTxn(async(Ctx)=>{
				var gotMine = await RepoUserLang.BatGetByIdWithDel(Ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoUserLang.BatGetByIdWithDel(Ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || gotMine.Descr != "before_mine" || gotMine.Owner != _ownerA){
					throw new Exception("permission denied should rollback my row update");
				}
				if(gotOther is null || gotOther.Descr != "before_other" || gotOther.Owner != _ownerB){
					throw new Exception("permission denied should not update other row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatUpdUserLang_Should_ThrowDataIllegalOrConflict_OnUniqConflict", async(o)=>{
			var uniq1 = _token + "_upd_conflict_1";
			var uniq2 = _token + "_upd_conflict_2";
			var row1 = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = uniq1,
				RelLangType = ELangIdentType.Bcp47,
				RelLang = uniq1,
			};
			var row2 = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = uniq2,
				RelLangType = ELangIdentType.Bcp47,
				RelLang = uniq2,
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoUserLang.BatAdd(Ctx, AsyE(row1, row2), CT.None);
				return NIL;
			});
			_userLangIds.Add(row1.Id);
			_userLangIds.Add(row2.Id);

			row1.UniqName = uniq2;
			try{
				await SvcUserLang.BatUpdUserLang(MkUserCtx(_ownerA), AsyE(row1), CT.None);
				throw new Exception("BatUpdUserLang conflict should throw");
			}
			catch(Exception ex){
				AssertThrowsErrItem(ex, ItemsErr.Common.DataIllegalOrConflict, nameof(ISvcUserLang.BatUpdUserLang));
			}
			return NIL;
		});
	}
}
