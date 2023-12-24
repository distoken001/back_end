using System;
using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
	public interface IBoxTypeAdded
    {
        Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id);
    }
}

