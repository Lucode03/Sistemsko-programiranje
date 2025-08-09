using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kes
{
    internal class FIFOKes<K, V> : IKesable<K, V>
    {
        private readonly int _kapacitet;
        private readonly Dictionary<K, V> _kes;
        private readonly Queue<K> _fifoRed;
        private readonly object _lockObjekat;

        public FIFOKes(int kapacitet)
        {
            if (kapacitet < 1)
                throw new ArgumentOutOfRangeException(nameof(kapacitet));
            _kapacitet = kapacitet;
            _kes = new Dictionary<K, V>();
            _fifoRed = new Queue<K>();
            _lockObjekat = new object();
        }
        public void DodajIliAzuriraj(K kljuc, V vrednost)
        {
            lock (_lockObjekat)
            {
                if (_kes.ContainsKey(kljuc))
                {
                    _kes[kljuc] = vrednost;
                }
                else
                {
                    _kes[kljuc] = vrednost;
                    _fifoRed.Enqueue(kljuc);

                    if (_kes.Count > _kapacitet)
                    {
                        var najstariji = _fifoRed.Dequeue();
                        _kes.Remove(najstariji);
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
                _fifoRed.Clear();
                _kes.Clear();
            }
        }

        public bool ProbajDaPribavisVrednost(K kljuc, out V vrednost)
        {
            lock (_lockObjekat)
            {
                if (_kes.TryGetValue(kljuc, out vrednost))
                {
                    return true;
                }
                vrednost = default!;
                return false;
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
