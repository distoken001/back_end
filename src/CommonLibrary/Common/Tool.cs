namespace CommonLibrary.Common.Common
{
    public static class Tool
    {
        /// <summary>
        /// 获取用户身份
        /// </summary>
        /// <param name="buyer"></param>
        /// <param name="seller"></param>
        /// <returns></returns>
        public static BelongUserEnum getBelongUserEnum(string current, string buyer, string seller)
        {
            if (string.IsNullOrEmpty(current))
            {
                return BelongUserEnum.未知;
            }
            else if (current.Equals(buyer, StringComparison.OrdinalIgnoreCase) && current.Equals(seller, StringComparison.OrdinalIgnoreCase))
            {
                return BelongUserEnum.同是是买家卖家;
            }
            else if (current.Equals(buyer, StringComparison.OrdinalIgnoreCase))
            {
                return BelongUserEnum.买家;
            }
            else if (current.Equals(seller, StringComparison.OrdinalIgnoreCase))
            {
                return BelongUserEnum.卖家;
            }
            else
            {
                return BelongUserEnum.游客;
            };
        }
    }
}