#if false
var instA = new ClassA();
var TypeA = typeof(ClassA);
var Int = TypeA.GetProperty("Int").GetValue(instA);
System.Console.WriteLine(Int);

TypeA.GetProperty("String").SetValue(instA, "xxx");
System.Console.WriteLine(instA.String);
throw new Exception("testAOT");

public class ClassA{
	public string String{get;set;} = "String";
	public int Int{get;set;} = 1;
	public bool Bool{get;set;} = true;
}






#endif
