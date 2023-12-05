using DeMarketAPI.Common.Model.HttpApiModel.ResponseModel;
using DeMarketAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

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
                var fileName = string.Format("{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(file.FileName));
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
                var fileName = string.Format("{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(file.FileName));
                string grandparentDirectory = Directory.GetParent(Directory.GetParent(_environment.ContentRootPath).FullName).FullName;
                var uploadDirectory = Path.Combine(grandparentDirectory, "docs/compress"); // 修改为你选择的目录
                var filePath = Path.Combine(uploadDirectory, fileName);
                ImageFormat imageFormat = ImageFormat.Jpeg;
                string fileExtension = Path.GetExtension(fileName).TrimStart('.').ToLower(); ; ;
                switch (fileExtension)
                {
                    case "png":
                        imageFormat = ImageFormat.Png;
                        break;
                    case "gif":
                        imageFormat = ImageFormat.Gif;
                        break;
                    case "ico":
                        imageFormat = ImageFormat.Icon;
                        break;
                    case "jpeg":
                        imageFormat = ImageFormat.Jpeg;
                        break;
                    case "webp":
                        imageFormat = ImageFormat.Webp;
                        break;
                    case "jpg":
                        imageFormat = ImageFormat.Jpeg;
                        break;
                }
              
                CompressAndSaveImage(file.OpenReadStream(), filePath, imageFormat);
                fileName = _configuration["ApiDomain"] + "/docs/compress/" + fileName;
                return Json(new WebApiResult(1, "上传图片", fileName));
            }
            else
            {
                return Json(new WebApiResult(-1, "没有上传的问题"));
            }
        }
        private void CompressAndSaveImage(Stream imageStream, string savePath, ImageFormat imageFormat)
        {
            using (Image image = Image.FromStream(imageStream))
            {
                // 设置压缩质量，这里设置为50（范围从0到100，100为最高质量）
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);

                // 获取 JPEG 图像编码器
                ImageCodecInfo jpegEncoder = GetEncoder(imageFormat);

                // 保存压缩版图片
                image.Save(savePath, jpegEncoder, encoderParameters);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            // 如果找不到指定格式的编码器，可以返回默认的 JPEG 编码器
            return codecs[1]; // 这里假设索引为1的编码器是 JPEG 编码器
        }
    }
}
