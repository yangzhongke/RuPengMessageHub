using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuPengMessageHub.Server.Filters
{
    public class ExceptionFilter : IExceptionFilter
    {
        private ILogger logger;

        public ExceptionFilter(ILoggerFactory logFactory)
        {
            this.logger = logFactory.CreateLogger(typeof(ExceptionFilter));
        }

        public void OnException(ExceptionContext context)
        {
            this.logger.LogError("未处理异常" + context.Exception);
        }
    }
}
