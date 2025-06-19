using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XyzLibrary.Models;
using XyzLibrary.Repositorys;
using XyzModels;
using XyzModels.DbModels;
using XyzModels.DbModels.Views;

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
		[HttpPost("SearchById")]
		public async Task<IActionResult> SearchById(RequestId requestId)
		/*
		 * Usualmente, este endpoint sería un [HTTPGET], pero dado el requerimiento de la prueba de trabajar con JSON, quedo como [HTTPPOST]
		 * En un caso normal, quedaría por ruta el ID que desearía ver, por ejemplo: https://{tuURL}/Document/SearchById/1
		 */
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			//se valida que el id venga en json
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
			//busca por el id ingresado
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
				return BadRequest(response);
			}

			// se pasan los parametros insertados y la cantidad de registros por pagina (dado que se definen 10 queda fijo en este ejemplo)
			var resultSearch = await _documentsRepository.SearchDocumentByFilters(filterDocuments, 10);

			return Ok(resultSearch);
		}
		/// <summary>
		/// Crea un documento y carga índices asociados.
		/// </summary>
		[HttpPost("CreateDocument")]
		public async Task<IActionResult> CreateDocument(CreateDocument document)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			//valida que si viene sin índices asociados retorne error
			if (document.Pages.Count() == 0)
			{
				var response = new ErrorResponse
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Debes ingresar como minimo 1 índice para este documento",
					Detail = "Ingresa al menos un índice en el arreglo json para poder ingresar este documento",
				};
				return BadRequest(response);
			}
			//procede a crear el documento
			var responseDocument = await _documentsRepository.CreateDocument(document);
			if (responseDocument == null)
			{
				var response = new ErrorResponse
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "No Pudimos guardar el documento",
					Detail = "Por favor contacta a soporte",
				};
				return BadRequest(response);
			}

			return Ok(responseDocument);

		}
		/// <summary>
		/// Actualiza un documento, borra los índices asociados y carga índices nuevos.
		/// </summary>
		[HttpPut("UpdateDocument")]
		public async Task<IActionResult> UpdateDocument(UpdateDocument document)
		{
			//valida el modelo, para cumplir con los caracteres limite y validaciones
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			//valida que vaya un id, dado que si no va no encontrara ningun registro que editar
			if (document.document.Id == 0)
			{
				var response = new ErrorResponse
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "'Id' es requerido para actualizar el documento.",
					Detail = "Debes ingresar un Id",
				};
				return BadRequest(response);
			}
			//valida que mas de un objeto en los indices
			if (document.Pages.Count() == 0)
			{
				var response = new ErrorResponse
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Debes ingresar como minimo 1 índice para este documento",
					Detail = "Ingresa al menos un índice en el arreglo json para poder ingresar este documento",
				};
				return BadRequest(response);
			}

			//si pasa las validaciones intenta actualizar el documento
			var responseDocument = await _documentsRepository.UpdateDocument(document);
			if (responseDocument == null)
			{
				var response = new ErrorResponse
				{
					StatusCode = StatusCodes.Status500InternalServerError,
					Message = "No Pudimos Actualizar el documento",
					Detail = "Por favor contacta a soporte",
				};
				return BadRequest(response);
			}

			return Ok(responseDocument);

		}
		/// <summary>
		/// Borra un documento y sus índices.
		/// </summary>
		[HttpDelete("DeleteDocument")]
		public async Task<IActionResult> DeleteDocument(RequestId requestId)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			//se valida que el id venga en json
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
			//procede a borrar el documento (Desactivar) y borrar los índices.
			var deletedDocument = await _documentsRepository.DeleteDocument(requestId.Id);
			if (deletedDocument == null) 
			{
				var response = new ErrorResponse
				{
					StatusCode = StatusCodes.Status400BadRequest,
					Message = "Problema al eliminar el registro",
					Detail = "Contacta a soporte",
				};
				return BadRequest(response);
			}

			return Ok(deletedDocument);
		}

	}
}
