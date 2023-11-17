using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProxiesTelegram;

public interface IProxyStorageService : IProxyService
{
    Task SaveProxy(string host, int port);
}
