using Godot;
using System;

public static class JokerFactory
{
    public static void StworzJokery(JokerManager jokerManager)
    {
    #region ukonczone jokery
        jokerManager.DodajJokera(new Joker("Pierwszy joker", "Przy zagraniu karty +4 gracz musi dobrać 5 kart", RzadkoscJokera.Zwykly, WarunekAktywacji.SpecjalnaKarta,
        (logikaGry) => logikaGry.TurnManager.DlugDobierania = 1, "+4"));

        jokerManager.DodajJokera(new Joker("Odbicie",
        "Jeśli masz dług do dobrania, możesz zagrać kartę odwrócenia, która odbije dług", RzadkoscJokera.Zwykly, WarunekAktywacji.SpecjalnaKarta,
        (logikaGry) =>
        {
            if (logikaGry.TurnManager.DlugDobierania > 0)
                logikaGry.TurnManager.ZmienKierunek();
        }, "ZmianaKierunku")
        {
            CzyPozwalaNaZagranie = (karta, logikaGry) =>
            {
                return karta.Wartosc == "ZmianaKierunku" && logikaGry.TurnManager.DlugDobierania > 0;
            }
        });

        jokerManager.DodajJokera(new Joker("Omiń mnie!", "Jeśli gracz zagra kartę STOP, dług do dobrania przechodzi na następnego gracza", RzadkoscJokera.Zwykly, WarunekAktywacji.SpecjalnaKarta,
       (logikaGry) =>
       {
           if (logikaGry.TurnManager.DlugDobierania > 0)
           {
               int start = logikaGry.TurnManager.AktualnyGraczIndex;
               int nextIndex = start;
               do
               {
                   nextIndex += logikaGry.TurnManager.KierunekGry;
                   logikaGry.TurnManager.UporzadkujIndex();
               } while (logikaGry.ListaGraczy[nextIndex].rekaGracza.Count == 0);
               logikaGry.TurnManager.UstawWybranegoGracza(nextIndex);
           }
       }, "Stop")
        {
            CzyPozwalaNaZagranie = (karta, logikaGry) =>
            {
                return karta.Wartosc == "Stop" && logikaGry.TurnManager.DlugDobierania > 0;
            }
        });
        
        jokerManager.DodajJokera(new Joker("Sprint", "Jeśli masz więcej niż 10 kart, możesz zagrać 2 karty podczas jednej tury", RzadkoscJokera.Zwykly, WarunekAktywacji.Inne,
        (logikaGry) =>
        {
            if (logikaGry.ListaGraczy[logikaGry.TurnManager.AktualnyGraczIndex].rekaGracza.Count > 10)
                logikaGry.TurnManager.UstawWybranegoGracza(logikaGry.TurnManager.AktualnyGraczIndex);

        }, "ma wiecej niz 10 kart")
        {
            CzyPozwalaNaZagranie = (karta, logikaGry) =>
            {
                return logikaGry.ListaGraczy[logikaGry.TurnManager.AktualnyGraczIndex].rekaGracza.Count > 10;
            }
        });
    #endregion

        jokerManager.DodajJokera(new Joker("Mieszacz", "Możesz zamienić swoje karty z kartami innego gracza (możliwy raz do wykorzystania)", RzadkoscJokera.Rzadki, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "kliknieto"));

        jokerManager.DodajJokera(new Joker("Szpieg", "Możesz podejrzeć kartę innego gracza (możliwy raz do wykorzystania)", RzadkoscJokera.Rzadki, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "kliknieto"));

        jokerManager.DodajJokera(new Joker("Kolorowy chaos", "Jeśli gracz zagra zmianę koloru, następny gracz też musi wybrać nowy kolor", RzadkoscJokera.Zwykly, WarunekAktywacji.Kolor,
        (logikaGry) => { return; }, "kolor"));

        jokerManager.DodajJokera(new Joker("Skok w bok", "Po zagraniu karty, wszyscy gracze są pomijani i możesz zagrać jeszcze jedną kartę", RzadkoscJokera.Rzadki, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "skipnij"));

        jokerManager.DodajJokera(new Joker("Pomocna ręka", "Anuluje twój dług", RzadkoscJokera.Rzadki, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "anulujDlug"));

        jokerManager.DodajJokera(new Joker("Zamiana koloru", "Pozwala na zamianę koloru bez dzikiej karty jeśli masz jedną kartę", RzadkoscJokera.Legendarny, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "zmien kolor"));

        jokerManager.DodajJokera(new Joker("Przeskok", "Wybierz gracza od którego zacznie się kolejna tura (ilość użyć: 5)", RzadkoscJokera.Rzadki, WarunekAktywacji.Inne,
        (logikaGry) => { return; }, "klikniecie"));
    }
}
