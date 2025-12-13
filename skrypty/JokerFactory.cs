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
        (logikaGry) => logikaGry.TurnManager.DlugDobierania += 4, "+4")); // dziala

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
        
        lista.Add(new Joker("Pomocna ręka", "Anuluje twój dług", RzadkoscJokera.Rzadki, WarunekAktywacji.Pasywny,
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
    
        lista.Add(new Joker("Red joker", "Przy zagraniu czerwonej karty gracz musi dobrać +1 kartę", RzadkoscJokera.Zwykly, WarunekAktywacji.Kolor,
            (logikaGry) => {
                logikaGry.TurnManager.DlugDobierania += 1;
                logikaGry.TurnManager.ZakonczTure();
            }, "Czerwony"));
        lista.Add(new Joker("Blue joker", "Przy zagraniu niebieskiej karty gracz musi dobrać +1 kartę", RzadkoscJokera.Zwykly, WarunekAktywacji.Kolor,
            (logikaGry) => {
                logikaGry.TurnManager.DlugDobierania += 1;
                logikaGry.TurnManager.ZakonczTure();
            }, "Niebieski"));
        lista.Add(new Joker("Yellow joker", "Każda żółta parzysta karta zmniejsza dług o 1", RzadkoscJokera.Zwykly, WarunekAktywacji.Pasywny,
            (logikaGry) => {
                foreach (Karta karta in logikaGry.ListaGraczy[logikaGry.TurnManager.AktualnyGraczIndex].rekaGracza)
                {
                    int.TryParse(karta.Wartosc, out int wartosc);
                    if(wartosc % 2 == 0 && logikaGry.DlugDobierania > 0)
                    {
                        logikaGry.TurnManager.DlugDobierania -= 1;
                        if(logikaGry.DlugDobierania == 0)
                            break;
                    }
                }
            }, "Zolty"));
        lista.Add(new Joker("Green joker", "Każda zielone nieparzysta karta zmniejsza dług o 1", RzadkoscJokera.Zwykly, WarunekAktywacji.Pasywny,
            (logikaGry) => {
                foreach (Karta karta in logikaGry.ListaGraczy[logikaGry.TurnManager.AktualnyGraczIndex].rekaGracza)
                {
                    int.TryParse(karta.Wartosc, out int wartosc);
                    if(wartosc % 2 == 0 && logikaGry.DlugDobierania > 0)
                    {
                        logikaGry.TurnManager.DlugDobierania -= 1;
                        if(logikaGry.DlugDobierania == 0)
                            break;
                    }
                }
            }, "Zielony"));
        lista.Add(new Joker("Mały pożar", "Każda zagrana czerwona karta dodaje +1 do długu za każde czerwone 0,1,2,3,4 w ręce", RzadkoscJokera.Zwykly, WarunekAktywacji.Kolor,
        (logikaGry) => {
            foreach (Karta karta in logikaGry.ListaGraczy[logikaGry.TurnManager.AktualnyGraczIndex].rekaGracza)
            {
                int.TryParse(karta.Wartosc, out int wartosc);
                if(wartosc < 5 && karta.Kolor == "Czerwony")
                {
                    logikaGry.TurnManager.DlugDobierania += 1;
                }
            }
        }, "Czerwony"));
        lista.Add(new Joker("Wysoka fala", "Każda zagrana niebieska karta dodaje +1 do długu za każdą niebieską 5,6,7,8,9 w ręce", RzadkoscJokera.Zwykly, WarunekAktywacji.Kolor,
        (logikaGry) => {
            foreach (Karta karta in logikaGry.ListaGraczy[logikaGry.TurnManager.AktualnyGraczIndex].rekaGracza)
            {
                int.TryParse(karta.Wartosc, out int wartosc);
                if(wartosc > 4 && wartosc < 10 && karta.Kolor == "Niebieski")
                {
                    logikaGry.TurnManager.DlugDobierania += 1;
                }
            }
        }, "Niebieski"));
        lista.Add(new Joker("RGB", "Każda zagrana czerwona 2, zielona 4 i niebieska 8 dodaje połowe wartości zagranej karty", RzadkoscJokera.Zwykly, WarunekAktywacji.Kombinacja,
        (logikaGry) => {
            int.TryParse(logikaGry.GornaKartaNaStosie.Wartosc, out int wartosc);
            logikaGry.TurnManager.DlugDobierania += wartosc / 2;
        }, "Czerwony_2_Zielony_4_Niebieski_8"));
        lista.Add(new Joker("Bumerang", "Każda zagrana 7 odwraca kokejke", RzadkoscJokera.Zwykly, WarunekAktywacji.Wartosc,
        (logikaGry) => {
            int.TryParse(logikaGry.GornaKartaNaStosie.Wartosc, out int wartosc);
            if(wartosc == 7)
                logikaGry.TurnManager.ZmienKierunek();
        }, "7"));
        lista.Add(new Joker("Cztery ściany", "Każda zagrana 4 blokuje następnego gracza", RzadkoscJokera.Zwykly, WarunekAktywacji.Wartosc,
        (logikaGry) => {
            int.TryParse(logikaGry.GornaKartaNaStosie.Wartosc, out int wartosc);
            if(wartosc == 4)
                logikaGry.TurnManager.PominTure();
        }, "4"));
    
    #endregion
        return lista;
    }
}
