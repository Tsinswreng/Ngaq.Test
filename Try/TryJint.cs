using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Tsinswreng.CsTools;

namespace Ngaq.Test.Try;
using Kv = System.Collections.Generic.Dictionary<str, obj?>;
public class TryJint {
	public void TryClrFnEtRtn() {
		var engine = new Engine();{
		}
		engine.SetValue(
			"log"
			,new ClrFunction(
				engine
				,"log"
				,(self, args)=>{
					if (args.Length > 0){
						Console.WriteLine(args.At(0).ToString());
					}
					return JsValue.Undefined;
				}
			)
		);

		var R = engine.Evaluate(@"
function hello() {
	log('qwq主人嗚嗚')
	return '123'
};
hello();
		");
		System.Console.WriteLine(R);
	}

	public void TryExchangeData(){
		var engine = new Engine();{}

		var Dict = new Kv(){
			["foo"] = "bar",
			["num"] = 2,
		};
		var json = ToolJson.DictToJson(Dict);
		engine.SetValue("Arg", json);
		var R = engine.Evaluate(@"
function hello() {
	let obj = JSON.parse(Arg)
	obj['foo'] = 'baz'
	return JSON.stringify(obj)
};
hello();
		");
		System.Console.WriteLine(R);
	}
}
