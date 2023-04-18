namespace deMarketService.Model
{
    public static class StringConstant
    {
        public const string DatabaseConnectionString = "ConnectionStrings:IdentityConnection";
        public const string DatabaseConnectionString_ReadOnly = "ConnectionStrings:IdentityConnectionRead";
        public const string RedisConnectionString = "ConnectionStrings:RedisConnection";
        public const string RabbitMQUser = "RabbitMQ:User";
        public const string RabbitMQPwd = "RabbitMQ:Pwd";
        public const string RabbitMQHost = "RabbitMQ:Host";
        public const string RabbitMQPort = "RabbitMQ:Port";
        public const string MetaVersion = "Meta:Version";
        public const string MetaEnv = "Meta:Env";


        public const string secretKey = "f9d4f3eda81c44eda42bd25458c9fcb4";
        public const string issuer = "deMarketIssuer";
        public const string audience = "deMarketAudience";


        public const string DeMarketServiceConsulServiceName = "DeMarketServiceConsulServiceName";
    }
}
