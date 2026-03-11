namespace Ngaq.Test;
using Jeffijoe.MessageFormat;
public class TestIcuMsgFmt{
	public void _(){

//await new TestWord().TestGetChangedWordsAfterTime();

var fmt = new MessageFormatter();
var Template = "You have {0, plural, =0{no messages} =1{one message} other{# messages}}.";
for(var i = 0; i < 10; i++){
	System.Console.WriteLine(
		fmt.FormatMessage(Template,new Dictionary<str,obj>{["0"]=i})
	);
}

	}
}
