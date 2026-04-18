using System.Runtime.CompilerServices;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngaq.Backend.Test.Domains.Kv;

/// <summary>
/// ISvcKv 測試主組裝器:
/// 1) 統一初始化測試數據
/// 2) 統一清理測試數據
/// 3) 組裝各被測函數的子測試
/// </summary>
public partial class TestISvcKv: ITester{
	readonly ISvcKv SvcKv;
	readonly IRepo<PoKv, IdKv> RepoKv;

	IdUser _ownerA = new();
	IdUser _ownerB = new();
	str _token = "";
	readonly List<IdKv> _ids = [];

	PoKv _kvA_I64_11 = new();
	PoKv _kvA_I64_22 = new();
	PoKv _kvA_Str_A = new();
	PoKv _kvA_Str_B = new();
	PoKv _kvB_I64_11 = new();
	PoKv _kvB_Str_A = new();

	public TestISvcKv(
		ISvcKv SvcKv
		,IRepo<PoKv, IdKv> RepoKv
	){
		this.SvcKv = SvcKv;
		this.RepoKv = RepoKv;
	}

	/// <summary>
	/// 註冊測試:
	/// setup -> 各函數用例 -> cleanup
	/// </summary>
	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;

		var register = Node.MkTestFnRegister(
			typeof(TestISvcKv)
			,[typeof(ISvcKv), typeof(IRepo<PoKv, IdKv>)]
			,[]
			,nameof(TestISvcKv)
		);
		var R = register.Register;

		R("SvcKv_Setup_InsertSeedData", async(o)=>{
			await InsertSeedData();
			return NIL;
		});

		RegisterBatGetByOwnerEtKI64(Node);
		RegisterBatGetByOwnerEtKStr(Node);

		R("SvcKv_Cleanup_AllInsertedData", async(o)=>{
			await CleanupData();
			return NIL;
		});

		return Node;
	}

	/// <summary>
	/// 寫入測試種子數據，覆蓋:
	/// - 不同 owner
	/// - I64 key / Str key
	/// </summary>
	async Task InsertSeedData(){
		_ownerA = new IdUser();
		_ownerB = new IdUser();
		_token = "ut_svckv_" + Guid.NewGuid().ToString("N");

		_kvA_I64_11 = new PoKv{
			Id = new IdKv(),
			Owner = _ownerA,
			KType = EKvType.I64,
			KI64 = 11,
			VType = EKvType.Str,
			VStr = _token + "_v_a_i64_11",
		};
		_kvA_I64_22 = new PoKv{
			Id = new IdKv(),
			Owner = _ownerA,
			KType = EKvType.I64,
			KI64 = 22,
			VType = EKvType.Str,
			VStr = _token + "_v_a_i64_22",
		};
		_kvA_Str_A = new PoKv{
			Id = new IdKv(),
			Owner = _ownerA,
			KType = EKvType.Str,
			KStr = _token + "_k_a",
			VType = EKvType.Str,
			VStr = _token + "_v_a_str_a",
		};
		_kvA_Str_B = new PoKv{
			Id = new IdKv(),
			Owner = _ownerA,
			KType = EKvType.Str,
			KStr = _token + "_k_b",
			VType = EKvType.Str,
			VStr = _token + "_v_a_str_b",
		};
		_kvB_I64_11 = new PoKv{
			Id = new IdKv(),
			Owner = _ownerB,
			KType = EKvType.I64,
			KI64 = 11,
			VType = EKvType.Str,
			VStr = _token + "_v_b_i64_11",
		};
		_kvB_Str_A = new PoKv{
			Id = new IdKv(),
			Owner = _ownerB,
			KType = EKvType.Str,
			KStr = _token + "_k_a",
			VType = EKvType.Str,
			VStr = _token + "_v_b_str_a",
		};

		await RunNoTxn(async(Ctx)=>{
			await RepoKv.BatAdd(
				Ctx,
				AsyE(
					_kvA_I64_11,
					_kvA_I64_22,
					_kvA_Str_A,
					_kvA_Str_B,
					_kvB_I64_11,
					_kvB_Str_A
				),
				CT.None
			);
			return NIL;
		});

		_ids.Clear();
		_ids.AddRange([
			_kvA_I64_11.Id,
			_kvA_I64_22.Id,
			_kvA_Str_A.Id,
			_kvA_Str_B.Id,
			_kvB_I64_11.Id,
			_kvB_Str_A.Id,
		]);
	}

	/// <summary>
	/// 清理測試數據。無論前序用例成敗，結尾都嘗試硬刪。
	/// </summary>
	async Task CleanupData(){
		await RunNoTxn(async(Ctx)=>{
			if(_ids.Count > 0){
				await RepoKv.BatHardDelById(Ctx, AsyE(_ids.ToArray()), CT.None);
			}
			return NIL;
		});
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

	/// <summary>
	/// 只允許被枚舉一次的 IAsyncEnumerable。
	/// 若被重複消費，會在第二次 GetAsyncEnumerator 直接拋錯。
	/// </summary>
	sealed class OneShotAsyE<T>: IAsyncEnumerable<T>{
		readonly IReadOnlyList<T> _items;
		i32 _enumeratorCreated = 0;

		public OneShotAsyE(params T[] Items){
			_items = Items;
		}

		public IAsyncEnumerator<T> GetAsyncEnumerator(CT Ct = default){
			if(Interlocked.Exchange(ref _enumeratorCreated, 1) == 1){
				throw new InvalidOperationException("OneShotAsyE can only be enumerated once.");
			}
			return Impl(Ct).GetAsyncEnumerator(Ct);
		}

		async IAsyncEnumerable<T> Impl([EnumeratorCancellation] CT Ct){
			foreach(var item in _items){
				Ct.ThrowIfCancellationRequested();
				yield return item;
				await Task.Yield();
			}
		}
	}
}
