using System;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ListenService.Model
{

    [Event("BoxMinted")]
    public class BoxMintedEventDTO : IEventDTO
    {
        [Parameter("address", "user", 1, true)]
        public string User { get; set; }

        [Parameter("string", "cardType", 2, false)]
        public string BoxType { get; set; }

        [Parameter("uint256", "numberOfBoxs", 3, false)]
        public BigInteger NumberOfBoxs { get; set; }
    }
}

