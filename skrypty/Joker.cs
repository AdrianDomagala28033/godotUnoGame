using Godot;
using System;
public enum RzadkoscJokera
{
    Zwykly,
    Rzadki,
    Legendarny,
}
public enum WarunekAktywacji
{
    Kolor,
    Wartosc,
    SpecjalnaKarta,
    Kombinacja,
    Inne,
    Pasywny
}
public class Joker
{

    public string Nazwa { get; set; }
    public string Opis { get; set; }
    public RzadkoscJokera RzadkoscJokera { get; set; }
    public WarunekAktywacji WarunekAktywacji { get; set; }
    public Action<LogikaGry> Efekt { get; set; }
    public string Parametr { get; set; }
    public Func<Karta, LogikaGry, bool> CzyPozwalaNaZagranie { get; set; } = (karta, logikaGry) => false;
    public int IloscUzyc { get; set; }

    public Joker(string nazwa, string opis, RzadkoscJokera rzadkoscJokera, WarunekAktywacji warunekAktywacji, Action<LogikaGry> efekt, string parametr)
    {
        Nazwa = nazwa;
        Opis = opis;
        RzadkoscJokera = rzadkoscJokera;
        WarunekAktywacji = warunekAktywacji;
        Efekt = efekt;
        Parametr = parametr;
    }

    public bool CzySpelniaWarunek(Karta karta)
    {
        switch (WarunekAktywacji)
        {
            case WarunekAktywacji.Kolor:
                return karta.Kolor == Parametr;
            case WarunekAktywacji.Wartosc:
                return karta.Wartosc == Parametr;
            case WarunekAktywacji.SpecjalnaKarta:
                if (!string.IsNullOrEmpty(Parametr))
                    return karta.Wartosc == Parametr;
                return karta.Wartosc == "+4" || karta.Wartosc == "Stop" || karta.Wartosc == "ZmianaKierunku";
            case WarunekAktywacji.Kombinacja:
                // przykładowy format parametru: "Niebieski_7"
                string[] dane = Parametr.Split('_');
                for (int i = 0; i < dane.Length; i += 2)
                {
                    if(karta.Kolor == dane[i] && karta.Wartosc == dane[i+1])
                        return true;
                }
                return false; 
            default:
                return false;
        }
    }
}
