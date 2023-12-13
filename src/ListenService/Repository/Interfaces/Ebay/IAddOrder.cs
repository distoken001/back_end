using System;
using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
	public interface IAddOrder
    {
        Task StartAsync(string nodeWss, string nodeHttps,string contractAddress, ChainEnum chain_id);
    }
}

