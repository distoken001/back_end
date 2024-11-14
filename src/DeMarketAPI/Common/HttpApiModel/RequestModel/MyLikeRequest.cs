using System;

namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class MyLikeRequest
    {
        public int pageSize { get; set; } = 10;
        public int pageIndex { get; set; } = 1;
    }
}