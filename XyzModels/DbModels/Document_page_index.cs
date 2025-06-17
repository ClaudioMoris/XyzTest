using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XyzModels.DbModels
{
    public class Document_page_index
    {
		public long Id { get; set; }
		public long Document_id { get; set; }
		public string? Name { get; set; }
		public int Page { get; set; }
		public DateTime Created_at { get; set; }
	}
}
