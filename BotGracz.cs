using Godot;
using System.Collections.Generic;
using System;

public partial class BotGracz
{
    public Karta WybierzKarteDoZagrania(List<Karta> reka, Karta gornaKarta, int dlug, Func<Karta, Karta, int, bool> funkcjaSprawdzajacaRuch)
    {
        foreach (Karta karta in reka)
        {
            if (funkcjaSprawdzajacaRuch(karta, gornaKarta, dlug))
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