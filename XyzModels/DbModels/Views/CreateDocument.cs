using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XyzModels.DbModels.Views
{
    public class CreateDocument
    {
		public Document document { get; set; }
		public IEnumerable<Document_page_index> Pages { get; set; }
	}
}
