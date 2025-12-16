# <img align="center" src="https://raw.githubusercontent.com/dalrankov/OpenMicroFiscal/master/icon.png"/> OpenMicroFiscal

<a href="https://www.nuget.org/packages/OpenMicroFiscal"><img alt="NuGet Version" src="https://img.shields.io/nuget/v/OpenMicroFiscal"></a>
<a href="https://www.nuget.org/packages/OpenMicroFiscal"><img alt="NuGet Downloads" src="https://img.shields.io/nuget/dt/OpenMicroFiscal"></a>

Softver namijenjen za elektronsku fiskalizaciju računa pravnih lica u PDV sistemu unutar Crne Gore.
Implementacija je minimalistička i služi za izdavanje/storniranje računa ka pravnim licima (biznisima) koji plaćaju transakcionim
računom ili kreditnom/debitnom karticom firme.

Pored toga što služi svrsi koja je meni konkretno potrebna, može biti i kvalitetna osnova drugima koji žele da
implementiraju elektronsku fiskalizaciju u .NET-u.

## Kako se koristi biblioteka?

Testni sertifikat možete pronaći u folderu `test-certs` pod nazivom `test.pfx`. Lozinka je `123456`.

### Registracija servisa

Postoje tri načina da registrujete servis za fiskalizaciju.

#### 1. Dependency Injection kroz konfiguraciju

Popunite sljedeće varijable kroz vašu konfiguraciju (`IConfiguration` interfejs).

````env
# Test ili Production
FiscalizationSettings__Environment=Test

FiscalizationSettings__IssuerIdType=TIN
FiscalizationSettings__IssuerIdNumber=12345678
FiscalizationSettings__IssuerName="TEST DOO"
FiscalizationSettings__IssuerCountry=MNE
FiscalizationSettings__IssuerCity=PODGORICA
FiscalizationSettings__IssuerAddress="ULICA BB"
FiscalizationSettings__IssuerSoftwareCode=ss123ss123
FiscalizationSettings__IssuerEnuCode=si747we972
FiscalizationSettings__IssuerBusinessUnitCode=xx123xx123
FiscalizationSettings__IssuerOperatorCode=oo123oo123
````

Zatim pozovite ekstenziju `AddFiscalization` koja će registrovati neophodne servise za fiskalizaciju. Ovdje ste dužni da
predate implementaciju delegate-a koja kao rezultat vraća sertifikat. Kao parametar funkcije dostupna vam je
instanca `IServiceProvider` interfejsa (koja je u ovom slučaju odbačena nazivom `_`) koja vam može pomoći u
instanciranju sertifikata. Npr: Ukoliko sertifikat držite u konfiguraciji i želite odatle da ga ekstraktujete kako biste
ga iskoristili ovdje.

````csharp
services.AddFiscalization(_ => () => new X509Certificate2("test.pfx", "123456"));
````

Nakon ovoga je uspješno registrovan servis tipa `InvoiceService`, spreman za upotrebu.

#### 2. Dependency Injection sa direktnim predavanjem podešavanja

Ovdje direktno unosite podatke kroz instancu klase `FiscalizationSettings` i predajete implementaciju delegate-a za
instanciranje sertifikata.

````csharp
services.AddFiscalization(
    new FiscalizationSettings
    {
        Environment = FiscalizationEnvironment.Test,
        IssuerIdType = TaxIdType.Tin,
        IssuerIdNumber = "12345678",
        IssuerName = "TEST DOO",
        IssuerCountry = "MNE",
        IssuerCity = "PODGORICA",
        IssuerAddress = "ULICA BB",
        IssuerSoftwareCode = "ss123ss123",
        IssuerEnuCode = "si747we972",
        IssuerBusinessUnitCode = "xx123xx123",
        IssuerOperatorCode = "oo123oo123"
    }, 
    () => new X509Certificate2("test.pfx", "123456"));
````

Nakon ovoga je uspješno registrovan servis tipa `InvoiceService`, spreman za upotrebu.

#### 3. Ručno

Klasa `InvoiceService` je dizajnirana tako da se može instancirati po potrebi, bez upotrebe Dependency Injection-a.

> Ako koristite ovaj način instanciranja, vodite računa o instanci klase `HttpClient` koja mora biti dispose-ovana nakon
> upotrebe. Klasa `InvoiceService` to neće uraditi sama.

````csharp
using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("Base adresa do servisa za fiskalizaciju");

var invoiceService = new InvoiceService(
    httpClient,
    new FiscalizationSettings
    {
        Environment = FiscalizationEnvironment.Test,
        IssuerIdType = TaxIdType.Tin,
        IssuerIdNumber = "12345678",
        IssuerName = "TEST DOO",
        IssuerCountry = "MNE",
        IssuerCity = "PODGORICA",
        IssuerAddress = "ULICA BB",
        IssuerSoftwareCode = "ss123ss123",
        IssuerEnuCode = "si747we972",
        IssuerBusinessUnitCode = "xx123xx123",
        IssuerOperatorCode = "oo123oo123"
    },
    () => new X509Certificate2("test.pfx", "123456"));
````

----

### Izdavanje računa

````csharp
var request = new CreateInvoiceRequest
{
    Type = InvoiceType.Invoice,
    OrderNumber = 15,
    PaymentMethod = PaymentMethodType.BankTransfer,
    BankNumber = "123-456-789",
    PaymentDeadline = new DateOnly(2022, 02, 25),
    TaxDate = new DateOnly(2022, 02, 25),
    Buyer = new Buyer
    {
        IdType = TaxIdType.Tin,
        IdNumber = "12345678",
        Name = "TOP BUYER DOO",
        Address = "4 JULA BB",
        City = "PODGORICA",
        Country = "MNE"
    },
    Items = new[]
    {
        new InvoiceItem
        {
            Code = "#guma",
            Name = "Guma za automobil",
            Unit = "komad",
            UnitPrice = 75.00M,
            Quantity = 4
        },
        new InvoiceItem
        {
            Code = "#burger",
            Name = "Burger",
            Unit = "komad",
            UnitPrice = 5.00M,
            Quantity = 2,
            Vat = new InvoiceItem.VatSpec
            {
                Rate = 7.00M
            }
        }
    },
    Note = "Molimo vas da račun platite u navedenom roku. Hvala."
};
````

### Storniranje računa

````csharp
var request = new CreateInvoiceRequest
{
    Type = InvoiceType.Corrective,
    OrderNumber = 16,
    PaymentMethod = PaymentMethodType.BankTransfer,
    BankNumber = "123-456-789",
    PaymentDeadline = new DateOnly(2022, 02, 25),
    TaxDate = new DateOnly(2022, 02, 25),
    Buyer = new Buyer
    {
        IdType = TaxIdType.Tin,
        IdNumber = "12345678",
        Name = "TOP BUYER DOO",
        Address = "4 JULA BB",
        City = "PODGORICA",
        Country = "MNE"
    },
    Items = new[]
    {
        new InvoiceItem
        {
            Code = "#guma",
            Name = "Guma za automobil",
            Unit = "komad",
            UnitPrice = 75.00M,
            Quantity = -4
        },
        new InvoiceItem
        {
            Code = "#burger",
            Name = "Burger",
            Unit = "komad",
            UnitPrice = 5.00M,
            Quantity = -2,
            Vat = new InvoiceItem.VatSpec
            {
                Rate = 7.00M
            }
        }
    },
    Note = "Kupac je odustao od kupovine.",
    Original = new CreateInvoiceRequest.OriginalInvoice
    {
        Id = "0AE36859887129D6363C40F662FF9AE4", // IIC
        IssuedAt = DateTime.Parse("2024-03-09T17:43:33Z")
    }
};
````

### Izuzeće od plaćanja PDV-a stavke

````csharp
Vat = new InvoiceItem.VatSpec
{
    ExemptionReason = "VAT_CL17" // Razlog izuzeća
}
````

### Rezultat

````csharp
var result = await invoiceService.CreateInvoiceAsync(request);

Console.WriteLine($"XML: {result.EnvelopedRequestXmlText}");
Console.WriteLine($"Invoice Number: {result.InvoiceNumber}");
Console.WriteLine($"Total Price: {result.TotalPrice} EUR");
Console.WriteLine($"IIC: {result.Iic}");
Console.WriteLine($"FIC: {result.Fic}");
Console.WriteLine($"Date and time: {result.CreatedAt}");
Console.WriteLine($"Verification URL: {result.VerificationUrl}");
````
