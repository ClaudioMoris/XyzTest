using Dapper;
using Npgsql;
using XyzModels;
using XyzModels.DbModels;
using XyzModels.DbModels.Views;

namespace XyzLibrary.Repositorys
{
	public interface IDocumentsRepository
	{
		Task<Document> CreateDocument(CreateDocument createDocument);
		Task<Document> DeleteDocument(long id);
		Task<DocumentViewModel> SearchById(long id);
		Task<DocumentPaginated> SearchDocumentByFilters(FilterDocuments filterDocuments, int rows);
		Task<Document> UpdateDocument(UpdateDocument updateDocument);
	}
	public class DocumentsRepository : IDocumentsRepository
	{
		private readonly string connectionString;
		public DocumentsRepository(EnvironmentVariable environmentVariable)
		{
			connectionString = $"Host={environmentVariable.hostDb};Database={environmentVariable.databaseNameDb};Username={environmentVariable.usernameDb};Password={environmentVariable.passwordDb}";
			if (string.IsNullOrEmpty(connectionString))
			{
				throw new InvalidOperationException("Las variables de entorno necesarias no están configuradas.");
			}

		}
		/*
		 * busca un documento y lo retorna juntos con sus indices
		 */
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


		public async Task<Document> CreateDocument(CreateDocument createDocument)
		{
			using var conn = new NpgsqlConnection(connectionString);

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

			var savedDocument = await conn.QueryFirstOrDefaultAsync<Document>(@"insert into document (name,description,author_full_name,author_email,serial_code,publication_code,created_at,active)
																		values(@Name,@Description,@Author_full_name,@Author_email,@Serial_code,@Publication_code,@Created_at,@active) RETURNING*", documentParam);
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
				var savedPages = await conn.ExecuteAsync(@"insert into document_page_index (document_id,name,page,created_at) values(@Document_id,@Name,@Page,@Created_at)", createDocument.Pages);
			}
			return savedDocument;
		}

		public async Task<Document> UpdateDocument(UpdateDocument updateDocument)
		{
			using var conn = new NpgsqlConnection(connectionString);
			//busca el registro y lo obtiene para modificarlo si no esta eliminado
			var resultDocument = await conn.QueryFirstOrDefaultAsync<Document>("select * from document where id = @id and active = true", new { updateDocument.document.Id });
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
																				  updated_at = @Updated_at where id = @Id RETURNING*", resultDocument);
			//si es null retorna para devolver error en controller
			if (updatedDocument == null)
			{
				return updatedDocument;
			}
			/*
			 * se borran todos los indices asociados a un documento
			 * dado que se valida que se actualizara el docuemnto, para llegar a este paso se debe haber modificado exitosamente
			 */
			await conn.ExecuteAsync(@"delete from document_page_index where document_id = @Id", resultDocument);
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
				var savedPages = await conn.ExecuteAsync(@"insert into document_page_index (document_id,name,page,created_at) values(@Document_id,@Name,@Page,@Created_at)", updateDocument.Pages);
			}
			return updatedDocument;
		}

		public async Task<Document> DeleteDocument(long id)
		{
			using var conn = new NpgsqlConnection(connectionString);
			//se desactiva el documento
			var deletedDocument = await conn.QueryFirstOrDefaultAsync<Document>(@"update document set active = false where id = @id RETURNING*", new { id });
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
