using Microsoft.AspNetCore.Mvc;

namespace WorkforceExecution.WebApi.Controllers;

// Sayfalar artik wwwroot'taki statik html degil, Razor View olarak MVC uzerinden donuyor.
// Yetkilendirme JWT ile client tarafinda yapildigi icin action'lar sadece view'i teslim eder;
// veri erisimi yine [Authorize] korumali /api/* endpointleri uzerinden gerceklesir.
public class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();          // Login

    [HttpGet("/plan")]
    public IActionResult Plan() => View();           // Teknik Ofis - Gunluk Plan

    [HttpGet("/tasks")]
    public IActionResult Tasks() => View();          // Head of Master - Gorevler

    [HttpGet("/approvals")]
    public IActionResult Approvals() => View();      // Onay kuyrugu (HoM/SC/PM)

    [HttpGet("/report")]
    public IActionResult Report() => View();         // Daily Report + KPI
}
