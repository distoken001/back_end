using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class EditUserCommand
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }
    }
}
