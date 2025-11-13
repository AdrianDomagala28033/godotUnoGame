using Godot;
using System;
using System.Collections.Generic;

public partial class TestJokery : Node
{
    private JokerManager jokerManager;
    private LogikaGry logikaGry;
    private TurnManager turnManager;

    public override void _Ready()
    {
        GD.Print("TEST JOKEROW");

        var gracze = new List<Gracz>
        {
            new Gracz("bot1", false, 0),
            new Gracz("bot2", false, 1),
            new Gracz("bot3", false, 2)
        };

        turnManager = new TurnManager(gracze);

        logikaGry = new LogikaGry();
        AddChild(logikaGry);
        logikaGry.ListaGraczy = gracze;
        logikaGry.TurnManager = turnManager;
        logikaGry.JokerManager = jokerManager;

        jokerManager = new JokerManager();
        JokerFactory.StworzJokery(jokerManager);

        try
        {
            logikaGry.UIManager.Inicjalizuj(logikaGry);
        }
        catch (Exception) { }

        GD.Print($"Dodano jokery: {jokerManager.listaJokerow.Count}");

        var sprintJoker = jokerManager.listaJokerow.Find(j => j.Nazwa == "Sprint");
        var aktualnyGracz = turnManager.AktualnyGracz;

        for (int i = 0; i < 11; i++)
            aktualnyGracz.rekaGracza.Add(new Karta());

        if (sprintJoker.CzyPozwalaNaZagranie(null, logikaGry))
            GD.Print("Joker 'Sprint' można zagrać");
        else
            GD.Print("Joker 'Sprint' nie można zagrać");

        var pierwszyJoker = jokerManager.listaJokerow.Find(j => j.Nazwa == "Pierwszy joker");
        GD.Print($"Dług do dobrania przed jokerem: {turnManager.DlugDobierania}");
        pierwszyJoker.Efekt(logikaGry);
        GD.Print($"Dług do dobrania po jokerze: {turnManager.DlugDobierania}");
    }
}
