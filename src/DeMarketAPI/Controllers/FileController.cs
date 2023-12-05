using DeMarketAPI.Common.Model.HttpApiModel.ResponseModel;
using DeMarketAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;

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
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> UploadFile([FromForm] IFormCollection formCollection)
        {
            if ((formCollection == null || formCollection.Files.Count == 0))
            {
                return Json(new WebApiResult(-1, "没有可上传的文件"));
            }
            var file = formCollection.Files[0];
            if (file.Length > 0)
            {
                var fileName = file.FileName;
                string grandparentDirectory = Directory.GetParent(Directory.GetParent(_environment.ContentRootPath).FullName).FullName;
                var uploadDirectory = Path.Combine(grandparentDirectory, "docs"); // 修改为你选择的目录
                var filePath = Path.Combine(uploadDirectory, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                fileName = _configuration["ApiDomain"] + "/docs/" + fileName;
                return Json(new WebApiResult(1, "上传图片", fileName));
            }
            else
            {
                return Json(new WebApiResult(-1, "没有上传的问题"));
            }
        }
        [HttpPost("upload_compress")]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> UploadCompress([FromForm] IFormCollection formCollection)
        {
            if ((formCollection == null || formCollection.Files.Count == 0))
            {
                return Json(new WebApiResult(-1, "没有可上传的文件"));
            }
            var file = formCollection.Files[0];
            if (file.Length > 0)
            {
                var fileName = file.FileName;
                string grandparentDirectory = Directory.GetParent(Directory.GetParent(_environment.ContentRootPath).FullName).FullName;
                var uploadDirectory = Path.Combine(grandparentDirectory, "docs/compress"); // 修改为你选择的目录
                var filePath = Path.Combine(uploadDirectory, fileName);
                string fileExtension = Path.GetExtension(fileName).TrimStart('.').ToLower();
                CompressJpegQuality(file.OpenReadStream(), filePath);
                fileName = _configuration["ApiDomain"] + "/docs/compress/" + fileName;
                return Json(new WebApiResult(1, "上传图片", fileName));
            }
            else
            {
                return Json(new WebApiResult(-1, "没有上传的问题"));
            }
        }
        private void CompressJpegQuality(Stream imageStream, string outputPath)
        {
            var quality = 100;
            if (imageStream.Length < 200 * 1024) // 将 KB 转换为字节进行比较
            {
                quality = 100;
            }
            else if (imageStream.Length < 400 * 1024) // 将 KB 转换为字节进行比较
            {
                quality = 80;
            }
            else if (imageStream.Length < 600 * 1024) // 将 KB 转换为字节进行比较
            {
                quality = 70;
            }
            else if (imageStream.Length < 800 * 1024) // 将 KB 转换为字节进行比较
            {
                quality = 60;
            }
            else if (imageStream.Length < 1000 * 1024) // 将 KB 转换为字节进行比较
            {
                quality = 50;
            }
            else if (imageStream.Length < 2000 * 1024) // 将 KB 转换为字节进行比较
            {
                quality = 40;
            }
            else
            {
                quality = 30;
            }
            imageStream.Seek(0, SeekOrigin.Begin); // 将流的位置移回到开头
                                                   // 
            using (Image image = Image.Load(imageStream))
            {
                var encoder = new JpegEncoder { Quality = quality };
                // 保存图像到指定路径并应用压缩质量
                image.Save(outputPath, encoder);
            }

        }

    }
}
