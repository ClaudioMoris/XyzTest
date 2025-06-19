using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XyzModels
{
    public class RequestId
    {
        [Required(ErrorMessage ="Id es requerido")]
        public int Id { get; set; }
    }
}
