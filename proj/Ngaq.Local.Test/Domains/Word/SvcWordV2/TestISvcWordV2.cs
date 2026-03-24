using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Core.Tools;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public class TestISvcWordV2: ITester{
	readonly ISvcWordV2 SvcWordV2;
	readonly IRepo<PoWord, IdWord> RepoWord;
	readonly IRepo<PoWordProp, IdWordProp> RepoProp;
	readonly IRepo<PoWordLearn, IdWordLearn> RepoLearn;

	public TestISvcWordV2(
		ISvcWordV2 SvcWordV2
		,IRepo<PoWord, IdWord> RepoWord
		,IRepo<PoWordProp, IdWordProp> RepoProp
		,IRepo<PoWordLearn, IdWordLearn> RepoLearn
	){
		this.SvcWordV2 = SvcWordV2;
		this.RepoWord = RepoWord;
		this.RepoProp = RepoProp;
		this.RepoLearn = RepoLearn;
	}

	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test ??= new TestNode();
		var register = Test.MkTestFnRegister(
			typeof(TestISvcWordV2)
			,[
				typeof(ISvcWordV2)
				,typeof(IRepo<PoWord, IdWord>)
				,typeof(IRepo<PoWordProp, IdWordProp>)
				,typeof(IRepo<PoWordLearn, IdWordLearn>)
			]
			,[]
			,nameof(TestISvcWordV2)
		);
		var R = register.Register;
		register.TesteeFnNames = [
			nameof(ISvcWordV2.GetWordsToLearn)
			,nameof(ISvcWordV2.BatAddNewLearnRecord)
			,nameof(ISvcWordV2.BatAddNewWordToLearn)
			,nameof(ISvcWordV2.SoftDelJnWordInId)
		];

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

		R("BatAddNewLearnRecord_Should_InsertLearns_And_TouchWordBizUpdatedAt", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_learn_" + Guid.NewGuid().ToString("N");
			var w1 = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_w1", Lang = "en", BizUpdatedAt = Tempus.Zero};
			var w2 = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_w2", Lang = "en", BizUpdatedAt = Tempus.Zero};
			var learns = new[]{
				new PoWordLearn{Id = new IdWordLearn(), WordId = w1.Id, LearnResult = ELearn.Add},
				new PoWordLearn{Id = new IdWordLearn(), WordId = w1.Id, LearnResult = ELearn.Rmb},
				new PoWordLearn{Id = new IdWordLearn(), WordId = w2.Id, LearnResult = ELearn.Fgt},
			};

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(w1, w2), CT.None);
					return NIL;
				});

				await SvcWordV2.BatAddNewLearnRecord(MkUserCtx(owner), AsyE(learns), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var gotLearns = await ToList(RepoLearn.BatGetById(Ctx, AsyE(learns.Select(x=>x.Id).ToArray()), CT.None));
					if(gotLearns.Count != learns.Length || gotLearns.Any(x=>x is null)){
						throw new Exception("BatAddNewLearnRecord failed to insert all learn records");
					}

					var gotWords = await ToList(RepoWord.BatGetById(Ctx, AsyE(w1.Id, w2.Id), CT.None));
					if(gotWords.Count != 2 || gotWords.Any(x=>x is null)){
						throw new Exception("failed to load words after BatAddNewLearnRecord");
					}
					if(gotWords.Any(x=>x!.BizUpdatedAt.IsNullOrDefault())){
						throw new Exception("BatAddNewLearnRecord did not touch PoWord.BizUpdatedAt");
					}
					return NIL;
				});

				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoLearn.BatHardDelById(Ctx, AsyE(learns.Select(x=>x.Id).ToArray()), CT.None);
					await RepoWord.BatHardDelById(Ctx, AsyE(w1.Id, w2.Id), CT.None);
					return NIL;
				});
			}
		});

		R("BatAddNewWordToLearn_Should_Merge_SameHeadLang", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_add_" + Guid.NewGuid().ToString("N");
			var headDup = token + "_dup";
			var neoWords = new[]{
				new PoWord{Id = new IdWord(), Owner = IdUser.Zero, Head = headDup, Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = IdUser.Zero, Head = headDup, Lang = "en"},
				new PoWord{Id = new IdWord(), Owner = IdUser.Zero, Head = token + "_solo", Lang = "en"},
			};

			try{
				await SvcWordV2.BatAddNewWordToLearn(MkUserCtx(owner), AsyE(neoWords), CT.None);

				var all = await ToList(SvcWordV2.GetWordsToLearn(MkUserCtx(owner), CT.None));
				var tokenWords = all.Where(x=>x.Word.Head.StartsWith(token)).ToList();
				var dupCnt = tokenWords.Count(x=>x.Word.Head == headDup && x.Word.Lang == "en");

				if(dupCnt != 1){
					throw new Exception($"BatAddNewWordToLearn duplicate merge assert failed, expected 1, got {dupCnt}");
				}
				if(tokenWords.Count != 2){
					throw new Exception($"BatAddNewWordToLearn expected 2 words after merge, got {tokenWords.Count}");
				}
				if(tokenWords.Any(x=>x.Word.Owner != owner)){
					throw new Exception("BatAddNewWordToLearn owner isolation/injection assert failed");
				}

				await RunNoTxn(async(Ctx)=>{
					var wordIds = tokenWords.Select(x=>x.Word.Id).Distinct().ToArray();
					var propIds = (await ToList(RepoProp.GetAll(Ctx, CT.None)))
						.Where(x=>wordIds.Contains(x.WordId))
						.Select(x=>x.Id)
						.ToArray();
					var learnIds = (await ToList(RepoLearn.GetAll(Ctx, CT.None)))
						.Where(x=>wordIds.Contains(x.WordId))
						.Select(x=>x.Id)
						.ToArray();

					if(propIds.Length > 0){
						await RepoProp.BatHardDelById(Ctx, AsyE(propIds), CT.None);
					}
					if(learnIds.Length > 0){
						await RepoLearn.BatHardDelById(Ctx, AsyE(learnIds), CT.None);
					}
					await RepoWord.BatHardDelById(Ctx, AsyE(wordIds), CT.None);
					return NIL;
				});

				return NIL;
			}
			catch{
				await TryCleanupByHeadOwner(owner, token);
				throw;
			}
		});

		R("SoftDelJnWordInId_Should_SoftDelete_Word_And_Assets", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_soft_" + Guid.NewGuid().ToString("N");
			var word = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_w", Lang = "en"};
			var props = new[]{
				new PoWordProp{Id = new IdWordProp(), WordId = word.Id, KStr = KeysProp.Inst.description, VType = EKvType.Str, VStr = token + "_d1"},
				new PoWordProp{Id = new IdWordProp(), WordId = word.Id, KStr = KeysProp.Inst.note, VType = EKvType.Str, VStr = token + "_n1"},
			};
			var learns = new[]{
				new PoWordLearn{Id = new IdWordLearn(), WordId = word.Id, LearnResult = ELearn.Add},
				new PoWordLearn{Id = new IdWordLearn(), WordId = word.Id, LearnResult = ELearn.Rmb},
			};

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(word), CT.None);
					await RepoProp.BatAdd(Ctx, AsyE(props), CT.None);
					await RepoLearn.BatAdd(Ctx, AsyE(learns), CT.None);
					return NIL;
				});

				await SvcWordV2.SoftDelJnWordInId(MkUserCtx(owner), AsyE(word.Id), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var gotWord = await ToList(RepoWord.BatGetById(Ctx, AsyE(word.Id), CT.None));
					if(gotWord.Count != 1 || gotWord[0] is null || !gotWord[0]!.IsDeleted()){
						throw new Exception("SoftDelJnWordInId failed to soft-delete root word");
					}

					var gotProps = await ToList(RepoProp.GetManyInIdWithDel(Ctx, AsyE(props.Select(x=>x.Id).ToArray()), CT.None));
					if(gotProps.Count != props.Length || gotProps.Any(x=>x is null || !x.IsDeleted())){
						throw new Exception("SoftDelJnWordInId failed to soft-delete props");
					}

					var gotLearns = await ToList(RepoLearn.GetManyInIdWithDel(Ctx, AsyE(learns.Select(x=>x.Id).ToArray()), CT.None));
					if(gotLearns.Count != learns.Length || gotLearns.Any(x=>x is null || !x.IsDeleted())){
						throw new Exception("SoftDelJnWordInId failed to soft-delete learns");
					}

					return NIL;
				});

				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoProp.BatHardDelById(Ctx, AsyE(props.Select(x=>x.Id).ToArray()), CT.None);
					await RepoLearn.BatHardDelById(Ctx, AsyE(learns.Select(x=>x.Id).ToArray()), CT.None);
					await RepoWord.BatHardDelById(Ctx, AsyE(word.Id), CT.None);
					return NIL;
				});
			}
		});

		return Test;
	}

	Task<TRtn> RunNoTxn<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn){
		IDbFnCtx Ctx = new DbFnCtx();
		return Fn(Ctx);
	}

	static async IAsyncEnumerable<T> AsyE<T>(params T[] Items){
		foreach(var I in Items){
			yield return I;
		}
	}

	static async Task<List<T>> ToList<T>(IAsyncEnumerable<T>? Asy){
		if(Asy is null){
			return [];
		}
		var R = new List<T>();
		await foreach(var x in Asy){
			R.Add(x);
		}
		return R;
	}

	IUserCtx MkUser(IdUser UserId){
		return new UserCtx{UserId = UserId};
	}

	IDbUserCtx MkUserCtx(IdUser UserId){
		return new DbUserCtx(new DbFnCtx(), MkUser(UserId));
	}

	async Task TryCleanupByHeadOwner(IdUser Owner, str Token){
		try{
			await RunNoTxn(async(Ctx)=>{
				var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
					.Where(x=>x.Owner == Owner && x.Head.StartsWith(Token))
					.ToList();
				if(words.Count == 0){
					return NIL;
				}

				var wordIds = words.Select(x=>x.Id).Distinct().ToArray();
				var propIds = (await ToList(RepoProp.GetAll(Ctx, CT.None)))
					.Where(x=>wordIds.Contains(x.WordId))
					.Select(x=>x.Id)
					.ToArray();
				var learnIds = (await ToList(RepoLearn.GetAll(Ctx, CT.None)))
					.Where(x=>wordIds.Contains(x.WordId))
					.Select(x=>x.Id)
					.ToArray();

				if(propIds.Length > 0){
					await RepoProp.BatHardDelById(Ctx, AsyE(propIds), CT.None);
				}
				if(learnIds.Length > 0){
					await RepoLearn.BatHardDelById(Ctx, AsyE(learnIds), CT.None);
				}
				await RepoWord.BatHardDelById(Ctx, AsyE(wordIds), CT.None);
				return NIL;
			});
		}
		catch{
			// ignore cleanup errors in test helper
		}
	}
}
