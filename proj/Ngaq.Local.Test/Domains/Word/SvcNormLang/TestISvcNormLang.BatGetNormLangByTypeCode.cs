using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcNormLang{
	void RegisterBatGetNormLangByTypeCode(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcNormLang),
			[typeof(ISvcNormLang)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcNormLang.BatGetNormLangByTypeCode)];

		R("BatGetNormLangByTypeCode_Should_ReturnRowsByOwnerAndKeepInputOrder", async(o)=>{
			var got = SvcNormLang.BatGetNormLangByTypeCode(
				MkUserCtx(_ownerA),
				AsyE(
					(ELangIdentType.Bcp47, _token + "_zh_hant_tw"),
					(ELangIdentType.Bcp47, _token + "_not_exists"),
					(ELangIdentType.Bcp47, _token + "_en_us")
				),
				CT.None
			);
			var rows = await ToList(got);
			if(rows.Count != 3){
				throw new Exception("BatGetNormLangByTypeCode should align result count with input count");
			}
			if(rows[0]?.Code != _token + "_zh_hant_tw"){
				throw new Exception("BatGetNormLangByTypeCode should return the first matched row");
			}
			if(rows[1] is not null){
				throw new Exception("BatGetNormLangByTypeCode should return null for missing row");
			}
			if(rows[2]?.Code != _token + "_en_us"){
				throw new Exception("BatGetNormLangByTypeCode should return the third matched row");
			}
			return NIL;
		});

		R("BatGetNormLangByTypeCode_Should_IsolateByOwner", async(o)=>{
			var got = await SvcNormLang.BatGetNormLangByTypeCode(
				MkUserCtx(_ownerA),
				AsyE((ELangIdentType.Bcp47, _token + "_de_de")),
				CT.None
			).FirstOrDefaultAsync(CT.None);
			if(got is not null){
				throw new Exception("BatGetNormLangByTypeCode should isolate by owner");
			}
			return NIL;
		});
	}
}
