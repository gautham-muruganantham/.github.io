using System.Security.Claims;
using Learning.Models;
using Learning.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learning.Controllers;

[Authorize]
public class MatrimonyController(XmlDatabaseService db) : Controller
{
    [HttpGet]
    public IActionResult Index([FromQuery] SearchFilterViewModel filter)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userGender = User.FindFirst("gender")?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var minAge = filter.MinAge < 18 ? 18 : filter.MinAge;
        var maxAge = filter.MaxAge > 80 ? 80 : filter.MaxAge;
        if (maxAge < minAge)
        {
            maxAge = minAge;
        }

        filter.MinAge = minAge;
        filter.MaxAge = maxAge;

        var profiles = db.SearchProfiles(userId, userGender, filter);
        var vm = new MatrimonyViewModel
        {
            Filter = filter,
            Profiles = profiles,
            InterestCount = db.CountRelations(userId, "interest"),
            ShortlistCount = db.CountRelations(userId, "shortlist")
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleInterest(string toUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(toUserId))
        {
            db.ToggleRelation(userId, toUserId, "interest");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleShortlist(string toUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId) && !string.IsNullOrWhiteSpace(toUserId))
        {
            db.ToggleRelation(userId, toUserId, "shortlist");
        }

        return RedirectToAction(nameof(Index));
    }
}
