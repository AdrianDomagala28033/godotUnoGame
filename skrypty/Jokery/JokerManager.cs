using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class JokerManager
{
    private static Dictionary<string, DaneJokera> bazaJokerow = new Dictionary<string, DaneJokera>();
    private static bool zaladowano = false;

    public static void ZaladujJokery()
    {
        if (zaladowano) return;
        WczytajWszystkieJokery();
        zaladowano = true;
        GD.Print($"[JokerManager] Załadowano {bazaJokerow.Count} jokerów.");
    }
    private static void WczytajZasob(string sciezka)
    {
        if (ResourceLoader.Exists(sciezka))
        {
            var joker = GD.Load<DaneJokera>(sciezka);
            if (joker != null && !string.IsNullOrEmpty(joker.Id))
            {
                if (!bazaJokerow.ContainsKey(joker.Id))
                    bazaJokerow.Add(joker.Id, joker);
                else
                    GD.PrintErr($"[JokerManager] Duplikat ID: {joker.Id} w pliku {sciezka}");
            }
        }
        else
            GD.PrintErr($"[JokerManager] Nie znaleziono pliku: {sciezka}");
    }

    public static DaneJokera PobierzJokera(string id)
    {
        if (!zaladowano) ZaladujJokery();

        if (bazaJokerow.TryGetValue(id, out var joker))
            return joker;

        GD.PrintErr($"[JokerManager] Nie znaleziono jokera o ID: {id}");
        return null;
    }

    public static string[] PobierzWszystkieId()
    {
        if (!zaladowano) ZaladujJokery();
        return bazaJokerow.Keys.ToArray();
    }
    public static void WczytajWszystkieJokery()
    {
        string sciezkaFolderu = "res://ZasobyJokerow/Jokery";
        using var dir = DirAccess.Open(sciezkaFolderu);
        if(dir != null)
        {
            dir.ListDirBegin();
            string[] pliki = dir.GetFiles();
            foreach (var plik in pliki)
            {
                if(plik.EndsWith(".tres"))
                    WczytajZasob($"{sciezkaFolderu}/{plik}");
            }
        }
        else
            GD.PrintErr($"Nie udało się otworzyć folderu: {sciezkaFolderu}");
    }
}
