using Kes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kes
{
    internal static class TestiranjeKesa
    {
        static readonly string[] imenaSlika = Enumerable.Range(1, 1000).Select(i => $"slika_{i}.jpg").ToArray();

        public static void Testiraj()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Testiranje Lock kesa...");
            TestirajKes(new LockKes<string, string>(), "Lock Kes");

            Console.WriteLine("Testiranje ReaderWriterKesa...");
            TestirajKes(new ReaderWriterKes<string, string>(), "ReaderWriter Kes");

            Console.WriteLine("Testiranje Konkurentnog Kesa...");
            TestirajKes(new KonkurentniKes<string, string>(), "Konkurentni Kes");

            Console.WriteLine("Testiranje LRU Kesa...");
            TestirajKes(new LRUKes<string, string>(900), "LRU Kes");

            Console.WriteLine("Testiranje FIFO Kesa...");
            TestirajKes(new FIFOKes<string, string>(900), "FIFO Kes");

            Console.WriteLine("Testiranje Konkurentni FIFO Kesa...");
            TestirajKes(new KonkurentniFIFOKes<string, string>(900), "Konkurentni FIFO Kes");

            Console.WriteLine("Testiranje Optimizovanog LRU Kesa...");
            TestirajKes(new OptimizovanLRUKes<string, string>(900), "Optimizovani LRU Kes");

            Console.WriteLine("Testiranje Optimizovanog LRU Kesa2...");
            TestirajKes(new OptimizovanLRUKes2<string, string>(900, 20), "Optimizovani LRU Kes 2");

            Console.WriteLine("Kraj testiranja.");
        }

        private static void TestirajKes(IKesable<string, string> kes, string imeKesa)
        {
            Stopwatch sw = new();
            int brojNiti = 100;
            int brojOperacijaPoNiti = 1000;
            int ukupnoZadataka = brojNiti;
            int zavrsenoZadataka = 0;

            int pogodakKesa = 0;
            int promasajKesa = 0;
            int ukupnoPristupa = 0;

            object lockObj = new();

            sw.Start();

            using (ManualResetEvent mre = new(false))
            {
                for (int i = 0; i < brojNiti; i++)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        Random lokalniRandom = new(Guid.NewGuid().GetHashCode());

                        for (int j = 0; j < brojOperacijaPoNiti; j++)
                        {
                            string nazivSlike = imenaSlika[lokalniRandom.Next(imenaSlika.Length)];

                            bool pogodak = kes.ProbajDaPribavisVrednost(nazivSlike, out var putanja);
                            lock (lockObj)
                            {
                                ukupnoPristupa++;
                                if (pogodak)
                                    pogodakKesa++;
                                else
                                    promasajKesa++;
                            }

                            if (!pogodak)
                            {
                                // Simulacija pretrage slike na disku
                                Thread.Sleep(1);
                                putanja = $@"C:\slike\{nazivSlike}";
                                kes.DodajIliAzuriraj(nazivSlike, putanja);
                            }
                        }

                        if (Interlocked.Increment(ref zavrsenoZadataka) == ukupnoZadataka)
                        {
                            mre.Set();
                        }
                    });
                }

                mre.WaitOne(); // Čekamo da se sve niti završe
            }

            sw.Stop();

            Console.WriteLine($"{imeKesa} završeno za {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"Ukupan broj podataka u kesu: {kes.KolicinaPodataka}");
            Console.WriteLine($"Ukupno pristupa: {ukupnoPristupa}");
            Console.WriteLine($"Pogodaka (cache hit): {pogodakKesa}");
            Console.WriteLine($"Promasaja (cache miss): {promasajKesa}");
            Console.WriteLine($"Hit rate: {pogodakKesa * 100.0 / ukupnoPristupa:F2}%");
            Console.WriteLine();
        }
    }
}