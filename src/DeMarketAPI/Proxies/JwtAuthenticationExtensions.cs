//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.IdentityModel.Tokens;

//namespace DeMarketAPI.Proxies
//{
//    public static class JwtAuthenticationExtensions
//    {
//        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
//        {
//            var jwtOptions = new JwtOptions();
//            configuration.Bind("JwtOptions", jwtOptions);

//            services.AddSingleton(jwtOptions);
//            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//                .AddJwtBearer(options =>
//                {
//                    options.TokenValidationParameters = new TokenValidationParameters
//                    {
//                        ValidateIssuerSigningKey = true,
//                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
//                        ValidateIssuer = true,
//                        ValidIssuer = jwtOptions.Issuer,
//                        ValidateAudience = true,
//                        ValidAudience = jwtOptions.Audience,
//                        ValidateLifetime = true,
//                        ClockSkew = TimeSpan.Zero
//                    };

//                    options.Events = new JwtBearerEvents
//                    {
//                        OnAuthenticationFailed = context =>
//                        {
//                            if (context.Exception != null)
//                            {
//                                // 认证失败处理逻辑
//                                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
//                            }
//                            return Task.CompletedTask;
//                        },
//                        OnTokenValidated = context =>
//                        {
//                            // 认证成功处理逻辑
//                            return Task.CompletedTask;
//                        }
//                    };
//                });

//            return services;
//        }
//    }

//    public class JwtOptions
//    {
//        public string SecretKey { get; set; }
//        public string Issuer { get; set; }
//        public string Audience { get; set; }
//        public int ExpiryMinutes { get; set; }
//    }

//    public class JwtAuthenticationFilter : IAuthorizationFilter
//    {
//        private readonly JwtOptions _jwtOptions;

//        public JwtAuthenticationFilter(JwtOptions jwtOptions)
//        {
//            _jwtOptions = jwtOptions;
//        }

//        public void OnAuthorization(AuthorizationFilterContext context)
//        {
//            var authorizationHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
//            if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer "))
//            {
//                context.Result = new UnauthorizedResult();
//                return;
//            }

//            var tokenString = authorizationHeader.Substring("Bearer ".Length);
//            if (!ValidateToken(tokenString, out IEnumerable<Claim> claims))
//            {
//                context.Result = new UnauthorizedResult();
//                return;
//            }

//            // 在 HttpContext 中设置认证信息
//            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

//            return;
//        }

//        private bool ValidateToken(string tokenString, out IEnumerable<Claim> claims)
//        {
//            claims = null;

//            var validationParameters = new TokenValidationParameters
//            {
//                ValidateIssuerSigningKey = true,
//                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
//                ValidateIssuer = true,
//                ValidIssuer = _jwtOptions.Issuer,
//                ValidateAudience = true,
//                ValidAudience = _jwtOptions.Audience,
//                ValidateLifetime = true,
//                ClockSkew = TimeSpan.Zero
//            };

//            try
//            {
//                var handler = new JwtSecurityTokenHandler();
//                handler.ValidateToken(tokenString, validationParameters, out SecurityToken validatedToken);

//                claims = ((JwtSecurityToken)validatedToken).Claims;
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }
//    }

//}
