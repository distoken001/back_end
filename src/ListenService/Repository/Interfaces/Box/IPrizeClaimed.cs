using System;
using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
	public interface IPrizeClaimed
    {
        Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id);
    }
}

