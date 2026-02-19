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
System.Console.WriteLine(json);
	}

}
