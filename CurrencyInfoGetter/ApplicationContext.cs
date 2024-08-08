using CurrencyInfoGetter.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyInfoGetter
{
	public class ApplicationContext: DbContext
	{
		public DbSet<Rate> Currencies => Set<Rate>();

		private readonly string _connectionString;
		public ApplicationContext(IConfiguration config)
		{
			_connectionString = $"Server={config["DbServerAddress"]};User Id={config["DbUserId"]};" +
				$"Password={config["DbPassword"]};Port={config["DbPort"]};Database={config["DbName"]};";
		}
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql(_connectionString);
		}
	}
}
