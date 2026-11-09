using BasicSqlService;
using ModelPLM;
using System.Linq;
using System;

namespace DocTechn.KartyRozkrojow {

    public class KartaRozkroju : DokumentTechnologiczny {

        public KartaRozkroju(string tekstKoduKresk) : base(tekstKoduKresk) {
            int idRozkr = OdczytajIdZKoduKresk(tekstKoduKresk);
            if (idRozkr == -1) return;
            Rozkroj = new RozkrojPLM(idRozkr);
            if (!Rozkroj.RozkrojWczytanyPoprawnie) Bledy.Add($"Błąd wczytywania rozkroju: {tekstKoduKresk}");
        }
        
        public RozkrojPLM Rozkroj { get; private set; }

        private int OdczytajIdZKoduKresk(string kodKresk) {
            try {
                if (!kodKresk.StartsWith("*MB") && !kodKresk.StartsWith("*MT") && !kodKresk.StartsWith("MB") && !kodKresk.StartsWith("MT"))
                    throw new Exception($"Błąd wczytywania rozkroju - błędny kod kresk.: {kodKresk}");
                string nesBonTxt  = kodKresk.Substring(3, 5);
                string barIdtTxt = kodKresk.Substring(8, 3);
                if (!int.TryParse(nesBonTxt, out int nesBon) | !int.TryParse(barIdtTxt, out int barIdt))
                    throw new Exception($"Błąd wczytywania rozkroju - błędny kod kresk.: {kodKresk}");
                // zmiana z dn. 06.10.2026 r.
                int nesId = SqlService.PobierzDaneZBazy(BazaDanych.Plm, "NESTING", ["NES_ID"], $"NES_BON={Math.Abs(nesBon)}", out string blad1).Max(x => int.Parse(x[0]));
                if (!blad1.IsNullOrEmpty())
                    throw new Exception($"Błąd wczytywania rozkroju {kodKresk} [{blad1}]");
                //string nesIdTxt = SqlService.PobierzPojedynczyString(BazaDanych.Plm, "NESTING", "NES_ID", $"NES_Bon={nesBon}", out string blad1);
                //if (!blad1.IsNullOrEmpty() || !int.TryParse(nesIdTxt, out int nesId)) {
                //    Bledy.Add($"Błąd wczytywania rozkroju {kodKresk} [{blad1}]");
                //    return -1;
                //}
                string barId = SqlService.PobierzPojedynczyString(BazaDanych.Plm, "NESTBAR", "BAR_ID", $"NES_ID={nesId} AND BAR_IDT={barIdt}", out string blad2);
                if (!blad2.IsNullOrEmpty())
                    throw new Exception($"Błąd wczytywania rozkroju {kodKresk} [{blad2}]");
                return int.TryParse(barId, out int id) ? id : throw new Exception($"Błąd wczytywania rozkroju {kodKresk} - błędne BAR_ID"); // raczej niemożliwe żeby BAR_ID był nieprawidłowy w bazie
            }
            catch (Exception e) {
                Bledy.Add(e.Message);
                return -1;
            }
        }
    }
}
