using System.Collections.Generic;
using System.Linq;

namespace DocTechn
{
    public abstract class DokumentTechnologiczny {

        protected DokumentTechnologiczny(string tekstKoduKresk) => KodKreskowyTxt = tekstKoduKresk;

        public string KodKreskowyTxt  { get; }
        public bool DaneWczytanePoprawnie => !Bledy.Any();
        public readonly List<string> Bledy = new();

    }
}
