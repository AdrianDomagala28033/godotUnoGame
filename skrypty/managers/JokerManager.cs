using Godot;
using System;
using System.Collections.Generic;

public class JokerManager
{
    public List<Joker> listaJokerow;

    public JokerManager()
    {
        listaJokerow = new List<Joker>();
    }
    public void DodajJokera(Joker joker)
    {
        listaJokerow.Add(joker);
    }
    public void UsunJokera(Joker joker)
    {
        listaJokerow.Remove(joker);
    }
    public void SprawdzAktywacje(Karta karta, LogikaGry logikaGry)
    {
        foreach (Joker joker in listaJokerow)
        {
            if (joker.CzySpelniaWarunek(karta))
            {
                joker.Efekt?.Invoke(logikaGry);
            }
        }
    }
    public bool CzyJokerPozwalaNaZagranie(Karta karta, LogikaGry logikaGry)
    {
        foreach (Joker joker in listaJokerow)
        {
            if (joker.CzyPozwalaNaZagranie(karta, logikaGry))
                return true;
        }
        return false;
    }
}
