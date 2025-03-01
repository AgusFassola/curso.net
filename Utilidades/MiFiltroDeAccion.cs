using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaAPI.Utilidades
{
    public class MiFiltroDeAccion : IActionFilter
    {
        private readonly ILogger<MiFiltroDeAccion> logger;

        public MiFiltroDeAccion(ILogger<MiFiltroDeAccion> logger)
        {
            this.logger = logger;
        }
       
        //antes de la accion
        public void OnActionExecuting(ActionExecutingContext context)
        {
            logger.LogInformation("Ejecutando la accion");
        }

        //despues de la accion
        public void OnActionExecuted(ActionExecutedContext context)
        {
            logger.LogInformation("Ejecutando ejecutada");

        }


    }
}
