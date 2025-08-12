using Kes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kes
{
    public class LRUKes<K, V> : IKesable<K, V>
    {
        private readonly int _kapacitet;
        private readonly Dictionary<K, LinkedListNode<(K kljuc, V vrednost)>> _kes;
        private readonly LinkedList<(K kljuc, V vrednost)> _lruLista;
        private readonly object _lockObjekat;

        public LRUKes(int kapacitet)
        {
            if (kapacitet < 1)
                throw new ArgumentOutOfRangeException(nameof(kapacitet));
            _kapacitet = kapacitet;
            _kes = new Dictionary<K, LinkedListNode<(K, V)>>();
            _lruLista = new LinkedList<(K, V)>();
            _lockObjekat = new object();
        }

        public bool ProbajDaPribavisVrednost(K kljuc, out V vrednost)
        {
            lock (_lockObjekat)
            {
                if (_kes.TryGetValue(kljuc, out var cvor))
                {
                    _lruLista.Remove(cvor);
                    _lruLista.AddFirst(cvor);
                    vrednost = cvor.Value.vrednost;
                    return true;
                }
                vrednost = default!;
                return false;
            }
        }

        public void DodajIliAzuriraj(K kljuc, V vrednost)
        {
            lock (_lockObjekat)
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
                        var lru = _lruLista.Last!;
                        _kes.Remove(lru.Value.kljuc);
                        _lruLista.RemoveLast();
                    }
                }
            }
        }

        public bool IzbaciKljuc(K kljuc)
        {
            lock (_lockObjekat)
            {
                if (_kes.TryGetValue(kljuc, out var cvor))
                {
                    _kes.Remove(kljuc);
                    _lruLista.Remove(cvor);
                    return true;
                }
                return false;
            }
        }

        public void OcistiKes()
        {
            lock (_lockObjekat)
            {
                _lruLista.Clear();
                _kes.Clear();
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