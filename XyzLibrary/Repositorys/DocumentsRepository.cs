using Dapper;
using Npgsql;
using System.Linq.Expressions;
using XyzModels;
using XyzModels.DbModels;
using XyzModels.DbModels.Views;

namespace XyzLibrary.Repositorys
{
	public interface IDocumentsRepository
	{   /// <summary>
		/// Crea un documento y sus índices asociados, Requiere <see cref="XyzModels.DbModels.Views.CreateDocument"/>
		/// </summary>
		Task<Document> CreateDocument(CreateDocument createDocument);
		/// <summary>
		/// Desactiva un documento y borra los índices asociados
		/// </summary>
		Task<Document> DeleteDocument(long id);
		/// <summary>
		/// Busca un Documento y sus índices por el ID
		/// </summary>
		Task<DocumentViewModel> SearchById(long id);
		/// <summary>
		/// Busca un documento y sus índices por una serie de filtros, retorna la mayor cantidad de coincidencias posibles,
		/// Requiere <see cref="FilterDocuments"/>
		/// </summary>
		Task<DocumentPaginated> SearchDocumentByFilters(FilterDocuments filterDocuments, int rows);
		/// <summary>
		/// Actualiza un documento y remplaza todos sus indices por los proporcionados,
		/// Requiere <see cref="XyzModels.DbModels.Views.UpdateDocument"/>
		/// </summary>
		Task<Document> UpdateDocument(UpdateDocument updateDocument);
	}
	public class DocumentsRepository : IDocumentsRepository
	{
		private readonly string connectionString;
		public DocumentsRepository(EnvironmentVariable environmentVariable)//obtencion de las variables de entorno para crear la cadena de conexion a bd
		{
			connectionString = $"Host={environmentVariable.hostDb};Database={environmentVariable.databaseNameDb};Username={environmentVariable.usernameDb};Password={environmentVariable.passwordDb}";
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new InvalidOperationException("Las variables de entorno necesarias no están configuradas.");
			}

		}
		public async Task<DocumentViewModel> SearchById(long id)
		{
			using var conn = new NpgsqlConnection(connectionString);

			var resultDocument = await conn.QueryFirstOrDefaultAsync<Document>("select * from document where id = @id and active = true", new { id });
			if (resultDocument == null)
			{
				return null;
			}
			var resultPages = await conn.QueryAsync<Document_page_index>("select * from document_page_index where document_id = @id", new { id });

			var resultViewModel = new DocumentViewModel
			{
				Document = resultDocument,
				Pages = resultPages,
			};

			return resultViewModel;

		}
		public async Task<DocumentPaginated> SearchDocumentByFilters(FilterDocuments filterDocuments, int rows)
		{
			//por defecto quedo en 10 dado el requerimiento(usualmente se pasaria por la request para que se pudiera mostrar un item "catidad por pagina")
			var offset = (filterDocuments.Page - 1) * rows;

			var filtersCustom = new
			{
				filterDocuments.Id,
				filterDocuments.Serial_code,
				filterDocuments.Publication_code,
				filterDocuments.AuthorOrEmail,
				offset,
				rows
			};
			using var conn = new NpgsqlConnection(connectionString);
			//se obtiene el conteo de filas para el calculo del paginado
			var rowsCount = await conn.ExecuteScalarAsync<int>(@"select count(*) from document
													  where (id = @Id or serial_code = @Serial_code or publication_code = @Publication_code 
													  or lower(author_full_name) like lower(CONCAT('%', @AuthorOrEmail, '%')) or lower(author_email) like lower(CONCAT('%', @AuthorOrEmail, '%'))) and active = true", filtersCustom);

			var rowsSelected = await conn.QueryAsync<Document>(@"select * from document
																 where (id = @Id or serial_code = @Serial_code or publication_code = @Publication_code 
																 or lower(author_full_name) like lower(CONCAT('%', @AuthorOrEmail, '%')) or lower(author_email) like lower(CONCAT('%', @AuthorOrEmail, '%'))) and active = true
																 order by id offset @offset rows fetch next @rows rows only", filtersCustom);
			//se calculan y se declara una variable con la clase para retornar
			var documentPaginated = new DocumentPaginated
			{
				Page = filterDocuments.Page,
				PageSize = rows,
				PageCount = (int)Math.Ceiling(rowsCount / (double)rows),
				DocumentsCount = rowsCount,
				Documents = rowsSelected
			};
			return documentPaginated;
		}

		public async Task<Document> CreateDocument(CreateDocument createDocument) // metodo modificado con la ayuda de ChatGPT (https://chatgpt.com/share/68539212-62e4-800a-87ea-491f45d8ec7f)
		{
			var documentParam = new//objeto para ser generado el documento
			{
				createDocument.document.Name,
				createDocument.document.Description,
				createDocument.document.Author_full_name,
				createDocument.document.Author_email,
				createDocument.document.Serial_code,
				createDocument.document.Publication_code,
				Created_at = DateTime.Now,
				createDocument.document.active,

			};

			using var conn = new NpgsqlConnection(connectionString);
			await conn.OpenAsync();//se abre la conexion para solicitud con posible rollBack, apra evitar errores en bd

			using var transaction = await conn.BeginTransactionAsync();//transaction guarda la transaccion para confirmarla o volverla atras en bd segun resultado

			try
			{
				var savedDocument = await conn.QueryFirstOrDefaultAsync<Document>(@"insert into document (name,description,author_full_name,author_email,serial_code,publication_code,created_at,active)
																		values(@Name,@Description,@Author_full_name,@Author_email,@Serial_code,@Publication_code,@Created_at,@active) RETURNING*", documentParam, transaction);
				if (savedDocument == null) //si es null retorna para devolver error en controller
				{
					return savedDocument;
				}

				if (createDocument.Pages.Count() > 0) //si es mayo a 0 los indices ingresa para asignarles un id y posteriormente ser guardados
				{
					foreach (var page in createDocument.Pages)
					{
						page.Document_id = savedDocument.Id;
						page.Created_at = DateTime.Now;
					}                                                                  
					var savedPages = await conn.ExecuteAsync(@"insert into document_page_index (document_id,name,page,created_at) values(@Document_id,@Name,@Page,@Created_at)", createDocument.Pages, transaction);
				}
				await transaction.CommitAsync();//confirma los cambios en bd
				return savedDocument;
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();//vuelve atras todas las transacciones en espera
				return null;
			}
		}
		public async Task<Document> UpdateDocument(UpdateDocument updateDocument)
		{
			using var conn = new NpgsqlConnection(connectionString);
			await conn.OpenAsync();//se abre la conexion para solicitud con posible rollBack, apra evitar errores en bd
			using var transaction = await conn.BeginTransactionAsync();//transaction guarda la transaccion para confirmarla o volverla atras en bd segun resultado

			try
			{
				//busca el registro y lo obtiene para modificarlo si no esta eliminado
				var resultDocument = await conn.QueryFirstOrDefaultAsync<Document>("select * from document where id = @id and active = true", new { updateDocument.document.Id }, transaction);
				if (resultDocument == null)
				{
					return resultDocument;
				}
				resultDocument.Name = updateDocument.document.Name;
				resultDocument.Description = updateDocument.document.Description;
				resultDocument.Author_full_name = updateDocument.document.Author_full_name;
				resultDocument.Author_email = updateDocument.document.Author_email;
				resultDocument.Serial_code = updateDocument.document.Serial_code;
				resultDocument.Publication_code = updateDocument.document.Publication_code;
				resultDocument.Updated_at = DateTime.Now;


				var updatedDocument = await conn.QueryFirstOrDefaultAsync<Document>(@"update document 
																				  set name = @Name, description = @Description, author_full_name = @Author_full_name, 
																				  author_email = @Author_email, serial_code = @Serial_code, publication_code = @Publication_code,
																				  updated_at = @Updated_at where id = @Id RETURNING*", resultDocument, transaction);
				//si es null retorna para devolver error en controller
				if (updatedDocument == null)
				{
					return updatedDocument;
				}
				/*
				 * se borran todos los indices asociados a un documento
				 * dado que se valida que se actualizara el docuemnto, para llegar a este paso se debe haber modificado exitosamente
				 */
				await conn.ExecuteAsync(@"delete from document_page_index where document_id = @Id", resultDocument, transaction);
				/*
				 * si es mayor a 0 los indices ingresa para asignarles un id y posteriormente ser guardados
				 * ademas se agrega fecha de creacion
				 */
				if (updateDocument.Pages.Count() > 0)
				{
					foreach (var page in updateDocument.Pages)
					{
						page.Document_id = updatedDocument.Id;
						page.Created_at = DateTime.Now;
					}
					var savedPages = await conn.ExecuteAsync(@"insert into document_page_index (document_id,name,page,created_at) values(@Document_id,@Name,@Page,@Created_at)", updateDocument.Pages, transaction);
				}
				await transaction.CommitAsync();//confirma los cambios en bd
				return updatedDocument;
			}
			catch(Exception ex) 
			{
				await transaction.RollbackAsync();//vuelve atras todas las transacciones en espera
				return null;
			}
		}

		public async Task<Document> DeleteDocument(long id)
		{
			using var conn = new NpgsqlConnection(connectionString);

			DateTime dateTime = DateTime.Now;

			//se desactiva el documento
			var deletedDocument = await conn.QueryFirstOrDefaultAsync<Document>(@"update document set deleted_at = @dateTime, active = false where id = @id RETURNING*", new { id, dateTime });
			if (deletedDocument == null)
			{
				return deletedDocument;
			}
			//se eliminan los índices
			await conn.ExecuteAsync(@"delete from document_page_index where document_id = @Id", new { id });

			return deletedDocument;
		}


	}


}
