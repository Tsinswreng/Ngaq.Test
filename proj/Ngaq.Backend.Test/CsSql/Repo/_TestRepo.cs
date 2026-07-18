using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
namespace Ngaq.Backend.Test.CsSql.Repo;

public partial class TestRepo : ITester{
	/// SQL 命令建立器，讓每個資料庫用例在同一交易上下文中執行。
	readonly ISqlCmdMkr SqlCmdMkr;

	/// 資料表註冊中心，供需要直接測試 ITable overload 的用例取得表定義。
	readonly ITblMgr TblMgr;

	/// 通用鍵值資料的 Repository，被多數基礎 CRUD 測試共用。
	readonly IRepo<PoKv, IdKv> Repo;

	/// 單詞聚合根的 Repository。
	readonly IRepo<PoWord, IdWord> RepoWord;

	/// 單詞屬性的 Repository。
	readonly IRepo<PoWordProp, IdWordProp> RepoProp;

	/// 單詞學習記錄的 Repository。
	readonly IRepo<PoWordLearn, IdWordLearn> RepoLearn;

	/// 建立 Repository 測試器，依賴均由測試管理器的 DI 容器提供。
	public partial TestRepo(
		ISqlCmdMkr SqlCmdMkr
		,ITblMgr TblMgr
		,IRepo<PoKv, IdKv> Repo
		,IRepo<PoWord, IdWord> RepoWord
		,IRepo<PoWordProp, IdWordProp> RepoProp
		,IRepo<PoWordLearn, IdWordLearn> RepoLearn
	);

	/// 組裝 IRepo 各 API 的測試節點；資料庫用例保持順序執行，避免共享庫互相干擾。
	public partial ITestNode RegisterTestsInto(ITestNode? Test);

	/// 將少量固定測試資料轉成異步序列，對齊 IRepo 的批次 API。
	private static partial IAsyncEnumerable<T> AsyE<T>(params T[] Items);

	/// 在獨立交易上下文中執行一個測試步驟，並由框架統一釋放資料庫資源。
	private partial Task<TRtn> RunInTxnIfNoCtx<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn);
}
