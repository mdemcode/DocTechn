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
                if (_atest.IsNullOrEmpty()) UstawAtestWytopProton();
                return _atest;
            }
        }
        public string Wytop {
            get {
                if (_wytop.IsNullOrEmpty()) UstawAtestWytopProton();
                return _wytop;
            }
        }
        public override bool Hold => DaneDbProton.Any(d => d[17].Equals("HOLD")); // DaneDbAsprova.Any(d => d[16].Equals("HOLD"));
        public override bool Uwolniony => DaneDbProton.Any(d => d[17].Equals("UWOLNIONE")); // DaneDbAsprova.Any(d => d[16].Equals("UWOLNIONE"));
        public override StatusWykonania Status => _statusWyk ??= UstawStatusWykonania();
        //public override List<OperacjaAsprova> Operacje => _operacjeAsprova ??= WczytajOperacjeAsprova();
        public override List<OperacjaRozpProton> Operacje => _operacjeProton ??= WczytajOperacjeProton();

        // ASPROVA
        //  0       1       2         3         4         5         6         7         8         9         10        11         12         13         14         15         16
        // [atest] [wytop] [oper_10] [oper_20] [oper_30] [oper_40] [oper_50] [oper_60] [oper_70] [oper_80] [oper_90] [oper_100] [oper_110] [oper_120] [oper_130] [oper_140] [status_pozycji] ORDER BY id DESC
        //private IEnumerable<string[]> _daneDbAsprova;
        //public IEnumerable<string[]> DaneDbAsprova => _daneDbAsprova ??= WczytajDaneAsprova();
        //private List<OperacjaAsprova> _operacjeAsprova;

        // OPERACJE Z ROZPISKI PROTON
        // 0       1       2          3         4      5         6         7         8         9           10     11      12       13         14       15       16      17      18
        // [atest] [wytop] [gilot_dz] [prostow] [pily] [palniki] [przekaz] [wiertar] [obr_kra] [sp_blacho] [fazy] [laser] [plazma] [frezarka] [kooper] [montaz] [prasy] [cecha] [hold_opis]
        private IEnumerable<string[]> _daneDbProton;
        public IEnumerable<string[]> DaneDbProton => _daneDbProton ??= WczytajDaneProton();
        private List<OperacjaRozpProton> _operacjeProton;

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
        public KartaTechnDetal(ZleceniePLM zlecPLM, DetalPLM detal) : base($"{zlecPLM.Nr} | {detal.Grupa} | {detal.NrPrzewodnika}", zlecPLM) { // {zlecPLM.Kod:D3} {detal.NrPrzewodnika}
            Detal = detal;
            if (!Detal.DanePodstawoweWczytanePoprawnie) Bledy.Add("KD - Błąd wczytywania danych detalu!");
            _sztWyk = -1; // wszystkie szt.
        }

        public void ZmienAtestWytop(string nowyAtest, string nowyWytop) {
            string polecenieSQL = $"UPDATE det " +
                                  $"SET    Atest = '{nowyAtest}', Wytop = '{nowyWytop}' " +
                                  $"FROM   ROZ_DETALE AS det INNER JOIN " +
                                  $"       ROZ_POZYCJE_WKE AS lpON det.Pozycja_WKE_id = lp.Id INNER JOIN " +
                                  $"       ROZ_GRUPY AS gr ON lp.Grupa_id = gr.Id INNER JOIN " +
                                  $"       PROJ_NAGLOWKI_PROJEKTOW AS zl ON gr.Naglowek_Projektu_id = zl.Id " +
                                  $"WHERE  (det.Numer_karty_tech = '{NrPrzewodnika}') AND (gr.Numer_grupy = '{NrGr}') AND (zl.Numer_projektu = '{NrZlec}')";
            _ = SqlService.ZapiszDoBazyNowyLubZmiany(BazaDanych.Proton, polecenieSQL);
            _atest = nowyAtest;
            _wytop = nowyWytop;
        }
        // OPERACJE Z ROZPISKI PROTON
        // UWAGA! W tabeli ROZ_DETALE może być kilka pozycji tego samego detalu - dla każdej POZ_WW oddzielnie
        private IEnumerable<string[]> WczytajDaneProton() {
            string polecenieSQL = "SELECT d.Atest, d.Wytop, d.Gilotyny_dziurkarki_id, d.Prostowanie_id, d.Pily_id, d.Palniki_id, d.Przekazanie_id, d.Wiertarki_id, " + 
                                            "d.Obrobka_krawedzi_id, d.Spawanie_blachownic_id, d.Fazy_id, d.Laser_id, d.Plazma_id, d.Frezarka_id, d.Kooperacja_id, " +
                                            "d.Montaz_id, d.Prasy_id, d.Cecha_na_twardo_id, d.HOLD_opis " +
                                  "FROM PROJ_NAGLOWKI_PROJEKTOW AS zl INNER JOIN " +
                                        "ROZ_GRUPY AS gr ON zl.Id = gr.Naglowek_Projektu_id INNER JOIN " +
                                        "ROZ_POZYCJE_WKE AS lp ON gr.Id = lp.Grupa_id INNER JOIN " +
                                        "ROZ_DETALE AS d ON lp.Id = d.Pozycja_WKE_id " +
                                  $"WHERE (zl.Numer_projektu = '{NrZlec}') AND (gr.Numer_grupy = '{NrGr}') and Numer_karty_tech = '{NrPrzewodnika}'";
            IEnumerable<string[]> daneDB = SqlService.PobierzDaneZBazy(BazaDanych.Proton, polecenieSQL, 19, out string blad).ToList();
            if (blad.IsNullOrEmpty() && daneDB.Any()) return daneDB;
            Bledy.Add("Nie znalazłem detalu w rozpisce Proton!");
            return new List<string[]>();
        }
        private List<OperacjaRozpProton> WczytajOperacjeProton() {
            List<OperacjaRozpProton> operacjeProton = new ();
            if (!DaneDbProton.Any()) return operacjeProton;
            // 0       1       2          3         4      5         6         7         8         9           10     11      12       13         14       15       16      17
            // [atest] [wytop] [gilot_dz] [prostow] [pily] [palniki] [przekaz] [wiertar] [obr_kra] [sp_blacho] [fazy] [laser] [plazma] [frezarka] [kooper] [montaz] [prasy] [cecha]
            string[] daneOper = DaneDbProton.First();
            if (DaneDbProton.Any(d => d[2] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.GilotynyDziurkarki, DaneDbProton.First(d => d[2] != "{NULL}")[2].Trim()));
            }
            if (DaneDbProton.Any(d => d[3] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Prostowanie, DaneDbProton.First(d => d[3] != "{NULL}")[3].Trim()));
            }
            if (DaneDbProton.Any(d => d[4] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Pily, DaneDbProton.First(d => d[4] != "{NULL}")[4].Trim()));
            }
            if (DaneDbProton.Any(d => d[5] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Palniki, DaneDbProton.First(d => d[5] != "{NULL}")[5].Trim()));
            }
            if (DaneDbProton.Any(d => d[6] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Przekazanie, DaneDbProton.First(d => d[6] != "{NULL}")[6].Trim()));
            }
            if (DaneDbProton.Any(d => d[7] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Wiertarki, DaneDbProton.First(d => d[7] != "{NULL}")[7].Trim()));
            }
            if (DaneDbProton.Any(d => d[8] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.ObrobkaKrawedzi, DaneDbProton.First(d => d[8] != "{NULL}")[8].Trim()));
            }
            if (DaneDbProton.Any(d => d[9] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.SpawanieBlachownic, DaneDbProton.First(d => d[9] != "{NULL}")[9].Trim()));
            }
            if (DaneDbProton.Any(d => d[10] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Fazy, DaneDbProton.First(d => d[10] != "{NULL}")[10].Trim()));
            }
            if (DaneDbProton.Any(d => d[11] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Laser, DaneDbProton.First(d => d[11] != "{NULL}")[11].Trim()));
            }
            if (DaneDbProton.Any(d => d[12] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Plazma, DaneDbProton.First(d => d[10] != "{NULL}")[10].Trim()));
            }
            if (DaneDbProton.Any(d => d[13] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Frezarka, DaneDbProton.First(d => d[10] != "{NULL}")[10].Trim()));
            }
            if (DaneDbProton.Any(d => d[14] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Kooperacja, DaneDbProton.First(d => d[10] != "{NULL}")[10].Trim()));
            }
            if (DaneDbProton.Any(d => d[15] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.MontazDetali, DaneDbProton.First(d => d[10] != "{NULL}")[10].Trim()));
            }
            if (DaneDbProton.Any(d => d[16] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.Prasy, DaneDbProton.First(d => d[10] != "{NULL}")[10].Trim()));
            }
            if (DaneDbProton.Any(d => d[17] != "{NULL}")) {
                operacjeProton.Add(new OperacjaRozpProton(OperacjaRozpProton.TypOperacji.CehaNaTwardo, DaneDbProton.First(d => d[10] != "{NULL}")[10].Trim()));
            }
            return operacjeProton;
        }
        private void UstawAtestWytopProton() {
            if (!DaneDbProton.Any()) {
                _atest = "err";
                _wytop = "err";
                return;
            }
            string atest = DaneDbProton.All(d => d[0] == "{NULL}") ? DaneDbProton.First()[0] : DaneDbProton.First(d => d[0] != "{NULL}")[0];
            _atest = (atest.IsNullOrEmpty() || atest == "{NULL}") ? string.Empty : atest;
            string wytop = DaneDbProton.All(d => d[1] == "{NULL}") ? DaneDbProton.First()[1] : DaneDbProton.First(d => d[1] != "{NULL}")[1];
            _wytop = (wytop.IsNullOrEmpty() || wytop == "{NULL}") ? string.Empty : wytop;
        }
        private StatusWykonania UstawStatusWykonania() {
            if (Operacje is null || !Operacje.Any()) return StatusWykonania.Nieznany; // nie znalezione w rozpisceProton
            if (Operacje.All(o => o.Status == OperacjaRozpProton.StatusOperacji.DoWydania)) return StatusWykonania.DoWydania;
            if (Operacje.Any(o => o.Status == OperacjaRozpProton.StatusOperacji.Wydana)) return StatusWykonania.Wydany;
            if (Operacje.All(o => o.Status == OperacjaRozpProton.StatusOperacji.Wykonana)) return StatusWykonania.Wykonany;
            return StatusWykonania.Nieznany;
        }

        // ASPROVA
        //private IEnumerable<string[]> WczytajDaneAsprova() {
        //    IEnumerable<string[]> daneDB = SqlService.PobierzDaneZBazy(BazaDanych.Asprova,
        //                                                                "ut_eq_z1_rd_tech_arch",
        //                                                                new[] {"atest", "wytop", "oper_10", "oper_20", "oper_30", "oper_40", "oper_50", "oper_60", "oper_70", 
        //                                                                    "oper_80", "oper_90", "oper_100", "oper_110", "oper_120", "oper_130", "oper_140", "status_pozycji"},
        //                                                                $"zlecenie = '{NrZlec}' AND nr_karty_tch = '{NrPrzewodnika}' ORDER BY id DESC",
        //                                                                out string blad).ToList();
        //    if (blad.IsNullOrEmpty() && daneDB.Any()) return daneDB;
        //    Bledy.Add("Detal nie wyeksportowany do Asprova!");
        //    return new List<string[]>();
        //}
        //private List<OperacjaAsprova> WczytajOperacjeAsprova() { // DaneDbAsprova[2 - 15]
        //    List<OperacjaAsprova> operacjeAsprova = new ();
        //    if (!DaneDbAsprova.Any()) return operacjeAsprova;
        //    // [oper_10] [oper_20] [oper_30] [oper_40] [oper_50] [oper_60] [oper_70] [oper_80] [oper_90] [oper_100] [oper_110] [oper_120] [oper_130] [oper_140]
        //    string[] daneOper = DaneDbAsprova.First();
        //    if (!daneOper[2].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.GilotynyDziurkarki, daneOper[2].Trim())); // 10
        //    if (!daneOper[3].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Prostowanie, daneOper[3].Trim()));        // 20
        //    if (!daneOper[4].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Pily, daneOper[4].Trim()));               // 30
        //    if (!daneOper[5].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Palniki, daneOper[5].Trim()));            // 40
        //    if (!daneOper[6].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Przekazanie, daneOper[6].Trim()));        // 50
        //    if (!daneOper[7].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Wiertarki, daneOper[7].Trim()));          // 60
        //    if (!daneOper[8].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.ObrobkaKrawedzi, daneOper[8].Trim()));    // 70
        //    if (!daneOper[9].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.SpawanieBlachownic, daneOper[9].Trim())); // 80
        //    if (!daneOper[10].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Fazy, daneOper[10].Trim()));             // 90
        //    if (!daneOper[11].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Laser, daneOper[11].Trim()));            // 100
        //    if (!daneOper[12].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Plazma, daneOper[12].Trim()));           // 110
        //    if (!daneOper[13].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Frezarka, daneOper[13].Trim()));         // 120
        //    if (!daneOper[14].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Kooperacja, daneOper[14].Trim()));       // 130
        //    if (!daneOper[15].Trim().IsNullOrEmpty()) operacjeAsprova.Add(new OperacjaAsprova(TypOperacji.Montaz, daneOper[15].Trim()));           // 140
        //    return operacjeAsprova;
        //}
        //private void UstawAtestWytop() {
        //    if (!DaneDbAsprova.Any()) {
        //        _atest = "err";
        //        _wytop = "err";
        //        return;
        //    }
        //    string[] daneFinal = DaneDbAsprova.First();
        //    _atest = (daneFinal[0].IsNullOrEmpty() || daneFinal[0] == "{NULL}") ? "[nie wpisano]" : daneFinal[0];
        //    _wytop = (daneFinal[1].IsNullOrEmpty() || daneFinal[1] == "{NULL}") ? "[nie wpisano]" : daneFinal[1];
        //}
        //private StatusWykonania UstawStatusWykonania() {
        //    if (Operacje is null || !Operacje.Any()) return StatusWykonania.Nieznany; // nie zaimportowane do Asprovy
        //    if (Operacje.All(o => o.Status == StatusOperacji.DoWydania)) return StatusWykonania.DoWydania;
        //    if (Operacje.Any(o => o.Status == StatusOperacji.Wydana)) return StatusWykonania.Wydany;
        //    if (Operacje.All(o => o.Status == StatusOperacji.Wykonana)) return StatusWykonania.Wykonany;
        //    return StatusWykonania.Nieznany;
        //}

    }
}
