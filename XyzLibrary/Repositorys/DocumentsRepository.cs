using XyzModels;

namespace XyzLibrary.Repositorys
{
	public interface IDocumentRepository
	{
	}
	public class DocumentsRepository : IDocumentRepository
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





	}


}
