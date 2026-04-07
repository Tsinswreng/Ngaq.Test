using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsErr;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterLlmDictWordToJnWord(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2)
			,[typeof(ISvcWordV2)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.LlmDictWordToJnWord)];

		R("LlmDictWordToJnWord_WhenMapped_Should_MapLangAndBuildProps", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_llm_map_" + Guid.NewGuid().ToString("N");
			var map = new PoNormLangToUserLang{
				Id = new IdNormLangToUserLang(),
				Owner = owner,
				NormLangType = ELangIdentType.Bcp47,
				NormLang = token + "_en_us",
				UserLang = token + "_my_en",
				Descr = "test map",
			};
			var req = new ReqLlmDict{
				Query = new Query{
					Term = "hello",
				},
				OptLang = new OptLang{
					SrcLang = new NormLangWithName{
						Type = ELangIdentType.Bcp47,
						Code = map.NormLang!,
					},
				},
			};
			var resp = new RespLlmDict{
				Head = token + "_head",
				Descrs = [token + "_d1", token + "_d2"],
				Pronunciations = [
					new TextedPronunciation{
						TextType = "Ipa",
						Text = token + "_ipa",
					}
				],
			};

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoNormLangToUserLang.BatAdd(Ctx, AsyE(map), CT.None);
					return NIL;
				});

				var got = await SvcWordV2.LlmDictWordToJnWord(MkUserCtx(owner), req, resp, CT.None);
				if(got.Owner != owner){
					throw new Exception("LlmDictWordToJnWord should set owner from context");
				}
				if(got.Head != resp.Head){
					throw new Exception("LlmDictWordToJnWord should map head from resp");
				}
				if(got.Lang != map.UserLang){
					throw new Exception("LlmDictWordToJnWord should map lang via NormLangToUserLang");
				}
				var descCnt = got.Props.Count(x=>x.KStr == KeysProp.Inst.description);
				var pronCnt = got.Props.Count(x=>x.KStr == KeysProp.Inst.pronunciation);
				if(descCnt != 2 || pronCnt != 1){
					throw new Exception("LlmDictWordToJnWord should map description and pronunciation props");
				}
				if(got.Props.Any(x=>x.WordId != got.Id)){
					throw new Exception("LlmDictWordToJnWord should ensure foreign keys");
				}
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoNormLangToUserLang.BatHardDelById(Ctx, AsyE(map.Id), CT.None);
					return NIL;
				});
			}
		});

		R("LlmDictWordToJnWord_WhenNotMapped_Should_ThrowNormLangToUserLangIsNotMapped", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_llm_unmap_" + Guid.NewGuid().ToString("N");
			var req = new ReqLlmDict{
				Query = new Query{
					Term = "hello",
				},
				OptLang = new OptLang{
					SrcLang = new NormLangWithName{
						Type = ELangIdentType.Bcp47,
						Code = token + "_no_map_lang",
					},
				},
			};
			var resp = new RespLlmDict{
				Head = token + "_head",
				Descrs = [token + "_d1"],
				Pronunciations = [],
			};

			try{
				_ = await SvcWordV2.LlmDictWordToJnWord(MkUserCtx(owner), req, resp, CT.None);
				throw new Exception("LlmDictWordToJnWord should throw when mapping is missing");
			}
			catch(Exception ex){
				if(ex is not AppErr appErr){
					throw new Exception("LlmDictWordToJnWord should throw AppErr when mapping is missing");
				}
				if(!ReferenceEquals(appErr.Type, ItemsErr.Word.NormLangToUserLangIsNotMapped)){
					throw new Exception("LlmDictWordToJnWord should throw NormLangToUserLangIsNotMapped");
				}
			}
			return NIL;
		});
	}
}
