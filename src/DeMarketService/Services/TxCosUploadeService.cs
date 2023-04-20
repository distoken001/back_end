using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using deMarketService.Services.Interfaces;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using static COSXML.Model.Tag.ListAllMyBuckets;
using TencentCloud.Mvj.V20190926.Models;

namespace deMarketService.Services
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
    }
}
