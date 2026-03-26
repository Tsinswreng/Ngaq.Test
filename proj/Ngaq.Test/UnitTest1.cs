using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Infra;
using Ngaq.Core.Tools;
using Ngaq.Local.TsNgaq;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Local.Domains.Word.Svc;
using System.Text;

namespace Ngaq.Test;

public class TestMigrateTsNgaq {
	[Fact]
	public async Task Test1() {
		var Ct = new CT();
		var Migrator = new TsNgaqMigrator("c:/E/_code/ngaq/db/userDb/user-1.sqlite");
		var JnWords = await Migrator.ToJnWords(Ct);
		var seria = new Seria();
		seria.DictMapper = CoreDictMapper.Inst;
		var i = -1;
		var jsons = new List<str>();
		foreach(var word in JnWords){
			i++;
			//var dict = word.SerializeToDict();
			var json = JSON.stringify(word);

			var word2 = JSON.parse<JnWord>(json);
			var json2 = JSON.Stringify(word2);
			if(json != json2){
				throw new Exception($"at pos {i} Json not equal.");
			}
			jsons.Add(json);
		}
		var wordsPackInfo = new WordsPackInfo{
			Type = EWordsPack.LineSepJnWordJsonGZip
		};
		var jsonLines = str.Join('\n', jsons);
		var compressed = SvcWord.CompressGZip(Encoding.UTF8.GetBytes(jsonLines));
		var textWithBlob = ToolTextWithBlob.Pack(
			JSON.Stringify(wordsPackInfo), compressed
		);
		File.WriteAllBytes("./TsNgaq.tb", textWithBlob.ToByteArr());

	}
}
