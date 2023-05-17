using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace deMarketService.Proxies
{
    /// <summary>
    /// 邮件服务
    /// </summary>
    public class EmailProxy
    {
        private string UserName { get; set; }
        private string PassWord { get; set; }
        private string Host { get; set; }
        private int Port { get; set; }
        public ILogger _logger;

        public EmailProxy(IConfiguration config, ILogger<EmailProxy> logger)
        {
            if (string.IsNullOrEmpty(config["Email:Host"]))
            {
                Host = "smtp.exmail.qq.com";
            }
            else
            {
                Host = config["Email:Host"].Trim();
            }

            if (string.IsNullOrEmpty(config["Email:Port"]))
            {
                Port = 587;
            }
            else
            {
                Port = Convert.ToInt32(config["Email:Port"].Trim());
            }
            _logger = logger;

            if (!string.IsNullOrEmpty(config["Email:UserName"]))
            {
                UserName = config["Email:UserName"].Trim();
            }
            if (!string.IsNullOrEmpty(config["Email:PassWord"]))
            {
                PassWord = config["Email:PassWord"].Trim();
            }
        }

        /// <summary>
        /// 邮件发送
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="toMailAddressList"></param>
        /// <returns></returns>
        public async Task<bool> SendMailAsync(string subject, string body, List<string> toMailAddressList)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                throw new Exception("Email:UserName为空");
            }
            if (string.IsNullOrEmpty(PassWord))
            {
                throw new Exception("Email:PassWord为空");
            }

            SmtpClient smtpClient = new SmtpClient();
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
            smtpClient.Host = Host;//指定SMTP服务器
            smtpClient.Port = Port;
            smtpClient.Credentials = new NetworkCredential(UserName, PassWord);//用户名和密码
            smtpClient.EnableSsl = true;
            MailAddress fromAddress = new MailAddress(UserName);//用户名和密码
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = fromAddress;
            toMailAddressList.ForEach(e =>
            {
                mailMessage.To.Add(e);
            });
            mailMessage.Subject = subject;//主题
            mailMessage.Body = body;//内容
            mailMessage.BodyEncoding = Encoding.Default;//正文编码
            mailMessage.IsBodyHtml = true;//设置为HTML格式
            mailMessage.Priority = MailPriority.Normal;//优先级
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error SendMailAsync:{ex.ToString()}");
                return false;
            }
        }
    }
}
