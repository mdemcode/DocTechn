using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BasicSqlService;
using ModelPLM;

namespace DocTechn.KartyTechnologiczne
{
    public class KartaTechnMontaz : KartaTechnologiczna {

        public override string NrGr { get; }
        public override string Lp { get; }
        //private string LpWgKoduProton => UtworzLpProton(); // zmienione na zwykłe Lp
        public override bool Hold { get; }
        public override bool Uwolniony { get; }
        public override StatusWykonania Status { get; }
        public override int Szt { get; }
        //public override List<OperacjaAsprova> Operacje { get; }
        public override List<OperacjaRozpProton> Operacje { get; }
        public override string ToolTipText => $"Uwagi:\n{WczytaneUwagi}\n{DodanaUwaga}\nWszystkie sztuki elementu: {Szt}\n{_alertErrInfo}";
        //
        public override string ToString() => $"{NrZlec} | {NrGr} | {Lp} | {SztWykTxt}";

        public KartaTechnMontaz(string tekstKoduKresk, ZleceniePLM zlecPLM) : base(tekstKoduKresk, zlecPLM) {
            string[] kodKrSplit = tekstKoduKresk.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (kodKrSplit.Length < 3) {
                Bledy.Add("KM - Błędny kod kreskowy");
                return;
            }
            _sztWyk                    = -1;                                // wszystkie szt.
            Lp                         = kodKrSplit[kodKrSplit.Length - 1]; // ostatnia pozycja tablicy
            NrGr                       = OkreslNrGr(kodKrSplit);
            if (WczytajDaneProton(out string[] daneProton)) {
                Szt       = daneProton[0].IsNullOrEmpty() || daneProton[0].Equals("{NULL}") ? -1 : int.Parse(daneProton[0]);
                Hold      = false; //!daneAsprova[1].IsNullOrEmpty() && daneAsprova[1].Equals("HOLD");
                Uwolniony = true; //!daneAsprova[1].IsNullOrEmpty() && daneAsprova[1].Equals("UWOLNIONE");
                Operacje = new List<OperacjaRozpProton> {
                    new(OperacjaRozpProton.TypOperacji.SkladaniePozycji, daneProton[1]),
                    new(OperacjaRozpProton.TypOperacji.SpawaniePozycji, daneProton[2]),
                    new(OperacjaRozpProton.TypOperacji.FrezowaniePozycji, daneProton[3])
                };
                Status = UstawStatusWykonania(daneProton[1], daneProton[2], daneProton[3]);
            }
            else Bledy.Add("KM - Błąd wczytywania danych z rozpiski Proton!");
        }

        private bool WczytajDaneProton(out string[] daneOut) {
            daneOut = null;
            string polecenieSQL = "SELECT lp.Ilosc_szt, lp.Skladania_WKE_id, lp.Spawanie_WKE_id, lp.Frezowanie_WKE_id " +
                                  "FROM ROZ_POZYCJE_WKE AS lp INNER JOIN " +
                                        "ROZ_GRUPY AS g ON lp.Grupa_id = g.Id INNER JOIN " +
                                        "PROJ_NAGLOWKI_PROJEKTOW AS p ON g.Naglowek_Projektu_id = p.Id " +
                                  $"WHERE (p.Numer_projektu = '{NrZlec}') AND (g.Numer_grupy = '{NrGr}') AND (lp.Nr_pozycji_WW = '{Lp}')";
            IEnumerable<string[]> daneDB = SqlService.PobierzDaneZBazy(BazaDanych.Proton, polecenieSQL, 4, out string blad).ToList();
            bool                  ok     = daneDB.Any();
            if (ok) daneOut = daneDB.First();
            return ok; //true;
        }

        /// <summary> { szt, hold, składanie, spawanie, frezarka } </summary>
        //private bool WczytajDaneAsprovy(out string[] daneOut) {
        //    daneOut = null;
        //    bool ok = SqlService.PobierzPojedynczyWiersz(BazaDanych.Asprova,
        //                                                 "ut_eq_z1_rm_tech_arch",
        //                                                 new[] {"ilosc", "status_pozycji", "oper_10", "oper_20", "oper_30"},
        //                                                 $"zlecenie = '{NrZlec}' AND grupa = '{NrGr}' AND nrpoz_wys = '{Lp}'",
        //                                                 out string[] elementDB);
        //    if (ok) daneOut = elementDB;
        //    return ok; //true;
        //}

        private static string OkreslNrGr(string[] kodKrSplit) {
            string grTmp = kodKrSplit[1];
            // Bug !!! skaner nie czyta '_' ("podłogi") - zamienia na spację. Poniżej tymczasowe rozwiązanie problemu
            if (kodKrSplit.Length > 3) {
                for (int u = 2; u < kodKrSplit.Length - 1; u++) {
                    grTmp += $"_{kodKrSplit[u]}";
                }
            }
            // Bug ^^^
            return grTmp;
        }
        private static StatusWykonania UstawStatusWykonania(string op10, string op20, string op30) {
            return (op10, op20, op30) switch {
                ("V", "V", _) => StatusWykonania.DoWydania,
                (_, "V", _) => StatusWykonania.Wydany,
                ("W", "W", "W") => StatusWykonania.Wykonany,
                ("W", "W", _) => StatusWykonania.Pospawany,
                ("W", _, _) => StatusWykonania.Zlozony,
                (_, _, _) => StatusWykonania.Nieznany
            };
        }
        //private string UtworzLpProton() {
        //    string zlecPart = NrZlec.Substring(0, 8);
        //    string lpPart   = Lp.DopiszZeraWiodace();
        //    return $"{zlecPart}/{lpPart}";
        //}
    }
}
