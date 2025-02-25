using BibliotecaAPI.Validaciones;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BibliotecaAPI.Entidades
{
    public class Autor 
    {
        public int Id { get; set; }

        [Required( ErrorMessage = "El campo {0} es requerido")] // con {0} muestra el valor en cuestion
        [StringLength(150, ErrorMessage = "el campo {0} debe tener {1} caracteres o menos")]
        //esto quiere decir que si me mandan un autor desde el cliente, necesariamente debe contener un nombre
        [PrimeraLetraMayuscula]
        public required string Nombres { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")] // con {0} muestra el valor en cuestion
        [StringLength(150, ErrorMessage = "el campo {0} debe tener {1} caracteres o menos")]
        [PrimeraLetraMayuscula]
        public required string Apellidos { get; set; }

        [StringLength(20, ErrorMessage = "el campo {0} debe tener {1} caracteres o menos")]
        public string? Identificacion { get; set; }
        [Unicode(false)]
        public string? Foto { get; set; }
        public List<AutorLibro> Libros { get; set; } = [];

        //las validaciones por modelo entran despues de las validaciones por atributo
        //las validaciones por atributo son mas faciles de reutilizar
        //IEnumerable me sirve para retornar muchos resultados, en este caso varios errores de validacion



        //PARA COMENTAR VARIAS LINEAS CONTROL+K, CONTROL C, PARA DESCOMENTAR= CONTRL +K, CONTROL +U

        //[range(18, 120)]//para validar por ejemplo una edad
        //public int edad { get; set; }

        //[creditcard]//unicamente valida que sea un formato de tarjeta de credito
        //public string? tarjetadecredito { get; set; }

        //[url]//unicamente valida que sea un formato de url
        //public string? url { get; set; }
    }
}
