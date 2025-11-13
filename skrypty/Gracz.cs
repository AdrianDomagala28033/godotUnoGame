using Godot;
using System;
using System.Collections.Generic;

public class Gracz
{
    public string Nazwa { get; set; }
    public bool JestCzlowiekiem { get; set; }
    public List<Karta> rekaGracza { get; set; } = new List<Karta>();
    public int Index { get; set; }
    public Gracz(string nazwa, bool jestCzlowiekiem, int index)
    {
        Nazwa = nazwa;
        JestCzlowiekiem = jestCzlowiekiem;
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
