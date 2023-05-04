namespace deMarketService.Common.Model
{
    /// <summary>
    /// 用户四要素验证类型
    /// </summary>
    public enum UserInfoValidType
    {
        /// <summary>
        /// 姓名身份证号码验证
        /// </summary>
        UserInfo = 1,
        /// <summary>
        /// 手机号验证
        /// </summary>
        Mobile = 2,
        /// <summary>
        /// 银行卡验证
        /// </summary>
        BankCardNum = 3,
        /// <summary>
        /// 银行卡归属地
        /// </summary>
        BankLocation = 4,
        /// <summary>
        /// 姓名银行卡
        /// </summary>
        NameBank = 5,
        /// <summary>
        /// 姓名、身份证号、银行卡和手机号验证
        /// </summary>
        BankCardNumMobile = 6,
    }

    /// <summary>
    /// 高灯云接口错误代码
    /// </summary>
    public enum EReqErrCode
    {
        /// <summary>
        /// 正常
        /// </summary>
        S0 = 0,

        /// <summary>
        /// 请求错误,服务器无法理解http请求
        /// </summary>
        S400 = 400,

        /// <summary>
        /// 禁止访问
        /// </summary>
        S403 = 403,

        /// <summary>
        /// 服务器内部错误
        /// </summary>
        S500 = 500,

        /// <summary>
        /// 版本错误
        /// </summary>
        S1001 = 1001,

        /// <summary>
        /// 缺少公共参数
        /// </summary>
        S1002 = 1002,

        /// <summary>
        /// 时间戳错误
        /// </summary>
        S1003 = 1003,

        /// <summary>
        /// appkey不存在
        /// </summary>
        S1004 = 1004,

        /// <summary>
        /// app已关闭
        /// </summary>
        S1005 = 1005,

        /// <summary>
        /// 开发者已被关闭
        /// </summary>
        S1006 = 1006,

        /// <summary>
        /// 签名错误
        /// </summary>
        S1007 = 1007,

        /// <summary>
        /// 开发者资质待审核
        /// </summary>
        S1008 = 1008,

        /// <summary>
        /// 开发者资质审核未通过
        /// </summary>
        S1009 = 1009,

        /// <summary>
        /// 参数检验错误
        /// </summary>
        S1010 = 1010,

        /// <summary>
        /// 脏数据,联系管理员
        /// </summary>
        S1011 = 1011,

        /// <summary>
        /// 接口调用次数不足
        /// </summary>
        S1012 = 1012,

        /// <summary>
        /// 接口计费错误
        /// </summary>
        S1013 = 1013,

        /// <summary>
        /// 业务参数错误
        /// </summary>
        S1014 = 1014,

        /// <summary>
        /// 依赖服务返回拒绝(一般业务参数错误)
        /// </summary>
        S1015 = 1015,

        /// <summary>
        /// 依赖服务故障
        /// </summary>
        S1016 = 1016,
    }

    /// <summary>
    /// 高灯云接口环境变量
    /// </summary>
    public enum EApiEnv
    {
        /// <summary>
        /// 测试
        /// </summary>
        Test,

        /// <summary>
        /// 正式
        /// </summary>
        Prod,
    }
    public enum OrderStatus
    {
        Initial,//待购买0
        Ordered,//被下单1
        Completed,//已完成2
        BuyerBreak,//买家毁约3
        SellerBreak,//卖家毁约4
        SellerCancelWithoutDuty,//卖家无责取消5
        BuyerLanchCancel,//买家发起取消6
        SellerLanchCancel,//卖家发起取消7
        SellerRejectCancel,//卖家拒绝取消8
        BuyerRejectCancel,//买家拒绝取消9
        ConsultCancelCompleted//协商取消完成10
    }
    public enum ChainEnum : int
    {
        MainNet = 1,
        ArbitrumOne = 42161,
        optimisticEthereum = 10,
        Polygon = 137,
        Ropsten = 3,
        Rinkeby = 4,
        Goerli = 5,
        Kovan = 42,
        Bsc = 56,
        Sokol = 77,
        BscTestnet = 97,
        Xdai = 100,
        Heco = 100,
        Opera = 250,
        HecoTestnet = 256,
        OptimisticGoerli = 420,
        Moonbeam = 1284,
        Moonriver = 1285,
        MoonbaseAlpha = 1287,
        FtmTestnet = 4002,
        Chiado = 10200,
        AvalancheFujiTestnet = 43113,
        Avalanche = 43114,
        PolygonMumbai = 80001,
        ArbitrumTestnet = 421613,
        ArbitrumGoerli = 421613,
        Aepolia = 11155111,
        Aurora = 1313161554,
        AuroraTestnet = 1313161555,
        Harmony = 1666600000,
        HarmonyTest = 1666700000
    }
}
