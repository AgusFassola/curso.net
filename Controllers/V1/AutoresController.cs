﻿using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Migrations;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPI.Utilidades;
using BibliotecaAPI.Utilidades.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel;
using System.Linq.Dynamic.Core;
//using System.Reflection.Metadata.Ecma335;

namespace BibliotecaAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/autores")]
    [Authorize(Policy = "esadmin")]
    [FiltroAgregarCabeceras("controlador","autores")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger<AutoresController> logger;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IServicioAutores servicioAutoresV1;
        private const string contenedor = "autores";
        private const string cache = "autores-obtener";

        //ctor + enter y se completa solo el constructor
        public AutoresController(ApplicationDbContext context, IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos, ILogger<AutoresController> logger,
            IOutputCacheStore outputCacheStore, IServicioAutores servicioAutoresV1)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.logger = logger;
            this.outputCacheStore = outputCacheStore;
            this.servicioAutoresV1 = servicioAutoresV1;
        }

        //[HttpGet("/lista-de-autores")]// para acceder tambien desde la ruta localhost/lista-de-autores
        [HttpGet(Name ="ObtenerAutoresV1")]
        [AllowAnonymous]//permise acceder a cualquiera aunque tenga un authorize
        [OutputCache(Tags = [cache])]
        [ServiceFilter<MiFiltroDeAccion>()]
        [FiltroAgregarCabeceras("accion", "obtener-autores")]
        [ServiceFilter<HATEOASAutoresAttribute>()]

        public async Task<IEnumerable<AutorDTO>> 
            Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            return await servicioAutoresV1.Get(paginacionDTO);
        }

        //[HttpGet("primero")]//api/autores/primero
        //public async Task<Autor> GetPrimerAutor()
        //{
        //    return await context.Autores.FirstAsync();
        //}

        [HttpGet("{id:int}", Name = "ObtenerAutorV1")] //le estoy agregando un id al /api/autores
        [AllowAnonymous]
        [EndpointSummary("Obtiene autor por ID")] // informacion en el swagger, documentar mejor
        [EndpointDescription("obtiene autor, incluye sus libros")]
        [ProducesResponseType<AutorConLibrosDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OutputCache(Tags = [cache])]
        [ServiceFilter<HATEOASAutorAttribute>()]
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


        [HttpGet("filtrar", Name = "FiltrarAutoresV1")]
        [AllowAnonymous]
        public async Task<ActionResult> Filtrar([FromQuery] AutorFiltroDTO autorFiltroDTO)
        {
            var queryable = context.Autores.AsQueryable();

            if (!string.IsNullOrEmpty(autorFiltroDTO.Nombres))
            {
                queryable = queryable.Where(x => x.Nombres.Contains(autorFiltroDTO.Nombres));
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.Apellidos))
            {
                queryable = queryable.Where(x => x.Apellidos.Contains(autorFiltroDTO.Apellidos));
            }

            if (autorFiltroDTO.IncluirLibros)
            {
                queryable = queryable.Include(x => x.Libros).ThenInclude(x => x.Libro);
            }

            if (autorFiltroDTO.TieneFoto.HasValue)
            {
                if (autorFiltroDTO.TieneFoto.Value)
                {
                    queryable = queryable.Where(x => x.Foto != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.Foto == null);

                }
            }

            if (autorFiltroDTO.TieneLibros.HasValue)
            {
                if (autorFiltroDTO.TieneLibros.Value)
                {
                    queryable = queryable.Where(x => x.Libros.Any());

                }
                else
                {
                    queryable = queryable.Where(x => !x.Libros.Any());

                }
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.TituloLibro))
            {
                queryable = queryable.Where(x => x.Libros.Any( 
                    y => y.Libro!.Titulo.Contains(autorFiltroDTO.TituloLibro)));

            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.CampoOrdenar))
            {
                var tipoOrden = autorFiltroDTO.OrdenAscendente ? "ascending" : "descending";
                try
                {
                    queryable = queryable.OrderBy($"{autorFiltroDTO.CampoOrdenar} {tipoOrden}");

                }
                catch(Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Nombres);
                    logger.LogError(ex.Message, ex);

                }
            }
            else
            {
                queryable = queryable.OrderBy(x => x.Nombres);
            }

                var autores = await queryable
                    .Paginar(autorFiltroDTO.PaginacionDTO).ToListAsync();

            if (autorFiltroDTO.IncluirLibros)
            {
                var autoresDTO = mapper.Map<IEnumerable<AutorConLibrosDTO>>(autores);
                return Ok(autoresDTO);
            }
            else
            {
                var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
                return Ok(autoresDTO);
            } 
        } 
        
        [HttpPost(Name = "CrearAutorV1")]
        public async Task<ActionResult> Post(AutorCreacionDTO autorCreacionDTO)
        {
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            context.Add(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutorV1", new {id = autor.Id}, autorDTO);
        }

        [HttpPost("con-foto", Name = "CrearAutorConFotoV1")]
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
            await outputCacheStore.EvictByTagAsync(cache, default);

            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("ObtenerAutorV1", new { id = autor.Id }, autorDTO);
        }

        [HttpPut("{id:int}", Name = "ActualizarAutorV1")]
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
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();//204
        }

        [HttpPatch("{id:int}", Name ="PatchAutorV1")]
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
            await outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

        [HttpDelete("{id:int}", Name ="BorrarAutorV1")]
        public async Task<ActionResult> Delete(int id)
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if(autor is null)
            {
                return NotFound();
            }

            context.Remove(autor);
            await context.SaveChangesAsync();
            await outputCacheStore.EvictByTagAsync(cache, default);
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
