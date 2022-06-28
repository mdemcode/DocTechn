using PrzewodnikPLM.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocTechn.KartyTechnologiczne
{
    public abstract class KartaTechnologiczna : DokumentTechnologiczny {

        protected readonly ZleceniePLM _zlecenie;
        public int IdZlecDB => _zlecenie.IdDB;
        public string NrZlec => _zlecenie.Nr;
        public int KodZlec => _zlecenie.Kod;
        public string Sekcja => _zlecenie.Sekcja;
        //
        public abstract string NrGr { get; }
        public abstract string Lp { get; }
        public abstract bool Hold { get; }
        public abstract bool Uwolniony { get; }
        public abstract StatusWykonania Status { get; }
        /// <summary> Ilość sztuk detalu (przewodnik) lub elementu (karta montaż) </summary>
        public abstract int Szt { get; }
        public abstract List<OperacjaAsprova> Operacje { get; }
        public abstract string Uwagi { get; }
        public abstract string ToolTipText { get; }

        public KartaTechnologiczna(string tekstKoduKresk) : base(tekstKoduKresk) { }

    }
}
        ////

        //protected KartaTechnologiczna(ZleceniePLM zlecPLM) {
        //    if (zlecPLM is null || !zlecPLM.ZlecenieWczytanePopr) Bledy.Add("Błąd wczytywania zlecenia!");
        //    _zlecenie = zlecPLM;
        //}


    public enum StatusWykonania {           // kombinacje dla montażu:
        Nieznany,
        DoWydania,                          // 'V' / 'V'
        Wydany, // w trakcie wykonywania    // bryg / 'V'
        Zlozony, // tylko montaż            // 'W' / bryg
        Pospawany, // tylko montaż          // 'W' / 'W'
        Wykonany                            // po frezarce ??
    }
