using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcNormLang{
	void RegisterBatSoftDelNormLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLang),
			[typeof(ISvcNormLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLang.BatSoftDelNormLang)];

		R("BatSoftDelNormLang_Should_SoftDeleteAndHideFromGet", async(o)=>{
			var row = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerA,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_soft_del_es_es",
				NativeName = "seed_soft_del",
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLang.BatAdd(Ctx, AsyE(row), CT.None);
				return NIL;
			});
			_ids.Add(row.Id);

			await SvcNormLang.BatSoftDelNormLang(MkUserCtx(_ownerA), AsyE(row), CT.None);

			await RunNoTxn(async(Ctx)=>{
				var got = await RepoNormLang.BatGetByIdWithDel(Ctx, AsyE(row.Id), CT.None).FirstOrDefaultAsync(CT.None);
				if(got is null || !got.IsDeleted()){
					throw new Exception("BatSoftDelNormLang should soft delete row");
				}
				return NIL;
			});

			var gotBySvc = await SvcNormLang.BatGetNormLangByTypeCode(
				MkUserCtx(_ownerA),
				AsyE((row.Type, row.Code)),
				CT.None
			).FirstOrDefaultAsync(CT.None);
			if(gotBySvc is not null){
				throw new Exception("soft-deleted row should be hidden from BatGetNormLangByTypeCode");
			}
			return NIL;
		});

		R("BatSoftDelNormLang_Should_ThrowPermissionDenied_WhenOwnerMismatch", async(o)=>{
			var row = new PoNormLang{
				Id = new IdNormLang(),
				Owner = _ownerB,
				Type = ELangIdentType.Bcp47,
				Code = _token + "_soft_del_perm_denied",
				NativeName = "seed_soft_perm",
			};
			await RunNoTxn(async(Ctx)=>{
				await RepoNormLang.BatAdd(Ctx, AsyE(row), CT.None);
				return NIL;
			});
			_ids.Add(row.Id);

			try{
				await SvcNormLang.BatSoftDelNormLang(MkUserCtx(_ownerA), AsyE(row), CT.None);
				throw new Exception("BatSoftDelNormLang should throw permission denied");
			}
			catch(Exception Ex){
				AssertThrowsErrItem(Ex, KeysErr.Common.PermissionDenied, nameof(ISvcNormLang.BatSoftDelNormLang));
			}
			return NIL;
		});
	}
}
