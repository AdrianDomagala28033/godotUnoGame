using Godot;
using System;
using System.Collections.Generic;

public class JokerManager
{

    public JokerManager()
    {
        
    }
    public void SprawdzAktywacje(Karta karta, LogikaGry logikaGry, Gracz gracz)
    {
        foreach (Joker joker in gracz.PosiadaneJokery)
        {
            if (joker.CzySpelniaWarunek(karta))
            {
                GD.Print($"[JokerManager] Aktywacja '{joker.Nazwa}' u gracza {gracz.Index}!");
                gracz.DodajPunkty(3);
                joker.Efekt?.Invoke(logikaGry);
            }
        }
    }
    public bool CzyJokerPozwalaNaZagranie(Karta karta, LogikaGry logikaGry, Gracz gracz)
    {
        foreach (Joker joker in gracz.PosiadaneJokery)
        {
            if (joker.CzyPozwalaNaZagranie(karta, logikaGry))
                return true;
        }
        return false;
    }
}
