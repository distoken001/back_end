using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonLibrary.Model.DataEntityModel
{
    [Table("tokens")]
    public class tokens
    {
        [Key]
        public long id { get; set; }

        public string token_name { get; set; }
        public string icon { get; set; }
        public DateTime create_time { get; set; }
        public int weight { get; set; }
        public int status { get; set; }
    }
}