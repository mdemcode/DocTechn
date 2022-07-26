﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocTechn.KartyRozkrojow
{
    //  - do usunięcia - zrobiono jedną klasę dla blach i profili: KartaRozkroju
    public abstract class KartaRozkroj : DokumentTechnologiczny {

        protected KartaRozkroj(string tekstKoduKresk) : base(tekstKoduKresk) { }

        protected abstract int OdczytajIdZKoduKresk(string kodKresk);

        public override string ToString() => $"Karta rozkroju nr: {KodKreskowyTxt}";

    }
}
