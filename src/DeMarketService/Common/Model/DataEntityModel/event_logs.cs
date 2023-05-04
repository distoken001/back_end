using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace deMarketService.Common.Model.DataEntityModel
{
    [Table("event_logs")]
    public class event_logs
    {
        [Key]
        public int id { get; set; }

        /**
         * 事件名
         */
        public String event_name { get; set; }
        /**
         * 调用人
         */
        public String operater { get; set; }

        /**
         * 订单编号
         */
        public String order_id { get; set; }

        public String hash { get; set; }
        /**
         * 发送数据
         */
        public String data { get; set; }

        /**
         * 
         */
        public int status { get; set; }

        /**
         * 创建时间
         */
        public DateTime create_time { get; set; }

        /**
         * 修改时间
         */
        public DateTime update_time { get; set; }

        /**
         * 修改人
         */
        public String updater { get; set; }
        /**
         * 创建人
         */
        public String creator { get; set; }
        /// <summary>
        ///链id
        /// </summary>
        public ChainEnum chain_id { get; set; }

    }
}
