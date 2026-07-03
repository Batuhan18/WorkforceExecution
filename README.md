# Workforce Execution Platform

ICN Case Study cozumu — gunluk saha operasyonlarinin (plan, crew, gerceklesen, onay, rapor) tek platformdan yonetimi.

## Teknolojiler
- .NET 8 Web API — Onion Architecture (Domain / Application / Persistence / WebApi)
- CQRS + MediatR, Repository Pattern, EF Core 8 (MSSQL LocalDB)
- JWT Authentication + rol bazli yetkilendirme (TechOffice, HeadOfMaster, SiteChief, ProjectManager)
- SignalR — anlik bildirim ve canli Daily Report
- Frontend: ASP.NET Core MVC Razor Views + Bootstrap 5 + vanilla JS (sayfalar HomeController uzerinden, css/js wwwroot icinden)

## Calistirma
Gereksinim: .NET 8 SDK + SQL Server LocalDB (Visual Studio ile gelir).

    cd WorkforceExecution
    dotnet run --project Presentation/WorkforceExecution.WebApi

Tarayici: http://localhost:5000  (Swagger: http://localhost:5000/swagger)

Ilk calistirmada veritabani otomatik olusturulur ve WBS_V3.xlsx'ten cikarilan verilerle
(482 satirlik ToW/SToW/SSToW katalogu, lokasyonlar, worker tipleri, ornek gunluk plan) seed'lenir.

LocalDB yerine baska SQL Server kullanacaksaniz `appsettings.json > ConnectionStrings:DefaultConnection` degistirin.

## Demo Kullanicilar (sifre: Icn123!)
| Rol | E-posta |
|---|---|
| Teknik Ofis (A/B bolge) | abolgetechoffice@icn.com, bbolgetechoffice@icn.com |
| Head of Master (lokasyon basina) | 30aaa-hom@icn.com ... 30fff-hom@icn.com, 99uuu-hom@icn.com |
| Site Chief (A/B bolge) | abolge_sc@icn.com, bbolge_sc@icn.com |
| Project Manager | pm@icn.com |

## Sayfalar (MVC Razor Views)
| Rota | View | Kim |
|---|---|---|
| / | Home/Index | Login |
| /plan | Home/Plan | Teknik Ofis |
| /tasks | Home/Tasks | Head of Master |
| /approvals | Home/Approvals | HoM / Site Chief / PM |
| /report | Home/Report | Tum roller |

## Is Akisi
1. **T-1** — Teknik Ofis `/plan` uzerinden Daily Plan olusturur (KKK -> Project -> ToW -> SToW -> SSToW, Qty, ManDay). Satirlar lokasyonun HoM'una atanir, HoM'a SignalR bildirimi gider.
2. **T0** — HoM `/tasks`'de atanan isleri gorur, crew olusturur (worker type + kisi) ve isi alir.
3. **Gun sonu** — HoM Fact Qty / ManDay / Overtime girer; is baslamadiysa comment zorunlu. Kayit onaya duser.
4. **Onay** — HoM -> Site Chief -> PM (`/approvals`). Her adimda ilgili role canli bildirim.
5. **Rapor** — PM onayi ile kayit `/report`'deki Daily Report'ta konsolide olur; KPI kartlari (Plan/Fact Qty, ManDay, Overtime, Verimlilik) SignalR ile canli guncellenir. Bu tablo Power BI'in dogrudan okuyabilecegi yapidadir.

## Mimari Notlar
- **Onion**: bagimliliklar iceriye dogru — WebApi -> Persistence -> Application -> Domain. Domain hicbir seye bagimli degil.
- **CQRS**: her use-case Application/Features altinda ayri Command/Query + Handler. Controller'lar sadece MediatR'a delege eder (thin controller).
- **Guvenlik**: JWT claim'leri (userId, role, crewRegionId, locationId) ile hem endpoint bazli `[Authorize(Roles=...)]` hem handler icinde veri bazli yetki kontrolu (orn. SC sadece kendi bolgesinin kaydini onaylayabilir). SignalR hub'i da JWT ile korunur (query string access_token).
- **MVP tercihi**: sema `EnsureCreated` ile kurulur; uretimde migration'a gecis icin `Infrastructure/.../Migrations/README.md`'ye bakin.
