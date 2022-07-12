using System.Collections.Generic;
using System.Linq;

namespace DocTechn
{
    public abstract class DokumentTechnologiczny {

        public string KodKreskowyTxt  { get; }
        public bool DaneWczytanePoprawnie => !Bledy.Any();
        public readonly List<string> Bledy = new();

        protected DokumentTechnologiczny(string tekstKoduKresk) {
            KodKreskowyTxt = tekstKoduKresk;
        }

    }
}
