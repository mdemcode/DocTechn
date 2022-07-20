using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocTechn
{
    public static class DocTechnTools {

        //#region EXTENSIONS
        //public static int? ParseToIntOrNull(this string input) => int.TryParse(input, out int result) ? result : null;
        //public static bool IsNullOrEmpty(this string oryg) => oryg is null || oryg.Length == 0 || oryg.Equals(""); //string.IsNullOrEmpty(oryg); // >> BasicSqlService.Helpers;
        //#endregion

        /// <summary> Określa typ karty technologicznej na podstawie tekstu z kodu kreskowego </summary>
        public static TypKartyTechn OkreslTypKarty(string tekstKoduKresk) { // ToDo - To będzie do zmiany !
            TypKartyTechn typOut;
            // >>> przewodniki
            if (IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{1,3}\s[0-9]{1,7}[\/]{0,1}.{0,})$")) typOut = TypKartyTechn.KartaDetal; 
            // >>> karta rozkroju (blachy)
            else if(IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{1,}-[0-9]{1,})$") || IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{8})$"))  typOut = TypKartyTechn.KartaRozkrBlacha; 
            // >>> karta rozkroju (profile)
            else if (IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{1,}\$[0-9]{1,})$"))  typOut = TypKartyTechn.KartaRozkrProfil; 
            // >>> ktechnologia montaż
            else if (IsBarcodeCorrect(tekstKoduKresk, @"^([0-9]{1,3}\s.{1,15}-\s[0-9]{1,5}[\/]{0,1}.{0,})$"))  typOut = TypKartyTechn.KartaMontaz; 
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
        BlednyKod,
        NieOkreslony
    }

}
