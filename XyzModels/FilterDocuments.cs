using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XyzModels
{
    public class FilterDocuments
    {
		public long Id { get; set; }
		public string? Serial_code { get; set; }
		public string? Publication_code { get; set; }
		public string? AuthorOrEmail { get; set; }
		public int Page { get; set; } = 1;
	}
}
