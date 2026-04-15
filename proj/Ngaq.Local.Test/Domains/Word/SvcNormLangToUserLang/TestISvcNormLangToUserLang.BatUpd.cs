using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcNormLangToUserLang{
	void RegisterBatUpdNormLangToUserLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLangToUserLang),
			[typeof(ISvcNormLangToUserLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLangToUserLang.BatUpdNormLangToUserLang)];

		R("BatUpdNormLangToUserLang_Should_UpdateOwnedRow_AndTouchBizUpdatedAt", async(o)=>{
			var row = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_upd_ko_kr",
				UserLang = _token + "_user_ko_old",
				Descr = "before_upd",
				BizUpdatedAt = Tempus.FromUnixMs(2000),
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLangToUserLang.BatAdd(Ctx, AsyE(row), CT.None);
				return NIL;
			});
			_ids.Add(row.Id);

			var upd = new PoNormLangToUserLang{
				Id = row.Id,
				Owner = _ownerA,
				NormLangType = row.NormLangType,
				NormLang = row.NormLang,
				UserLang = _token + "_user_ko_new",
				Descr = "after_upd",
				BizUpdatedAt = Tempus.FromUnixMs(1),
			};
			await SvcNormLangToUserLang.BatUpdNormLangToUserLang(MkUserCtx(_ownerA), AsyE(upd), CT.None);

			await RunNoTxn(async(Ctx)=>{
				var got = await RepoNormLangToUserLang.BatGetByIdWithDel(Ctx, AsyE(row.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(got is null){
					throw new Exception("BatUpdNormLangToUserLang should keep row");
				}
				if(got.UserLang != _token + "_user_ko_new"){
					throw new Exception("BatUpdNormLangToUserLang should update UserLang");
				}
				if(got.Descr != "after_upd"){
					throw new Exception("BatUpdNormLangToUserLang should update Descr");
				}
				if(got.BizUpdatedAt <= Tempus.FromUnixMs(1)){
					throw new Exception("BatUpdNormLangToUserLang should refresh BizUpdatedAt");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatUpdNormLangToUserLang_Should_ThrowPermissionDenied_WhenContainsNonOwnedRows", async(o)=>{
			var mine = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_upd_perm_mine",
				UserLang = _token + "_user_perm_mine",
				Descr = "before_mine",
			};
			var other = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerB,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_upd_perm_other",
				UserLang = _token + "_user_perm_other",
				Descr = "before_other",
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLangToUserLang.BatAdd(Ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_ids.Add(mine.Id);
			_ids.Add(other.Id);

			var updMine = new PoNormLangToUserLang{
				Id = mine.Id,
				Owner = mine.Owner,
				NormLangType = mine.NormLangType,
				NormLang = mine.NormLang,
				UserLang = _token + "_after_mine_should_not_apply",
				Descr = "after_mine_should_not_apply",
			};
			var updOther = new PoNormLangToUserLang{
				Id = other.Id,
				Owner = other.Owner,
				NormLangType = other.NormLangType,
				NormLang = other.NormLang,
				UserLang = _token + "_after_other_should_not_apply",
				Descr = "after_other_should_not_apply",
			};
			try{
				await SvcNormLangToUserLang.BatUpdNormLangToUserLang(MkUserCtx(_ownerA), AsyE(updMine, updOther), CT.None);
				throw new Exception("BatUpdNormLangToUserLang should throw permission denied");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, KeysErr.Common.PermissionDenied, nameof(ISvcNormLangToUserLang.BatUpdNormLangToUserLang));
			}

			await RunNoTxn(async(Ctx)=>{
				var gotMine = await RepoNormLangToUserLang.BatGetByIdWithDel(Ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoNormLangToUserLang.BatGetByIdWithDel(Ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || gotMine.Descr != "before_mine"){
					throw new Exception("permission denied should rollback my row update");
				}
				if(gotOther is null || gotOther.Descr != "before_other"){
					throw new Exception("permission denied should not update other row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatUpdNormLangToUserLang_Should_ThrowDataIllegalOrConflict_OnUniqConflict", async(o)=>{
			var norm1 = _token + "_upd_conflict_1";
			var norm2 = _token + "_upd_conflict_2";
			var row1 = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = norm1,
				UserLang = _token + "_upd_user_conflict_1",
			};
			var row2 = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = norm2,
				UserLang = _token + "_upd_user_conflict_2",
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLangToUserLang.BatAdd(Ctx, AsyE(row1, row2), CT.None);
				return NIL;
			});
			_ids.Add(row1.Id);
			_ids.Add(row2.Id);

			row1.NormLang = norm2;
			try{
				await SvcNormLangToUserLang.BatUpdNormLangToUserLang(MkUserCtx(_ownerA), AsyE(row1), CT.None);
				throw new Exception("BatUpdNormLangToUserLang conflict should throw");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, KeysErr.Common.DataIllegalOrConflict, nameof(ISvcNormLangToUserLang.BatUpdNormLangToUserLang));
			}
			return NIL;
		});
	}
}
