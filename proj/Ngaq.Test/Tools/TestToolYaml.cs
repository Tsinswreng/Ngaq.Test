using Ngaq.Core.Tools;
using Tsinswreng.CsTools;

namespace Ngaq.Test.Tools;

public class TestToolYaml{
	public delegate IDictionary<string, object?> DlgtYamlStrToDict(string yamlStr);
	public DlgtYamlStrToDict FnYamlStrToDict = ToolYaml.YamlStrToDict;

	public void TryYamlStrToDict(){
		var YamlStr =
"""
# 本來不應該加上不必要的註釋、這裏爲了方便說明格式 特地帶上了註釋。
__content1: &__content1 |+
  console.log(
  	"Hello, world!"
  );
__content2: &__content2 |+
  foreach(var i in list){
      Console.WriteLine(i);
  }

# md大標題後無代碼塊則設成null
__content3: &__content3 null

# md大標題有代碼塊但代碼塊內容爲空則設成空字符串
__content4: &__content4 "" #下面 插入的錨點和 原始yaml之間要空一行

name: Tsins
foo: *__content1
bar: *__content2
c3: *__content3
c4: *__content4

""";
var dict = FnYamlStrToDict(YamlStr);
var json = ToolJson.DictToJson(dict);
System.Console.WriteLine(dict.Count);
System.Console.WriteLine(json);
	}

		public void Run(){
var yaml =
"""
__descr1: &__descr1 |+
  
  【動詞】操；干；搞
  
  指在極端不雅的語境中，用來表達性行為或強烈的不滿與憤怒。該詞語在正式場合和書面語中通常被視為禁忌，並且可能引起冒犯。
  
  *用法*：
  - fuck someone/something: 與某人/某物發生性關係；對某人/某事感到非常憤怒
  
  *例句*：
  1. I can't believe this happened! I really want to fuck this up.
     （我真不敢相信這事發生了！我真的想搞砸這一切。）
  
  *注意*：
  由於該詞語具有冒犯性，建議在非正式和非公開場合謹慎使用。
  
Head: fuck
Pronunciations:
  - /fʌk/
Descrs:
  - *__descr1
""";
var dict = ToolYaml.YamlStrToDict(yaml);
System.Console.WriteLine(dict);
System.Console.WriteLine(ToolJson.DictToJson(dict));
	}

	public void Run2(){
{
	var yaml =
"""
a: |+
  
  
  1
""";
	var dict = ToolYaml.YamlStrToDict(yaml);
	System.Console.WriteLine(dict["a"]);
}
{
	var yaml =
"""
a: |+


  1
""";
	var dict = ToolYaml.YamlStrToDict(yaml);
	System.Console.WriteLine(dict["a"]);
}
{
	var yaml =
"""
a: |+
  

  1
""";
	var dict = ToolYaml.YamlStrToDict(yaml);
	System.Console.WriteLine(dict["a"]);
}
	}

}

