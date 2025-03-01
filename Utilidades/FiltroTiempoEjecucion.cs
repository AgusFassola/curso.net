﻿using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace BibliotecaAPI.Utilidades
{
    public class FiltroTiempoEjecucion : IAsyncActionFilter
    {
        private readonly ILogger<FiltroTiempoEjecucion> logger;

        public FiltroTiempoEjecucion( ILogger<FiltroTiempoEjecucion> logger)
        {
            this.logger = logger;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //antes de la ejecucion
            var stopWatch = Stopwatch.StartNew();
            logger.LogInformation($"INICIO Acción: {context.ActionDescriptor.DisplayName}");

            await next();

            //despues de la ejecucion
            stopWatch.Stop();
            logger.LogInformation($"FIN Acción: {context.ActionDescriptor.DisplayName} - Tiempo: {stopWatch.ElapsedMilliseconds} ms");

        }
    }
}
