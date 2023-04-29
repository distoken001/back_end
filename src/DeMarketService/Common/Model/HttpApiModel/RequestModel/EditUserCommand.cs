using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace deMarketService.Common.Model.HttpApiModel.RequestModel
{
    public class EditUserCommand
    {
        /// <summary>
        /// 昵称
        /// </summary>
        [Required]
        public string NickName { get; set; }

        public IFormCollection FormCollection { get; set; }
    }
}
