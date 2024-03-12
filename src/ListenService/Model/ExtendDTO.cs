using System;
using Grpc.Core;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using CommonLibrary.Common.Common;
using Nethereum.Util;
namespace ListenService.Model
{
    [FunctionOutput]
    public class ExtendDTO
    {
        [Parameter("uint256", "seller_ratio", 1)]
        public BigInteger SellerRatio { get; set; }
    }
}

