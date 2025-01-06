using System.ComponentModel.DataAnnotations.Schema;

namespace CommonLibrary.Model.DataEntityModel
{
    [Table("telegram_user_chat")]
    public class telegram_user_chat
    {
        public int id { get; set; }
        public string user_name { get; set; }
        public long user_id { get; set; }
        public long chat_id { get; set; }
        public string verify_code { get; set; }
        public DateTime create_time { get; set; }
        public DateTime update_time { get; set; }
        public int state { get; set; }
    }
}