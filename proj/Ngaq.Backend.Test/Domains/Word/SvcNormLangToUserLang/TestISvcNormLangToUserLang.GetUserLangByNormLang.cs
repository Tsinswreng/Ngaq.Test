using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcNormLangToUserLang{
	void RegisterGetUserLangByNormLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLangToUserLang),
			[typeof(ISvcNormLangToUserLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLangToUserLang.GetUserLangByNormLang)];

		R("GetUserLangByNormLang_Should_ReturnMappedUserLang_ForOwner", async(o)=>{
			var got = await SvcNormLangToUserLang.GetUserLangByNormLang(
				MkUserCtx(_ownerA),
				ELangIdentType.Bcp47,
				_token + "_zh_hant_tw",
				CT.None
			);
			if(got != _token + "_user_zh"){
				throw new Exception("GetUserLangByNormLang should return mapped user lang for owner");
			}
			return NIL;
		});

		R("GetUserLangByNormLang_Should_ReturnNull_WhenOnlyOtherOwnerHasMapping", async(o)=>{
			var got = await SvcNormLangToUserLang.GetUserLangByNormLang(
				MkUserCtx(_ownerA),
				ELangIdentType.Bcp47,
				_token + "_de_de",
				CT.None
			);
			if(got is not null){
				throw new Exception("GetUserLangByNormLang should isolate by owner");
			}
			return NIL;
		});
	}
}
