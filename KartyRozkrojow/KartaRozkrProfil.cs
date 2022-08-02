using ModelPLM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocTechn.KartyRozkrojow
{
    /// <summary> Karta rozkroju profile - do usunięcia - zrobiono jedną klasę dla blach i profili: KartaRozkroju </summary>
    public class KartaRozkrProfil : KartaRozkroj {

        public RozkrojPLM Rozkroj { get; private set; }
        
        public KartaRozkrProfil(string tekstKoduKresk) : base(tekstKoduKresk) {
            int idRozkr = OdczytajIdZKoduKresk(tekstKoduKresk);
            Rozkroj = new RozkrojPLM(idRozkr);
            if (!Rozkroj.RozkrojWczytanyPoprawnie) Bledy.Add($"Błąd wczytywania rozkroju: {tekstKoduKresk}");
        }

        protected sealed override int OdczytajIdZKoduKresk(string kodKresk) {
            // ToDo gdy będzie już znana postać kodu kreskowego karty Rozkrojow
            return -1;
        }

    }
}
