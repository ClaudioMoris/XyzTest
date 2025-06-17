using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XyzLibrary.Models;
using XyzLibrary.Repositorys;
using XyzModels;

namespace XyzLibrary.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DocumentController : Controller
	{
		private readonly IDocumentsRepository _documentsRepository;
		public DocumentController(IDocumentsRepository documentsRepository)
		{
			_documentsRepository = documentsRepository;
		}

		/// <summary>
		/// Busca un documento por su ID.
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> SearchById(RequestId requestId)
		{
			if (requestId.Id == 0)
			{
				var response = new ErrorResponse
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Error al ingresar el ID",
					Detail = "Debes proporcionar un ID válido",
				};
				return BadRequest(response);
			}

			var resultData = await _documentsRepository.SearchById(requestId.Id);

			if (resultData == null)
			{
				var response = new ErrorResponse
				{
					StatusCode = StatusCodes.Status404NotFound,
					Message = "No se puede encontrar el ID solicitado",
					Detail = "ID no encontrado. Revisa los datos o intenta con otro identificador",
				};
				return NotFound(response);
			}
			return Ok(resultData);
		}

		/// <summary>
		/// Busqueda de documentos por sus atributos.
		/// </summary>
		[HttpPost("SearchByFilters")]
		public async Task<IActionResult> SearchByFilters(FilterDocuments filterDocuments)
		{
			if (!ModelState.IsValid)//valida el modelo
			{
				return BadRequest(ModelState);
			}

			if (filterDocuments.Id == 0 || filterDocuments.Page == 0) //valida id y page sea mayor que 0
			{
				var response = new ErrorResponse
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "El valor de 'id' o 'página' debe ser numérico y mayor que 0",
					Detail = "Solicitud incorrecta: 'id' o 'página' contiene un valor no válido",
				};
				return NotFound(response);
			}

			// se pasan los parametros insertados y la cantidad de registros por pagina (dado que se definen 10 queda fijo en este ejemplo)
			var resultSearch = await _documentsRepository.SearchDocumentByFilters(filterDocuments, 10);

			return Ok(resultSearch);
		}

	}
}
