using Blake2Fast;
using FleetSharp.Sigma;
using FleetSharp.SigmaFi.Types;
using FleetSharp.Types;
using FleetSharp.SpectrumFi;

namespace FleetSharp.SigmaFi
{
    public class SigmaFi
    {
        public string nodeURL;
        public FleetSharp.NodeInterface node;

        public SigmaFi(string nodeURL)
        {
            this.nodeURL = nodeURL;
            node = new FleetSharp.NodeInterface(nodeURL);
        }

        private static long SAFE_MIN_BOX_VALUE = 1000000;
        private static int ergoSecondsPerBlock = 120;

        //https://github.com/capt-nemo429/sigmafi-ui/blob/main/src/offchain/plugins.ts
        private static string ERG_BOND_CONTRACT = "100204000402d805d601b2a5730000d602e4c6a70808d603db6308a7d604c1a7d605e4c6a705089592a3e4c6a70704d19683040193c27201d0720293db63087201720393c17201720493e4c67201040ec5a7d801d606b2a5730100ea02d19683060193c27201d0720293c17201e4c6a7060593e4c67201040ec5a793c27206d0720593db63087206720393c1720672047205";
        private static List<string> TOKEN_BOND_CONTRACT_TEMPLATE = new List<string> {
            "10060400040004020580897a0e20",
            "0402d805d601b2a5730000d602e4c6a70808d603db6308a7d604c1a7d605e4c6a705089592a3e4c6a70704d19683040193c27201d0720293db63087201720393c17201720493e4c67201040ec5a7d803d606db63087201d607b27206730100d608b2a5730200ea02d19683090193c27201d0720293c172017303938c7207017304938c720702e4c6a7060593b17206730593e4c67201040ec5a793c27208d0720593db63087208720393c1720872047205"
        };

        private static string ORDER_ON_CLOSE_ERG_CONTRACT = "1012040005e80705c09a0c08cd03a11d3028b9bc57b6ac724485e99960b89c278db6bab5d2b961b01aee29405a0205a0060601000e20eccbd70bb2ed259a3f6888c4b68bbd963ff61e2d71cdfda3c7234231e1e4b76604020400043c04100400040401010402040601010101d80bd601b2a5730000d602e4c6a70408d603e4c6a70704d604e4c6a70505d605e30008d606e67205d6077301d6087302d6097303d60a957206d801d60a7e72040683024406860272099d9c7e720706720a7e7208068602e472059d9c7e730406720a7e72080683014406860272099d9c7e7207067e7204067e720806d60b730595937306cbc27201d804d60c999aa37203e4c672010704d60db2a5730700d60eb2720a730800d60f8c720e02d1ed96830b0193e4c67201040ec5a793e4c672010508720293e4c672010605e4c6a70605e6c67201080893db63087201db6308a793c17201c1a7927203730990720c730a92720c730b93c2720dd0720293c1720d7204ed9591720f720bd801d610b2a5730c009683020193c27210d08c720e01937ec1721006720f730d957206d802d610b2720a730e00d6118c72100295917211720bd801d612b2a5730f009683020193c27212d08c721001937ec17212067211731073117202";
        private static string ORDER_FIXED_ERG_CONTRACT = "100f040005e80705c09a0c08cd03a11d3028b9bc57b6ac724485e99960b89c278db6bab5d2b961b01aee29405a0205a0060601000e20eccbd70bb2ed259a3f6888c4b68bbd963ff61e2d71cdfda3c7234231e1e4b76604020400040401010402040601010101d80ad601b2a5730000d602e4c6a70408d603e4c6a70505d604e30008d605e67204d6067301d6077302d6087303d609957205d801d6097e72030683024406860272089d9c7e72060672097e7207068602e472049d9c7e73040672097e72070683014406860272089d9c7e7206067e7203067e720706d60a730595937306cbc27201d803d60bb2a5730700d60cb27209730800d60d8c720c02d1ed9683090193e4c67201040ec5a793e4c672010508720293e4c672010605e4c6a70605e6c67201080893db63087201db6308a793c17201c1a793e4c672010704e4c6a7070493c2720bd0720293c1720b7203ed9591720d720ad801d60eb2a57309009683020193c2720ed08c720c01937ec1720e06720d730a957205d802d60eb27209730b00d60f8c720e029591720f720ad801d610b2a5730c009683020193c27210d08c720e01937ec1721006720f730d730e7202";

        private static List<string> ORDER_ON_CLOSE_TOKEN_CONTRACT_TEMPLATE = new List<string> {
            "101c04000e20",//tokenid
            "05e80705c09a0c08cd03a11d3028b9bc57b6ac724485e99960b89c278db6bab5d2b961b01aee29405a0205a0060601000e20",//bond contract hash
            "040204000400043c041004000580897a0402040404000580897a040201010402040604000580897a040201010101d80cd601b2a5730000d602e4c6a70408d603e4c6a70704d6047301d605e4c6a70505d606e30008d607e67206d6087302d6097303d60a7304d60b957207d801d60b7e720506830244068602720a9d9c7e720806720b7e7209068602e472069d9c7e730506720b7e720906830144068602720a9d9c7e7208067e7205067e720906d60c730695937307cbc27201d806d60d999aa37203e4c672010704d60eb2a5730800d60fdb6308720ed610b2720f730900d611b2720b730a00d6128c721102d1ed96830e0193e4c67201040ec5a793e4c672010508720293e4c672010605e4c6a70605e6c67201080893db63087201db6308a793c17201c1a7927203730b90720d730c92720d730d93c2720ed0720293c1720e730e938c7210017204938c721002720593b1720f730fed95917212720cd803d613b2a5731000d614db63087213d615b272147311009683050193c27213d08c72110193c172137312938c7215017204937e8c72150206721293b1721473137314957207d802d613b2720b731500d6148c72130295917214720cd803d615b2a5731600d616db63087215d617b272167317009683050193c27215d08c72130193c172157318938c7217017204937e8c72170206721493b172167319731a731b7202"
        };
        private static List<string> ORDER_FIXED_TOKEN_CONTRACT_TEMPLATE = new List<string> {
            "101904000e20",//tokenid
            "05e80705c09a0c08cd03a11d3028b9bc57b6ac724485e99960b89c278db6bab5d2b961b01aee29405a0205a0060601000e20",//bond contract hash
            "0402040004000580897a0402040404000580897a040201010402040604000580897a040201010101d80bd601b2a5730000d602e4c6a70408d6037301d604e4c6a70505d605e30008d606e67205d6077302d6087303d6097304d60a957206d801d60a7e72040683024406860272099d9c7e720706720a7e7208068602e472059d9c7e730506720a7e72080683014406860272099d9c7e7207067e7204067e720806d60b730695937307cbc27201d805d60cb2a5730800d60ddb6308720cd60eb2720d730900d60fb2720a730a00d6108c720f02d1ed96830c0193e4c67201040ec5a793e4c672010508720293e4c672010605e4c6a70605e6c67201080893db63087201db6308a793c17201c1a793e4c672010704e4c6a7070493c2720cd0720293c1720c730b938c720e017203938c720e02720493b1720d730ced95917210720bd803d611b2a5730d00d612db63087211d613b27212730e009683050193c27211d08c720f0193c17211730f938c7213017203937e8c72130206721093b1721273107311957206d802d611b2720a731200d6128c72110295917212720bd803d613b2a5731300d614db63087213d615b272147314009683050193c27213d08c72110193c172137315938c7215017203937e8c72150206721293b172147316731773187202"
        };


        //https://github.com/capt-nemo429/sigmafi-ui/blob/main/src/maps/verifiedAssets.ts
        private static List<SigmaFiVerifiedAsset> SigmaFiVerifiedAssets = new List<SigmaFiVerifiedAsset> {
            new SigmaFiVerifiedAsset { tokenId = "erg",  metadata = new SigmaFiVerifiedAssetMetadata("ERG", 9) },
            new SigmaFiVerifiedAsset { tokenId = "03faf2cb329f2e90d6d23b58d91bbb6c046aa143261cc21f52fbe2824bfcbf04",  metadata = new SigmaFiVerifiedAssetMetadata("SigUSD", 2)},
            new SigmaFiVerifiedAsset { tokenId = "003bd19d0187117f130b62e1bcab0939929ff5c7709f843c5c4dd158949285d0",  metadata = new SigmaFiVerifiedAssetMetadata("SigRSV", 0) },
            new SigmaFiVerifiedAsset { tokenId = "d71693c49a84fbbecd4908c94813b46514b18b67a99952dc1e6e4791556de413",  metadata = new SigmaFiVerifiedAssetMetadata("ergopad", 2) },
            new SigmaFiVerifiedAsset { tokenId = "1fd6e032e8476c4aa54c18c1a308dce83940e8f4a28f576440513ed7326ad489",  metadata = new SigmaFiVerifiedAssetMetadata("Paideia", 4) },
            new SigmaFiVerifiedAsset { tokenId = "9a06d9e545a41fd51eeffc5e20d818073bf820c635e2a9d922269913e0de369d",  metadata = new SigmaFiVerifiedAssetMetadata("SPF", 6) }
        };
        /*
        //https://github.com/capt-nemo429/sigmafi-ui/blob/main/src/views/DashboardView.vue
        private static List<string> StaticOpenOrdersErgoTrees = new List<string> {
            "101c04000e20472c3d4ecaa08fb7392ff041ee2e6af75f4a558810a74b28600549d5392810e805e80705c09a0c08cd03a11d3028b9bc57b6ac724485e99960b89c278db6bab5d2b961b01aee29405a0205a0060601000e207db936eb6a8d804fdf8faee368ee6f5ce4943246798734cfdf0b0f88d56afc8c040204000400043c041004000580897a0402040404000580897a040201010402040604000580897a040201010101d80cd601b2a5730000d602e4c6a70408d603e4c6a70704d6047301d605e4c6a70505d606e30008d607e67206d6087302d6097303d60a7304d60b957207d801d60b7e720506830244068602720a9d9c7e720806720b7e7209068602e472069d9c7e730506720b7e720906830144068602720a9d9c7e7208067e7205067e720906d60c730695937307cbc27201d806d60d999aa37203e4c672010704d60eb2a5730800d60fdb6308720ed610b2720f730900d611b2720b730a00d6128c721102d1ed96830e0193e4c67201040ec5a793e4c672010508720293e4c672010605e4c6a70605e6c67201080893db63087201db6308a793c17201c1a7927203730b90720d730c92720d730d93c2720ed0720293c1720e730e938c7210017204938c721002720593b1720f730fed95917212720cd803d613b2a5731000d614db63087213d615b272147311009683050193c27213d08c72110193c172137312938c7215017204937e8c72150206721293b1721473137314957207d802d613b2720b731500d6148c72130295917214720cd803d615b2a5731600d616db63087215d617b272167317009683050193c27215d08c72130193c172157318938c7217017204937e8c72170206721493b172167319731a731b7202",
            "101c04000e20003bd19d0187117f130b62e1bcab0939929ff5c7709f843c5c4dd158949285d005e80705c09a0c08cd03a11d3028b9bc57b6ac724485e99960b89c278db6bab5d2b961b01aee29405a0205a0060601000e207db936eb6a8d804fdf8faee368ee6f5ce4943246798734cfdf0b0f88d56afc8c040204000400043c041004000580897a0402040404000580897a040201010402040604000580897a040201010101d80cd601b2a5730000d602e4c6a70408d603e4c6a70704d6047301d605e4c6a70505d606e30008d607e67206d6087302d6097303d60a7304d60b957207d801d60b7e720506830244068602720a9d9c7e720806720b7e7209068602e472069d9c7e730506720b7e720906830144068602720a9d9c7e7208067e7205067e720906d60c730695937307cbc27201d806d60d999aa37203e4c672010704d60eb2a5730800d60fdb6308720ed610b2720f730900d611b2720b730a00d6128c721102d1ed96830e0193e4c67201040ec5a793e4c672010508720293e4c672010605e4c6a70605e6c67201080893db63087201db6308a793c17201c1a7927203730b90720d730c92720d730d93c2720ed0720293c1720e730e938c7210017204938c721002720593b1720f730fed95917212720cd803d613b2a5731000d614db63087213d615b272147311009683050193c27213d08c72110193c172137312938c7215017204937e8c72150206721293b1721473137314957207d802d613b2720b731500d6148c72130295917214720cd803d615b2a5731600d616db63087215d617b272167317009683050193c27215d08c72130193c172157318938c7217017204937e8c72170206721493b172167319731a731b7202"
        };*/

        //private static List<string>? OpenOrdersErgoTrees = null;
        //private static List<string>? OngoingLoansErgoTrees = null;

        //Bond
        private string buildBondContract(string tokenId)
        {
            if (tokenId == "erg") return ERG_BOND_CONTRACT;
            return string.Join(tokenId, TOKEN_BOND_CONTRACT_TEMPLATE);
        }

        //Orders

        private string buildFromTemplate(List<string> template, List<string> constants)
        {
            List<string> ret = new List<string>();
            var len = template.Count > constants.Count ? template.Count : constants.Count;

            for (var i = 0; i < len; i++)
            {
                if (i < template.Count) ret.Add(template[i]);
                if (i < constants.Count) ret.Add(constants[i]);
            }

            return string.Join("", ret).ToLowerInvariant();
        }

        private string BuildOrderContract(string tokenId, string type)
        {
            if (tokenId == "erg")
            {
                return type == "on-close" ? ORDER_ON_CLOSE_ERG_CONTRACT : ORDER_FIXED_ERG_CONTRACT;
            }

            var hash = Convert.ToHexString(Blake2b.ComputeHash(32, FleetSharp.Tools.HexToBytes(buildBondContract(tokenId))));
            var template = type == "on-close" ? ORDER_ON_CLOSE_TOKEN_CONTRACT_TEMPLATE : ORDER_FIXED_TOKEN_CONTRACT_TEMPLATE;

            return buildFromTemplate(template, new List<string> { tokenId, hash });
        }

        //Result is cached.
        public List<string> OpenOrderErgoTrees(string? filterTokenId = null)
        {
            //No caching for now.
            //if (OpenOrdersErgoTrees != null) return OpenOrdersErgoTrees;

            //Only do this if the property was still empty
            List<string> ret = new List<string>();

            //ret.AddRange(StaticOpenOrdersErgoTrees);

            //Create the ergotree's for all verified assets (always on-close)
            foreach (var asset in SigmaFiVerifiedAssets)
            {
                if (asset != null && asset.tokenId != null)
                {
                    if (filterTokenId == null || asset.tokenId == filterTokenId) ret.Add(BuildOrderContract(asset.tokenId, "on-close"));
                }
            }

            //OpenOrdersErgoTrees = ret;

            return ret;
        }

        public List<string> OngoingLoanErgoTrees(string? filterTokenId = null)
        {
            //No caching for now.
            //if (OngoingLoansErgoTrees != null) return OngoingLoansErgoTrees;

            //Only do this if the property was still empty
            List<string> ret = new List<string>();

            //Create the ergotree's for all verified assets (always on-close)
            foreach (var asset in SigmaFiVerifiedAssets)
            {
                if (asset != null && asset.tokenId != null)
                {
                    if (filterTokenId == null || asset.tokenId == filterTokenId) ret.Add(buildBondContract(asset.tokenId));
                }
            }

            //OngoingLoansErgoTrees = ret;

            return ret;
        }

        private string ExtractTokenIdFromOrderContract(string contract)
        {
            if (contract.StartsWith(ORDER_ON_CLOSE_TOKEN_CONTRACT_TEMPLATE[0]) || contract.StartsWith(ORDER_FIXED_TOKEN_CONTRACT_TEMPLATE[0]))
            {
                var start = ORDER_ON_CLOSE_TOKEN_CONTRACT_TEMPLATE[0].Length;
                return contract.Substring(start, 64);
            }

            return "erg";
        }
        private string ExtractTokenIdFromBondContract(string contract)
        {
            if (contract.StartsWith(TOKEN_BOND_CONTRACT_TEMPLATE[0]))
            {
                var start = TOKEN_BOND_CONTRACT_TEMPLATE[0].Length;
                return contract.Substring(start, 64);
            }

            return "erg";
        }

        public async Task<SigmaFiOrderExtended> ParseOrderBox(NodeBox box)
        {
            var tokenId = ExtractTokenIdFromOrderContract(box.ergoTree);
            var token = await Cache.GetTokenFromCache(this.node, tokenId);

            var borrower = box?.additionalRegisters?.R4 != null ? ConstantSerializer.SParse(box.additionalRegisters.R4) : "";
            var amount = box?.additionalRegisters?.R5 != null ? ConstantSerializer.SParse(box.additionalRegisters.R5) : 0;
            var repayment = box?.additionalRegisters?.R6 != null ? ConstantSerializer.SParse(box.additionalRegisters.R6) : 0;
            var maturityLength = box?.additionalRegisters?.R7 != null ? ConstantSerializer.SParse(box.additionalRegisters.R7) : 0;

            var interestValue = (repayment ?? 0) - (amount ?? 0);
            SigmaFiVerifiedAssetAmount interestObj = await CreateSigmaFiVerifiedAssetAmount(tokenId, interestValue, new SigmaFiVerifiedAssetMetadata(tokenId == "erg" ? "erg" : token.name, tokenId == "erg" ? 9 : token.decimals));
            SigmaFiVerifiedAssetAmount requestedObj = await CreateSigmaFiVerifiedAssetAmount(tokenId, amount, new SigmaFiVerifiedAssetMetadata(tokenId == "erg" ? "erg" : token.name, tokenId == "erg" ? 9 : token.decimals));
            SigmaFiVerifiedAssetAmount repaymentObj = await CreateSigmaFiVerifiedAssetAmount(tokenId, repayment, new SigmaFiVerifiedAssetMetadata(tokenId == "erg" ? "erg" : token.name, tokenId == "erg" ? 9 : token.decimals));

            double interestPercentage = Math.Round(Convert.ToDouble(interestValue) / Convert.ToDouble(amount ?? 0) * 100.0, 3);
            var apr = Math.Round((interestPercentage / ((Convert.ToDouble(maturityLength ?? 0) * Convert.ToDouble(ergoSecondsPerBlock)) / 60.0 / 60.0 / 24.0)) * 365.0, 3);

            double collateralUSDValue = 0;

            List<SigmaFiVerifiedAssetAmount> collateral = new List<SigmaFiVerifiedAssetAmount>();

            if (box.value > SAFE_MIN_BOX_VALUE)
            {
                var assetObj = await CreateSigmaFiVerifiedAssetAmount("erg", box.value, new SigmaFiVerifiedAssetMetadata("erg", 9));
                collateral.Add(assetObj);
                collateralUSDValue += assetObj.usdValue ?? 0;
            }

            if (box.assets != null)
            {
                foreach (var asset in box.assets)
                {
                    var collateralToken = await Cache.GetTokenFromCache(this.node, asset.tokenId);
                    var assetObj = await CreateSigmaFiVerifiedAssetAmount(asset.tokenId, asset.amount, new SigmaFiVerifiedAssetMetadata(((collateralToken?.name ?? "") == "" ? asset.tokenId : collateralToken?.name), collateralToken.decimals));
                    collateral.Add(assetObj);
                    collateralUSDValue += assetObj.usdValue ?? 0;
                }
            }

            var collateralizationRatio = ((collateralUSDValue - (interestObj.usdValue ?? 0)) / (requestedObj.usdValue ?? 0)) * 100.0;

            var order = new SigmaFiOrderExtended
            {
                borrower = ErgoAddress.fromPublicKeyBytes(borrower, Network.Mainnet).encode(Network.Mainnet),
                requested = requestedObj,
                repayment = repaymentObj,
                maturityLength = maturityLength,
                collateral = collateral,
                collateralizationRatio = collateralizationRatio,
                interest = interestObj,
                interestPercentage = interestPercentage,
                APR = apr,
                box = box
            };

            return order;
        }
        public async Task<SigmaFiBond> ParseBondBox(NodeBox box)
        {
            var indexedHeight = await node.GetIndexedHeight();
            var currentHeight = indexedHeight?.fullHeight ?? 0;

            var tokenId = ExtractTokenIdFromBondContract(box.ergoTree);
            var token = await Cache.GetTokenFromCache(this.node, tokenId);

            var borrower = box?.additionalRegisters?.R5 != null ? ConstantSerializer.SParse(box.additionalRegisters.R5) : "";
            var repayment = box?.additionalRegisters?.R6 != null ? ConstantSerializer.SParse(box.additionalRegisters.R6) : 0;
            var term = (box?.additionalRegisters?.R7 != null ? ConstantSerializer.SParse(box.additionalRegisters.R7) : 0);
            var lender = box?.additionalRegisters?.R8 != null ? ConstantSerializer.SParse(box.additionalRegisters.R8) : "";

            SigmaFiVerifiedAssetAmount repaymentObj = await CreateSigmaFiVerifiedAssetAmount(tokenId, repayment, new SigmaFiVerifiedAssetMetadata(tokenId == "erg" ? "erg" : token.name, tokenId == "erg" ? 9 : token.decimals));

            double collateralUSDValue = 0;

            List<SigmaFiVerifiedAssetAmount> collateral = new List<SigmaFiVerifiedAssetAmount>();

            if (box.value > SAFE_MIN_BOX_VALUE)
            {
                var assetObj = await CreateSigmaFiVerifiedAssetAmount("erg", box.value, new SigmaFiVerifiedAssetMetadata("erg", 9));
                collateral.Add(assetObj);
                collateralUSDValue += assetObj.usdValue ?? 0;
            }

            if (box.assets != null)
            {
                foreach (var asset in box.assets)
                {
                    var collateralToken = await Cache.GetTokenFromCache(this.node, asset.tokenId);
                    var assetObj = await CreateSigmaFiVerifiedAssetAmount(asset.tokenId, asset.amount, new SigmaFiVerifiedAssetMetadata(((collateralToken?.name ?? "") == "" ? asset.tokenId : collateralToken?.name), collateralToken.decimals));
                    collateral.Add(assetObj);
                    collateralUSDValue += assetObj.usdValue ?? 0;
                }
            }

            var collateralizationRatio = (collateralUSDValue / (repaymentObj.usdValue ?? 0)) * 100.0;

            var bond = new SigmaFiBond
            {
                borrower = ErgoAddress.fromPublicKeyBytes(borrower, Network.Mainnet).encode(Network.Mainnet),
                lender = ErgoAddress.fromPublicKeyBytes(lender, Network.Mainnet).encode(Network.Mainnet),
                repayment = repaymentObj,
                collateral = collateral,
                collateralizationRatio = collateralizationRatio,
                termInBlocks = term - currentHeight,
                box = box
            };

            return bond;

        }
        public async Task<List<SigmaFiOrderExtended>?> GetAllOpenOrders(string? filterTokenIdRequested = null, string? filterTokenIdCollateral = null)
        {
            List<string> ergoTrees = OpenOrderErgoTrees(filterTokenIdRequested);
            List<NodeBox> boxes = new List<NodeBox>();
            List<SigmaFiOrderExtended> openOrders = new List<SigmaFiOrderExtended>();

            foreach (var ergoTree in ergoTrees)
            {
                var unspent = await node.GetUnspentBoxesByErgoTree(ergoTree);
                if (unspent != null) boxes.AddRange(unspent);
            }

            //boxes should contain all unspent sigmafi order boxes
            foreach (var box in boxes)
            {
                var order = await ParseOrderBox(box);
                if (filterTokenIdRequested == null || order.requested?.tokenId == filterTokenIdRequested)
                {
                    if (filterTokenIdCollateral == null || (order.collateral?.Exists(x => x.tokenId == filterTokenIdCollateral) ?? false))
                    {
                        openOrders.Add(order);
                    }
                }
            }

            return openOrders;
        }

        //If liquidable = false then only show those that have a positive term, if true then only those that have a negative term. Null returns all.
        public async Task<List<SigmaFiBond>?> GetAllOngoingLoans(string? filterTokenIdRepayment = null, string? filterTokenIdCollateral = null)
        {
            List<string> ergoTrees = OngoingLoanErgoTrees(filterTokenIdRepayment);
            List<NodeBox> boxes = new List<NodeBox>();
            List<SigmaFiBond> ongoingLoans = new List<SigmaFiBond>();

            foreach (var ergoTree in ergoTrees)
            {
                var unspent = await node.GetUnspentBoxesByErgoTree(ergoTree);
                if (unspent != null) boxes.AddRange(unspent);
            }

            //boxes should contain all unspent sigmafi order boxes
            foreach (var box in boxes)
            {
                var bond = await ParseBondBox(box);
                if (filterTokenIdRepayment == null || bond.repayment?.tokenId == filterTokenIdRepayment)
                {
                    if (filterTokenIdCollateral == null || (bond.collateral?.Exists(x => x.tokenId == filterTokenIdCollateral) ?? false))
                    {
                        ongoingLoans.Add(bond);
                    }
                }
            }

            return ongoingLoans;
        }

        //Aggregates all NodeBalanceés of all contract addresses.
        public async Task<NodeBalance> GetTVL()
        {
            List<NodeBalance> balances = new List<NodeBalance>();
            NodeBalance totalBalance = new NodeBalance();

            totalBalance.unconfirmed = new NodeBalanceWrapper();
            totalBalance.confirmed = new NodeBalanceWrapper();
            totalBalance.unconfirmed.nanoErgs = 0;
            totalBalance.unconfirmed.tokens = new List<NodeBalanceToken>();
            totalBalance.confirmed.nanoErgs = 0;
            totalBalance.confirmed.tokens = new List<NodeBalanceToken>();

            //gets the balances of all the contract addresses.
            var ergoTrees = OpenOrderErgoTrees().Concat(OngoingLoanErgoTrees());
            foreach (var ergoTree in ergoTrees)
            {
                var temp = await node.GetAddressBalance(ErgoAddress.fromErgoTree(ergoTree, Network.Mainnet).encode(Network.Mainnet));
                if (temp != null)
                {
                    balances.Add(temp);
                }
            }

            foreach (var balance in balances)
            {
                totalBalance.unconfirmed.nanoErgs += balance?.unconfirmed?.nanoErgs;
                if (balance?.unconfirmed?.tokens != null) totalBalance.unconfirmed.tokens.AddRange(balance.unconfirmed.tokens);

                totalBalance.confirmed.nanoErgs += balance?.confirmed?.nanoErgs;
                if (balance?.confirmed?.tokens != null) totalBalance.confirmed.tokens.AddRange(balance.confirmed.tokens);
            }

            totalBalance.unconfirmed.tokens = totalBalance.unconfirmed.tokens.GroupBy(x => x.tokenId).Select(x => new NodeBalanceToken() { tokenId = x.First().tokenId, amount = x.Sum(y => y.amount), decimals = x.First().decimals, name = x.First().name }).ToList();
            totalBalance.confirmed.tokens = totalBalance.confirmed.tokens.GroupBy(x => x.tokenId).Select(x => new NodeBalanceToken() { tokenId = x.First().tokenId, amount = x.Sum(y => y.amount), decimals = x.First().decimals, name = x.First().name }).ToList();

            return totalBalance;
        }

        private static async Task<SigmaFiVerifiedAssetAmount> CreateSigmaFiVerifiedAssetAmount(string? tokenId, long? amount, SigmaFiVerifiedAssetMetadata? metadata)
        {
            var tokenUSDPrice = await SpectrumFi.SpectrumFi.GetLastPriceForTokenInUSDCached(tokenId.ToLowerInvariant());
            var amountWithDecimals = (Convert.ToDouble(amount ?? 0) / Math.Pow(10, metadata?.decimals ?? 0));

            return new SigmaFiVerifiedAssetAmount()
            {
                tokenId = tokenId,
                amount = amount,
                metadata = metadata,
                amountWithDecimals = amountWithDecimals,
                usdValue = Math.Round(tokenUSDPrice * amountWithDecimals, 2)
            };
        }
    }
}