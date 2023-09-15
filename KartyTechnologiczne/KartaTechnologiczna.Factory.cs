using ModelPLM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DocTechn.KartyTechnologiczne
{
    public abstract partial class KartaTechnologiczna {

        public static class Factory {
            
            // KD
            //
            public static KartaTechnDetal NowaKartaDetal(string nazwaDetalu, string nrZlec, out bool blad) {
                blad = false;
                string      kodKreskTxt = string.Empty;
                ZleceniePLM zlec        = ZleceniePLM.Factory.NoweZlecenieWgNumeruZl(nrZlec);
                if (!zlec.ZlecenieWczytanePopr) {
                    blad = true; // błąd wczytywania zlecenia
                    return new KartaTechnDetal(kodKreskTxt, zlec);
                }
                DetalPLM detal = DetalPLM.Factory.NowyDetalWgIdZlecINazwyDetalu(zlec.IdDB, nazwaDetalu);
                return new KartaTechnDetal(zlec, detal);
            }
            public static KartaTechnDetal NowaKartaDetal(DetalPLM detal, out bool blad) {
                blad = false;
                ZleceniePLM zlec                     = ZleceniePLM.Factory.NoweZlecenieWgNumeruZl(detal.NrZlec);
                if (!zlec.ZlecenieWczytanePopr) blad = true; // błąd wczytywania zlecenia
                return new KartaTechnDetal(zlec, detal);
            }
            public static KartaTechnDetal NowaKartaDetal(string kodKreskTxt, out bool blad) {
                if (kodKreskTxt.Contains(" | ")) { return NowaKartaDetalQR(kodKreskTxt, out blad); }
                blad = false;
                string[] kodKreskSplit = kodKreskTxt.Split();
                if (kodKreskSplit.Length != 2 || !int.TryParse(kodKreskSplit[0], out int kodZl)) {
                    blad = true; // błędny kod kreskowy
                    return new KartaTechnDetal("", ZleceniePLM.Factory.NoweZlecenieWgKoduZl(0));
                }
                ZleceniePLM zl = ZleceniePLM.Factory.NoweZlecenieWgKoduZl(kodZl);
                if (!zl.ZlecenieWczytanePopr) {
                    blad = true; // błąd wczytywania zlecenia
                    return new KartaTechnDetal("", zl);
                }
                DetalPLM detal = DetalPLM.Factory.NowyDetalWgIdZlecINumeruPrzewodnika(zl.IdDB, kodKreskSplit[1]);
                return new KartaTechnDetal(zl, detal);
            }
            private static KartaTechnDetal NowaKartaDetalQR(string kodKreskTxt, out bool blad) {
                blad = false;
                string[] kodKreskSplit = kodKreskTxt.Split('|');
                if (kodKreskSplit.Length != 3) {
                    blad = true; // błędny kod kreskowy
                    return new KartaTechnDetal("", ZleceniePLM.Factory.NoweZlecenieWgKoduZl(0));
                }
                ZleceniePLM zl = ZleceniePLM.Factory.NoweZlecenieWgNumeruZl(kodKreskSplit[0].Trim());
                if (!zl.ZlecenieWczytanePopr) {
                    blad = true; // błąd wczytywania zlecenia
                    return new KartaTechnDetal("", zl);
                }
                DetalPLM detal = DetalPLM.Factory.NowyDetalWgIdZlecINumeruPrzewodnika(zl.IdDB, kodKreskSplit[2].Trim());
                return new KartaTechnDetal(zl, detal);
            }/// <summary> Tylko dla potrzeb okreśenia DataContextu dla LbSkanowanePrzewodniki </summary>
            public static KartaTechnDetal NowaPustaKartaDetal() {
                return new KartaTechnDetal("", ZleceniePLM.Factory.NoweZlecenieWgKoduZl(0));
            }

            // KM
            //
            public static KartaTechnMontaz NowaKartaMontaz(string kodKreskTxt, out bool blad) {
                blad = false;
                string[] kodKrSplit = kodKreskTxt.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (kodKrSplit.Length < 3 || !int.TryParse(kodKrSplit[0], out int kodZl)) {
                    blad = true; // błędny kod kreskowy
                    return new KartaTechnMontaz("", ZleceniePLM.Factory.NoweZlecenieWgKoduZl(0)); // ZlecenieWczytanePoprawnie -> false
                }
                ZleceniePLM zl = ZleceniePLM.Factory.NoweZlecenieWgKoduZl(kodZl);
                return new KartaTechnMontaz(kodKreskTxt, zl);
            }
            public static KartaTechnMontaz NowaKartaMontazPLM(string kodKreskTxt, out bool blad) {
                blad = false;
                string[] kodKrSplit = kodKreskTxt.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (kodKrSplit.Length < 3) {
                    blad = true;                                                                  // błędny kod kreskowy
                    return new KartaTechnMontaz("", ZleceniePLM.Factory.NoweZlecenieWgKoduZl(0)); // ZlecenieWczytanePoprawnie -> false
                }
                ZleceniePLM zl = ZleceniePLM.Factory.NoweZlecenieWgNumeruZl(kodKrSplit[0]);
                return new KartaTechnMontaz(kodKreskTxt, zl);
            }/// <summary> Tylko dla potrzeb okreśenia DataContextu dla LbSkanowanePrzewodniki </summary>
            public static KartaTechnMontaz NowaPustaKartaMontaz() {
                return new KartaTechnMontaz("", ZleceniePLM.Factory.NoweZlecenieWgKoduZl(0)); // ZlecenieWczytanePoprawnie -> false
            }

        }
    }
}
