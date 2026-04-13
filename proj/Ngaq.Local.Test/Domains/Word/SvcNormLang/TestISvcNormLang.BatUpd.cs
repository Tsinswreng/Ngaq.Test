using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcNormLang{
	void RegisterBatUpdNormLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLang),
			[typeof(ISvcNormLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLang.BatUpdNormLang)];

		R("BatUpdNormLang_Should_UpdateOwnedRow_AndTouchBizUpdatedAt", async(o)=>{
			var row = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_upd_ko_kr",
				NativeName = "before_upd",
				BizUpdatedAt = Tempus.FromUnixMs(2000),
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLang.BatAdd(Ctx, AsyE(row), CT.None);
				return NIL;
			});
			_ids.Add(row.Id);

			var upd = new PoNormLang{
				Id = row.Id,
				Owner = _ownerA,
				Type = row.Type,
				Code = row.Code,
				NativeName = "after_upd",
				BizUpdatedAt = Tempus.FromUnixMs(1),
			};
			await SvcNormLang.BatUpdNormLang(MkUserCtx(_ownerA), AsyE(upd), CT.None);

			await RunNoTxn(async(Ctx)=>{
				var got = await RepoNormLang.BatGetByIdWithDel(Ctx, AsyE(row.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(got is null){
					throw new Exception("BatUpdNormLang should keep row");
				}
				if(got.NativeName != "after_upd"){
					throw new Exception("BatUpdNormLang should update NativeName");
				}
				if(got.BizUpdatedAt <= Tempus.FromUnixMs(1)){
					throw new Exception("BatUpdNormLang should refresh BizUpdatedAt");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatUpdNormLang_Should_ThrowPermissionDenied_WhenContainsNonOwnedRows", async(o)=>{
			var mine = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_upd_perm_mine",
				NativeName = "before_mine",
			};
			var other = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerB,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_upd_perm_other",
				NativeName = "before_other",
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLang.BatAdd(Ctx, AsyE(mine, other), CT.None);
				return NIL;
			});
			_ids.Add(mine.Id);
			_ids.Add(other.Id);

			var updMine = new PoNormLang{
				Id = mine.Id,
				Owner = mine.Owner,
				Type = mine.Type,
				Code = mine.Code,
				NativeName = "after_mine_should_not_apply",
			};
			var updOther = new PoNormLang{
				Id = other.Id,
				Owner = other.Owner,
				Type = other.Type,
				Code = other.Code,
				NativeName = "after_other_should_not_apply",
			};
			try{
				await SvcNormLang.BatUpdNormLang(MkUserCtx(_ownerA), AsyE(updMine, updOther), CT.None);
				throw new Exception("BatUpdNormLang should throw permission denied");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, ItemsErr.Common.PermissionDenied, nameof(ISvcNormLang.BatUpdNormLang));
			}

			await RunNoTxn(async(Ctx)=>{
				var gotMine = await RepoNormLang.BatGetByIdWithDel(Ctx, AsyE(mine.Id), CT.None).FirstOrDefaultAsync(CT.None);
				var gotOther = await RepoNormLang.BatGetByIdWithDel(Ctx, AsyE(other.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(gotMine is null || gotMine.NativeName != "before_mine"){
					throw new Exception("permission denied should rollback my row update");
				}
				if(gotOther is null || gotOther.NativeName != "before_other"){
					throw new Exception("permission denied should not update other row");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatUpdNormLang_Should_ThrowDataIllegalOrConflict_OnUniqConflict", async(o)=>{
			var code1 = _token + "_upd_conflict_1";
			var code2 = _token + "_upd_conflict_2";
			var row1 = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = code1,
			};
			var row2 = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = code2,
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLang.BatAdd(Ctx, AsyE(row1, row2), CT.None);
				return NIL;
			});
			_ids.Add(row1.Id);
			_ids.Add(row2.Id);

			row1.Code = code2;
			try{
				await SvcNormLang.BatUpdNormLang(MkUserCtx(_ownerA), AsyE(row1), CT.None);
				throw new Exception("BatUpdNormLang conflict should throw");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, ItemsErr.Common.DataIllegalOrConflict, nameof(ISvcNormLang.BatUpdNormLang));
			}
			return NIL;
		});
	}
}
