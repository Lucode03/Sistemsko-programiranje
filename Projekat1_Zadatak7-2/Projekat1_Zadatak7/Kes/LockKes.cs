using Kes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kes
{
    internal class LockKes<K, V> : IKesable<K, V>
    {
        private readonly Dictionary<K, V> _kes = new();
        private readonly object _lockObjekat = new();

        public void DodajIliAzuriraj(K kljuc, V vrednost)
        {
            lock (_lockObjekat)
            {
                _kes[kljuc] = vrednost;
            }
        }

        public bool IzbaciKljuc(K kljuc)
        {
            lock (_lockObjekat)
            {
                return _kes.Remove(kljuc);
            }
        }

        public void OcistiKes()
        {
            lock (_lockObjekat)
            {
                _kes.Clear();
            }
        }

        public bool ProbajDaPribavisVrednost(K kljuc, out V vrednost)
        {
            lock (_lockObjekat)
            {
                return _kes.TryGetValue(kljuc, out vrednost);
            }
        }

        public int KolicinaPodataka
        {
            get
            {
                lock (_lockObjekat)
                {
                    return _kes.Count;
                }
            }
        }
    }
}
