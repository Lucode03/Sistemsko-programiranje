using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kes
{
    internal class OptimizovanLRUKes2<K, V>: IKesable<K, V>
    {
        private readonly int _kapacitet;
        private readonly Dictionary<K, LinkedListNode<(K kljuc, V vrednost)>> _kes;
        private readonly LinkedList<(K kljuc, V vrednost)> _lruLista;
        private readonly ReaderWriterLockSlim _lock;
        private readonly int _brojZaIzbacivanje;

        public OptimizovanLRUKes2(int kapacitet, int brojZaIzbacivanje)
        {
            if (kapacitet < 1)
                throw new ArgumentOutOfRangeException(nameof(kapacitet));
            _kapacitet = kapacitet;
            _kes = new Dictionary<K, LinkedListNode<(K, V)>>();
            _lruLista = new LinkedList<(K, V)>();
            _lock = new ReaderWriterLockSlim();
            if (brojZaIzbacivanje < 1 || brojZaIzbacivanje > kapacitet)
                throw new ArgumentOutOfRangeException(nameof(brojZaIzbacivanje));
            _brojZaIzbacivanje = brojZaIzbacivanje;
        }

        // Ne moze samo klasican ReadLock zato sto ukoliko vrednost postoji,
        // ona mora da se postavi na pocetak liste jer je najskorije koriscena.
        // Zato se koristi ova verzija sa Upgradeable
        public bool ProbajDaPribavisVrednost(K kljuc, out V vrednost)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_kes.TryGetValue(kljuc, out var cvor))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _lruLista.Remove(cvor);
                        _lruLista.AddLast(cvor);
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                    vrednost = cvor.Value.vrednost;
                    return true;
                }
                vrednost = default!;
                return false;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void DodajIliAzuriraj(K kljuc, V vrednost)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_kes.TryGetValue(kljuc, out var postojeci))
                {
                    _lruLista.Remove(postojeci);
                    postojeci.Value = (kljuc, vrednost);
                    _lruLista.AddFirst(postojeci);
                }
                else
                {
                    var cvor = new LinkedListNode<(K, V)>((kljuc, vrednost));
                    _lruLista.AddFirst(cvor);
                    _kes.Add(kljuc, cvor);

                    if (_kes.Count > _kapacitet)
                    {
                        IzbaciElemente(_brojZaIzbacivanje);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void IzbaciElemente(int brojElemenata)
        {
            while (brojElemenata-- > 0 && _lruLista.Last != null)
            {
                var lru = _lruLista.Last!;
                _kes.Remove(lru.Value.kljuc);
                _lruLista.RemoveLast();
            }
        }

        public bool IzbaciKljuc(K kljuc)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_kes.TryGetValue(kljuc, out var cvor))
                {
                    _kes.Remove(kljuc);
                    _lruLista.Remove(cvor);
                    return true;
                }
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void OcistiKes()
        {
            _lock.EnterWriteLock();
            try
            {
                _lruLista.Clear();
                _kes.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public int KolicinaPodataka
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _kes.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public void PisiKes()
        {
            foreach (KeyValuePair<K, LinkedListNode<(K kljuc, V vrednost)>> par in _kes)
            {
                Console.Write($"Key: {par.Key}");
                if (par.Value.Value.vrednost != null)
                    Console.WriteLine(" Ima putanje");
                else
                    Console.WriteLine(" Nema putanje");
            }
        }
    }
}
