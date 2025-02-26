using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
//using System.Reflection.Metadata.Ecma335;

namespace BibliotecaAPI.Controllers
{
    [ApiController]
    [Route("api/autores")]
    [Authorize(Policy = "esadmin")]
    
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private const string contenedor = "autores";


        //ctor + enter y se completa solo el constructor
        public AutoresController(ApplicationDbContext context, IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        //[HttpGet("/lista-de-autores")]// para acceder tambien desde la ruta localhost/lista-de-autores
        [HttpGet]
        [AllowAnonymous]//permise acceder a cualquiera aunque tenga un authorize
        public async Task<IEnumerable<AutorDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Autores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable
                .OrderBy( x => x.Nombres)
                .Paginar(paginacionDTO).ToListAsync();

            //para guardar unicamente los valores que yo quiera mostrar y de la forma que yo quiera
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;
        }

        //[HttpGet("primero")]//api/autores/primero
        //public async Task<Autor> GetPrimerAutor()
        //{
        //    return await context.Autores.FirstAsync();
        //}

        [HttpGet("{id:int}", Name = "ObtenerAutor")] //le estoy agregando un id al /api/autores
        [AllowAnonymous]
        [EndpointSummary("Obtiene autor por ID")] // informacion en el swagger, documentar mejor
        [EndpointDescription("obtiene autor, incluye sus libros")]
        [ProducesResponseType<AutorConLibrosDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<AutorConLibrosDTO>> Get([Description("El id del autor")] int id)
        {
            var autor = await context.Autores
                .Include(x => x.Libros)
                .ThenInclude(x => x.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if(autor is null)
            {
                return NotFound();
            }
            var autorDTO = mapper.Map<AutorConLibrosDTO>(autor);
            return autorDTO;
        }
        
        [HttpPost]
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            context.Add(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutor", new {id = autor.Id}, autorDTO);
        }

        [HttpPost("con-foto")]
        public async Task<ActionResult> PostConFoto([FromForm]
            AutorCreacionDTOConFoto autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);

            if(autorCreacionDTO.Foto is not null)
            {
                var url = await almacenadorArchivos.Almacenar(contenedor,
                    autorCreacionDTO.Foto);
                autor.Foto = url;
            }

            context.Add(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutor", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, 
            [FromForm] AutorCreacionDTOConFoto autorCreacionDTO)
        {

            var existeAutor = await context.Autores.AnyAsync(x => x.Id == id);

            if(!existeAutor)
            {
                return NotFound();
            }

            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;

            if(autorCreacionDTO.Foto is not null)
            {
                var fotoActual = await context
                    .Autores.Where(x => x.Id == id)
                    .Select(x => x.Foto).FirstAsync();

                var url = await almacenadorArchivos.Editar(fotoActual, contenedor,
                    autorCreacionDTO.Foto);
                autor.Foto = url;
            }

            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();//204
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AutorPatchDTO> patchDoc)
        {
            if(patchDoc is null)
            {
                return BadRequest();
            }

            var autorDB = await context.Autores.FirstOrDefaultAsync(x => x.Id == id); 

            if(autorDB is null)
            {
                return NotFound();
            }
            var autorPatchDTO = mapper.Map<AutorPatchDTO>(autorDB);

            patchDoc.ApplyTo(autorPatchDTO, ModelState);

            var esValido = TryValidateModel(autorPatchDTO);

            if (!esValido)
            {
                return ValidationProblem();
            }

            mapper.Map(autorPatchDTO, autorDB);

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if(autor is null)
            {
                return NotFound();
            }

            context.Remove(autor);
            await context.SaveChangesAsync();
            await almacenadorArchivos.Borrar(autor.Foto, contenedor);

            return NoContent();
        }

        ////recibir un string por parametro
        //[HttpGet("{nombre:alpha}")]
        //public async Task<IEnumerable<Autor>> Get(string nombre)
        //{
        //    return await context.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();
        //}

        ////agregar 2 parametros de ruta
        //[HttpGet("{parametro1}/{parametro2?}")]
        //public ActionResult Get(string parametro1, string? parametro2) //autores/agus/mateo
        //{
        //    return Ok(new { parametro1, parametro2 });
        //}

    }
}
