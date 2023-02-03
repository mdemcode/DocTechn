using ModelPLM;

namespace DocTechn.KartyRozkrojow
{
    /// <summary> Karta rozkroju zbiorcza </summary>
    public class KartaRozkrZbiorcza : KartaRozkroj {

        public KartaRozkrZbiorcza(string tekstKoduKresk) : base(tekstKoduKresk) {
            int nesBon = OdczytajIdZKoduKresk(tekstKoduKresk);
            Nesting = new NestingPLM(nesBon);
            if (!Nesting.RozkrojeWczytanePoprawnie) Bledy.Add($"Błąd wczytywania Nesting`u: {tekstKoduKresk}!");
        }

        public NestingPLM Nesting { get; } // >> tu jest lista rozkrojów i detali Nestingu

        public override string ToString() => $"Karta zbiorcza rozkrojów nr: {KodKreskowyTxt}";


        protected sealed override int OdczytajIdZKoduKresk(string kodKresk) {
            string nesBonTxt = kodKresk.Substring(3, 5);
            return int.TryParse(nesBonTxt, out int nesBon) ? nesBon : -1;
        }
    }
}
