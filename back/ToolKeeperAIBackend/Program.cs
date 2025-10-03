using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Service.Abstraction;
using Service.Db;
using Service.Implementation;
using Service.Settings;
using System.Text.Json;
using System.Threading.Tasks;
using ToolKeeperAIBackend.Automapper;
using ToolKeeperAIBackend.Middlewares;

namespace ToolKeeperAIBackend
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<AppSettings>(builder.Configuration.GetRequiredSection(nameof(AppSettings)));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()   
                        .AllowAnyMethod()   
                        .AllowAnyHeader();  
                });
            });

            builder.Services.AddDbContextFactory<ToolKeeperDbContext>((IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                optionsBuilder.UseNpgsql(connectionString);
            });

            builder.Services.AddHttpClient<HttpClient>(nameof(HttpClient), (serviceProvider, httpClient) =>
             {
                 var settings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value.ModelAPISettings;

                 httpClient.BaseAddress = new Uri(string.Join(':', settings.Host, settings.Port));
             });

            builder.Services.AddAutoMapper(typeof(AppMappingProfile));

            builder.Services.AddTransient<IToolKitService, ToolKitService>();
            builder.Services.AddTransient<IToolService, ToolService>();
            builder.Services.AddTransient<IEmployeeService, EmployeeService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("Logs/myapp-{Date}.log", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Warning)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var settings = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;

                Console.WriteLine(JsonSerializer.Serialize(settings));
                
                try
                {
                    IHttpClientFactory httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
                    using HttpClient httpClient = httpClientFactory.CreateClient(nameof(HttpClient));

                    using var response = await httpClient.GetAsync("health");
                    var str = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(str);

                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                var db = scope.ServiceProvider.GetRequiredService<ToolKeeperDbContext>();
                db.Database.Migrate();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseCors("AllowAll");
            app.MapControllers();

            app.Run();
		}
	}
}
