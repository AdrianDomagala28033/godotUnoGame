using Godot;
using System;
using System.Collections.Generic;

public class Gracz
{
    public string Nazwa { get; set; }
    public bool CzyToGraczLokalny { get; set; }
    public List<Karta> rekaGracza { get; set; } = new List<Karta>();
    public int Index { get; set; }
    //public List<DaneJokera> PosiadaneJokery {get; set;} = new List<DaneJokera>();
    public bool CzyUkonczyl {get; set;} = false;
    public int Wynik {get; set;}
    public int Miejsce {get; set;}
    public long IdGracza {get; set;}
    public bool CzyGotowy {get; set;} = false;
    public Gracz(string nazwa, long idGracza, bool czyToGraczLokalny, int index)
    {
        Nazwa = nazwa;
        IdGracza = idGracza;
        CzyToGraczLokalny = czyToGraczLokalny;
        Index = index;
    }
    public Karta WybierzKarteDoZagrania(List<Karta> reka, int dlug, Func<Karta, int, bool> funkcjaSprawdzajacaRuch)
    {
        foreach (Karta karta in reka)
        {
            if (funkcjaSprawdzajacaRuch(karta, dlug))
            {
                return karta;
            }
        }
        return null;
    }
    public string WybierzKolor(List<Karta> reka)
    {
        string[] kolory = ["Czerwony", "Niebieski", "Zolty", "Zielony"];
        Random random = new Random();
        return kolory[random.Next(kolory.Length)];
    }
    
}
