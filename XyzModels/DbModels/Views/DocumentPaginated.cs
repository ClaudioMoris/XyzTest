using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XyzModels.DbModels.Views
{
    public class DocumentPaginated
    {
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int PageCount { get; set; }
		public int DocumentsCount { get; set; }
		public IEnumerable<Document> Documents { get; set; }
	}
}
