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
            this.Logger = logger;
            this.Configuration = configuration;
        }

        protected IDisposable PushLogContext()
        {
            return LogContext.PushProperty("Controller", this.GetType().Name);
        }

        protected void LogInformation(string message, params object[] args)
        {
            this.Logger.Information(message, args);
        }

        protected void LogError(Exception ex, string message, params object[] args)
        {
            this.Logger.Error(ex, message, args);
        }

        protected void LogWarning(string message, params object[] args)
        {
            this.Logger.Warning(message, args);
        }

        protected Guid? GetCurrentUserId()
        {
            return this.User.GetUserId();
        }

        protected Guid? GetCurrentSellerId()
        {
            var sellerIdClaim = this.User.FindFirst("SellerId")?.Value;
            return Guid.TryParse(sellerIdClaim, out var id) && id != Guid.Empty ? id : null;
        }

        protected UserRole GetCurrentRole()
        {
            return this.User.GetRole();
        }
    }
}
