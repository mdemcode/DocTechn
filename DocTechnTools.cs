using System;
using System.Text.RegularExpressions;

namespace DocTechn
{
    public static class DocTechnTools {

        #region EXTENSIONS
        public static string DopiszZeraWiodace(this string inTxt) {
            string   outTxt        = inTxt;
            string[] splitted      = inTxt.Split('/', '_', '-');
            int      ileZnakowMaNr = splitted[0].Length;
            if (ileZnakowMaNr >= 5) return outTxt;
            for (int i = 0; i < 5 - ileZnakowMaNr; i++) {
                outTxt = "0" + outTxt;
            }
            return outTxt;
        }
        //public static int? ParseToIntOrNull(this string input) => int.TryParse(input, out int result) ? result : null;
        //public static bool IsNullOrEmpty(this string oryg) => oryg is null || oryg.Length == 0 || oryg.Equals(""); //string.IsNullOrEmpty(oryg); // >> BasicSqlService.Helpers;
        #endregion

        /// <summary> Określa typ karty technologicznej na podstawie tekstu z kodu kreskowego </summary>
        public static TypKartyTechn OkreslTypKarty(string tekstKoduKresk) {
            TypKartyTechn typOut;
            // >>> przewodniki QR [nrZlec | nrGr | nrPrzew]  /  przewodniki kod kresk. [kodZlec nrPrzew]
            //string kodBezGwiazdek = tekstKoduKresk.Substring(1, tekstKoduKresk.Length - 2);
            if (IsBarcodeCorrect(tekstKoduKresk.Trim(), @"^([0-9]{2}\.[0-9]{5}\..{1,3}\s\|\s.{1,}-\s\|\s[0-9]{1,7}[\/]{0,1}.{0,})$") 
                || IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{1,3}\s[0-9]{1,7}[\/]{0,1}.{0,})$"))
                typOut = TypKartyTechn.KartaDetal; 
            // >>> karta rozkroju (blachy) - stara wersja
            else if(IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{1,}-[0-9]{1,})$") || IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{8})$"))  
                typOut = TypKartyTechn.KartaRozkrBlacha; 
            // >>> karta rozkroju (profile) - stara wersja
            else if (IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{1,}\$[0-9]{1,})$"))  typOut = TypKartyTechn.KartaRozkrProfil; 
            // >>> k. techn. montaż (wersja z kodem zl. / wersja z numerem zl.)
            else if (IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{1,3}\s.{1,15}-\s[0-9]{1,5}[\/]{0,1}.{0,})$") 
                     || IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{2}\.[0-9]{5}\..{1,3}\s.{1,15}-\s[0-9]{1,5}[\/]{0,1}.{0,})$"))  typOut = TypKartyTechn.KartaMontaz; 
            // >>> karta rozkroju PLM
            else if (tekstKoduKresk.StartsWith("*MB") || tekstKoduKresk.StartsWith("MB") || tekstKoduKresk.StartsWith("*MT") || tekstKoduKresk.StartsWith("MT"))  
                typOut = (tekstKoduKresk.EndsWith("000*") || tekstKoduKresk.EndsWith("000")) ? TypKartyTechn.KartaRozkrZbiorcza : TypKartyTechn.KartaRozkrPLM; 
            // >>> bledny format kodu
            else  typOut = TypKartyTechn.BlednyKod; 
            //
            return typOut;
        }
        public static string TypKartyToString(TypKartyTechn typ) {
            return typ switch {
                TypKartyTechn.KartaDetal => "Karta Detalowa",
                TypKartyTechn.KartaMontaz => "Karta montażowa",
                TypKartyTechn.KartaRozkrBlacha => "Karta rozkroju (blachy)",
                TypKartyTechn.KartaRozkrProfil => "Karta rozkroju (profile)",
                TypKartyTechn.KartaRozkrZbiorcza => "Zbiorcza karta rozkrojów",
                TypKartyTechn.KartaRozkrPLM => "Karta rozkroju",
                TypKartyTechn.BlednyKod => "Błędny kod!",
                TypKartyTechn.NieOkreslony => "Nie określony - zeskanuj pierwszą ...",
                _ => "Error",
            };
        }
        //Sprawdzanie czy przekazany kod jest zgodny z wyrazeniem regularnym
        public static bool IsBarcodeCorrect(string stringToCheck, string regExp) {
            try {
                Match matchResult = Regex.Match(stringToCheck, regExp, RegexOptions.None);
                if (matchResult.Success) {
                    return true;
                }
            }
            catch (ArgumentException) {
                return false;
            }
            return false;
        }

    }


    public enum TypKartyTechn {
        KartaDetal,
        KartaMontaz,
        KartaRozkrBlacha,
        KartaRozkrProfil,
        KartaRozkrZbiorcza,
        KartaRozkrPLM,
        BlednyKod,
        NieOkreslony
    }

}
