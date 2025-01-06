using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using DeMarketAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeMarketAPI.Services
{
    public class TxCosUploadeService : ITxCosUploadeService
    {
        private CosXml cosXml;
        private readonly IConfiguration configuration;

        public TxCosUploadeService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public CosXml CosXml
        {
            get
            {
                if (cosXml == null)
                {
                    CosXmlConfig config = new CosXmlConfig.Builder()
                     .IsHttps(true)  //设置默认 HTTPS 请求
                     .SetRegion("ap-beijing")  //设置一个默认的存储桶地域
                     .SetDebugLog(true)  //显示日志
                     .SetAppid(configuration["Cos:AppId"])
                     .Build();  //创建 CosXmlConfig 对象
                    long durationSecond = 600;  //每次请求签名有效时长，单位为秒
                    QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(
                    configuration["Cos:SecretId"], configuration["Cos:SecretKey"], durationSecond);
                    cosXml = new CosXmlServer(config, cosCredentialProvider);
                }
                return cosXml;
            }
        }

        public async Task<string> Upload(byte[] bytes, string cosName)
        {
            try
            {
                var _cosPath = $"de-market/{cosName}";
                PutObjectRequest putObjectRequest = new PutObjectRequest("demarket-1303108648", _cosPath, bytes);
                CosXml.PutObject(putObjectRequest);
                return $"https://demarket-1303108648.cos.ap-beijing.myqcloud.com/{_cosPath}";
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败
                throw new Exception("CosClientException: " + clientEx);
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                throw new Exception("CosServerException: " + serverEx.GetInfo());
                //请求失败
            }
        }

        public string GetCredential()
        {
            string region = "ap-beijing";//"ap-guangzhou";  // bucket 所在区域
            string allowPrefix = "*"; // 这里改成允许的路径前缀，可以根据自己网站的用户登录态判断允许上传的具体路径，例子： a.jpg 或者 a/* 或者 * (使用通配符*存在重大安全风险, 请谨慎评估使用)
            string[] allowActions = new string[] {  // 允许的操作范围，这里以上传操作为例
                "name/cos:PutObject",
                "name/cos:PostObject",
                "name/cos:InitiateMultipartUpload",
                "name/cos:ListMultipartUploads",
                "name/cos:ListParts",
                "name/cos:UploadPart",
                "name/cos:CompleteMultipartUpload"
            };
            string secretId = configuration["Cos:SecretId"];//"COS_KEY"; // 云 API 密钥 Id
            string secretKey = configuration["Cos:SecretKey"];//"COS_SECRET"; // 云 API 密钥 Key

            Dictionary<string, object> values = new Dictionary<string, object>();
            values.Add("bucket", "demarket-1303108648");
            values.Add("region", region);
            values.Add("allowPrefix", allowPrefix);
            // 也可以通过 allowPrefixes 指定路径前缀的集合
            // values.Add("allowPrefixes", new string[] {
            //     "path/to/dir1/*",
            //     "path/to/dir2/*",
            // });
            values.Add("allowActions", allowActions);
            values.Add("durationSeconds", 1800);

            values.Add("secretId", secretId);
            values.Add("secretKey", secretKey);

            var dic = COSSTS.STSClient.genCredential(values);
            return Newtonsoft.Json.JsonConvert.SerializeObject(dic);
        }
    }
}