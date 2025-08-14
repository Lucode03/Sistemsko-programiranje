using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Projekat3_Zadatak18
{
    internal class Server
    {
        private readonly HttpListener _listener;
        private volatile bool _zaustaviSe;
        private int _brojAktivnihZahteva = 0;
        private readonly ManualResetEvent _sviZahteviGotovi = new ManualResetEvent(true);
        private readonly CocktailHandler _cocktailHandler;

        public Server(string urlPrefix)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(urlPrefix);
            _zaustaviSe = false;
            _cocktailHandler = new CocktailHandler();
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Server je startovan.");
            _ = OsluskujZahteveAsync();
        }

        public void Stop()
        {
            Console.WriteLine("Server ce biti zaustavljen cim se zavrse svi aktivni zahtevi...");
            _zaustaviSe = true;
            _sviZahteviGotovi.WaitOne();
            _listener.Stop();
            Console.WriteLine("Server zaustavljen.");
        }

        private async Task OsluskujZahteveAsync()
        {
            try
            {
                while (!_zaustaviSe)
                {
                    var context = await _listener.GetContextAsync();

                    if (Interlocked.Increment(ref _brojAktivnihZahteva) == 1)
                        _sviZahteviGotovi.Reset();

                    Observable.StartAsync(() => ObradiZahtevAsync(context))
                        .SubscribeOn(TaskPoolScheduler.Default)
                        .Subscribe(_ => {},
                            ex => Console.WriteLine($"[ERROR] {ex.Message}"),
                            () =>
                            {
                                if (Interlocked.Decrement(ref _brojAktivnihZahteva) == 0)
                                    _sviZahteviGotovi.Set();
                            });
                }
            }
            catch (HttpListenerException lEx)
            {
                if (_zaustaviSe)
                {
                    Console.WriteLine("Listener je zaustavljen.");
                }
                else
                {
                    Console.WriteLine($"Desila se greska sa listenerom: {lEx.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Desila se greska na serveru: {ex.Message}");
            }
        }

        private async Task ObradiZahtevAsync(HttpListenerContext context)
        {
            string putanja = context.Request.Url.AbsolutePath.TrimStart('/');       
            if (putanja == "favicon.ico")
            {
                await PosaljiOdgovorAsync(context, 204, "No Content");
                return;
            }
            string query = context.Request.Url.Query;
            string? param = context.Request.QueryString["letter"];
            Console.WriteLine($"[LOG] Zahtev primljen: {putanja+query} | Vreme: {DateTime.Now}");

            if (!(putanja.StartsWith("ingredients", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("[GRESKA] Zahtev mora poceti sa ingredients.");
                await PosaljiOdgovorAsync(context, 400, "Bad Request.");
                return;
            }         
            /*if (string.IsNullOrWhiteSpace(putanja))
            {
                Console.WriteLine("[GRESKA] Lose postavljen zahtev.");
                await PosaljiOdgovorAsync(context, 400, "Bad Request.");
                return;
            }*/
            if (context.Request.HttpMethod.ToLower() != "get")
            {
                Console.WriteLine("[GRESKA] Losa metoda zahteva (metoda mora biti GET).");
                await PosaljiOdgovorAsync(context, 400, "Bad Request.");
                return;
            }
            if (string.IsNullOrWhiteSpace(query) ||!query.Contains("letter="))
            {
                Console.WriteLine("[GRESKA] Lose postavljen upit.");
                await PosaljiOdgovorAsync(context, 400, "Bad Request.");                
                return;
            }
            if (string.IsNullOrWhiteSpace(param) || param.Length != 1)
            {
                Console.WriteLine("[GRESKA] Parametar upita mora biti tacno jedno slovo.");
                await PosaljiOdgovorAsync(context, 400, "Bad Request.");                
                return;
            }
            char pocetnoSlovo = param[0];
            if (!Ekstenzije.ProveraOpsega(pocetnoSlovo))
            {
                Console.WriteLine("[GRESKA] Parametar upita mora biti slovo.");
                await PosaljiOdgovorAsync(context, 400, "Bad Request.");                
                return;
            }
            try
            {
                await _cocktailHandler.ObradaZahtevaAsync(context,pocetnoSlovo);
                Console.WriteLine($"[INFO] Isporuceni sastojci za pica sa pocetnim slovom : {pocetnoSlovo}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GRESKA] {ex.Message}");
                await PosaljiOdgovorAsync(context, 500, "Internal Server Error.");
            }
        }

        private async Task PosaljiOdgovorAsync(HttpListenerContext ctx, int statusCode, string message)
        {
            ctx.Response.StatusCode = statusCode;
            ctx.Response.ContentType = "text/plain";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            ctx.Response.ContentLength64 = buffer.Length;
            await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            ctx.Response.OutputStream.Close();
        }
    }
}
