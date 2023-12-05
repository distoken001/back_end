using CommonLibrary.Model;
using DeMarketAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DeMarketAPI.Proxies
{
    public class TokenFilter : IAsyncAuthorizationFilter
    {
        private readonly IConfiguration _configuration;
        private readonly bool _requireAuth;

        public TokenFilter(IConfiguration configuration, bool requireAuth = false)
        {
            _configuration = configuration;
            _requireAuth = requireAuth;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {

            if (!_requireAuth)
            {
                return;
            }
          
            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token == null)
            {
                var path = context.HttpContext.Request.Path.Value;
                if (!path.Equals("/api/user/login", StringComparison.OrdinalIgnoreCase) && !path.Equals("/api/notice/sendemail", StringComparison.OrdinalIgnoreCase) && !path.Equals("/api/order/list", StringComparison.OrdinalIgnoreCase) && !path.Equals("/api/notice/cooperate", StringComparison.OrdinalIgnoreCase) && !path.Equals("/api/orderauction/list", StringComparison.OrdinalIgnoreCase) && !path.Equals("/api/orderauction/switch_like", StringComparison.OrdinalIgnoreCase) && !path.Equals("/api/order/switch_like", StringComparison.OrdinalIgnoreCase) && !path.Equals("/api/notice/sendBot", StringComparison.OrdinalIgnoreCase) && !path.Equals("/api/notice/sendBotAuction", StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new JsonResult(new { message = "Authorization header not found" }) { StatusCode = 401 };
                }
            }
            else
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(StringConstant.secretKey);
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = StringConstant.issuer,
                        ValidAudience = StringConstant.audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var identity = new ClaimsIdentity(jwtToken.Claims);
                    //if (chainId != null)
                    //{
                    //    identity.AddClaim(new Claim("chain_id", chainId));
                    //}
                    context.HttpContext.User = new ClaimsPrincipal(identity);
                }
                catch
                {
                    context.Result = new JsonResult(new { message = "Invalid token" }) { StatusCode = 401 };
                }
            }
        }

    }
}





