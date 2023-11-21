using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DeMarketAPI.Common.Model.HttpApiModel.RequestModel
{
    public class EditUserRequest
    {
        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }
    }
    public class EditUserNickRequest
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public string VerifyCode { get; set; }
    }
    public class EditUserEmaiRequest
    {
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
    }
}
