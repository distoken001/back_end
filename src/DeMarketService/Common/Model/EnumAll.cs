namespace deMarketService.Model
{
    public static class EnumAll
    {
        /*
        /// <summary>
        /// 身份证正反面
        /// </summary>
        public enum IdcardTypeEnum
        {
            /// <summary>
            /// 身份证正面
            /// </summary>
            IdcardUser = 1,
            /// <summary>
            /// 身份证反面
            /// </summary>
            IdcardNation = 2
        }*/

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
        
    }
}
