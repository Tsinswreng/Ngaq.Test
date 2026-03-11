using Microsoft.Extensions.DependencyInjection;
using Ngaq.Test.CsSqlHelper.Integration.Repo;

namespace Ngaq.Test;

public static class DiTest{
	public static IServiceCollection SetupTest(this IServiceCollection z){
		z.AddScoped<TestRepo>();
		return z;
	}
}
