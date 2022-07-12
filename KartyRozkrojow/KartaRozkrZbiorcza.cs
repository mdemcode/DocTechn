using ModelPLM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocTechn.KartyRozkrojow
{
    /// <summary> Karta rozkroju zbiorcza </summary>
    public class KartaRozkrZbiorcza : KartaRozkroj {

        public NestingPLM Nesting { get; } // >> tu jest lista rozkrojów i detali Nestingu

        public override string ToString() => $"Karta zbiorcza rozkrojów nr: {KodKreskowyTxt}";


        public KartaRozkrZbiorcza(string tekstKoduKresk) : base(tekstKoduKresk) {
            int nesId = OdczytajIdZKoduKresk(tekstKoduKresk);
            Nesting = new NestingPLM(nesId);
            if (!Nesting.RozkrojeWczytanePoprawnie) Bledy.Add($"Błąd wczytywania Nesting`u: {tekstKoduKresk}!");
        }


        protected sealed override int OdczytajIdZKoduKresk(string kodKresk) {
            // ToDo gdy będzie już znana postać kodu kreskowego karty Rozkrojow
            return -1;
        }
    }
}
