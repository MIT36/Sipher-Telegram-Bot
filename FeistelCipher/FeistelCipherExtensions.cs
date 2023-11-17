using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace FeistelCipher;

public static class FeistelCipherExtensions
{
    public static IServiceCollection AddFeistelSipher(this IServiceCollection services)
    {
        services.AddScoped<IFeistelSipher, FeistelCipherClassic>(sv =>
        {
            var config = sv.GetRequiredService<IConfiguration>();
            var key = config["Key"];
            return !string.IsNullOrEmpty(key)
                ? new FeistelCipherClassic(Encoding.UTF8.GetBytes(key))
                : new FeistelCipherClassic();
        });

        return services;
    }
}
