using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace ListenService.Model
{
    [FunctionOutput]
    public class ExtendDTO
    {
        [Parameter("uint256", "seller_ratio", 1)]
        public BigInteger SellerRatio { get; set; }
    }
}