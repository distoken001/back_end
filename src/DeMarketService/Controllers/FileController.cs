using deMarketService.Common.Model.HttpApiModel.ResponseModel;
using deMarketService.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileUploadExample.Controllers
{
    [Route("api/[controller]")]
    public class FileController : BaseController
    {
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        public FileController(IHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormCollection formCollection)
        {
            if ((formCollection == null || formCollection.Files.Count == 0))
            {
                return Json(new WebApiResult(-1, "没有可上传的文件"));
            }
            var file = formCollection.Files[0];
            if (file.Length > 0)
            {
                var fileName = string.Format("{0}_{1}_cp{2}", DateTime.Now.ToString("yyyyMMddhhmmss"), new Random().Next(10000), Path.GetExtension(file.FileName));
                string grandparentDirectory = Directory.GetParent(Directory.GetParent(_environment.ContentRootPath).FullName).FullName;
                var uploadDirectory = Path.Combine(grandparentDirectory, "uploads"); // 修改为你选择的目录
                var filePath = Path.Combine(uploadDirectory, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                fileName = _configuration["ApiDomain"] + "/uploads/" + fileName;
                return Json(new WebApiResult(1, "上传图片", fileName));
            }
            else
            {
                return Json(new WebApiResult(-1, "没有上传的问题"));
            }
        }
    }
}
