

using Microsoft.EntityFrameworkCore;
using WhatsAppBulkMessaging.Application.DTOs;
using WhatsAppBulkMessaging.Application.Interfaces;
using WhatsAppBulkMessaging.Infrastructure.Data;
using WhatsAppBulkMessaging.Infrastructure.Repositories;
using WhatsAppBulkMessaging.Infrastructure.Services;

namespace WhatsAppBulkMessaging.API.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        services.Configure<WhatsAppOptions>(configuration.GetSection("WhatsApp"));

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection")));

        services.AddDbContextFactory<AppDbContext>(
            options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Scoped);

        services.AddScoped<IWhatsAppTemplateRepository, WhatsAppTemplateRepository>();
        services.AddScoped<IWhatsAppTemplateService, WhatsAppTemplateService>();
        services.AddHttpClient<IWhatsAppMetaService, WhatsAppMetaService>();
        services.AddHttpClient<IWhatsAppService, WhatsAppMetaService>();
        services.AddScoped<IWhatsAppMessageRepository, WhatsAppMessageRepository>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IPhoneNumberService, PhoneNumberService>();
        services.AddScoped<IRecipientValidationService,RecipientValidationService>();

        services.AddScoped<IWhatsAppMessageHistoryService,WhatsAppMessageHistoryService>();

        return services;
    }
}