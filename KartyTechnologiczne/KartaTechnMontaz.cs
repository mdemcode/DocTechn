using System;
using System.Collections.Generic;
using System.Globalization;
using BasicSqlService;
using ModelPLM;

namespace DocTechn.KartyTechnologiczne
{
    public class KartaTechnMontaz : KartaTechnologiczna {

        public override string NrGr { get; }
        public override string Lp { get; }
        public override bool Hold { get; }
        public override bool Uwolniony { get; }
        public override StatusWykonania Status { get; }
        public override int Szt { get; }
        public override List<OperacjaAsprova> Operacje { get; }
        public override string ToolTipText => $"Uwagi: {Uwagi}\nWszystkie sztuki elementu: {Szt}\n{_alertErrInfo}";
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
            if (WczytajDaneZAsprovy(out string[] daneAsprova)) {
                Szt       = (daneAsprova[0].IsNullOrEmpty() || daneAsprova[0].Equals("{NULL}")) ? -1 : int.Parse(daneAsprova[0], NumberStyles.AllowDecimalPoint);
                Hold      = !daneAsprova[1].IsNullOrEmpty() && daneAsprova[1].Equals("HOLD");
                Uwolniony = !daneAsprova[1].IsNullOrEmpty() && daneAsprova[1].Equals("UWOLNIONE");
                Operacje = new List<OperacjaAsprova> {
                    new(TypOperacji.Montaz, daneAsprova[2]),
                    new(TypOperacji.Spawanie, daneAsprova[3]),
                    new(TypOperacji.Frezarka, daneAsprova[4])
                };
                Status = UstawStatusWykonania(daneAsprova[2], daneAsprova[3], daneAsprova[4]);
            }
            else Bledy.Add("KM - Błąd wczytywania danych z Asprovy!");
        }

        /// <summary> { szt, hold, składanie, spawanie, frezarka } </summary>
        private bool WczytajDaneZAsprovy(out string[] daneOut) {
            daneOut = null;
            bool ok = SqlService.PobierzPojedynczyWiersz(BazaDanych.Asprova,
                                                         "ut_eq_z1_rm_tech_arch",
                                                         new[] {"ilosc", "status_pozycji", "oper_10", "oper_20", "oper_30"},
                                                         $"zlecenie = '{NrZlec}' AND grupa = '{NrGr}' AND nrpoz_wys = '{Lp}'",
                                                         out string[] elementDB);
            if (ok) daneOut = elementDB;
            return ok; //true;
        }
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

        //protected override bool OdczytajDaneWgKoduKresk(string kodKresk) {
        //    throw new NotImplementedException();
        //}
    }
}
