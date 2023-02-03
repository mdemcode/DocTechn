using ModelPLM;

namespace DocTechn.KartyRozkrojow
{
    /// <summary> Karta rozkroju profile - do usunięcia - zrobiono jedną klasę dla blach i profili: KartaRozkroju </summary>
    public class KartaRozkrProfil : KartaRozkroj {
        
        public KartaRozkrProfil(string tekstKoduKresk) : base(tekstKoduKresk) {
            int idRozkr = OdczytajIdZKoduKresk(tekstKoduKresk);
            Rozkroj = new RozkrojPLM(idRozkr);
            if (!Rozkroj.RozkrojWczytanyPoprawnie) Bledy.Add($"Błąd wczytywania rozkroju: {tekstKoduKresk}");
        }

        public RozkrojPLM Rozkroj { get; private set; }

        protected sealed override int OdczytajIdZKoduKresk(string kodKresk) {
            // 
            return -1;
        }

    }
}
