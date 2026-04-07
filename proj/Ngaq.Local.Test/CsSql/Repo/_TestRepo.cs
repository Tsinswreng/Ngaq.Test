using Tsinswreng.CsTreeTest;
using Tsinswreng.CsSql;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
namespace Ngaq.Local.Test.CsSql.Repo;

public partial class TestRepo : ITester{
	ISqlCmdMkr SqlCmdMkr;
	IRepo<PoKv, IdKv> Repo;
	IRepo<PoWord, IdWord> RepoWord;
	IRepo<PoWordProp, IdWordProp> RepoProp;
	IRepo<PoWordLearn, IdWordLearn> RepoLearn;
	public TestRepo(
		ISqlCmdMkr SqlCmdMkr
		,IRepo<PoKv, IdKv> Repo
		,IRepo<PoWord, IdWord> RepoWord
		,IRepo<PoWordProp, IdWordProp> RepoProp
		,IRepo<PoWordLearn, IdWordLearn> RepoLearn
	){
		this.SqlCmdMkr = SqlCmdMkr;
		this.Repo = Repo;
		this.RepoWord = RepoWord;
		this.RepoProp = RepoProp;
		this.RepoLearn = RepoLearn;
	}
	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test??=new TestNode();
		Test.Ordered = true;
		
		RegisterSlctManyInIdsWithDel(Test);
		RegisterBatSlctById(Test);
		RegisterBatInsert(Test);
		RegisterBatUpd(Test);
		RegisterBatExistsAndUpsert(Test);
		RegisterDelInId(Test);
		RegisterGetAll(Test);
		RegisterAgg(Test);
		return Test;
	}

	static async IAsyncEnumerable<T> AsyE<T>(params T[] Items){
		foreach(var I in Items) yield return I;
	}

	Task<TRtn> RunInTxnIfNoCtx<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn){
		return SqlCmdMkr.EnsureTxn(null, CT.None, Fn);
	}
}
