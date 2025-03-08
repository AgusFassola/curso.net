using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V1
{
    public class RestriccionesDominioController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IServiciosUsuarios serviciosUsuarios;

        public RestriccionesDominioController(ApplicationDbContext context, 
            IServiciosUsuarios serviciosUsuarios)
        {
            this.context = context;
            this.serviciosUsuarios = serviciosUsuarios;
        }

        [HttpPost]
        public async Task<ActionResult> Post(RestriccionDominioCreacionDTO restriccionDominioCreacionDTO)
        {
            var llaveDB = await context.LlavesAPI.FirstOrDefaultAsync(x => x.Id ==
            restriccionDominioCreacionDTO.LlaveId);

            if (llaveDB is null)
            {
                return NotFound();
            }
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();
            

            if (usuarioId != llaveDB.UsuaruiId)
            {
                return Forbid();
            }

            var restriccionDominio = new RestriccionDominio
            {
                Dominio = restriccionDominioCreacionDTO.Dominio,
                LlaveId = restriccionDominioCreacionDTO.LlaveId
            };

            context.Add(restriccionDominio);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id,
            RestriccionDominioActualizacionDTO restriccionDominioActualizacionDTO)
        {
            var restriccionDB = await context.RestriccionesDominio.Include(x => x.Llave)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDB is null)
            {
                return NotFound();
            }
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();


            if (usuarioId != restriccionDB.Llave!.UsuaruiId )
            {
                return Forbid();
            }

            restriccionDB.Dominio = restriccionDominioActualizacionDTO.Dominio;


            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var restriccionDB = await context.RestriccionesDominio.Include(x => x.Llave)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDB is null)
            {
                return NotFound();
            }
            var usuarioId = serviciosUsuarios.ObtenerUsuarioId();


            if (usuarioId != restriccionDB.Llave!.UsuaruiId)
            {
                return Forbid();
            }

            context.Remove(restriccionDB);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
