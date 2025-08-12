using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server1
{
    internal class Server
    {
        private readonly HttpListener _listener;
        private readonly string _rootFolder;
        private readonly KesZaSlike _kesZaSlike;
        
        private volatile bool _zaustaviSe;
        private int _brojAktivnihZahteva = 0;
        private readonly ManualResetEvent _sviZahteviGotovi = new ManualResetEvent(true);
        // Setovan kada su svi zavrseni
        // Resetovan kada nisu

        public Server(string rootFolder, string urlPrefix)
        {
            _rootFolder = rootFolder;
            _listener = new HttpListener();
            _listener.Prefixes.Add(urlPrefix);
            _kesZaSlike = new ProsireniKesZaSlike(rootFolder);
            _zaustaviSe = false;
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine($"Server je startovan.");
            ThreadPool.QueueUserWorkItem(_ => OsluskujZahteve());
        }

        public void Stop()
        {
            Console.WriteLine("Server ce biti zaustavljen cim se zavrse svi aktivni zahtevi...");
            _zaustaviSe = true;
            _sviZahteviGotovi.WaitOne();
            _listener.Stop();
            Console.WriteLine("Server je zaustavljen.");
        }

        private void OsluskujZahteve()
        {
            try
            {
                while (!_zaustaviSe)
                {
                    var context = _listener.GetContext();
                    if (Interlocked.Increment(ref _brojAktivnihZahteva) == 1)
                        _sviZahteviGotovi.Reset();

                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        try
                        {
                            ObradiZahtev(context);
                        }
                        finally
                        {
                            if (Interlocked.Decrement(ref _brojAktivnihZahteva) == 0)
                                _sviZahteviGotovi.Set();
                        }
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

        private void ObradiZahtev(HttpListenerContext context)
        {
            string nazivFajla = context.Request.Url.AbsolutePath.TrimStart('/');
            if (nazivFajla == "favicon.ico")
            {
                PosaljiOdgovor(context, 204, ""); // 204 No Content
                return;
            }
            Console.WriteLine($"[LOG] Zahtev primljen: {nazivFajla} | Vreme: {DateTime.Now}");
            if (string.IsNullOrWhiteSpace(nazivFajla))
            {
                Console.WriteLine("[GRESKA] Lose postavljen zahtev.");
                PosaljiOdgovor(context, 400, "Bad Request.");
                return;
            }
            if (context.Request.HttpMethod.ToLower() != "get")
            {
                Console.WriteLine("[GRESKA] Losa metoda zahteva (metoda mora biti GET).");
                PosaljiOdgovor(context, 400, "Bad Request.");
                return;
            }
            if (!Ekstenzije.ValidnaEkstenzija(nazivFajla))
            {
                Console.WriteLine("[GRESKA] Losa ekstenzija trazenog fajla.");
                PosaljiOdgovor(context, 400, "Bad Request.");
                return;
            }
            try
            {
                var podacoOSlici = _kesZaSlike.PribaviSliku(nazivFajla);
                if (podacoOSlici == null)
                {
                    Console.WriteLine($"[INFO] Fajl nije pronadjen: {nazivFajla}");
                    PosaljiOdgovor(context, 404, $"Image '{nazivFajla}' Not Found.");
                    return;
                }

                context.Response.ContentType = Ekstenzije.VratiMimeTip(nazivFajla);
                context.Response.ContentLength64 = podacoOSlici.Length;
                context.Response.OutputStream.Write(podacoOSlici, 0, podacoOSlici.Length);
                context.Response.OutputStream.Close();

                Console.WriteLine($"[INFO] Isporucena slika: {nazivFajla}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska: {ex.Message}");
                PosaljiOdgovor(context, 500, "Internal Server Error.");
            }
            /*finally
            {
                _kesZaSlike.PisiRecnik();
            }*/
        }

        private void PosaljiOdgovor(HttpListenerContext context, int statusKod, string poruka)
        {
            context.Response.StatusCode = statusKod;
            byte[] buffer = Encoding.UTF8.GetBytes(poruka);
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }
}
