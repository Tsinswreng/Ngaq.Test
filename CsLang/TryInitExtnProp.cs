namespace Ngaq.Test.CsLang;

public class TryInitExtnProp{
	public void Try(){
		var a = new A{
			Name = "a"
		};
		System.Console.WriteLine(a.Name);
	}
}

class A{

}

static class ExtnA{
	extension(A z){
		public string Name{get{return "Name";}set{}}
	}
}
