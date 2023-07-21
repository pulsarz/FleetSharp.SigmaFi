using FleetSharp.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FleetSharp.SigmaFi
{
    internal class Cache
    {
        private static ConcurrentDictionary<string, TokenDetail<long>> tokenCache = new ConcurrentDictionary<string, TokenDetail<long>>();

        public static async Task<TokenDetail<long>?> GetTokenFromCache(NodeInterface node, string tokenId)
        {
            TokenDetail<long>? ret = null;
            if (tokenId.ToLowerInvariant() == "erg") return null;
            var succes = tokenCache.TryGetValue(tokenId, out ret);

            if (succes && ret != null) return ret;
            else
            {
                //Retrieve token but still not locked
                try
                {
                    ret = await node.GetToken(tokenId);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to retrieve token {tokenId} from the node!\n{e.ToString()}");
                }


                if (ret != null)
                {
                    tokenCache.TryAdd(tokenId, ret);
                }

            }

            return ret;
        }
    }
}
