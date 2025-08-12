using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server1
{
    internal class ProsireniKesZaSlike:KesZaSlike
    {
        private readonly ConcurrentDictionary<string, EventWaitHandle> _lockoviPretrage;

        public ProsireniKesZaSlike(string rootFolder) : base(rootFolder) 
        {
            _lockoviPretrage = new ConcurrentDictionary<string, EventWaitHandle>();
        }

        public override async Task<byte[]?> PribaviSlikuAsync(string nazivFajla)
        {
            // Prvo se pokusava u glavnom kesu
            if (_kes.ProbajDaPribavisVrednost(nazivFajla, out byte[] podaci))
                return podaci;

            EventWaitHandle waitHandle = null;
            bool pravaNit = false;

            try
            {
                // Ako postoji vec EventHandle vrati se taj,
                // ako ne, napravi se novi i postavi se prvaNit na true.
                waitHandle = _lockoviPretrage.GetOrAdd(nazivFajla, key =>
                {
                    pravaNit = true;
                    return new ManualResetEvent(false);
                });

                if (!pravaNit)
                {
                    // Ako nije u pitanju prva pristigla nit, onda se ona blokira dok prva nit ne zavrsi posao
                    //Console.WriteLine($"[NEE!]Ja nisam prva nit i ja cekam da prva nit zavrsi za trazenje slike {nazivFajla}");
                    waitHandle.WaitOne(3000);

                    if (_kes.ProbajDaPribavisVrednost(nazivFajla, out podaci))
                        return podaci;

                    return null; // Fajl nije pronadjen
                }

                // Ako je prva nit, ona ce obaviti pretragu
                //Console.WriteLine($"[URA!]Ja sam prva nit i ja krecem da trazim {nazivFajla}.");
                string? putanjaDoFajla = NadjiFajl(nazivFajla);
                if (putanjaDoFajla == null)// da li da ovde stavimo odmah return ili da izbacimo
                    return null;
                podaci = await File.ReadAllBytesAsync(putanjaDoFajla);
                //podaci = putanjaDoFajla != null ? await File.ReadAllBytesAsync(putanjaDoFajla) : null;

                // Azuriraj kes
                _kes.DodajIliAzuriraj(nazivFajla, podaci);

                return podaci;
            }
            finally
            {
                if (pravaNit)
                {
                    // Signalizira se drugim nitima da je pretraga zavrsena
                    ((ManualResetEvent)waitHandle).Set();

                    // Cisti se privremeni waitHandle
                    _lockoviPretrage.TryRemove(nazivFajla, out _);

                    waitHandle.Close();
                }
            }

        }
    }
}
