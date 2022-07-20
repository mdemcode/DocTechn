using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BasicSqlService;
using ModelPLM;

namespace DocTechn.KartyTechnologiczne
{
    public class KartaTechnDetal : KartaTechnologiczna {

        private string _atest;
        private string _wytop;
        private StatusWykonania? _statusWyk;
        //
        public DetalPLM Detal { get; }
        public string NrPrzewodnika => Detal.NrPrzewodnika;
        public bool Luzem => Detal.Luzem;
        // override
        public override string NrGr => Detal.Grupa;
        public override string Lp => Detal.Lp;
        public override int Szt => Detal.Szt;
        //
        public string Atest {
            get {
                if (_atest.IsNullOrEmpty()) UstawAtestWytop(); // '[nie wpisano atestu]' jeśli null or empty w DB
                return _atest;
            }
        }
        public string Wytop {
            get {
                if (_wytop.IsNullOrEmpty()) UstawAtestWytop(); // '[nie wpisano wytopu]' jeśli null or empty w DB
                return _wytop;
            }
        }
        public override bool Hold => DaneDbAsprova.Any(d => d[16].Equals("HOLD"));
        public override bool Uwolniony => DaneDbAsprova.Any(d => d[16].Equals("UWOLNIONE"));
        public override StatusWykonania Status => _statusWyk ??= UstawStatusWykonania();
        public override List<OperacjaAsprova> Operacje => _operacjeAsprova ??= WczytajOperacjeAsprova();
        // ASPROVA
        //  0       1       2         3         4         5         6         7         8         9         10        11         12         13         14         15         16
        // [atest] [wytop] [oper_10] [oper_20] [oper_30] [oper_40] [oper_50] [oper_60] [oper_70] [oper_80] [oper_90] [oper_100] [oper_110] [oper_120] [oper_130] [oper_140] [status_pozycji] ORDER BY id DESC
        private IEnumerable<string[]> _daneDbAsprova;
        public IEnumerable<string[]> DaneDbAsprova => _daneDbAsprova ??= WczytajDaneAsprova();
        private List<OperacjaAsprova> _operacjeAsprova;
        //
        public override string ToolTipText => $"Miejsce składowania: {MiejsceSkladowania}\nUwagi:\n{WczytaneUwagi}\n{DodanaUwaga}\nWszystkie sztuki detalu: {Szt}\n{_alertErrInfo}";
        //
        //public bool ZmianaTechnologii { get; set; } = false;

        public override string ToString() {
            string gr = NrGr.Length > 25 ? $"{NrGr.Substring(0, 22)}..." : NrGr;
            return $"{NrZlec} | {gr} | {NrPrzewodnika} | {SztWykTxt} | At. {Atest} | Wyt. {Wytop}";
        }

        /// <summary> Konstruktor tylko do celów pomocniczych (nie ustawia Detalu!) </summary>
        public KartaTechnDetal(string tekstKoduKresk, ZleceniePLM zlecPLM) : base(tekstKoduKresk, zlecPLM) { }
        public KartaTechnDetal(ZleceniePLM zlecPLM, DetalPLM detal) : base($"{zlecPLM.Kod:D3} {detal.NrPrzewodnika}", zlecPLM) {
            Detal = detal;
            if (!Detal.DanePodstawoweWczytanePoprawnie) Bledy.Add("KD - Błąd wczytywania danych detalu!");
            _sztWyk = -1; // wszystkie szt.
        }


        public void ZmienAtestWytop(string nowyAtest, string nowyWytop) {
            _atest = nowyAtest;
            _wytop = nowyWytop;
        }
        // ASPROVA
        private IEnumerable<string[]> WczytajDaneAsprova() {
            IEnumerable<string[]> daneDB = SqlService.PobierzDaneZBazy(BazaDanych.Asprova,
                                                                        "ut_eq_z1_rd_tech_arch",
                                                                        new[] {"atest", "wytop", "oper_10", "oper_20", "oper_30", "oper_40", "oper_50", "oper_60", "oper_70", 
                                                                            "oper_80", "oper_90", "oper_100", "oper_110", "oper_120", "oper_130", "oper_140", "status_pozycji"},
                                                                        $"zlecenie = '{NrZlec}' AND nr_karty_tch = '{NrPrzewodnika}' ORDER BY id DESC",
                                                                        out string blad).ToList();
            if (!blad.IsNullOrEmpty() || !daneDB.Any()) return new List<string[]>();
            return daneDB;
        }
        private List<OperacjaAsprova> WczytajOperacjeAsprova() { // DaneDbAsprova[2 - 15]
            List<OperacjaAsprova> operacjeAsprova = new ();
            // [oper_10] [oper_20] [oper_30] [oper_40] [oper_50] [oper_60] [oper_70] [oper_80] [oper_90] [oper_100] [oper_110] [oper_120] [oper_130] [oper_140]
            string[] daneOper = DaneDbAsprova.First();
            if (!daneOper[2].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.GilotynyDziurkarki, daneOper[2].Trim())); // 10
            if (!daneOper[3].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Prostowanie, daneOper[3].Trim()));        // 20
            if (!daneOper[4].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Pily, daneOper[4].Trim()));               // 30
            if (!daneOper[5].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Palniki, daneOper[5].Trim()));            // 40
            if (!daneOper[6].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Przekazanie, daneOper[6].Trim()));        // 50
            if (!daneOper[7].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Wiertarki, daneOper[7].Trim()));          // 60
            if (!daneOper[8].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.ObrobkaKrawedzi, daneOper[8].Trim()));    // 70
            if (!daneOper[9].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.SpawanieBlachownic, daneOper[9].Trim())); // 80
            if (!daneOper[10].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Fazy, daneOper[10].Trim()));             // 90
            if (!daneOper[11].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Laser, daneOper[11].Trim()));            // 100
            if (!daneOper[12].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Plazma, daneOper[12].Trim()));           // 110
            if (!daneOper[13].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Frezarka, daneOper[13].Trim()));         // 120
            if (!daneOper[14].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Kooperacja, daneOper[14].Trim()));       // 130
            if (!daneOper[15].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Montaz, daneOper[15].Trim()));           // 140
            return operacjeAsprova;
        }
        private void UstawAtestWytop() {
            string[] daneFinal = DaneDbAsprova.First();
            _atest = (daneFinal[0].IsNullOrEmpty() || daneFinal[0] == "{NULL}") ? "[nie wpisano atestu]" : daneFinal[0];
            _wytop = (daneFinal[1].IsNullOrEmpty() || daneFinal[1] == "{NULL}") ? "[nie wpisano wytopu]" : daneFinal[1];
        }
        private StatusWykonania UstawStatusWykonania() {
            if (Operacje is null || !Operacje.Any()) return StatusWykonania.Nieznany; // nie zaimportowane do Asprovy
            if (Operacje.All(o => o.Status == StatusOperacji.DoWydania)) return StatusWykonania.DoWydania;
            if (Operacje.Any(o => o.Status == StatusOperacji.Wydana)) return StatusWykonania.Wydany;
            if (Operacje.All(o => o.Status == StatusOperacji.Wykonana)) return StatusWykonania.Wykonany;
            return StatusWykonania.Nieznany;
        }

        //protected override bool OdczytajDaneWgKoduKresk(string kodKresk) {
        //    throw new NotImplementedException();
        //}
    }
}
