using System;
using Gamesbakery.Core;
using Gamesbakery.WebGUI.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;


namespace Gamesbakery.Controllers
{
    using ILogger = Serilog.ILogger;

    public abstract class BaseController : Controller
    {
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;

        protected BaseController(ILogger logger, IConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration;
        }

        protected IDisposable PushLogContext()
        {
            return LogContext.PushProperty("Controller", GetType().Name);
        }

        protected void LogInformation(string message, params object[] args)
        {
            Logger.Information(message, args);
        }

        protected void LogError(Exception ex, string message, params object[] args)
        {
            Logger.Error(ex, message, args);
        }

        protected void LogWarning(string message, params object[] args)
        {
            Logger.Warning(message, args);
        }

        protected Guid? GetCurrentUserId()
        {
            return User.GetUserId();
        }

        protected Guid? GetCurrentSellerId()
        {
            var sellerIdClaim = User.FindFirst("SellerId")?.Value;
            return Guid.TryParse(sellerIdClaim, out var id) && id != Guid.Empty ? id : null;
        }

        protected UserRole GetCurrentRole()
        {
            return User.GetRole();
        }
    }
}