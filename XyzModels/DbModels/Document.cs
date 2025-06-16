using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XyzModels.DbModels
{
	class Document
	{
		public long Id { get; set; }
		public string? Name { get; set; }
		public string? Description { get; set; }
		public string? Author_full_name { get; set; }
		public string? Author_email { get; set; }
		public string? Serial_code { get; set; }
		public string? Publication_code { get; set; }
		public DateTime Created_at { get; set; }
		public DateTime Updated_at { get; set; }
		public DateTime deleted_at { get; set; }
		public bool active { get; set; } = true;
	}
}
