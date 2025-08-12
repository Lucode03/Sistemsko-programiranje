using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kes
{
    public interface IKesable<K, V>
    {
        void DodajIliAzuriraj(K kljuc, V vrednost);
        bool ProbajDaPribavisVrednost(K kljuc, out V vrednost);
        bool IzbaciKljuc(K kljuc);
        void OcistiKes();
        int KolicinaPodataka { get; }
    }
}
