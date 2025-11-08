using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Tools;

namespace Ngaq.Test;

public class TestJson
{
	public void Test(){
		var Kv = new PoKv();
		Kv.Id = new();
		System.Console.WriteLine(
			JSON.stringify(Kv)
		);

		// IWebAns<PoKv> Ans = new WebAns<PoKv>();
		// Ans.Data = Kv;
		IWebAns<obj> Ans = new WebAns();
		Ans.Data = Kv;
		Ans.Errors??=new List<IAppErrView>();
		Ans.Errors.Add(AppErr.Mk(ItemsErr.Common.ArgErr, ["UserName"]));
		var AnsJson = JSON.stringify(Ans);
		System.Console.WriteLine(
			AnsJson
		);

		var ReParse = WebAns.Deserialize<PoKv>(AnsJson);
		System.Console.WriteLine(
			ReParse
		);
	}
}
