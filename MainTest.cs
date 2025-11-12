#if false
// See https://aka.ms/new-console-template for more information
/*
cd Ngaq.Test
dotnet publish -c Release -r win-x64
./bin/Release/net10.0/win-x64/publish/Ngaq.Test.exe
 */


// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;
// using Ngaq.Core.Infra;
// using Ngaq.Core.Model.Po.Kv;
// using Ngaq.Core.Model.Po.Learn_;
// using Ngaq.Core.Model.Po.Word;
// using Ngaq.Local.Db;
// using Ngaq.Local.Db.TswG;
// using Ngaq.Local.Sql;
// using Tsinswreng.CsSqlHelper;
// using Tsinswreng.CsSqlHelper.Sqlite;


// var DbPath = "E:/TestNgaq.Sqlite";
// var DbCtx = new LocalDbCtx{DbPath = DbPath};
// DbCtx.Database.EnsureCreated();



// var dict = DictCtx.ToDictT<ClassB>(new ClassB());
// System.Console.WriteLine(dict.Count);
// foreach(var(k,v) in dict){
// 	Console.WriteLine($"{k}:{v}");
// }

//AppTblInfo.Inst.Init();





// var d = DictCtx.GetTypeDictT<PoWord>();
// foreach(var (k,v) in d){
// 	System.Console.Write(k+"  ");
// 	System.Console.Write(v);
// 	System.Console.WriteLine();
// }




//var r =typeof(System.Int128?);
// static Type T(object o){
// 	return o.GetType();
// }

// System.Console.WriteLine(T(1));
// System.Console.WriteLine(T(1L));
// System.Console.WriteLine(T(1.0));
// System.Console.WriteLine(T(""));
// System.Console.WriteLine(T(TblMgr));





// System.Console.WriteLine(
// 	new SqlLatest().GenSql()
// );



//throw new Exception("AOT");


// interface I{
// 	void M(){
// 		System.Console.WriteLine(123);
// 	}
// }

// class C:I{
// 	static void Test(){
// 		var c = new C();

// 		c.M();

// 		((I)c).M();
// 	}
// }

#endif
