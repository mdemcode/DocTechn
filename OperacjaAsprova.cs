using BasicSqlService;

namespace DocTechn
{
    public class OperacjaAsprova {

        public OperacjaAsprova(TypOperacji typ, string brygada) {
            Typ     = typ;
            Brygada = brygada;
            Status = brygada.IsNullOrEmpty()
                ? StatusOperacji.Brak
                : brygada switch {
                    "V" => StatusOperacji.DoWydania,
                    "W" => StatusOperacji.Wykonana,
                    _ => StatusOperacji.Wydana // symbol brygady
                };
        }

        public TypOperacji Typ { get; }
        public string Brygada { get; }
        public StatusOperacji Status { get; }

    }

    public enum StatusOperacji {
        Brak,
        DoWydania,
        Wydana,
        Wykonana
    }

    public enum TypOperacji{         // kolumna w tabeli Asprovy rd // rm
        Nieznana,            // O0   // 
        GilotynyDziurkarki,  // O1   // oper_10
        Prostowanie,         // O2   // oper_20
        Pily,                // O3   // oper_30
        Palniki,             // O4   // oper_40
        Wiertarki,           // O5   // oper_60
        ObrobkaKrawedzi,     // O6   // oper_70
        SpawanieBlachownic,  // O7   // oper_80
        Fazy,                // O8   // oper_90
        Laser,               // O9   // oper_100
        Plazma,              // O10  // oper_110
        Frezarka,            // O11  // oper_120                     // oper_30
        Montaz,              // O12  // oper_140                     // oper_10
        Spawanie,            // O13  //                              // oper_20
        Kooperacja,          // O15  // oper_130
        Przekazanie,         // O15  // oper_50  // Kooperacja = Przekazanie
        Prasy,               // O16  // oper_150
        CehaNaTwardo,        // O17  // oper_160
        //
        Wszystkie = -1       // O14  //
    }
}
