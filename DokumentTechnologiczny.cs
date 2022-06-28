using System;
using System.Collections.Generic;
using System.Linq;

namespace DocTechn
{
    public abstract class DokumentTechnologiczny {

        public string KodKreskowyTxt  { get; }
        public bool DanePodstWczytanePoprawnie { get; }
        public bool UtworzonaPoprawnie { get; protected set; }
        //
        public readonly List<string> Bledy = new();


        protected DokumentTechnologiczny(string tekstKoduKresk) {
            KodKreskowyTxt = tekstKoduKresk;
            //DanePodstWczytanePoprawnie = OdczytajDaneKoduKresk(tekstKoduKresk);
        }

        protected abstract bool OdczytajDaneWgKoduKresk(string kodKresk);
    }
}
