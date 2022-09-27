using BasicSqlService;

namespace DocTechn
{
    public class OperacjaRozpProton {

        public TypOperacji Typ { get; }
        public string Brygada { get; }
        public StatusOperacji Status { get; }

        public OperacjaRozpProton(TypOperacji typ, string brygada) {
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

        public enum StatusOperacji {
            Brak,
            DoWydania,
            Wydana,
            Wykonana
        }
        // wg tabeli SLO_OPERACJE w bazie Proton
        public enum TypOperacji {    //             // kolumna w tabeli Asprovy rd // rm
            Nieznana = 0,            // 
            CehaNaTwardo = 1,        //  O17        // oper_160
            Fazy = 2,                //  O8         // oper_90
            Frezarka = 3,            //  O11        // oper_120
            Kooperacja = 4,          //  O15        // oper_130
            Laser = 5,               //  O9         // oper_100
            MontazDetali = 6,        //  O12        // oper_140
            GilotynyDziurkarki = 7,  //  O1         // oper_10
            ObrobkaKrawedzi = 8,     //  O6         // oper_70
            Palniki = 9,             //  O4         // oper_40
            Pily = 10,               //  O3         // oper_30
            Plazma = 11,             //  O10        // oper_110
            Prasy = 12,              //  O16        // oper_150
            Prostowanie = 13,        //  O2         // oper_20
            Przekazanie = 14,        //  O15        // oper_50  // Kooperacja = Przekazanie
            SpawanieBlachownic = 15, //  O7         // oper_80
            Wiertarki = 16,          //  O5         // oper_60
            SkladaniePozycji = 19,   //             //                // oper_10
            SpawaniePozycji = 20,    //  O13        //                // oper_20
            FrezowaniePozycji = 21,  //             //                // oper_30
            //
            Wszystkie = -1           // O14         //
        }
    }
}
