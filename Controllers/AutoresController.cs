using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        //ctor + enter y se completa solo el constructor
        public AutoresController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        //[HttpGet("/lista-de-autores")]// para acceder tambien desde la ruta localhost/lista-de-autores
        [HttpGet]//se accede desde api/autores
        [AllowAnonymous]//permise acceder a cualquiera aunque tenga un authorize
        public async Task<IEnumerable<AutorDTO>> Get()
        {
            var autores = await context.Autores.ToListAsync();//para crear una lista con los autores
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
        public async Task<ActionResult<AutorConLibrosDTO>> Get( int id)
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
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDtO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDtO);
            context.Add(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutor", new {id = autor.Id}, autorDTO);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, AutorCreacionDTO autorCreacionDtO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDtO);
            autor.Id = id;
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
            var registrosBorrados = await context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (registrosBorrados == 0)//no se borro ningun registro
            {
                return NotFound();
            }
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
