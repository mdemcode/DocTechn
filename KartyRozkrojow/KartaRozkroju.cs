﻿using BasicSqlService;
using ModelPLM;
using System;

namespace DocTechn.KartyRozkrojow
{
    public class KartaRozkroju : DokumentTechnologiczny {
        
        public RozkrojPLM Rozkroj { get; private set; }

        public KartaRozkroju(string tekstKoduKresk) : base(tekstKoduKresk) {
            int idRozkr = OdczytajIdZKoduKresk(tekstKoduKresk);
            Rozkroj = new RozkrojPLM(idRozkr);
            if (!Rozkroj.RozkrojWczytanyPoprawnie) Bledy.Add($"Błąd wczytywania rozkroju: {tekstKoduKresk}");
        }

        private int OdczytajIdZKoduKresk(string kodKresk) {
            if (!kodKresk.StartsWith("*MB") && !kodKresk.StartsWith("*MT") && !kodKresk.StartsWith("MB") && !kodKresk.StartsWith("MT")) {
                Bledy.Add($"Błąd wczytywania rozkroju - błędny kod kresk.: {kodKresk}");
                return -1;
            }
            string nesBonTxt  = kodKresk.Substring(3, 5);
            string barIdtTxt = kodKresk.Substring(8, 3);
            if (!int.TryParse(nesBonTxt, out int nesBon) | !int.TryParse(barIdtTxt, out int barIdt)) {
                Bledy.Add($"Błąd wczytywania rozkroju - błędny kod kresk.: {kodKresk}");
                return -1;
            }
            string nesIdTxt = SqlService.PobierzPojedynczyString(BazaDanych.Plm, "NESTING", "NES_ID", $"NES_Bon={nesBon}", out string blad1);
            if (!blad1.IsNullOrEmpty() || !int.TryParse(nesIdTxt, out int nesId)) {
                Bledy.Add($"Błąd wczytywania rozkroju: {kodKresk}");
                return -1;
            }
            string barId    = SqlService.PobierzPojedynczyString(BazaDanych.Plm, "NESTBAR", "BAR_ID", $"NES_ID={nesId} AND BAR_IDT={barIdt}", out string blad2);
            return blad2.IsNullOrEmpty() && int.TryParse(barId, out int id) ? id : -1;
        }
    }
}
