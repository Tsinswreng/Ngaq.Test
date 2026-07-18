using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;

namespace Ngaq.Backend.Test.CsSql.Repo;

public partial class TestRepo{
	/// 建立 Repository 測試器，保存由測試管理器注入的共用依賴。
	public partial TestRepo(
		ISqlCmdMkr SqlCmdMkr
		,ITblMgr TblMgr
		,IRepo<PoKv, IdKv> Repo
		,IRepo<PoWord, IdWord> RepoWord
		,IRepo<PoWordProp, IdWordProp> RepoProp
		,IRepo<PoWordLearn, IdWordLearn> RepoLearn
	){
		this.SqlCmdMkr = SqlCmdMkr;
		this.TblMgr = TblMgr;
		this.Repo = Repo;
		this.RepoWord = RepoWord;
		this.RepoProp = RepoProp;
		this.RepoLearn = RepoLearn;
	}

	/// 組裝 IRepo 各 API 的測試節點；資料庫用例保持順序執行，避免共享庫互相干擾。
	public partial ITestNode RegisterTestsInto(ITestNode? Test){
		Test ??= new TestNode();
		Test.Ordered = true;

		RegisterSlctManyInIdsWithDel(Test);
		RegisterBatSlctById(Test);
		RegisterBatInsert(Test);
		RegisterBatUpd(Test);
		RegisterBatExistsAndUpsert(Test);
		RegisterDelInId(Test);
		RegisterOrdSoftDelById(Test);
		RegisterGetAll(Test);
		RegisterAgg(Test);
		RegisterIncludeEntitysByKeys(Test);
		return Test;
	}

	/// 將少量固定測試資料轉成異步序列，對齊 IRepo 的批次 API。
	private static async partial IAsyncEnumerable<T> AsyE<T>(params T[] Items){
		foreach(var Item in Items){
			yield return Item;
		}
	}

	/// 在獨立交易上下文中執行一個測試步驟，並由框架統一釋放資料庫資源。
	private partial Task<TRtn> RunInTxnIfNoCtx<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn){
		return SqlCmdMkr.EnsureTxn(null, CT.None, Fn);
	}
}
