using FleetSharp.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetSharp.SigmaFi.Types
{
    public class SigmaFiVerifiedAssetMetadata
    {
        public string? name { get; set; }
        public int? decimals { get; set; }

        public SigmaFiVerifiedAssetMetadata(string? name, int? decimals)
        {
            this.name = name;
            this.decimals = decimals;
        }
    }
    public class SigmaFiVerifiedAsset
    {
        public string? tokenId { get; set; }
        public SigmaFiVerifiedAssetMetadata? metadata { get; set; }
    }

    public class SigmaFiVerifiedAssetAmount : SigmaFiVerifiedAsset
    {
        public long? amount { get; set; }
        public double? amountWithDecimals { get; set; }
        public double? usdValue { get; set; }
    }

    public class SigmaFiOrder
    {
        public string? borrower { get; set; }
        public SigmaFiVerifiedAssetAmount? requested { get; set; }
        public SigmaFiVerifiedAssetAmount? repayment { get; set; }
        public int? maturityLength { get; set; }//amount of blocks before default
    }

    

    public class SigmaFiOrderExtended : SigmaFiOrder
    {
        public NodeBox? box { get; set; }
        public List<SigmaFiVerifiedAssetAmount>? collateral { get; set; }
        public double collateralizationRatio { get; set; }
        public SigmaFiVerifiedAssetAmount? interest { get; set; }
        public double interestPercentage { get; set; }
        public double APR { get; set; }
    }
}
