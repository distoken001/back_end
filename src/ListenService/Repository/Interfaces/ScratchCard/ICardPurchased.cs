using System;
using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
	public interface ICardPurchased
    {
        Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id);
    }
}

