using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterMergeWord(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		register.TesteeFnNames = [
			nameof(ISvcWordV2.GetWordMergeResult),
			nameof(ISvcWordV2.MergeWord),
			nameof(ISvcWordV2.MergeWord_NewDescrAsAdd),
		];
		var R = register.Register;

		R("GetWordMergeResult_WhenLocalNotExist_Should_ReturnLocalNotExist", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_merge_get_" + Guid.NewGuid().ToString("N");
			var remote = MkMergeInputWord(owner, token + "_h1", "en", [
				token + "_d1",
			]);
			var results = await ToList(SvcWordV2.GetWordMergeResult(MkUserCtx(owner), AsyE(remote), CT.None));
			if(results.Count != 1){
				throw new Exception("GetWordMergeResult should return one item for one input");
			}
			if(results[0].Result != EJnWordMergeResult.LocalNotExist){
				throw new Exception("GetWordMergeResult should classify missing local as LocalNotExist");
			}
			if(results[0].Merged.Head != remote.Head || results[0].Merged.Lang != remote.Lang){
				throw new Exception("GetWordMergeResult merged root should keep remote biz-id");
			}
			return NIL;
		});

		R("MergeWord_WhenExistingWord_Should_AppendOnlyMissingAssets", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_merge_apply_" + Guid.NewGuid().ToString("N");
			var head = token + "_h1";
			var local = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = head,
				Lang = "en",
				BizCreatedAt = UnixMs.FromUnixMs(1000),
			};
			var oldDesc = new PoWordProp{
				Id = new IdWordProp(),
				WordId = local.Id,
				KType = EKvType.Str,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = token + "_d0",
				BizCreatedAt = UnixMs.FromUnixMs(1000),
			};
			var remote = new JnWord{
				Word = new PoWord{
					Id = new IdWord(),
					Owner = owner,
					Head = head,
					Lang = "en",
					BizCreatedAt = UnixMs.FromUnixMs(3000),
				},
				Props = [
					new PoWordProp{
						Id = oldDesc.Id,
						KType = EKvType.Str,
						KStr = KeysProp.Inst.description,
						VType = EKvType.Str,
						VStr = token + "_d0",
					},
					new PoWordProp{
						Id = new IdWordProp(),
						KType = EKvType.Str,
						KStr = KeysProp.Inst.description,
						VType = EKvType.Str,
						VStr = token + "_d1",
					},
				],
				Learns = [],
			};
			remote.EnsureForeignId();

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(local), CT.None);
					await RepoProp.BatAdd(Ctx, AsyE(oldDesc), CT.None);
					return NIL;
				});

				await SvcWordV2.MergeWord(MkUserCtx(owner), AsyE(remote), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.Where(x=>x.Owner == owner && x.Head == head && x.Lang == "en")
						.ToList();
					if(words.Count != 1){
						throw new Exception("MergeWord should not create duplicate root by same biz-id");
					}
					var wordId = words[0].Id;
					var props = (await ToList(RepoProp.GetAll(Ctx, CT.None)))
						.Where(x=>x.WordId == wordId && x.KStr == KeysProp.Inst.description)
						.ToList();
					if(props.Count != 2){
						throw new Exception("MergeWord should append only new assets");
					}
					var d0Cnt = props.Count(x=>x.VStr == token + "_d0");
					var d1Cnt = props.Count(x=>x.VStr == token + "_d1");
					if(d0Cnt != 1 || d1Cnt != 1){
						throw new Exception("MergeWord should keep old desc and append one new desc");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});

		R("MergeWord_NewDescrAsAdd_Should_CreateAddLearnsFromNewDescriptions", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_merge_addlearn_" + Guid.NewGuid().ToString("N");
			var head = token + "_h1";
			var local = new PoWord{
				Id = new IdWord(),
				Owner = owner,
				Head = head,
				Lang = "en",
			};
			var oldDesc = new PoWordProp{
				Id = new IdWordProp(),
				WordId = local.Id,
				KType = EKvType.Str,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = token + "_d0",
			};
			var remote = new JnWord{
				Word = new PoWord{
					Id = new IdWord(),
					Owner = owner,
					Head = head,
					Lang = "en",
				},
				Props = [
					new PoWordProp{
						Id = oldDesc.Id,
						KType = EKvType.Str,
						KStr = KeysProp.Inst.description,
						VType = EKvType.Str,
						VStr = token + "_d0",
					},
					new PoWordProp{
						Id = new IdWordProp(),
						KType = EKvType.Str,
						KStr = KeysProp.Inst.description,
						VType = EKvType.Str,
						VStr = token + "_d1",
					},
					new PoWordProp{
						Id = new IdWordProp(),
						KType = EKvType.Str,
						KStr = KeysProp.Inst.description,
						VType = EKvType.Str,
						VStr = token + "_d2",
					},
				],
				Learns = [],
			};
			remote.EnsureForeignId();

			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(local), CT.None);
					await RepoProp.BatAdd(Ctx, AsyE(oldDesc), CT.None);
					return NIL;
				});

				await SvcWordV2.MergeWord_NewDescrAsAdd(MkUserCtx(owner), AsyE(remote), CT.None);

				await RunNoTxn(async(Ctx)=>{
					var word = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
						.FirstOrDefault(x=>x.Owner == owner && x.Head == head && x.Lang == "en")
						?? throw new Exception("merged word should exist");
					var learns = (await ToList(RepoLearn.GetAll(Ctx, CT.None)))
						.Where(x=>x.WordId == word.Id && x.LearnResult == ELearn.Add)
						.ToList();
					if(learns.Count != 2){
						throw new Exception("MergeWord_NewDescrAsAdd should add learns for only newly-added descriptions");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await TryCleanupByHeadOwner(owner, token);
			}
		});
	}

	static JnWord MkMergeInputWord(IdUser Owner, str Head, str Lang, IList<str> Descriptions){
		var id = new IdWord();
		var props = new List<PoWordProp>();
		foreach(var desc in Descriptions){
			props.Add(new PoWordProp{
				Id = new IdWordProp(),
				WordId = id,
				KType = EKvType.Str,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = desc,
			});
		}
		return new JnWord{
			Word = new PoWord{
				Id = id,
				Owner = Owner,
				Head = Head,
				Lang = Lang,
			},
			Props = props,
			Learns = [],
		};
	}
}
