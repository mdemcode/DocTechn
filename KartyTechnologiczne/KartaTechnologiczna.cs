using BasicSqlService;
using ModelPLM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocTechn.KartyTechnologiczne
{
    public abstract partial class KartaTechnologiczna : DokumentTechnologiczny {

        // ZLECENIE
        protected readonly ZleceniePLM _zlecenie;
        public int IdZlecDB => _zlecenie.IdDB;
        public string NrZlec => _zlecenie.Nr;
        public int KodZlec => _zlecenie.Kod;
        public string Sekcja => _zlecenie.Sekcja;
        
        // ABSTRACT
        public abstract string NrGr { get; }
        public abstract string Lp { get; }
        public abstract bool Hold { get; }
        public abstract bool Uwolniony { get; }
        public abstract StatusWykonania Status { get; }
        /// <summary> Ilość sztuk detalu (przewodnik) lub elementu (karta montaż) </summary>
        public abstract int Szt { get; }
        public abstract List<OperacjaAsprova> Operacje { get; }
        public abstract string ToolTipText { get; }
        //
        protected int _sztWyk;
        protected string _alertErrInfo;
        protected string _uwagi;
        //
        public string Uwagi => _uwagi;
        public int SztWyk => _sztWyk;
        public string SztWykTxt => _sztWyk == -1 ? $"{Szt} szt. [wszystkie]" : $"{_sztWyk} szt.";
        public bool Alert { get; private set; }
        public bool Error { get; private set; }
        public string AlertErrInfo => _alertErrInfo;
        public string MiejsceSkladowania { get; protected set; }
        //


        public KartaTechnologiczna(string tekstKoduKresk, ZleceniePLM zlecPLM) : base(tekstKoduKresk) {
            if (zlecPLM is null || !zlecPLM.ZlecenieWczytanePopr) Bledy.Add("Błąd wczytywania zlecenia!");
            _zlecenie = zlecPLM;
        }

        public bool ZmienSztWyk(int szt) {
            if (szt > Szt || szt < 1) return false;
            _sztWyk = szt == Szt ? -1 : szt; // szt == Szt -> wszystkie sztuki
            return true;
        }

        /// <summary> Ustawia odpowiednią flagę i dopisuje 'infoTxt' do _alertErrInfo </summary>
        /// <param name="error_alert"> true - jeśli error; false jeśli alert </param>
        public void DodajAlertErrInfo(string infoTxt, bool error_alert) {
            if (!Error) { // jeśli już jest ustawiony Error to nie zmieniaj!
                Error = error_alert;
                Alert = !error_alert;
            }
            if (_alertErrInfo.IsNullOrEmpty()) _alertErrInfo = "Błędy i ostrzeżenia:";
            _alertErrInfo += $"\n{infoTxt}";
        }

        /// <summary> Ustawia flagi Alert i Error na false, a pole _alertErrInfo na string.Empty </summary>
        public void WyczyscAlertErr() {
            Alert         = false;
            Error         = false;
            _alertErrInfo = string.Empty;
        }
        public void ZmienUwagi(string noweUwagi) {
            _uwagi = noweUwagi;
        }
        public void ZmienMiejsceSklad(string noweMiejsce) {
            MiejsceSkladowania = noweMiejsce;
        }

    }

    public enum StatusWykonania {           // kombinacje dla montażu:
        Nieznany,
        DoWydania,                          // 'V' / 'V'
        Wydany, // w trakcie wykonywania    // bryg / 'V'
        Zlozony, // tylko montaż            // 'W' / bryg
        Pospawany, // tylko montaż          // 'W' / 'W'
        Wykonany                            // po frezarce ??
    }
}
