using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace deMarketService.Common.Common
{
    public static class TokenHelper {

        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="secretKey">为密钥字符串</param>
        /// <param name="issuer">为签发者</param>
        /// <param name="audience">为接收者</param>
        /// <param name="expireMinutes">过期时间（单位为分钟）</param>
        /// <param name="claims">需要添加到 Token 中的声明</param>
        /// <returns>SHA256加密结果</returns>
        public static string GenerateToken(string secretKey, string issuer, string audience, int expireMinutes, params Claim[] claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = now.AddMinutes(expireMinutes),
                SigningCredentials = credentials,
                NotBefore = now
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// 校验token
        /// </summary>
        /// <param name="secretKey">为密钥字符串</param>
        /// <param name="issuer">为签发者</param>
        /// <param name="audience">为接收者</param>
        /// <param name="tokenString">token值</param>
        /// <param name="claims">需要添加到 Token 中的声明</param>
        /// <returns>SHA256加密结果</returns>
        public static bool ValidateJwt(string secretKey, string issuer, string audience, string tokenString, out IEnumerable<Claim> claims)
        {
            claims = null;

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(tokenString, validationParameters, out SecurityToken validatedToken);

                claims = ((JwtSecurityToken)validatedToken).Claims;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
}
