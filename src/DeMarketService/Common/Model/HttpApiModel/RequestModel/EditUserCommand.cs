using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class EditUserCommand
    {
        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }
    }
    public class EditUserNickCommand
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
    }
    public class EditUserEmaiCommand
    {
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
    }
}
