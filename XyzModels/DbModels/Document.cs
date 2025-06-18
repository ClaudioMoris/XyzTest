using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XyzModels.DbModels
{
	public class Document
	{
		public long Id { get; set; }
		[Required(ErrorMessage ="'Name' es requerido")]
		[StringLength(100, ErrorMessage = "'Name' no puede tener más de {1} caracteres")]
		public string? Name { get; set; }
		[StringLength(1000, ErrorMessage = "'Description' no puede tener más de {1} caracteres")]
		public string? Description { get; set; }
		[Required(ErrorMessage = "'Author_Full_Name' es requerido")]
		[StringLength(300, ErrorMessage = "'Author_Full_Name' no puede tener más de {1} caracteres")]
		public string? Author_full_name { get; set; }
		[Required(ErrorMessage = "'Author_email' es requerido")]
		[StringLength(100, ErrorMessage = "'Author_email' no puede tener más de {1} caracteres")]
		/*
		 * para esta etiqueta "EmailAddress" en particular se consulto a chatGPT (https://chatgpt.com/share/68524003-8018-800a-9558-74b297eed02d)
		 * usualmente lo hago directamente en el controlador para poder validar el .net .com o .cl del email pero creo que esto esta mas ordenado
		 * aun que solo valida ejamplo@ejemplo .
		 */
		[EmailAddress(ErrorMessage = "'Author_email' no es un email valido")]
		public string? Author_email { get; set; }
		[Required(ErrorMessage = "'Serial_code' es requerido")]
		[StringLength(16, ErrorMessage = "'Serial_code' no puede tener más de {1} caracteres")]
		public string? Serial_code { get; set; }
		[Required(ErrorMessage = "'Publication_code' es requerido")]
		[StringLength(100, ErrorMessage = "'Publication_code' no puede tener más de {1} caracteres")]
		public string? Publication_code { get; set; }
		public DateTime Created_at { get; set; }
		public DateTime Updated_at { get; set; }
		public DateTime deleted_at { get; set; }
		public bool active { get; set; } = true;
	}
}
