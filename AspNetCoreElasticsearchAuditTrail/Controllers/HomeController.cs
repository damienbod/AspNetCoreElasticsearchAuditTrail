﻿using AspNetCoreElasticsearchAuditTrail.ViewModels;
using AuditTrail;
using AuditTrail.Model;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreElasticsearchAuditTrail.Controllers;

public class HomeController(IAuditTrailProvider<CustomAuditTrailLog> auditTrailProvider) : Controller
{
    private readonly IAuditTrailProvider<CustomAuditTrailLog> _auditTrailProvider = auditTrailProvider;

    public IActionResult Index()
    {
        var auditTrailLog = new CustomAuditTrailLog()
        {
            User = GetUserName(),
            Origin = "HomeController:Index",
            Action = "Home GET",
            Log = "home page called doing something important enough to be added to the audit log.",
            Extra = "yep"
        };

        _auditTrailProvider.AddLog(auditTrailLog);
        return View();
    }

    public async Task<IActionResult> AuditTrailAsync()
    {
        var auditTrailLog = new CustomAuditTrailLog()
        {
            User = GetUserName(),
            Origin = "HomeController:AuditTrailAsync",
            Action = "AuditTrailAsync GET",
            Log = "user clicked the audit trail nav."
        };

        await _auditTrailProvider.AddLog(auditTrailLog);
        ViewData["Message"] = "Your application description page.";

        var auditTrailViewModel = new AuditTrailViewModel
        {
            AuditTrailLogs = (await _auditTrailProvider.QueryAuditLogs()).ToList(),
            Filter = "*",
            Skip = 0,
            Size = 10
        };
        return View(auditTrailViewModel);
    }

    public async Task<IActionResult> AuditTrailSearchAsync(string searchString, int skip, int amount)
    {
        var auditTrailLog = new CustomAuditTrailLog()
        {
            User = GetUserName(),
            Origin = "HomeController:AuditTrailSearchAsync",
            Action = "AuditTrailSearchAsync GET",
            Log = $"user clicked the audit trail nav. {searchString}"
        };

        await _auditTrailProvider.AddLog(auditTrailLog);

        var auditTrailViewModel = new AuditTrailViewModel
        {
            Filter = searchString,
            Skip = skip,
            Size = amount
        };

        if (skip > 0 || amount > 0)
        {
            var paging = new AuditTrailPaging
            {
                Size = amount,
                Skip = skip
            };

            auditTrailViewModel.AuditTrailLogs = (await _auditTrailProvider.QueryAuditLogs(searchString, paging)).ToList();

            return View(auditTrailViewModel);
        }

        auditTrailViewModel.AuditTrailLogs = (await _auditTrailProvider.QueryAuditLogs(searchString)).ToList();
        return View(auditTrailViewModel);
    }

    public IActionResult Contact()
    {
        var auditTrailLog = new CustomAuditTrailLog()
        {
            User = GetUserName(),
            Origin = "HomeController:Contact",
            Action = "Contact GET",
            Log = "user clicked the contact nav."
        };

        _auditTrailProvider.AddLog(auditTrailLog);
        ViewData["Message"] = "Your contact page.";

        return View();
    }

    public IActionResult Error()
    {
        var auditTrailLog = new CustomAuditTrailLog()
        {
            User = GetUserName(),
            Origin = "HomeController:Error",
            Action = "Error GET",
            Log = "something has gone wrong"
        };

        _auditTrailProvider.AddLog(auditTrailLog);
        ViewData["Message"] = "Your contact page.";
        return View();
    }

    private string GetUserName()
    {
        return User.Identity!.Name ?? "Anonymous";
    }
}
