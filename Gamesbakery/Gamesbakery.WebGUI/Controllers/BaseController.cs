using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using ILogger = Serilog.ILogger;

namespace Gamesbakery.Controllers
{
    public abstract class BaseController : Controller
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        protected BaseController(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected void LogInformation(string message, params object[] args)
        {
            if (ShouldLog())
            {
                _logger.Information(message, args);
            }
        }

        protected void LogWarning(string message, params object[] args)
        {
            if (ShouldLog())
            {
                _logger.Warning(message, args);
            }
        }

        protected void LogError(Exception ex, string message, params object[] args)
        {
            if (ShouldLog())
            {
                _logger.Error(ex, message, args);
            }
        }

        protected void LogError(string message, params object[] args)
        {
            if (ShouldLog())
            {
                _logger.Error(message, args);
            }
        }

        private bool ShouldLog()
        {
            var logRoles = _configuration.GetSection("LoggingSettings:LogRoles").Get<List<string>>() ?? new List<string>();
            var logControllers = _configuration.GetSection("LoggingSettings:LogControllers").Get<List<string>>() ?? new List<string>();
            var currentRole = HttpContext.Session.GetString("Role") ?? "Guest";
            var controllerName = GetType().Name.Replace("Controller", "");

            return logRoles.Contains(currentRole) && logControllers.Contains(controllerName);
        }

        protected IDisposable PushLogContext()
        {
            var disposables = new List<IDisposable>
            {
                LogContext.PushProperty("Action", ControllerContext.ActionDescriptor.ActionName),
                LogContext.PushProperty("Method", HttpContext

.Request.Method),
                LogContext.PushProperty("Username", HttpContext.Session.GetString("Username") ?? "Anonymous"),
                LogContext.PushProperty("Role", HttpContext.Session.GetString("Role") ?? "None")
            };

            return new CompositeDisposable(disposables);
        }

        private class CompositeDisposable : IDisposable
        {
            private readonly IEnumerable<IDisposable> _disposables;

            public CompositeDisposable(IEnumerable<IDisposable> disposables)
            {
                _disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
            }

            public void Dispose()
            {
                foreach (var disposable in _disposables)
                {
                    disposable?.Dispose();
                }
            }
        }
    }
}