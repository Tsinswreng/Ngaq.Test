using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcNormLangToUserLang{
	void RegisterBatSoftDelNormLangToUserLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLangToUserLang),
			[typeof(ISvcNormLangToUserLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLangToUserLang.BatSoftDelNormLangToUserLang)];

		R("BatSoftDelNormLangToUserLang_Should_SoftDeleteAndHideFromGet", async(o)=>{
			var row = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerA,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_soft_del_es_es",
				UserLang = _token + "_user_es",
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLangToUserLang.BatAdd(Ctx, AsyE(row), CT.None);
				return NIL;
			});
			_ids.Add(row.Id);

			await SvcNormLangToUserLang.BatSoftDelNormLangToUserLang(MkUserCtx(_ownerA), AsyE(row), CT.None);

			await RunNoTxn(async(Ctx)=>{
				var got = await RepoNormLangToUserLang.BatGetByIdWithDel(Ctx, AsyE(row.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(got is null || !got.IsDeleted()){
					throw new Exception("BatSoftDelNormLangToUserLang should soft delete row");
				}
				return NIL;
			});

			var mapped = await SvcNormLangToUserLang.GetUserLangByNormLang(
				MkUserCtx(_ownerA),
				ELangIdentType.Bcp47,
				row.NormLang!,
				CT.None
			);
			if(mapped is not null){
				throw new Exception("soft-deleted mapping should be hidden from GetUserLangByNormLang");
			}
			return NIL;
		});

		R("BatSoftDelNormLangToUserLang_Should_ThrowPermissionDenied_WhenOwnerMismatch", async(o)=>{
			var row = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = _ownerB,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = _token + "_soft_del_perm_denied",
				UserLang = _token + "_user_soft_del_perm_denied",
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLangToUserLang.BatAdd(Ctx, AsyE(row), CT.None);
				return NIL;
			});
			_ids.Add(row.Id);

			try{
				await SvcNormLangToUserLang.BatSoftDelNormLangToUserLang(MkUserCtx(_ownerA), AsyE(row), CT.None);
				throw new Exception("BatSoftDelNormLangToUserLang should throw permission denied");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, KeysErr.Common.PermissionDenied, nameof(ISvcNormLangToUserLang.BatSoftDelNormLangToUserLang));
			}
			return NIL;
		});
	}
}
