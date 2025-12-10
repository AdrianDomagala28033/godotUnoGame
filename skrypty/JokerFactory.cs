using Godot;
using System;
using System.Collections.Generic;

public static class JokerFactory
{
    public static List<Joker> StworzJokery()
    {
        List<Joker> lista = new List<Joker>();
    #region ukonczone jokery
        lista.Add(new Joker("Pierwszy joker", "Przy zagraniu karty +4 gracz musi dobrać 5 kart", RzadkoscJokera.Zwykly, WarunekAktywacji.SpecjalnaKarta,
        (logikaGry) => logikaGry.TurnManager.DlugDobierania = 1, "+4")); // dziala

        lista.Add(new Joker("Odbicie",
        "Jeśli masz dług do dobrania, możesz zagrać kartę odwrócenia, która odbije dług", RzadkoscJokera.Zwykly, WarunekAktywacji.SpecjalnaKarta,
        (logikaGry) =>
        {
            int dlug = logikaGry.TurnManager.DlugDobierania;
            if (logikaGry.TurnManager.DlugDobierania > 0)
            {
               logikaGry.TurnManager.ZmienKierunek();
               int aktualnyGracz = logikaGry.TurnManager.AktualnyGraczIndex;
               int kierunek = logikaGry.TurnManager.KierunekGry;
               int iloscGraczy = logikaGry.ListaGraczy.Count;
               int nextIndex = aktualnyGracz;
               do
               {
                   nextIndex += kierunek;
                   if(nextIndex >= iloscGraczy) nextIndex = 0;
                   if (nextIndex < 0) nextIndex = iloscGraczy - 1;
                   logikaGry.TurnManager.UporzadkujIndex();
               } while (logikaGry.ListaGraczy[nextIndex].rekaGracza.Count == 0);
               int indexDoUstawienia = nextIndex - kierunek;
               if (indexDoUstawienia >= iloscGraczy) indexDoUstawienia = 0;
               if (indexDoUstawienia < 0) indexDoUstawienia = iloscGraczy - 1;
               logikaGry.TurnManager.UstawWybranegoGracza(indexDoUstawienia);
               logikaGry.TurnManager.DlugDobierania = dlug;
               GD.Print($"[Joker] Dług {dlug} przekazany do gracza {nextIndex}!");
            }
        }, "ZmianaKierunku")
        {
            CzyPozwalaNaZagranie = (karta, logikaGry) =>
            {
                return karta.Wartosc == "ZmianaKierunku" && logikaGry.TurnManager.DlugDobierania > 0;
            }
        });

        lista.Add(new Joker("Omiń mnie!", "Jeśli gracz zagra kartę STOP, dług do dobrania przechodzi na następnego gracza", RzadkoscJokera.Zwykly, WarunekAktywacji.SpecjalnaKarta,
       (logikaGry) =>
       {
        GD.Print("SPRINT ODPALIŁ!");
           int dlug = logikaGry.TurnManager.DlugDobierania;
           if (logikaGry.TurnManager.DlugDobierania > 0)
           {
               int aktualnyGracz = logikaGry.TurnManager.AktualnyGraczIndex;
               int kierunek = logikaGry.TurnManager.KierunekGry;
               int iloscGraczy = logikaGry.ListaGraczy.Count;
               int nextIndex = aktualnyGracz;
               do
               {
                   nextIndex += kierunek;
                   if(nextIndex >= iloscGraczy) nextIndex = 0;
                   if (nextIndex < 0) nextIndex = iloscGraczy - 1;
                   logikaGry.TurnManager.UporzadkujIndex();
               } while (logikaGry.ListaGraczy[nextIndex].rekaGracza.Count == 0);
               int indexDoUstawienia = nextIndex - kierunek;
               if (indexDoUstawienia >= iloscGraczy) indexDoUstawienia = 0;
               if (indexDoUstawienia < 0) indexDoUstawienia = iloscGraczy - 1;
               logikaGry.TurnManager.UstawWybranegoGracza(indexDoUstawienia);
               logikaGry.TurnManager.DlugDobierania = dlug;
               GD.Print($"[Joker] Dług {dlug} przekazany do gracza {nextIndex}!");
           }
       }, "Stop")
        {
            CzyPozwalaNaZagranie = (karta, logikaGry) =>
            {
                return karta.Wartosc == "Stop" && logikaGry.TurnManager.DlugDobierania > 0 && karta.Kolor == logikaGry.GornaKartaNaStosie.Kolor;
            }
        });
        
        lista.Add(new Joker("Pomocna ręka", "Anuluje twój dług", RzadkoscJokera.Rzadki, WarunekAktywacji.Inne,
        (logikaGry) =>
        {
            if(logikaGry.TurnManager.DlugDobierania > 0)
            {
                logikaGry.TurnManager.DlugDobierania = 0;
            }
        }, "anulujDlug")
        {
            CzyPozwalaNaZagranie = (karta, logikaGry) =>
            {
                return logikaGry.TurnManager.DlugDobierania > 0 && karta.Kolor == logikaGry.GornaKartaNaStosie.Kolor;
            }
        });
    #endregion

        lista.Add(new Joker("Mieszacz", "Możesz zamienić swoje karty z kartami innego gracza (możliwy raz do wykorzystania)", RzadkoscJokera.Rzadki, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "kliknieto"));

        lista.Add(new Joker("Szpieg", "Możesz podejrzeć karty innego gracza (możliwy raz do wykorzystania)", RzadkoscJokera.Rzadki, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "kliknieto"));

        lista.Add(new Joker("Kolorowy chaos", "Jeśli gracz zagra zmianę koloru, następny gracz też musi wybrać nowy kolor", RzadkoscJokera.Zwykly, WarunekAktywacji.Kolor,
        (logikaGry) => { return; }, "kolor"));

        lista.Add(new Joker("Zamiana koloru", "Pozwala na zamianę koloru bez dzikiej karty jeśli masz jedną kartę", RzadkoscJokera.Legendarny, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "zmien kolor"));

        lista.Add(new Joker("Przeskok", "Wybierz gracza od którego zacznie się kolejna tura (ilość użyć: 5)", RzadkoscJokera.Rzadki, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "klikniecie"));

        return lista;
    }
}
