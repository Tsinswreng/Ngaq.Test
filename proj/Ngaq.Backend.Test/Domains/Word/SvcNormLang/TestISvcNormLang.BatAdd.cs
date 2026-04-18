using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcNormLang{
	void RegisterBatAddNormLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLang),
			[typeof(ISvcNormLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLang.BatAddNormLang)];

		R("BatAddNormLang_Should_CheckOwner_AndTouchBizUpdatedAt", async(o)=>{
			var row = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_add_ja_jp",
				NativeName = "before_add",
				BizUpdatedAt = Tempus.FromUnixMs(1),
			};
			await SvcNormLang.BatAddNormLang(MkUserCtx(_ownerA), AsyE(row), CT.None);
			_ids.Add(row.Id);

			await RunNoTxn(async(Ctx)=>{
				var got = await RepoNormLang.BatGetByIdWithDel(Ctx, AsyE(row.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(got is null){
					throw new Exception("BatAddNormLang should insert row");
				}
				if(got.Owner != _ownerA){
					throw new Exception("BatAddNormLang should keep checked owner");
				}
				if(got.BizUpdatedAt <= Tempus.FromUnixMs(1)){
					throw new Exception("BatAddNormLang should touch BizUpdatedAt");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatAddNormLang_Should_ThrowPermissionDenied_WhenOwnerMismatch", async(o)=>{
			var row = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerB,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_add_perm_denied",
			};
			try{
				await SvcNormLang.BatAddNormLang(MkUserCtx(_ownerA), AsyE(row), CT.None);
				throw new Exception("BatAddNormLang should throw permission denied");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, KeysErr.Common.PermissionDenied, nameof(ISvcNormLang.BatAddNormLang));
			}
			return NIL;
		});

		R("BatAddNormLang_Should_ThrowDataIllegalOrConflict_OnUniqConflict", async(o)=>{
			var code = _token + "_add_conflict";
			var existing = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = code,
				NativeName = "existing",
			};
			var neo = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = code,
				NativeName = "neo",
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLang.BatAdd(Ctx, AsyE(existing), CT.None);
				return NIL;
			});
			_ids.Add(existing.Id);
			_ids.Add(neo.Id);

			try{
				await SvcNormLang.BatAddNormLang(MkUserCtx(_ownerA), AsyE(neo), CT.None);
				throw new Exception("BatAddNormLang conflict should throw");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, KeysErr.Common.DataIllegalOrConflict, nameof(ISvcNormLang.BatAddNormLang));
			}
			return NIL;
		});
	}
}
