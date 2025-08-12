using Kes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kes
{
    internal class ReaderWriterKes<K, V> : IKesable<K, V>
    {
        private readonly Dictionary<K, V> _kes = new();
        private readonly ReaderWriterLockSlim _lockObjekat = new();

        public void DodajIliAzuriraj(K kljuc, V vrednost)
        {
            _lockObjekat.EnterWriteLock();
            try
            {
                _kes[kljuc] = vrednost;
            }
            finally
            {
                _lockObjekat.ExitWriteLock();
            }
        }

        public bool IzbaciKljuc(K kljuc)
        {
            _lockObjekat.EnterWriteLock();
            try
            {
                return _kes.Remove(kljuc);
            }
            finally
            {
                _lockObjekat.ExitWriteLock();
            }
        }

        public void OcistiKes()
        {
            _lockObjekat.EnterWriteLock();
            try
            {
                _kes.Clear();
            }
            finally
            {
                _lockObjekat.ExitWriteLock();
            }
        }

        public bool ProbajDaPribavisVrednost(K kljuc, out V vrednost)
        {
            _lockObjekat.EnterReadLock();
            try
            {
                return _kes.TryGetValue(kljuc, out vrednost);
            }
            finally
            {
                _lockObjekat.ExitReadLock();
            }
        }

        public int KolicinaPodataka
        {
            get
            {
                _lockObjekat.EnterReadLock();
                try
                {
                    return _kes.Count;
                }
                finally
                {
                    _lockObjekat.ExitReadLock();
                }
            }
        }
    }
}
