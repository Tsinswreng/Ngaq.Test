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

namespace Ngaq.Backend.Test.CsSql.Repo;

public partial class TestRepo{
	readonly List<IdWord> _aggWordIds = new();
	readonly List<IdWordProp> _aggPropIds = new();
	readonly List<IdWordLearn> _aggLearnIds = new();
	readonly List<IdWordProp> _aggPrevPropIds = new();
	readonly List<IdWordLearn> _aggPrevLearnIds = new();

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

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.OrdAddAgg)];
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

				var resp = await RepoWord.OrdAddAgg<JnWord>(Ctx, AsyE(batch.ToArray()), CT.None);
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

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.OrdGetAggByIdWithDel)];
		R("Agg_BatGet_By_Id", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_Insert_By_BatAddAgg not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var gotAsy = RepoWord.OrdGetAggByIdWithDel<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
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
				var gotAsy = RepoWord.GetAllAgg<JnWord>(Ctx, CT.None);
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

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.OrdHardUpdAgg)];
		R("Agg_HardUpd_Should_Replace_Includes", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_Insert_By_BatAddAgg not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				_aggPrevPropIds.Clear();
				_aggPrevLearnIds.Clear();
				_aggPrevPropIds.AddRange(_aggPropIds);
				_aggPrevLearnIds.AddRange(_aggLearnIds);

				var upds = new List<JnWord>();
				_aggPropIds.Clear();
				_aggLearnIds.Clear();
				foreach(var wordId in _aggWordIds){
					var w = new PoWord{
						Id = wordId,
						Owner = IdUser.Zero,
						Head = "agg_hard_upd_" + System.Guid.NewGuid().ToString("N"),
						Lang = "en",
					};
					var p = new PoWordProp{
						Id = new IdWordProp(),
						WordId = wordId,
						KType = EKvType.Str,
						KStr = "hard",
						VType = EKvType.Str,
						VStr = "hard_" + System.Guid.NewGuid().ToString("N"),
					};
					var l = new PoWordLearn{
						Id = new IdWordLearn(),
						WordId = wordId,
						LearnResult = ELearn.Rmb,
					};
					upds.Add(new JnWord{
						Word = w,
						Props = [p],
						Learns = [l],
					});
					_aggPropIds.Add(p.Id);
					_aggLearnIds.Add(l.Id);
				}

				await RepoWord.OrdHardUpdAgg<JnWord>(Ctx, AsyE(upds.ToArray()), CT.None);

				var got = RepoWord.OrdGetAggByIdWithDel<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				await foreach(var one in got){
					if(one is null || one.Props.Count != 1 || one.Learns.Count != 1){
						throw new Exception("HardUpd result mismatch");
					}
				}

				var oldProps = RepoProp.OrdGetByIdWithDel(Ctx, AsyE(_aggPrevPropIds.ToArray()), CT.None);
				await foreach(var old in oldProps){
					if(old is not null){
						throw new Exception("HardUpd should hard-delete removed props");
					}
				}
				var oldLearns = RepoLearn.OrdGetByIdWithDel(Ctx, AsyE(_aggPrevLearnIds.ToArray()), CT.None);
				await foreach(var old in oldLearns){
					if(old is not null){
						throw new Exception("HardUpd should hard-delete removed learns");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.OrdSoftUpdAgg)];
		R("Agg_SoftUpd_Should_SoftDelete_Missing", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_HardUpd_Should_Replace_Includes not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				_aggPrevPropIds.Clear();
				_aggPrevLearnIds.Clear();
				_aggPrevPropIds.AddRange(_aggPropIds);
				_aggPrevLearnIds.AddRange(_aggLearnIds);

				var upds = new List<JnWord>();
				_aggPropIds.Clear();
				_aggLearnIds.Clear();
				foreach(var wordId in _aggWordIds){
					var w = new PoWord{
						Id = wordId,
						Owner = IdUser.Zero,
						Head = "agg_soft_upd_" + System.Guid.NewGuid().ToString("N"),
						Lang = "en",
					};
					var p = new PoWordProp{
						Id = new IdWordProp(),
						WordId = wordId,
						KType = EKvType.Str,
						KStr = "soft",
						VType = EKvType.Str,
						VStr = "soft_" + System.Guid.NewGuid().ToString("N"),
					};
					var l = new PoWordLearn{
						Id = new IdWordLearn(),
						WordId = wordId,
						LearnResult = ELearn.Add,
					};
					upds.Add(new JnWord{
						Word = w,
						Props = [p],
						Learns = [l],
					});
					_aggPropIds.Add(p.Id);
					_aggLearnIds.Add(l.Id);
				}

				await RepoWord.OrdSoftUpdAgg<JnWord>(Ctx, AsyE(upds.ToArray()), CT.None);

				var oldProps = RepoProp.OrdGetByIdWithDel(Ctx, AsyE(_aggPrevPropIds.ToArray()), CT.None);
				await foreach(var old in oldProps){
					if(old is null || !old.IsDeleted()){
						throw new Exception("SoftUpd should soft-delete removed props");
					}
				}
				var oldLearns = RepoLearn.OrdGetByIdWithDel(Ctx, AsyE(_aggPrevLearnIds.ToArray()), CT.None);
				await foreach(var old in oldLearns){
					if(old is null || !old.IsDeleted()){
						throw new Exception("SoftUpd should soft-delete removed learns");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.SoftDelAggInId)];
		R("Agg_SoftDelete_Root_And_Includes", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_Insert_By_BatAddAgg not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await RepoWord.SoftDelAggInId<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("SoftDelAggInId returned null response");
				}

				var wordsAsy = RepoWord.OrdGetByIdWithDel(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				await foreach(var w in wordsAsy){
					if(w is null || !w.IsDeleted()){
						throw new Exception("Expected all word rows soft deleted");
					}
				}
				var propsAsy = RepoProp.OrdGetByIdWithDel(Ctx, AsyE(_aggPropIds.ToArray()), CT.None);
				await foreach(var p in propsAsy){
					if(p is null || !p.IsDeleted()){
						throw new Exception("Expected all prop rows soft deleted");
					}
				}
				var learnsAsy = RepoLearn.OrdGetByIdWithDel(Ctx, AsyE(_aggLearnIds.ToArray()), CT.None);
				await foreach(var l in learnsAsy){
					if(l is null || !l.IsDeleted()){
						throw new Exception("Expected all learn rows soft deleted");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.OrdGetAggById)];
		R("Agg_BatGet_By_Id_NonWithDel_Should_Exclude_SoftDeleted", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_SoftDelete_Root_And_Includes not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var gotAsy = RepoWord.OrdGetAggById<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				var got = new List<JnWord?>();
				await foreach(var one in gotAsy){
					got.Add(one);
				}
				if(got.Count != _aggWordIds.Count){
					throw new Exception($"Expected {_aggWordIds.Count} entries, got {got.Count}");
				}
				if(got.Any(x=>x is not null)){
					throw new Exception("BatGetAggById should not return soft-deleted roots");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.OrdGetAggByIdWithDel)];
		R("Agg_BatGet_By_Id_WithDel_Should_Include_SoftDeleted", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_SoftDelete_Root_And_Includes not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var gotAsy = RepoWord.OrdGetAggByIdWithDel<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				var got = new List<JnWord?>();
				await foreach(var one in gotAsy){
					got.Add(one);
				}
				if(got.Count != _aggWordIds.Count){
					throw new Exception($"Expected {_aggWordIds.Count} entries, got {got.Count}");
				}
				if(got.Any(x=>x is null)){
					throw new Exception("BatGetAggByIdWithDel should return soft-deleted roots");
				}
				if(got.Any(x=>x is not null && !x.Word.IsDeleted())){
					throw new Exception("WithDel aggregate root should carry deleted flag");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.OrdGetAggById)];
		R("Agg_BatGet_By_Id_NonWithDel_Should_Exclude_SoftDeleted_Includes_When_Root_Is_Alive", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var owner = new IdUser();
				var word = new PoWord{
					Id = new IdWord(),
					Owner = owner,
					Head = "agg_nonwithdel_include_" + System.Guid.NewGuid().ToString("N"),
					Lang = "en",
				};
				var keepProp = new PoWordProp{
					Id = new IdWordProp(),
					WordId = word.Id,
					KType = EKvType.Str,
					KStr = "keep_prop",
					VType = EKvType.Str,
					VStr = "keep_" + System.Guid.NewGuid().ToString("N"),
				};
				var delProp = new PoWordProp{
					Id = new IdWordProp(),
					WordId = word.Id,
					KType = EKvType.Str,
					KStr = "del_prop",
					VType = EKvType.Str,
					VStr = "del_" + System.Guid.NewGuid().ToString("N"),
				};
				var keepLearn = new PoWordLearn{
					Id = new IdWordLearn(),
					WordId = word.Id,
					LearnResult = ELearn.Add,
				};
				var delLearn = new PoWordLearn{
					Id = new IdWordLearn(),
					WordId = word.Id,
					LearnResult = ELearn.Rmb,
				};
				try{
					await RepoWord.OrdAdd(Ctx, AsyE(word), CT.None);
					await RepoProp.OrdAdd(Ctx, AsyE(keepProp, delProp), CT.None);
					await RepoLearn.OrdAdd(Ctx, AsyE(keepLearn, delLearn), CT.None);
					await RepoProp.SoftDelInId(Ctx, AsyE(delProp.Id), CT.None);
					await RepoLearn.SoftDelInId(Ctx, AsyE(delLearn.Id), CT.None);

					var got = await RepoWord.OrdGetAggById<JnWord>(Ctx, AsyE(word.Id), CT.None).FirstOrDefaultAsync(CT.None);
					if(got is null){
						throw new Exception("OrdGetAggById should return alive root");
					}
					if(got.Props.Any(x=>x.Id == delProp.Id)){
						throw new Exception("OrdGetAggById should exclude soft-deleted props from aggregate includes");
					}
					if(got.Learns.Any(x=>x.Id == delLearn.Id)){
						throw new Exception("OrdGetAggById should exclude soft-deleted learns from aggregate includes");
					}
					if(!got.Props.Any(x=>x.Id == keepProp.Id)){
						throw new Exception("OrdGetAggById should keep non-deleted props");
					}
					if(!got.Learns.Any(x=>x.Id == keepLearn.Id)){
						throw new Exception("OrdGetAggById should keep non-deleted learns");
					}
					return NIL;
				}
				finally{
					await RepoWord.HardDelAggInId<JnWord>(Ctx, AsyE(word.Id), CT.None);
				}
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.OrdGetAggByIdWithDel)];
		R("Agg_BatGet_By_Id_WithDel_Should_Include_SoftDeleted_Includes_When_Root_Is_Alive", async(o)=>{
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var owner = new IdUser();
				var word = new PoWord{
					Id = new IdWord(),
					Owner = owner,
					Head = "agg_withdel_include_" + System.Guid.NewGuid().ToString("N"),
					Lang = "en",
				};
				var keepProp = new PoWordProp{
					Id = new IdWordProp(),
					WordId = word.Id,
					KType = EKvType.Str,
					KStr = "keep_prop",
					VType = EKvType.Str,
					VStr = "keep_" + System.Guid.NewGuid().ToString("N"),
				};
				var delProp = new PoWordProp{
					Id = new IdWordProp(),
					WordId = word.Id,
					KType = EKvType.Str,
					KStr = "del_prop",
					VType = EKvType.Str,
					VStr = "del_" + System.Guid.NewGuid().ToString("N"),
				};
				var keepLearn = new PoWordLearn{
					Id = new IdWordLearn(),
					WordId = word.Id,
					LearnResult = ELearn.Add,
				};
				var delLearn = new PoWordLearn{
					Id = new IdWordLearn(),
					WordId = word.Id,
					LearnResult = ELearn.Rmb,
				};
				try{
					await RepoWord.OrdAdd(Ctx, AsyE(word), CT.None);
					await RepoProp.OrdAdd(Ctx, AsyE(keepProp, delProp), CT.None);
					await RepoLearn.OrdAdd(Ctx, AsyE(keepLearn, delLearn), CT.None);
					await RepoProp.SoftDelInId(Ctx, AsyE(delProp.Id), CT.None);
					await RepoLearn.SoftDelInId(Ctx, AsyE(delLearn.Id), CT.None);

					var got = await RepoWord.OrdGetAggByIdWithDel<JnWord>(Ctx, AsyE(word.Id), CT.None).FirstOrDefaultAsync(CT.None);
					if(got is null){
						throw new Exception("OrdGetAggByIdWithDel should return alive root");
					}
					if(!got.Props.Any(x=>x.Id == delProp.Id && x.IsDeleted())){
						throw new Exception("OrdGetAggByIdWithDel should include soft-deleted props in aggregate includes");
					}
					if(!got.Learns.Any(x=>x.Id == delLearn.Id && x.IsDeleted())){
						throw new Exception("OrdGetAggByIdWithDel should include soft-deleted learns in aggregate includes");
					}
					return NIL;
				}
				finally{
					await RepoWord.HardDelAggInId<JnWord>(Ctx, AsyE(word.Id), CT.None);
				}
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.GetAllAgg)];
		R("Agg_GetAllAgg_NonWithDel_Should_Exclude_SoftDeleted", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_SoftDelete_Root_And_Includes not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var gotAsy = RepoWord.GetAllAgg<JnWord>(Ctx, CT.None);
				var found = new HashSet<IdWord>();
				await foreach(var one in gotAsy){
					if(_aggWordIds.Contains(one.Word.Id)){
						found.Add(one.Word.Id);
					}
				}
				if(found.Count != 0){
					throw new Exception("GetAllAgg should exclude soft-deleted roots");
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.GetAllAggWithDel)];
		R("Agg_GetAllAgg_WithDel_Should_Include_SoftDeleted", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_SoftDelete_Root_And_Includes not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var gotAsy = RepoWord.GetAllAggWithDel<JnWord>(Ctx, CT.None);
				var found = new HashSet<IdWord>();
				await foreach(var one in gotAsy){
					if(_aggWordIds.Contains(one.Word.Id)){
						found.Add(one.Word.Id);
						if(!one.Word.IsDeleted()){
							throw new Exception("GetAllAggWithDel should return deleted roots with deleted flag");
						}
					}
				}
				foreach(var id in _aggWordIds){
					if(!found.Contains(id)){
						throw new Exception($"GetAllAggWithDel missing soft-deleted root: {id}");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.HardDelAggInId)];
		R("Agg_HardDelete_Root_And_Includes", async(o)=>{
			if(_aggWordIds.Count == 0){
				throw new Exception("Agg_Insert_By_BatAddAgg not executed");
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				var resp = await RepoWord.HardDelAggInId<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				if(resp is null){
					throw new Exception("HardDelAggInId returned null response");
				}

				var wordsAsy = RepoWord.OrdGetByIdWithDel(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				await foreach(var w in wordsAsy){
					if(w is not null){
						throw new Exception("Expected word row hard deleted");
					}
				}
				var propsAsy = RepoProp.OrdGetByIdWithDel(Ctx, AsyE(_aggPropIds.ToArray()), CT.None);
				await foreach(var p in propsAsy){
					if(p is not null){
						throw new Exception("Expected prop row hard deleted");
					}
				}
				var learnsAsy = RepoLearn.OrdGetByIdWithDel(Ctx, AsyE(_aggLearnIds.ToArray()), CT.None);
				await foreach(var l in learnsAsy){
					if(l is not null){
						throw new Exception("Expected learn row hard deleted");
					}
				}
				return NIL;
			});
		});

		register.TesteeFnNames = [nameof(IRepo<PoWord, IdWord>.HardDelAggInId)];
		R("Agg_Cleanup_HardDelete", async(o)=>{
			if(_aggWordIds.Count == 0){
				return NIL;
			}
			return await RunInTxnIfNoCtx(async(Ctx)=>{
				await RepoWord.HardDelAggInId<JnWord>(Ctx, AsyE(_aggWordIds.ToArray()), CT.None);
				return NIL;
			});
		});
	}
}
