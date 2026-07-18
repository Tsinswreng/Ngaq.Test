using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.CsSql.Repo;

public partial class TestRepo{
	/// 註冊有序軟刪 API 的測試用例。
	public partial void RegisterOrdSoftDelById(ITestNode Node){
		var Register = Node.MkTestFnRegister(
			typeof(TestRepo)
			,[typeof(IRepo<PoKv, IdKv>)]
			,[nameof(IRepo<PoKv, IdKv>.OrdSoftDelById)]
		);
		Register.Register(
			nameof(OrdSoftDelByIdMarksExistingRowsAndAcceptsMissingOrEmptyIds)
			,OrdSoftDelByIdMarksExistingRowsAndAcceptsMissingOrEmptyIds!
		);
	}

	/// 驗證有序軟刪會標記已存在資料、忽略不存在 ID，且空輸入不會破壞資料。
	public async partial Task<nil> OrdSoftDelByIdMarksExistingRowsAndAcceptsMissingOrEmptyIds(obj? O){
		var T = Assert.IsTrue;
		return await RunInTxnIfNoCtx(async(Ctx)=>{
			var First = MkOrdSoftDelEntity("first");
			var Second = MkOrdSoftDelEntity("second");
			var MissingId = new IdKv();

			try{
				// 先插入兩筆唯一資料，再把不存在的 ID 混入批次，驗證批次位置不會造成例外。
				await Repo.OrdAdd(Ctx, AsyE(First, Second), CT.None);
				var Resp = await Repo.OrdSoftDelById(
					Ctx
					,AsyE(First.Id, MissingId, Second.Id)
					,CT.None
				);
				T(Resp is not null);

				// 空批次應是安全的 no-op，且不能改變前一步已建立的刪除狀態。
				var EmptyResp = await Repo.OrdSoftDelById(Ctx, AsyE<IdKv>(), CT.None);
				T(EmptyResp is not null);

				// 普通讀取必須排除軟刪資料；WithDel 讀取則須保留原順序及刪除標記。
				var AliveRows = await Repo.OrdGetById(
					Ctx
					,AsyE(First.Id, Second.Id)
					,CT.None
				).ToListAsync(CT.None);
				T(AliveRows.Count == 2);
				T(AliveRows.All(X=>X is null));

				var DeletedRows = await Repo.OrdGetByIdWithDel(
					Ctx
					,AsyE(First.Id, Second.Id)
					,CT.None
				).ToListAsync(CT.None);
				T(DeletedRows.Count == 2);
				T(DeletedRows.All(X=>X is not null && X.IsDeleted()));
				return NIL;
			}finally{
				// 無論任何斷言是否失敗，都硬刪本用例建立的資料，避免污染共享測試庫。
				await Repo.OrdHardDelById(Ctx, AsyE(First.Id, Second.Id), CT.None);
			}
		});
	}

	/// 建立鍵和值均帶 GUID 的測試資料，避免與既有或並行測試資料重複。
	static PoKv MkOrdSoftDelEntity(str Label){
		var Suffix = Guid.NewGuid().ToString("N");
		return new PoKv{
			Id = new IdKv(),
			Owner = default,
			KType = EKvType.Str,
			KStr = $"ord_soft_del_{Label}_{Suffix}",
			VType = EKvType.Str,
			VStr = $"ord_soft_del_value_{Label}_{Suffix}",
		};
	}
}
