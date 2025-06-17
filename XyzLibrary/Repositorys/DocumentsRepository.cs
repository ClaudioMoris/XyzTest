using Dapper;
using Npgsql;
using XyzModels;
using XyzModels.DbModels;
using XyzModels.DbModels.Views;

namespace XyzLibrary.Repositorys
{
	public interface IDocumentsRepository
	{
		Task<DocumentViewModel> SearchById(long id);
		Task<DocumentPaginated> SearchDocumentByFilters(FilterDocuments filterDocuments, int rows);
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

			var rowsCount = await conn.ExecuteScalarAsync<int>(@"select count(*) from document
													  where id = @Id or serial_code = @Serial_code or publication_code = @Publication_code 
													  or lower(author_full_name) like lower(CONCAT('%', @AuthorOrEmail, '%')) or lower(author_email) like lower(CONCAT('%', @AuthorOrEmail, '%'))", filtersCustom);

			var rowsSelected = await conn.QueryAsync<Document>(@"select * from document
																 where id = @Id or serial_code = @Serial_code or publication_code = @Publication_code 
																 or lower(author_full_name) like lower(CONCAT('%', @AuthorOrEmail, '%')) or lower(author_email) like lower(CONCAT('%', @AuthorOrEmail, '%')) 
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



	}


}
