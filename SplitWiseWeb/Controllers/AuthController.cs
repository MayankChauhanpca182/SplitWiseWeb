using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SplitWiseRepository.ViewModels;

namespace SplitWiseWeb.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    // GET Login
    public IActionResult Login()
    {
        return View();
    }

    // POST Login
    [HttpPost]
    public IActionResult Login(LoginVM loginVM)
    {
        if (!ModelState.IsValid)
        {
            return View(loginVM);
        }

        // Redirect to dashboard
        return View(loginVM);
    }

    // GET Register
    public IActionResult Register()
    {
        string path1 = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
        return View(new RegisterVM());
    }

    // GET Register
    [HttpPost]
    public IActionResult Register(RegisterVM registerVM)
    {
        if (!ModelState.IsValid)
        {
            return View(registerVM);
        }

        // Add User

        return RedirectToAction("Login");
    }

    // GET ForgotPassword
    public IActionResult ForgotPassword()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
