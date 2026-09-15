using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Application.Services;
using WhatsAppBulkMessaging.Infrastructure.Configuration;
using WhatsAppBulkMessaging.Infrastructure.Data;
using WhatsAppBulkMessaging.Infrastructure.Repositories;
using WhatsAppBulkMessaging.Infrastructure.Services;

namespace WhatsAppBulkMessaging.API.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var whatsappSettings = configuration.GetSection("WhatsAppSettings")
                .Get<WhatsAppSettings>() ?? new WhatsAppSettings();

        services.AddSingleton(whatsappSettings);

        services.AddHttpClient<IWhatsAppService, WhatsAppService>();

        services.AddScoped<IExcelService, ExcelService>();

        services.AddScoped<IWhatsAppTemplateRepository, WhatsAppTemplateRepository>();

        services.AddScoped<IWhatsAppTemplateService, WhatsAppTemplateService>();
        
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IWhatsAppMessageRepository, WhatsAppMessageRepository>();

        return services;
    }
}