using CommonLibrary.Model;
using Microsoft.Extensions.Configuration;

namespace CommonLibrary.Common 
{
    public class ApolloConfigs
    {
        private readonly IConfiguration _config;

        public ApolloConfigs(IConfiguration configuration)
        {
            _config = configuration;
        }

        public string MetaEnv => _config[StringConstant.MetaEnv];

    }
}