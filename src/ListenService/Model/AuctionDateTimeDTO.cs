using System;
using Grpc.Core;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using CommonLibrary.Common.Common;

namespace ListenService.Model
{
    [FunctionOutput]
    public class AuctionDateTimeDTO
    {
        [Parameter("uint256", "startTime", 1)]
        public BigInteger StartTime { get; set; } // 卖家
        [Parameter("uint256", "endTime", 2)]
        public BigInteger EndTime { get; set; } // 买家
    }
}

