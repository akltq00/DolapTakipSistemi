## DolapTakipSistemi

Clean Architecture ile hazirlanan ASP.NET Core Web API, Swagger ve MSSQL kullanan ogrenci dolap zimmet uygulamasi.

> Not: Bu bilgisayarda .NET 10 SDK kurulu olmadigi icin proje `net7.0` hedefiyle olusturuldu. .NET 10 SDK kuruldugunda `src/*/*.csproj` icindeki `TargetFramework` degeri `net10.0` olarak guncellenebilir.

### Proje yapisi

```text
src/
  DolapTakipSistemi.Api/             # Swagger, controller ve web ekran
  DolapTakipSistemi.Application/     # DTO, servis ve repository sozlesmeleri
  DolapTakipSistemi.Domain/          # Entity ve domain kurallari
  DolapTakipSistemi.Infrastructure/  # EF Core, MSSQL ve repository implementasyonu
```

### Ozellikler

- Ana ekranda dolap kutucuklari
- Bos dolaba tiklayip ad, soyad, okul numarasi ve sifre ile zimmete alma
- Zimmetli dolaba tiklayip ayni sifre ile zimmeti kaldirma
- Swagger yaninda nasil kullanilir popup'i
- Admin sifresi ile korunan Database Editor
- Dolaplar icin CRUD API
- Swagger UI: `/swagger`
- MSSQL veritabani
- EF Core migration ve ilk calistirmada otomatik dolap olusturma

### Veritabani

Varsayilan connection string:

```json
"DefaultConnection": "Server=localhost,1433;Database=DolapTakipSistemi;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"
```

Kendi MSSQL bilginize gore `src/DolapTakipSistemi.Api/appsettings.json` dosyasindan guncelleyin.

Database Editor admin sifresi:

```json
"AdminSettings": {
  "Password": "admin1234"
}
```

Gelistirme disinda bu sifreyi mutlaka degistirin.

### Calistirma

```bash
dotnet restore DolapTakipSistemi.sln
dotnet run --project src/DolapTakipSistemi.Api/DolapTakipSistemi.Api.csproj
```

Uygulama acildiktan sonra:

- Web ekran: `https://localhost:PORT/`
- Swagger: `https://localhost:PORT/swagger`
- Database Editor: `https://localhost:PORT/database-editor.html`

> Daha once `EnsureCreated` ile olusmus eski bir veritabani varsa EF migration gecmisi olmayabilir. Gelistirme ortaminda en temiz yol eski `DolapTakipSistemi` veritabanini silip uygulamayi yeniden calistirmaktir.
