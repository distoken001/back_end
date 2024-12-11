using System;
namespace ListenService.Model
{
	public class DeBoxMessageDTO
	{
	public string to_user_id { get; set; }
		public string group_id { get; set; }
		public string object_name { get; set; }
		public string content { get; set; }
    }
}
