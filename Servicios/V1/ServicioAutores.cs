using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Servicios.V1
{
    public class ServicioAutores : IServicioAutores
    {
        private readonly ApplicationDbContext context;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;

        public ServicioAutores(ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<AutorDTO>> Get( PaginacionDTO paginacionDTO)
        {
            // throw new NotImplementedException();
            var queryable = context.Autores.AsQueryable();
            await httpContextAccessor.HttpContext!.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable
                .OrderBy(x => x.Nombres)
                .Paginar(paginacionDTO).ToListAsync();

            //para guardar unicamente los valores que yo quiera mostrar y de la forma que yo quiera
            var autoresDTO = mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;
        }
    }
}
