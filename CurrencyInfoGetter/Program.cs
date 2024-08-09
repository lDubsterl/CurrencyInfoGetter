using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

namespace CurrencyInfoGetter
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var config = builder.Configuration;
			// Add services to the container.
			builder.Services.AddDbContext<ApplicationContext>();
			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				var basePath = AppContext.BaseDirectory;
				var xmlPath = Path.Combine(basePath, "CurrencyInfoGetter.xml");

				options.IncludeXmlComments(xmlPath);

				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "Currency rate getter API",
					Description = "API для получения информации о курсе валют от нацбанка РБ." +
					" Для начала работы нужно указать данные для подключения к бд в appsettings.json"
				});
			});
			var app = builder.Build();

			using (var scope = app.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
				db.Database.Migrate();
			}

			// Configure the HTTP request pipeline.
			//if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}
