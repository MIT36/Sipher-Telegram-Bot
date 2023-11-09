using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace FeistelCipher;

public static class FeistelCipherExtensions
{
    public static IServiceCollection AddFeistelSipher(this IServiceCollection services, string key = null)
    {
        services.AddScoped<IFeistelSipher, FeistelCipherClassic>(sv => 
            !string.IsNullOrEmpty(key) ? new FeistelCipherClassic(Encoding.UTF8.GetBytes(key)) : new FeistelCipherClassic());
        return services;
    }
}
