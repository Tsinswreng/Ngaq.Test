using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Local.Test.Domains.Word;

public partial class TestISvcWordV2{
	void RegisterBatUpdHeadLang(ITestNode Node){
		var register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[]
		);
		var R = register.Register;
		register.TesteeFnNames = [nameof(ISvcWordV2.BatUpdHeadLang)];

		R("BatUpdHeadLang_WhenHeadLangUnchanged_Should_ReturnNullAndNoMove", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_updhl_same_" + Guid.NewGuid().ToString("N");
			var word = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_h1", Lang = "en"};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(word), CT.None);
					return NIL;
				});

				var args = new PoWord{Id = word.Id, Owner = owner, Head = word.Head, Lang = word.Lang};
				var rtn = await ToList(SvcWordV2.BatUpdHeadLang(MkUserCtx(owner), AsyE(args), CT.None));
				if(rtn.Count != 1 || rtn[0] is null || rtn[0]!.FinalId != word.Id || rtn[0]!.Result != EUpdBizIdResult.BizIdAlreadyEqual){
					throw new Exception("BatUpdHeadLang should return null when (Head,Lang) not changed");
				}
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatHardDelById(Ctx, AsyE(word.Id), CT.None);
					return NIL;
				});
			}
		});

		R("BatUpdHeadLang_WhenTargetHeadLangNotExist_Should_UpdateInPlace", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_updhl_move_" + Guid.NewGuid().ToString("N");
			var word = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_old", Lang = "en"};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(word), CT.None);
					return NIL;
				});

				var arg = new PoWord{Id = word.Id, Owner = owner, Head = token + "_new", Lang = "en"};
				var rtn = await ToList(SvcWordV2.BatUpdHeadLang(MkUserCtx(owner), AsyE(arg), CT.None));
				if(rtn.Count != 1 || rtn[0] is null || rtn[0]!.FinalId != word.Id || rtn[0]!.Result != EUpdBizIdResult.DataOfBizIdNotExist){
					throw new Exception("BatUpdHeadLang should return null when id remains unchanged");
				}

				await RunNoTxn(async(Ctx)=>{
					var got = await ToList(RepoWord.BatGetByIdWithDel(Ctx, AsyE(word.Id), CT.None));
					if(got.Count != 1 || got[0] is null || got[0]!.Head != arg.Head || got[0]!.Lang != arg.Lang){
						throw new Exception("BatUpdHeadLang should update (Head,Lang) in-place");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatHardDelById(Ctx, AsyE(word.Id), CT.None);
					return NIL;
				});
			}
		});

		R("BatUpdHeadLang_WhenTargetHeadLangExists_Should_MergeToTargetAndReturnTargetId", async(o)=>{
			var owner = new IdUser();
			var token = "ut_wv2_updhl_merge_" + Guid.NewGuid().ToString("N");
			var src = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_src", Lang = "en"};
			var dst = new PoWord{Id = new IdWord(), Owner = owner, Head = token + "_dst", Lang = "en"};
			var srcProp = new PoWordProp{
				Id = new IdWordProp(),
				WordId = src.Id,
				KType = EKvType.Str,
				KStr = KeysProp.Inst.note,
				VType = EKvType.Str,
				VStr = token + "_src_note",
			};
			var srcLearn = new PoWordLearn{
				Id = new IdWordLearn(),
				WordId = src.Id,
				LearnResult = ELearn.Add,
			};
			try{
				await RunNoTxn(async(Ctx)=>{
					await RepoWord.BatAdd(Ctx, AsyE(src, dst), CT.None);
					await RepoProp.BatAdd(Ctx, AsyE(srcProp), CT.None);
					await RepoLearn.BatAdd(Ctx, AsyE(srcLearn), CT.None);
					return NIL;
				});

				var arg = new PoWord{Id = src.Id, Owner = owner, Head = dst.Head, Lang = dst.Lang};
				var rtn = await ToList(SvcWordV2.BatUpdHeadLang(MkUserCtx(owner), AsyE(arg), CT.None));
				if(rtn.Count != 1 || rtn[0] is null || rtn[0]!.FinalId != dst.Id || rtn[0]!.Result != EUpdBizIdResult.BizIdNotEqual){
					throw new Exception("BatUpdHeadLang should return target id when merged");
				}

				await RunNoTxn(async(Ctx)=>{
					var srcGot = await ToList(RepoWord.BatGetByIdWithDel(Ctx, AsyE(src.Id), CT.None));
					var dstGot = await ToList(RepoWord.BatGetByIdWithDel(Ctx, AsyE(dst.Id), CT.None));
					if(srcGot.Count != 1 || srcGot[0] is null || !srcGot[0]!.IsDeleted()){
						throw new Exception("BatUpdHeadLang merge should soft-delete source word");
					}
					if(dstGot.Count != 1 || dstGot[0] is null || dstGot[0]!.IsDeleted()){
						throw new Exception("BatUpdHeadLang merge should keep target alive");
					}

					var prop = (await ToList(RepoProp.GetAll(Ctx, CT.None))).FirstOrDefault(x=>x.Id == srcProp.Id);
					var learn = (await ToList(RepoLearn.GetAll(Ctx, CT.None))).FirstOrDefault(x=>x.Id == srcLearn.Id);
					if(prop is null || prop.WordId != dst.Id){
						throw new Exception("BatUpdHeadLang merge should move prop foreign key to target id");
					}
					if(learn is null || learn.WordId != dst.Id){
						throw new Exception("BatUpdHeadLang merge should move learn foreign key to target id");
					}
					return NIL;
				});
				return NIL;
			}
			finally{
				await RunNoTxn(async(Ctx)=>{
					await RepoProp.BatHardDelById(Ctx, AsyE(srcProp.Id), CT.None);
					await RepoLearn.BatHardDelById(Ctx, AsyE(srcLearn.Id), CT.None);
					await RepoWord.BatHardDelById(Ctx, AsyE(src.Id, dst.Id), CT.None);
					return NIL;
				});
			}
		});

		R("BatUpdHeadLang_WhenWordIdNotExist_Should_ThrowWordOfIdNotFound", async(o)=>{
			var owner = new IdUser();
			var po = new PoWord{Id = new IdWord(), Owner = owner, Head = "ut_wv2_updhl_nf_" + Guid.NewGuid().ToString("N"), Lang = "en"};
			try{
				_ = await ToList(SvcWordV2.BatUpdHeadLang(MkUserCtx(owner), AsyE(po), CT.None));
				throw new Exception("BatUpdHeadLang should throw when PoWord.Id not found");
			}
			catch(Exception ex){
				if(ex is not AppErr appErr){
					throw new Exception("BatUpdHeadLang should throw AppErr for id-not-found");
				}
				if(!ReferenceEquals(appErr.Type, ItemsErr.Word.WordOfId__NotFound)){
					throw new Exception("BatUpdHeadLang should throw ItemsErr.Word.WordOfId__NotFound");
				}
			}
			return NIL;
		});
	}
}
