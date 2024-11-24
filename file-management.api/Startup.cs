using file_management.api.Clients;
using file_management.api.Filters;
using file_management.api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace file_management.api;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();

        services.AddHealthChecks();

        services.AddControllers();

        services.AddControllers(options => { options.Filters.Add<HttpGlobalExceptionFilter>(); });
        
        services.AddMemoryCache();
        
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IGoogleDriveClient, GoogleDriveClient>();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "FileManagement API", Version = "v1" });
        });
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseHealthChecks("/healthcheck");

        app.UseRouting();
        
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileManagement API v1");
            c.RoutePrefix = string.Empty;
        });

        app.UseRouting();
        
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        
        app.UseStaticFiles();
    }
}