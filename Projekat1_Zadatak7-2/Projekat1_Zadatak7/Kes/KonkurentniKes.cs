using Kes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kes
{
    public class KonkurentniKes<K, V> : IKesable<K, V>
    {
        private readonly ConcurrentDictionary<K, V> _kes = new();

        public void DodajIliAzuriraj(K kljuc, V vrednost)
        {
            _kes.AddOrUpdate(kljuc, vrednost,
                (istiKljuc, staraVrednost) => vrednost);
        }

        public bool IzbaciKljuc(K kljuc)
        {
            return _kes.Remove(kljuc, out _);
        }

        public void OcistiKes()
        {
            _kes.Clear();
        }

        public bool ProbajDaPribavisVrednost(K kljuc, out V vrednost)
        {
            return _kes.TryGetValue(kljuc, out vrednost);
        }

        public int KolicinaPodataka => _kes.Count;

        public void PisiKes()
        {
            foreach (KeyValuePair<K, V> par in _kes)
            {
                Console.Write($"Key: {par.Key}");
                if (par.Value != null)
                    Console.WriteLine(" Ima putanje");
                else
                    Console.WriteLine(" Nema putanje");
            }
        }
    }
}
