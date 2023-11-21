using System;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class NftOwnerOfResponse
    {
        [Parameter("address", 1)]
        public string Owner { get; set; }
    }
}

