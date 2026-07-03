using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Persistence.Context;

// WBS_V3.xlsx'ten cikarilan referans veriler + demo kullanicilar + ornek gunluk plan.
public static class DbInitializer
{
    public const string DefaultPassword = "Icn123!";

    public static async Task SeedAsync(AppDbContext context, IPasswordHasher hasher)
    {
        await context.Database.EnsureCreatedAsync();
        if (await context.Users.AnyAsync()) return; // zaten seed'li

        var basePath = Path.Combine(AppContext.BaseDirectory, "SeedData");

        // 1) Bolgeler ve lokasyonlar (WBS 'Lokasyon' sayfasi + tech office e-postalarindan bolge eslesmesi)
        var regionA = new CrewRegion { Name = "abolge" };
        var regionB = new CrewRegion { Name = "bbolge" };
        context.CrewRegions.AddRange(regionA, regionB);

        var locationRegions = new Dictionary<string, CrewRegion>
        {
            ["30AAA"] = regionA, ["30BBB"] = regionA, ["30CCC"] = regionA,
            ["30DDD"] = regionB, ["30EEE"] = regionB, ["30FFF"] = regionB, ["99UUU"] = regionB
        };
        var locations = locationRegions.ToDictionary(
            kv => kv.Key,
            kv => new Location { Code = kv.Key, CrewRegion = kv.Value });
        context.Locations.AddRange(locations.Values);

        // 2) Projeler (WBS ornek verisindeki proje kodlari)
        var projectCodes = new Dictionary<string, string>
        {
            ["30AAA"] = "AAA.0199.30AAA.0.KK.TZ0003",
            ["30BBB"] = "AAA.0199.30BBB.0.KK.TZ0004",
            ["30CCC"] = "AAA.0199.30CCC.0.KK.TZ0005",
            ["30DDD"] = "AAA.0199.30DDD.0.KK.TZ0006",
            ["30EEE"] = "AAA.0199.30EEE.0.KK.TZ0023",
            ["30FFF"] = "AAA.0199.30FFF.0.KK.TZ0024",
            ["99UUU"] = "0001_Unproductive Works"
        };
        var projects = projectCodes.ToDictionary(
            kv => kv.Value,
            kv => new Project { Code = kv.Value, Location = locations[kv.Key] });
        context.Projects.AddRange(projects.Values);

        // 3) Worker tipleri (WBS 'worker' sayfasi)
        var workerNames = new[] { "Rebar Fixer", "General Labor", "Operators", "Survey",
                                  "Slinger", "Formworker", "Architectural Worker", "Assembler", "Welder" };
        var workerTypes = workerNames.ToDictionary(n => n, n => new WorkerType { Name = n });
        context.WorkerTypes.AddRange(workerTypes.Values);

        // 4) Is katalogu: ToW -> SToW -> SSToW (WBS 'Sub_Type' sayfasi, 482 satir)
        var tows = new Dictionary<string, TypeOfWork>();
        var stows = new Dictionary<string, SubTypeOfWork>();
        var sstows = new Dictionary<string, SubSubTypeOfWork>();
        foreach (var line in File.ReadLines(Path.Combine(basePath, "work_catalog.csv")).Skip(1))
        {
            var c = line.Split(',');
            if (!tows.TryGetValue(c[0], out var tow))
            {
                tow = new TypeOfWork { Code = c[0] };
                tows[c[0]] = tow;
                context.TypeOfWorks.Add(tow);
            }
            var stowKey = c[0] + "|" + c[1];
            if (!stows.TryGetValue(stowKey, out var stow))
            {
                stow = new SubTypeOfWork { Code = c[1], TypeOfWork = tow };
                stows[stowKey] = stow;
                context.SubTypeOfWorks.Add(stow);
            }
            var sstow = new SubSubTypeOfWork
            {
                Code = c[2], Unit = c[3], TypeOfCode = c[4],
                Zzz = string.IsNullOrWhiteSpace(c[5]) ? null : c[5],
                SubTypeOfWork = stow
            };
            sstows[stowKey + "|" + c[2]] = sstow;
            context.SubSubTypeOfWorks.Add(sstow);
        }

        // 5) Kullanicilar (Authentication kurali: her KKK'ya bir HoM, her bolgeye bir tech office + site chief, projeye bir PM)
        var hash = hasher.Hash(DefaultPassword);
        var users = new Dictionary<string, AppUser>();
        void AddUser(string email, string fullName, UserRole role, Location? loc = null, CrewRegion? region = null)
        {
            var u = new AppUser { Email = email, FullName = fullName, PasswordHash = hash, Role = role, Location = loc, CrewRegion = region };
            users[email] = u;
            context.Users.Add(u);
        }

        AddUser("pm@icn.com", "Project Manager", UserRole.ProjectManager);
        AddUser("abolge_sc@icn.com", "A Bolge Site Chief", UserRole.SiteChief, region: regionA);
        AddUser("bbolge_sc@icn.com", "B Bolge Site Chief", UserRole.SiteChief, region: regionB);
        AddUser("abolgetechoffice@icn.com", "A Bolge Teknik Ofis", UserRole.TechOffice, region: regionA);
        AddUser("bbolgetechoffice@icn.com", "B Bolge Teknik Ofis", UserRole.TechOffice, region: regionB);
        foreach (var (code, loc) in locations)
            AddUser($"{code.ToLower()}-hom@icn.com", $"{code} Head of Master", UserRole.HeadOfMaster,
                    loc: loc, region: loc.CrewRegion);

        // 6) Ornek gunluk plan (WBS 'Daily_Report' sayfasi) - demo icin karisik durumlarla
        var today = DateTime.UtcNow.Date;
        var statusCycle = new[]
        {
            WorkItemStatus.Approved, WorkItemStatus.Approved, WorkItemStatus.PendingHomApproval,
            WorkItemStatus.PendingSiteChief, WorkItemStatus.PendingPm
        };
        int factIndex = 0;

        foreach (var line in File.ReadLines(Path.Combine(basePath, "sample_plans.csv")).Skip(1))
        {
            var c = line.Split(',');
            var locCode = c[0];
            if (!locations.TryGetValue(locCode, out var loc)) continue;
            if (!projects.TryGetValue(c[1], out var project)) continue;
            if (!sstows.TryGetValue(c[2] + "|" + c[3] + "|" + c[4], out var sstow)) continue;

            var techOffice = users[c[7]];
            var hom = users[$"{locCode.ToLower()}-hom@icn.com"];
            var hasFact = decimal.TryParse(c[9], NumberStyles.Any, CultureInfo.InvariantCulture, out var factQty);

            var item = new WorkItem
            {
                ReportDate = today,
                Location = loc,
                Project = project,
                SubSubTypeOfWork = sstow,
                PlannedQuantity = decimal.Parse(c[5], CultureInfo.InvariantCulture),
                PlannedManday = decimal.Parse(c[6], CultureInfo.InvariantCulture),
                SubmittedBy = techOffice,
                AssignedHom = hom,
                Status = WorkItemStatus.Planned
            };

            if (hasFact)
            {
                var status = statusCycle[factIndex % statusCycle.Length];
                factIndex++;

                item.FactQuantity = factQty;
                item.FactManday = decimal.Parse(c[10], CultureInfo.InvariantCulture);
                item.Overtime = 0;
                item.Status = status;
                if (factQty <= 0) item.Comment = "Is planlanan gunde baslamadi.";
                if (!string.IsNullOrWhiteSpace(c[12])) item.ZzzDetail = c[12];

                if (!string.IsNullOrWhiteSpace(c[11]) && workerTypes.TryGetValue(c[11], out var wt))
                    item.CrewAssignments.Add(new CrewAssignment { WorkerType = wt, WorkerCount = 4 });

                // Duruma uygun onay loglari
                if (status is WorkItemStatus.PendingSiteChief or WorkItemStatus.PendingPm or WorkItemStatus.Approved)
                    item.ApprovalLogs.Add(new ApprovalLog { Approver = hom, ApproverRole = UserRole.HeadOfMaster, IsApproved = true });
                if (status is WorkItemStatus.PendingPm or WorkItemStatus.Approved)
                {
                    var sc = users[loc.CrewRegion == regionA ? "abolge_sc@icn.com" : "bbolge_sc@icn.com"];
                    item.ApprovalLogs.Add(new ApprovalLog { Approver = sc, ApproverRole = UserRole.SiteChief, IsApproved = true });
                }
                if (status == WorkItemStatus.Approved)
                    item.ApprovalLogs.Add(new ApprovalLog { Approver = users["pm@icn.com"], ApproverRole = UserRole.ProjectManager, IsApproved = true });
            }

            context.WorkItems.Add(item);
        }

        await context.SaveChangesAsync();
    }
}
