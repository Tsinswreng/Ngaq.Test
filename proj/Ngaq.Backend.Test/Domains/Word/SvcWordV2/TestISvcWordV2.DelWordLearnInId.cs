using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

/// 驗證學習記錄直接刪除接口確實修改數據庫，而非只在調用端移除狀態。
public partial class TestISvcWordV2{
	/// 註冊學習記錄直接刪除的落庫測試。
	void RegisterDelWordLearnInId(ITestNode Node){
		var Register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[nameof(ISvcWordV2.DelWordLearnInId)]
		);
		Register.Register(
			nameof(DelWordLearnInIdShouldSoftDeletePersistedLearn),
			DelWordLearnInIdShouldSoftDeletePersistedLearn!
		);
	}

	/// 插入已持久化學習記錄後直接刪除，並從含軟刪資料的查詢確認刪除標記。
	async Task<nil> DelWordLearnInIdShouldSoftDeletePersistedLearn(obj? O){
		var Owner = new IdUser();
		var Token = "ut_del_learn_" + Guid.NewGuid().ToString("N");
		var Word = new PoWord{
			Id = new IdWord(),
			Owner = Owner,
			Head = Token,
			Lang = "en",
		};
		var Learn = new PoWordLearn{
			Id = new IdWordLearn(),
			WordId = Word.Id,
			LearnResult = ELearn.Rmb,
		};

		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Word), CT.None);
				await RepoLearn.OrdAdd(Ctx, AsyE(Learn), CT.None);
				return NIL;
			});

			await SvcWordV2.DelWordLearnInId(
				MkUserCtx(Owner),
				AsyE(Learn.Id),
				CT.None
			);

			await RunNoTxn(async(Ctx)=>{
				var Got = await ToList(
					RepoLearn.OrdGetByIdWithDel(Ctx, AsyE(Learn.Id), CT.None)
				);
				Assert.IsTrue(
					Got.Count == 1
					&& Got[0] is not null
					&& Got[0]!.IsDeleted(),
					"DelWordLearnInId should persist the soft-delete marker."
				);
				return NIL;
			});
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoLearn.OrdHardDelById(Ctx, AsyE(Learn.Id), CT.None);
				await RepoWord.OrdHardDelById(Ctx, AsyE(Word.Id), CT.None);
				return NIL;
			});
		}
	}
}
