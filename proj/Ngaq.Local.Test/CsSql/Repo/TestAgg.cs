using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Infra.IF;

namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo{
	readonly List<IdWord> _aggWordIds = new();
	readonly List<IdWordProp> _aggPropIds = new();
	readonly List<IdWordLearn> _aggLearnIds = new();

	void RegisterAgg(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[
				typeof(IRepo<PoWord, IdWord>)
				,typeof(IRepo<PoWordProp, IdWordProp>)
				,typeof(IRepo<PoWordLearn, IdWordLearn>)
			]
			,[]
		);
		var R = register.Register;

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.BatAddAgg)];
		R("Agg_Insert_By_BatAddAgg", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var batch = new List<JnWord>();
				for(var i = 0; i < 2; i++){
					var word = new PoWord{
						Id = new IdWord(),
						Owner = IdUser.Zero,
						Head = "agg_word_" + System.Guid.NewGuid().ToString("N"),
						Lang = "en",
					};
					var prop1 = new PoWordProp{
						Id = new IdWordProp(),
						WordId = word.Id,
						KType = EKvType.Str,
						KStr = "tag",
						VType = EKvType.Str,
						VStr = "v_" + System.Guid.NewGuid().ToString("N"),
					};
					var learn1 = new PoWordLearn{
						Id = new IdWordLearn(),
						WordId = word.Id,
						LearnResult = ELearn.Add,
					};
					batch.Add(new JnWord{
						Word = word,
						Props = [prop1],
						Learns = [learn1],
					});
				}

				var resp = await RepoWord.BatAddAgg<JnWord>(Ctx, AsyE(batch.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("BatAddAgg returned null response");
				}

				_aggWordIds.Clear();
				_aggPropIds.Clear();
				_aggLearnIds.Clear();
				_aggWordIds.AddRange(batch.Select(x=>x.Word.Id));
				_aggPropIds.AddRange(batch.SelectMany(x=>x.Props.Select(y=>y.Id)));
				_aggLearnIds.AddRange(batch.SelectMany(x=>x.Learns.Select(y=>y.Id)));
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.BatGetAggById)];
		R("Agg_BatGet_By_Id", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_Insert_By_BatAddAgg not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var gotAsy = await RepoWord.BatGetAggById<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				var got = new List<JnWord?>();
				await foreach(var item in gotAsy){
					got.Add(item);
				}

				if(got.Count != _aggWordIds.Count){
					throw new Exception($"Expected {_aggWordIds.Count} rows, got {got.Count}");
				}
				for(var i = 0; i < got.Count; i++){
					var one = got[i];
					if(one is null){
						throw new Exception($"Expected non-null aggregate at index {i}");
					}
					if(!one.Word.Id.Equals(_aggWordIds[i])){
						throw new Exception($"Word id mismatch at index {i}");
					}
					if(one.Props.Count < 1 || one.Learns.Count < 1){
						throw new Exception($"Expected include rows for aggregate at index {i}");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.GetAllAgg)];
		R("Agg_GetAllAgg_Should_Contain_Inserted", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_Insert_By_BatAddAgg not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var gotAsy = await RepoWord.GetAllAgg<JnWord>(Ctx, CT.None);
				var found = new HashSet<IdWord>();
				await foreach(var agg in gotAsy){
					if(_aggWordIds.Contains(agg.Word.Id)){
						found.Add(agg.Word.Id);
					}
				}

				foreach(var id in _aggWordIds){
					if(!found.Contains(id)){
						throw new Exception($"GetAllAgg missing inserted word id: {id}");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.BatSoftDelAggById)];
		R("Agg_SoftDelete_Root_And_Includes", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_Insert_By_BatAddAgg not executed");
			}

			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await RepoWord.BatSoftDelAggById<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("BatSoftDelAggById returned null response");
				}

				var wordsAsy = await RepoWord.BatGetById(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				await foreach(var w in wordsAsy){
					if(w is null || !w.IsDeleted()){
						throw new Exception("Expected all word rows soft deleted");
					}
				}
				var propsAsy = await RepoProp.BatGetById(Ctx, AsyE(_aggPropIds.ToArray()), CT.None);
				await foreach(var p in propsAsy){
					if(p is null || !p.IsDeleted()){
						throw new Exception("Expected all prop rows soft deleted");
					}
				}
				var learnsAsy = await RepoLearn.BatGetById(Ctx, AsyE(_aggLearnIds.ToArray()), CT.None);
				await foreach(var l in learnsAsy){
					if(l is null || !l.IsDeleted()){
						throw new Exception("Expected all learn rows soft deleted");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.BatHardDelAggById)];
		R("Agg_HardDelete_Root_And_Includes", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_Insert_By_BatAddAgg not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await RepoWord.BatHardDelAggById<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("BatHardDelAggById returned null response");
				}

				var wordsAsy = await RepoWord.BatGetById(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				await foreach(var w in wordsAsy){
					if(w is not null){
						throw new Exception("Expected word row hard deleted");
					}
				}
				var propsAsy = await RepoProp.BatGetById(Ctx, AsyE(_aggPropIds.ToArray()), CT.None);
				await foreach(var p in propsAsy){
					if(p is not null){
						throw new Exception("Expected prop row hard deleted");
					}
				}
				var learnsAsy = await RepoLearn.BatGetById(Ctx, AsyE(_aggLearnIds.ToArray()), CT.None);
				await foreach(var l in learnsAsy){
					if(l is not null){
						throw new Exception("Expected learn row hard deleted");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.BatHardDelAggById)];
		R("Agg_Cleanup_HardDelete", async(o)=>{
			if(_aggWordIds.Count == 0){
				return NIL;
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				await RepoWord.BatHardDelAggById<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				return NIL;
			});
		});
	}
}
