using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Ngaq.Test ;
public class TryLua {
	public void Try() {
		// 1️⃣ 註冊類型（AOT 必做）
		UserData.RegisterType<List<Dictionary<string, object?>>>();
		UserData.RegisterType<Dictionary<string, object?>>();
		UserData.RegisterType<KeyValuePair<string, object?>>();

		// 2️⃣ 準備數據
		var list = new List<Dictionary<string, object?>>
		{
			new() { ["name"] = "Alice", ["hp"] = 100 },
			new() { ["name"] = "Bob",  ["hp"] = 80 },
		};

		// 3️⃣ 建立腳本環境
		var script = new Script();
		script.Options.DebugPrint = s => Console.WriteLine(s);

		// 4️⃣ 傳進 Lua
		script.Globals["units"] = list;

		const string lua = @"
print('---- IList<IDict<string,object>> 遍歷 ----')
-- 用 Count 取長度
for i = 0, units.Count-1 do
	local dict = units[i]          -- 索引器取值
	-- 用 GetEnumerator 遍歷字典
	local iter = dict:GetEnumerator()
	while iter:MoveNext() do
		local kv = iter.Current
		print(kv.Key, kv.Value)
	end
end
return units.Count
";
		DynValue ret = script.DoString(lua);
		Console.WriteLine($"Lua 返回元素個數 = {ret.Number}");
	}


	public void Try2() {
		// 1️⃣ 註冊類型（AOT 必做）
		UserData.RegisterType<List<Dictionary<string, object?>>>();
		UserData.RegisterType<Dictionary<string, object?>>();
		UserData.RegisterType<KeyValuePair<string, object?>>();

		// 2️⃣ 準備數據
		var list = new List<Dictionary<string, object?>>
		{
			new() { ["name"] = "Alice", ["hp"] = 100 },
			new() { ["name"] = "Bob",  ["hp"] = 80 },
		};

		// 3️⃣ 建立腳本環境
		var script = new Script();
		script.Options.DebugPrint = s => Console.WriteLine(s);

		// 4️⃣ 傳進 Lua
		script.Globals["units"] = list;

		const string lua = @"
print('---- IList<IDict<string,object>> 遍歷 ----')
-- 用 Count 取長度
for i = 0, units.Count-1 do
	local dict = units[i]          -- 索引器取值
	-- 用 GetEnumerator 遍歷字典
	local iter = dict:GetEnumerator()
	while iter:MoveNext() do
		local kv = iter.Current
		print(kv.Key, kv.Value)
	end
end
return units.Count
";
		DynValue ret = script.DoString(lua);
		Console.WriteLine($"Lua 返回元素個數 = {ret.Number}");
	}


}


