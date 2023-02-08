using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenMicroFiscal.Models;

namespace OpenMicroFiscal;

public static class ConfigureServices
{
    private const string HttpClientName = "FiscalizationService";

    public static void AddFiscalization(
        this IServiceCollection services,
        Func<IServiceProvider, ProvideCertificate> provideCertificateFunc)
    {
        const string configSectionName = "FiscalizationSettings";

        services.AddSingleton(serviceProvider =>
            serviceProvider.GetRequiredService<IConfiguration>()
                .GetRequiredSection(configSectionName)
                .Get<FiscalizationSettings>()!);

        services.AddSingleton(provideCertificateFunc);

        services.AddHttpClient(HttpClientName, (serviceProvider, httpClient) =>
        {
            var settings = serviceProvider.GetRequiredService<FiscalizationSettings>();
            httpClient.BaseAddress = UriProvider.GetInvoiceFiscalizationUri(settings.Environment);
        });

        services.AddTransient<InvoiceService>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(HttpClientName);
            var settings = serviceProvider.GetRequiredService<FiscalizationSettings>();
            var provideCertificate = serviceProvider.GetRequiredService<ProvideCertificate>();
            return new InvoiceService(httpClient, settings, provideCertificate);
        });
    }

    public static void AddFiscalization(
        this IServiceCollection services,
        FiscalizationSettings settings,
        ProvideCertificate provideCertificate)
    {
        services.AddHttpClient(HttpClientName,
            httpClient => { httpClient.BaseAddress = UriProvider.GetInvoiceFiscalizationUri(settings.Environment); });

        services.AddTransient<InvoiceService>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(HttpClientName);
            return new InvoiceService(httpClient, settings, provideCertificate);
        });
    }
}