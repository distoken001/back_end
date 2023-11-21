using System;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace ListenService.Model
{

    [Event("CardPurchased")]
    public class CardPurchasedEventDTO : IEventDTO
    {
        [Parameter("address", "user", 1, true)]
        public string User { get; set; }

        [Parameter("string", "cardType", 2, false)]
        public string CardType { get; set; }

        [Parameter("uint256", "numberOfCards", 3, false)]
        public BigInteger NumberOfCards { get; set; }
    }
}

