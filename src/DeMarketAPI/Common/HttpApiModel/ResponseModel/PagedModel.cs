using Newtonsoft.Json;
using System.Collections.Generic;

namespace DeMarketAPI.Common.Model.HttpApiModel.ResponseModel
{
    public class PagedModel<TModel> where TModel : class
    {
        public PagedModel(int total, List<TModel> list)
        {
            this.Total = total;
            this.List = list;
        }

        [JsonProperty("total")]
        public int Total { get; set; }
        [JsonProperty("list")]
        public List<TModel> List { get; set; }
    }
}
