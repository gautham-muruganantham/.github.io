using System.Security.Claims;
using Learning.Models;
using Learning.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Learning.Controllers;

public class AccountController(XmlDatabaseService db) : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Matrimony");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = db.ValidateCredentials(model.Email.Trim(), model.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await SignInAsync(user);
        return RedirectToAction("Index", "Matrimony");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Matrimony");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (db.EmailExists(model.Email.Trim()))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
            return View(model);
        }

        var user = db.CreateUser(model);
        await SignInAsync(user);
        return RedirectToAction("Index", "Matrimony");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MatrimonyCookie");
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new("gender", user.Gender),
            new("age", user.Age.ToString())
        };

        var identity = new ClaimsIdentity(claims, "MatrimonyCookie");
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync("MatrimonyCookie", principal);
    }
}
