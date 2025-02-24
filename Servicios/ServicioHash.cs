using BibliotecaAPI.DTOs;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace BibliotecaAPI.Servicios
{
    public class ServicioHash : IServicioHash
    {
        public ResultadoHashDTO Hash(String input)//creo una funcion aleatoria que no recibe una sal y creo una sal aleatoria
        {
            var sal = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(sal);
            }
            return Hash(input, sal);
        }

        public ResultadoHashDTO Hash(string input, byte[] sal)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: input,
                salt: sal,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10_000, //10000 iteraciones para tener un hash mas dificil de romper
                numBytesRequested: 256 / 8 //pasamos 256 bits para definir el tamaño del hash
                ));

            return new ResultadoHashDTO
            {
                Hash = hashed,
                Sal = sal
            };
        }
    }
}
