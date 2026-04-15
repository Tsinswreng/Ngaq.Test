using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcNormLangToUserLang{
	void RegisterBatAddNormLangToUserLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLangToUserLang),
			[typeof(ISvcNormLangToUserLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLangToUserLang.BatAddNormLangToUserLang)];

		R("BatAddNormLangToUserLang_Should_CheckOwner_AndTouchBizUpdatedAt", async(o)=>{
			var row = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_add_ja_jp",
				UserLang = _token + "_user_ja",
				Descr = "before_add",
				BizUpdatedAt = Tempus.FromUnixMs(1),
			};
			await SvcNormLangToUserLang.BatAddNormLangToUserLang(MkUserCtx(_ownerA), AsyE(row), CT.None);
			_ids.Add(row.Id);

			await RunNoTxn(async(Ctx)=>{
				var got = await RepoNormLangToUserLang.BatGetByIdWithDel(Ctx, AsyE(row.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(got is null){
					throw new Exception("BatAddNormLangToUserLang should insert row");
				}
				if(got.Owner != _ownerA){
					throw new Exception("BatAddNormLangToUserLang should keep checked owner");
				}
				if(got.BizUpdatedAt <= Tempus.FromUnixMs(1)){
					throw new Exception("BatAddNormLangToUserLang should touch BizUpdatedAt");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatAddNormLangToUserLang_Should_ThrowPermissionDenied_WhenOwnerMismatch", async(o)=>{
			var row = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerB,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_add_perm_denied",
				UserLang = _token + "_user_perm_denied",
			};
			try{
				await SvcNormLangToUserLang.BatAddNormLangToUserLang(MkUserCtx(_ownerA), AsyE(row), CT.None);
				throw new Exception("BatAddNormLangToUserLang should throw permission denied");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, KeysErr.Common.PermissionDenied, nameof(ISvcNormLangToUserLang.BatAddNormLangToUserLang));
			}
			return NIL;
		});

		R("BatAddNormLangToUserLang_Should_ThrowDataIllegalOrConflict_OnUniqConflict", async(o)=>{
			var normLang = _token + "_add_conflict";
			var existing = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = normLang,
				UserLang = _token + "_user_conflict_1",
			};
			var neo = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = normLang,
				UserLang = _token + "_user_conflict_2",
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLangToUserLang.BatAdd(Ctx, AsyE(existing), CT.None);
				return NIL;
			});
			_ids.Add(existing.Id);
			_ids.Add(neo.Id);

			try{
				await SvcNormLangToUserLang.BatAddNormLangToUserLang(MkUserCtx(_ownerA), AsyE(neo), CT.None);
				throw new Exception("BatAddNormLangToUserLang conflict should throw");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, KeysErr.Common.DataIllegalOrConflict, nameof(ISvcNormLangToUserLang.BatAddNormLangToUserLang));
			}
			return NIL;
		});
	}
}
