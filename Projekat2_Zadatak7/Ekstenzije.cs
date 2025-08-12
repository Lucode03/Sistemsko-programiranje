using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Server1
{
    internal static class Ekstenzije
    {
        public static string VratiMimeTip(string putanjaDoFajla)
        {
            string ekstenzija = Path.GetExtension(putanjaDoFajla).ToLower();
            return ekstenzija switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".webp" => "image/webp",
                _ => "application/octet-stream",
            };
        }

        public static bool ValidnaEkstenzija(string nazivFajla)
        {
            string ekstenzija = Path.GetExtension(nazivFajla).ToLower();
            return ekstenzija switch
            {
                ".png" => true,
                ".jpg" => true,
                ".jpeg" => true,
                ".gif" => true,
                ".bmp" => true,
                ".svg" => true,
                ".webp" => true,
                _ => false
            };
        }
    }
}
