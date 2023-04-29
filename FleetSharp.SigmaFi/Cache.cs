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
            if (tokenId == "ERG" || tokenId == "erg") return null;
            var succes = tokenCache.TryGetValue(tokenId, out ret);

            if (succes && ret != null) return ret;
            else
            {
                //Retrieve token but still not locked
                ret = await node.GetToken(tokenId);

                if (ret != null)
                {
                    tokenCache.TryAdd(tokenId, ret);
                }
            }

            return ret;
        }
    }
}
