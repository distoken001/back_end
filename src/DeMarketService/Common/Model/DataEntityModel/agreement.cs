using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace deMarketService.Common.Model.DataEntityModel
{
    /// <summary>
    /// 用户协议
    /// </summary>
    [Table("agreement", Schema = "resource")]
    public class agreement
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        [Key]
        public Guid id { get; set; }
        /// <summary>
        /// 协议名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 版本
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 协议类型
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 协议内容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createtime { get; set; }
        /// <summary>
        /// 图片 解决格式问题
        /// </summary>
        public string pic { get; set; }
        /// <summary>
        /// 税源地id
        /// </summary>
        public Guid? supplier_id { get; set; }
    }
}
