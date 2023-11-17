using Microsoft.Extensions.Configuration;

namespace ProxiesTelegram;

public class ProxyServiceOptions
{
    public string ConnectionStringDb { get; set; }
    public string Site { get; set; }
}
