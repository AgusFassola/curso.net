namespace BibliotecaAPI
{
    public class BloquePeticionMiddleware
    {
        private readonly RequestDelegate next;

        public BloquePeticionMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext contexto)
        {
            if (contexto.Request.Path == "/bloqueado") //para bloquear o detener la pagina si ingesa a ese url
            {
                contexto.Response.StatusCode = 403;
                await contexto.Response.WriteAsync("Acceso denegado");
            }
            else
            {
                await next.Invoke(contexto);
            }
        }
    }

    public static class BloqueaPeticionMiddlewareExtensions
    {
        public static IApplicationBuilder UseBloqueaPeticion(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BloquePeticionMiddleware>();
        }
    }
}
