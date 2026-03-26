using System.Text;
using Ngaq.Core.Frontend.Kv;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Tools;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Core.Sys.Models;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterGetWordsToLearn(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2)
			,[typeof(ISvcWordV2)]
			,[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.GetWordsToLearn)];

		R("GetWordsToLearn_Should_ReturnOnlyOwnerWords_And_OverloadConsistent", async(o)=>{
			var ownerA = new IdUser();
			var ownerB = new IdUser();
			var token = "ut_wv2_get_" + Guid.NewGuid().ToString("N");
			var words = new[]{
				new PoWord{Id = new IdWord(), Owner = ownerA, Head = token + "_a_1", Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = ownerA, Head = token + "_a_2", Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = ownerB, Head = token + "_b_1", Lang = "en"},
			};

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(words), CT.None);
					return NIL;
				});

				var ctxA = MkUserCtx(ownerA);
				var gotA_1 = await ToList(SvcWordV2.GetWordsToLearn(ctxA, CT.None));
				var gotA_2 = await ToList(SvcWordV2.GetWordsToLearn(ctxA, (PreFilter?)null, CT.None));

				var fromFn1 = gotA_1.Where(x=>x.Word.Head.StartsWith(token)).ToList();
				var fromFn2 = gotA_2.Where(x=>x.Word.Head.StartsWith(token)).ToList();

				if(fromFn1.Count != 2 || fromFn1.Any(x=>x.Word.Owner != ownerA)){
					throw new Exception("GetWordsToLearn(owner) owner-isolation assert failed");
				}
				if(fromFn2.Count != 2 || fromFn2.Any(x=>x.Word.Owner != ownerA)){
					throw new Exception("GetWordsToLearn(owner,prefilter) owner-isolation assert failed");
				}

				var ids1 = fromFn1.Select(x=>x.Word.Id).OrderBy(x=>x.ToString()).ToArray();
				var ids2 = fromFn2.Select(x=>x.Word.Id).OrderBy(x=>x.ToString()).ToArray();
				if(!ids1.SequenceEqual(ids2)){
					throw new Exception("two overloads of GetWordsToLearn are inconsistent for null prefilter");
				}

				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatHardDelById(Ctx, AsyE(words.Select(x=>x.Id).ToArray()), CT.None);
					return NIL;
				});
			}
		});

		R("GetWordsToLearn_WhenUserHasNoWords_Should_Return_EmptyAsyE", async(o)=>{
			var owner = new IdUser();
			var got1 = await ToList(SvcWordV2.GetWordsToLearn(MkUserCtx(owner), CT.None));
			var got2 = await ToList(SvcWordV2.GetWordsToLearn(MkUserCtx(owner), (PreFilter?)null, CT.None));
			var got3 = await ToList(SvcWordV2.GetWordsToLearn(MkUserCtx(owner), new PreFilter(), CT.None));

			if(got1.Count != 0 || got2.Count != 0 || got3.Count != 0){
				throw new Exception("expected empty async enumerable when user has no words");
			}
			return NIL;
		});

		R("GetWordsToLearn_Should_ExcludeSoftDeletedWords", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_soft_ex_" + Guid.NewGuid().ToString("N");
			var keep = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_keep", Lang = "en"};
			var del = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_del", Lang = "en"};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(keep, del), CT.None);
					await RepoWord.SoftDelInId(Ctx, AsyE(del.Id), CT.None);
					return NIL;
				});

				var got = await ToList(SvcWordV2.GetWordsToLearn(MkUserCtx(owner), CT.None));
				var tokenWords = got.Where(x=>x.Word.Head.StartsWith(token)).ToList();
				if(tokenWords.Count != 1 || tokenWords[0].Word.Id != keep.Id){
					throw new Exception("GetWordsToLearn should exclude soft deleted words");
				}
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatHardDelById(Ctx, AsyE(keep.Id, del.Id), CT.None);
					return NIL;
				});
			}
		});

		R("GetWordsToLearn_WithExplicitCorePreFilter_Should_FilterByLang", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_pf_core_" + Guid.NewGuid().ToString("N");
			var words = new[]{
				new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_en_1", Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_en_2", Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_jp_1", Lang = "jp"},
			};
			var preFilter = new PreFilter{
				CoreFilter = [
					new FieldsFilter{
						Fields = [nameof(PoWord.Lang)],
						Filters = [
							new FilterItem{
								Operation = EFilterOperationMode.IncludeAny,
								ValueType = EValueType.String,
								Values = ["en"],
							}
						],
					}
				],
			};

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(words), CT.None);
					return NIL;
				});

				var got = await ToList(SvcWordV2.GetWordsToLearn(MkUserCtx(owner), preFilter, CT.None));
				var tokenWords = got.Where(x=>x.Word.Head.StartsWith(token)).ToList();
				if(tokenWords.Count != 2 || tokenWords.Any(x=>x.Word.Lang != "en")){
					throw new Exception("explicit core prefilter should keep only matched lang words");
				}
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatHardDelById(Ctx, AsyE(words.Select(x=>x.Id).ToArray()), CT.None);
					return NIL;
				});
			}
		});

		R("GetWordsToLearn_Should_UseCurStudyPlanPreFilter_WhenNoPrefilterArgument", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_cur_sp_pf_" + Guid.NewGuid().ToString("N");
			var words = new[]{
				new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_en_1", Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_jp_1", Lang = "jp"},
			};
			var preFilter = new PreFilter{
				CoreFilter = [
					new FieldsFilter{
						Fields = [nameof(PoWord.Lang)],
						Filters = [
							new FilterItem{
								Operation = EFilterOperationMode.IncludeAny,
								ValueType = EValueType.String,
								Values = ["jp"],
							}
						],
					}
				],
			};
			var poPreFilter = new PoPreFilter{
				Id = new IdPreFilter(),
				Owner = owner,
				Type = EPreFilterType.Json,
				Data = Encoding.UTF8.GetBytes(JSON.stringify(preFilter)),
			};
			var poStudyPlan = new PoStudyPlan{
				Id = new IdStudyPlan(),
				Owner = owner,
				PreFilterId = poPreFilter.Id,
			};
			var poCurStudyPlanKv = new PoKv{
				Id = new IdKv(),
				Owner = owner,
			};
			poCurStudyPlanKv.SetStrStr(KeysClientKv.CurStudyPlanId+"", poStudyPlan.Id+"");

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(words), CT.None);
					await RepoPreFilter.BatAdd(Ctx, AsyE(poPreFilter), CT.None);
					await RepoStudyPlan.BatAdd(Ctx, AsyE(poStudyPlan), CT.None);
					await RepoKv.BatAdd(Ctx, AsyE(poCurStudyPlanKv), CT.None);
					return NIL;
				});

				var got = await ToList(SvcWordV2.GetWordsToLearn(MkUserCtx(owner), CT.None));
				var tokenWords = got.Where(x=>x.Word.Head.StartsWith(token)).ToList();
				if(tokenWords.Count != 1 || tokenWords[0].Word.Lang != "jp"){
					throw new Exception("GetWordsToLearn should apply current study-plan prefilter when no prefilter argument");
				}
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoKv.BatHardDelById(Ctx, AsyE(poCurStudyPlanKv.Id), CT.None);
					await RepoStudyPlan.BatHardDelById(Ctx, AsyE(poStudyPlan.Id), CT.None);
					await RepoPreFilter.BatHardDelById(Ctx, AsyE(poPreFilter.Id), CT.None);
					await RepoWord.BatHardDelById(Ctx, AsyE(words.Select(x=>x.Id).ToArray()), CT.None);
					return NIL;
				});
			}
		});
	}
}
