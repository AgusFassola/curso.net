namespace BibliotecaAPI
{
    //aca está la logica del Middleware
    public class LogueaPeticionMiddleware
    {
        private readonly RequestDelegate next;

        public LogueaPeticionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            //viene la peticion
            var logger = contexto.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation($"Petición: {contexto.Request.Method} {contexto.Request.Path}");

            await next.Invoke(contexto);

            //se va la respuesta

            logger.LogInformation($"Respuesta: {contexto.Response.StatusCode}");
        }
    }

    //Acá se utiliza la logica del Middleware de forma sencilla
    public static class LogueaPeticionMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogueaPeticion(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogueaPeticionMiddleware>();
        }
    }
}
