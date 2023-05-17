using deMarketService.Model;
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

namespace deMarketService.Proxies
{
    public class TokenFilter : IAsyncAuthorizationFilter
    {
        private readonly IConfiguration _configuration;
        private readonly bool _requireAuth;

        public TokenFilter(IConfiguration configuration, bool requireAuth = true)
        {
            _configuration = configuration;
            _requireAuth = requireAuth;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var path = context.HttpContext.Request.Path.Value.ToLower();
            if (!path.Equals("/api/user/login")&&!path.Equals("/api/notice/sendemail") && !path.Equals("/api/notice/send"))
            {
                var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                //var chainId = context.HttpContext.Request.Headers["chain_id"].FirstOrDefault();
                if (token == null)
                {
                    if (_requireAuth)
                    {
                        context.Result = new JsonResult(new { message = "Authorization header not found" }) { StatusCode = 401 };
                    }
                    return;
                }

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





