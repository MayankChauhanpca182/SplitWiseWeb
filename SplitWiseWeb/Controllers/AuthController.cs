using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Controllers;

[Authorize]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    public IActionResult Index()
    {
        return View();
    }

    #region Login
    // GET Login
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    // POST Login
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Login(LoginVM loginVM)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        // Redirect to dashboard
        return View(loginVM);
    }
    #endregion

    #region Register
    // GET Register
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    // GET Register
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterUserVM registerUserVM)
    {
        if (!ModelState.IsValid)
        {
            return View(registerUserVM);
        }

        // Add User
        await _authService.RegisterUser(registerUserVM);

        return RedirectToAction("Login");
    }
    #endregion

    #region User Verification
    // GET UserVerification
    [AllowAnonymous]
    public IActionResult UserVerification(string token)
    {
        return RedirectToAction("Login");
    }
    #endregion

    #region Forgot Password
    // GET ForgotPassword
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }
    #endregion

    #region Logout
    // GET Logout
    public IActionResult Logout()
    {
        return RedirectToAction("Login");
    }
    #endregion

    #region Error
    [AllowAnonymous]
    [Route("/Auth/Error/{code}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int code)
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    #endregion
}
