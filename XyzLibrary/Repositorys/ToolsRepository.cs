using System.Text.RegularExpressions;

namespace XyzLibrary.Repositorys
{
	public interface IToolsRepository
	{
		bool ValidateHexCode(string code);

		/// <summary>
		/// Valida El codigo de publicacion, si esta erroneo retorna false, si es correcto retorna true
		/// </summary>
		bool ValidatePublicationCode(string code);
	}
	public class ToolsRepository : IToolsRepository
	{
		//este metodo fue creado con la ayuda de ChatGPT (https://chatgpt.com/share/68538dc3-93d0-800a-9e4c-2fdeb9ea3003)
		public bool ValidatePublicationCode(string code)
		{
			if (Regex.IsMatch(code, @"^ISO-\d+$") == true)
			{
				return true;
			}
			else if (Regex.IsMatch(code, @"^Ley N° \d{1,3}(\.\d{3})*$") == true)
			{
				return true;
			}
			else if (Regex.IsMatch(code, @"^P-\d{2}\.(19|20)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])$") == true)
			{
				return true;
			}
			return false;
		}
		//este metodo fue creado con la ayuda de ChatGPT (https://chatgpt.com/share/6853972a-9af8-800a-bad5-bc711f02350d)
		public bool ValidateHexCode(string code)
		{
			if (Regex.IsMatch(code, @"^(0x)?[0-9A-Fa-f]+$") == true)
			{
				return true;
			}
			return false;
		}





	}


}
