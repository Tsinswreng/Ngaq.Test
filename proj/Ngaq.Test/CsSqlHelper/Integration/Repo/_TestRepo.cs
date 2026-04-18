namespace Ngaq.Test.CsSql.Integration.Repo;

using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Backend.Word.Dao;
using Tsinswreng.CsSql;
public partial class TestRepo(
	DaoWord DaoWord
	,IRepo<PoWord, IdWord> RepoWord
	,ITblMgr TblMgr
	,ISqlCmdMkr SqlCmdMkr
){
	protected DaoWord DaoWord{get;set;} = DaoWord;
	protected IRepo<PoWord, IdWord> RepoWord{get;set;} = RepoWord;
	protected ITblMgr TblMgr{get;set;} = TblMgr;
	protected ISqlCmdMkr SqlCmdMkr{get;set;} = SqlCmdMkr;
}
