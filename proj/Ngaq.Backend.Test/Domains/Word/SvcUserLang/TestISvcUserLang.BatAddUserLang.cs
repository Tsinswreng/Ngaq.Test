using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcUserLang{
	void RegisterBatAddUserLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcUserLang)
			,[typeof(ISvcUserLang)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcUserLang.OrdAddUserLang)];

		R("BatAddUserLang_Should_CheckOwner_AndTouchBizUpdatedAt", async(o)=>{
			var row = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = _token + "_add_a_1",
				Descr = "before_add",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_add_a_1",
				BizUpdatedAt = UnixMs.FromUnixMs(1),
			};
			await SvcUserLang.OrdAddUserLang(MkUserCtx(_ownerA), AsyE(row), CT.None);
			_userLangIds.Add(row.Id);

			await RunNoTxn(async(Ctx)=>{
				var got = await RepoUserLang.OrdGetByIdWithDel(Ctx, AsyE(row.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(got is null){
					throw new Exception("BatAddUserLang should insert row");
				}
				if(got.Owner != _ownerA){
					throw new Exception("BatAddUserLang should keep checked owner");
				}
				if(got.BizUpdatedAt <= UnixMs.FromUnixMs(1)){
					throw new Exception("BatAddUserLang should touch BizUpdatedAt");
				}
				return NIL;
			});
			return NIL;
		});

		R("BatAddUserLang_Should_ThrowPermissionDenied_WhenOwnerMismatch", async(o)=>{
			var row = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerB,
				UniqName = _token + "_add_perm_denied",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_add_perm_denied",
			};
			try{
				await SvcUserLang.OrdAddUserLang(MkUserCtx(_ownerA), AsyE(row), CT.None);
				throw new Exception("BatAddUserLang should throw permission denied");
			}
			catch(Exception ex){
				AssertThrowsErrItem(ex, KeysErr.Common.PermissionDenied, nameof(ISvcUserLang.OrdAddUserLang));
			}
			return NIL;
		});

		R("BatAddUserLang_Should_ThrowDataIllegalOrConflict_OnUniqConflict", async(o)=>{
			var uniq = _token + "_add_conflict";
			var existing = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = uniq,
				RelLangType = ELangIdentType.Bcp47,
				RelLang = uniq,
			};
			var neo = new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = uniq,
				RelLangType = ELangIdentType.Bcp47,
				RelLang = uniq,
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoUserLang.OrdAdd(Ctx, AsyE(existing), CT.None);
				return NIL;
			});
			_userLangIds.Add(existing.Id);
			_userLangIds.Add(neo.Id);

			try{
				await SvcUserLang.OrdAddUserLang(MkUserCtx(_ownerA), AsyE(neo), CT.None);
				throw new Exception("BatAddUserLang conflict should throw");
			}
			catch(Exception ex){
				AssertThrowsErrItem(ex, KeysErr.Common.DataIllegalOrConflict, nameof(ISvcUserLang.OrdAddUserLang));
			}
			return NIL;
		});
	}
}
