using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Projekat3_Zadatak18
{
    internal class CocktailHandler
    {
        private static readonly HttpClient http = new HttpClient
        {
            BaseAddress = new Uri("https://www.thecocktaildb.com/api/json/v1/1/")
        };

        public async Task ObradaZahtevaAsync(HttpListenerContext context,char c)
        {
            var pocetnoSlovo = char.ToLower(c);

            await Observable.FromAsync(() => VratiKoktele(pocetnoSlovo))
                .SubscribeOn(TaskPoolScheduler.Default)
                .SelectMany(drinks => drinks.SelectMany(IzvuciSastojke))
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .GroupBy(i => i.ToLower())
                .SelectMany(g => g.Count().Select(count => new { Ingredient = g.Key, Count = count }))
                .ObserveOn(NewThreadScheduler.Default)
                .ToList()
                .Do(async list => await PosaljiOdgovorJsonAsync(context, 200, list));
                //.Catch((Exception ex) =>
                //{
                    //Console.WriteLine($"[GRESKA] {ex.Message}");
                    //return Observable.Empty<object>();
                //});
        }

        private async Task<Drink[]> VratiKoktele(char pocetnoSlovo)
        {
            var response = await http.GetStringAsync($"search.php?f={pocetnoSlovo}");
            var result = JsonSerializer.Deserialize<CocktailSearchResponse>(response);
            return result?.drinks ?? Array.Empty<Drink>();
        }

        private string[] IzvuciSastojke(Drink d)
        {
            return new[]
            {
                d.strIngredient1,
                d.strIngredient2,
                d.strIngredient3, 
                d.strIngredient4,
                d.strIngredient5, 
                d.strIngredient6, 
                d.strIngredient7,
                d.strIngredient8,
                d.strIngredient9,
                d.strIngredient10,
                d.strIngredient11,
                d.strIngredient12,
                d.strIngredient13,
                d.strIngredient14,
                d.strIngredient15
            }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        private async Task PosaljiOdgovorJsonAsync(HttpListenerContext context, int statusCode, object data)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var json = JsonSerializer.Serialize(data);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }

    public class CocktailSearchResponse
    {
        public Drink[] drinks { get; set; }
    }

    public class Drink
    {
        public string strIngredient1 { get; set; }
        public string strIngredient2 { get; set; }
        public string strIngredient3 { get; set; }
        public string strIngredient4 { get; set; }
        public string strIngredient5 { get; set; }
        public string strIngredient6 { get; set; }
        public string strIngredient7 { get; set; }
        public string strIngredient8 { get; set; }
        public string strIngredient9 { get; set; }
        public string strIngredient10 { get; set; }
        public string strIngredient11 { get; set; }
        public string strIngredient12 { get; set; }
        public string strIngredient13 { get; set; }
        public string strIngredient14 { get; set; }
        public string strIngredient15 { get; set; }
    }
}
