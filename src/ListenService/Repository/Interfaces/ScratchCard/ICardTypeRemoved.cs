using System;
using CommonLibrary.Common.Common;

namespace ListenService.Repository.Interfaces
{
    public interface ICardTypeRemoved
    {
        Task StartAsync(string nodeUrl, string contractAddress, ChainEnum chain_id);
    }
}

