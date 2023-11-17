using System;
using CommonLibrary.Common.Common;

namespace ListenWeb3.Repository.Interfaces
{
	public interface ICardPurchased
    {
        Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id);
    }
}

