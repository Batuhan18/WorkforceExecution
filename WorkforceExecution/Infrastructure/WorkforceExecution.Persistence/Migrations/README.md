MVP asamasinda sema `Database.EnsureCreated()` ile olusturulur (Program.cs).
Uretim ortamina gecerken EnsureCreated cagrisini kaldirip migration'a gecin:

    dotnet ef migrations add InitialCreate -p Infrastructure/WorkforceExecution.Persistence -s Presentation/WorkforceExecution.WebApi -o Migrations
    dotnet ef database update -p Infrastructure/WorkforceExecution.Persistence -s Presentation/WorkforceExecution.WebApi
