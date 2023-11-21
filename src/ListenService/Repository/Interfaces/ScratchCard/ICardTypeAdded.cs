using System;
using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
	public interface ICardTypeAdded
    {
        Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id);
    }
}

