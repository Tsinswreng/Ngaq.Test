using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Word;

/// 驗證單詞屬性直接刪除接口確實修改數據庫，而非只在調用端移除狀態。
public partial class TestISvcWordV2{
	/// 註冊屬性直接刪除的落庫測試。
	void RegisterDelWordPropInId(ITestNode Node){
		var Register = Node.MkTestFnRegister(
			typeof(TestISvcWordV2),
			[typeof(ISvcWordV2)],
			[nameof(ISvcWordV2.DelWordPropInId)]
		);
		Register.Register(
			nameof(DelWordPropInIdShouldSoftDeletePersistedProp),
			DelWordPropInIdShouldSoftDeletePersistedProp!
		);
	}

	/// 插入已持久化屬性後直接刪除，並從含軟刪資料的查詢確認刪除標記。
	async Task<nil> DelWordPropInIdShouldSoftDeletePersistedProp(obj? O){
		var Owner = new IdUser();
		var Token = "ut_del_prop_" + Guid.NewGuid().ToString("N");
		var Word = new PoWord{
			Id = new IdWord(),
			Owner = Owner,
			Head = Token,
			Lang = "en",
		};
		var Prop = new PoWordProp{
			Id = new IdWordProp(),
			WordId = Word.Id,
			KType = EKvType.Str,
			KStr = KeysProp.Inst.note,
			VType = EKvType.Str,
			VStr = Token,
		};

		try{
			await RunNoTxn(async(Ctx)=>{
				await RepoWord.OrdAdd(Ctx, AsyE(Word), CT.None);
				await RepoProp.OrdAdd(Ctx, AsyE(Prop), CT.None);
				return NIL;
			});

			await SvcWordV2.DelWordPropInId(
				MkUserCtx(Owner),
				AsyE(Prop.Id),
				CT.None
			);

			await RunNoTxn(async(Ctx)=>{
				var Got = await ToList(
					RepoProp.OrdGetByIdWithDel(Ctx, AsyE(Prop.Id), CT.None)
				);
				Assert.IsTrue(
					Got.Count == 1
					&& Got[0] is not null
					&& Got[0]!.IsDeleted(),
					"DelWordPropInId should persist the soft-delete marker."
				);
				return NIL;
			});
			return NIL;
		}finally{
			await RunNoTxn(async(Ctx)=>{
				await RepoProp.OrdHardDelById(Ctx, AsyE(Prop.Id), CT.None);
				await RepoWord.OrdHardDelById(Ctx, AsyE(Word.Id), CT.None);
				return NIL;
			});
		}
	}
}
