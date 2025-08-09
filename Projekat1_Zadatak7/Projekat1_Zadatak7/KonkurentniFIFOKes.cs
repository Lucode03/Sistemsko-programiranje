using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kes
{
    internal class KonkurentniFIFOKes<K, V>: IKesable<K, V>
    {
        private readonly int _kapacitet;
        private readonly ConcurrentDictionary<K, V> _kes;
        private readonly ConcurrentQueue<K> _fifoRed;
        private readonly object _lockObjekat;

        public KonkurentniFIFOKes(int kapacitet)
        {
            if (kapacitet < 1)
                throw new ArgumentOutOfRangeException(nameof(kapacitet));
            _kapacitet = kapacitet;
            _kes = new ConcurrentDictionary<K, V>();
            _fifoRed = new ConcurrentQueue<K>();
            _lockObjekat = new object();
        }
        // Posto se moze desiti da se menjaju i kes i fifoRed, ne mozemo da se
        // Oslanjamo na konkurentne strukture same po sebi
        // One ce da obezbede konkurentan pristup samoj strukturi, ali ne i obema
        // strukturama istovremeno, pa zato ovde koristimo lock
        public void DodajIliAzuriraj(K kljuc, V vrednost)
        {
            lock(_lockObjekat)
            {
                if (_kes.ContainsKey(kljuc))
                {
                    _kes[kljuc] = vrednost;
                }
                else
                {
                    _kes[kljuc] = vrednost;
                    _fifoRed.Enqueue(kljuc);

                    while (_kes.Count > _kapacitet && _fifoRed.TryDequeue(out var najstariji))
                    {
                        _kes.Remove(najstariji, out var staraVrednost);
                    }
                }
            }
        }

        public bool IzbaciKljuc(K kljuc)
        {
            // U ovoj implementaciji se ne izbacuje kluc na ovaj nacin
            // Jedino kako moze da se izbaci kljuc jeste ako dodje do zamene kada se 
            // Popuni kapacitet read
            return false;
        }

        public void OcistiKes()
        {
            lock (_lockObjekat)
            {
                _kes.Clear();
                _fifoRed.Clear();
            }
        }

        public bool ProbajDaPribavisVrednost(K kljuc, out V vrednost)
        {
            return _kes.TryGetValue(kljuc, out vrednost);
        }

        public int KolicinaPodataka => _kes.Count;
    }
}
