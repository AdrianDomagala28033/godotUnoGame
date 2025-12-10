// using Godot;
// using System;
// using System.Collections.Generic;

// public partial class TestJokery : Node
// {
//     private JokerManager jokerManager;
//     private LogikaGry logikaGry;
//     private TurnManager turnManager;

//     public override void _Ready()
//     {
//         GD.Print("=== TEST JOKEROW ===");

//         var gracze = new List<Gracz>
//         {
//             new Gracz("bot1", false, 0),
//             new Gracz("bot2", false, 1),
//             new Gracz("bot3", false, 2)
//         };

//         turnManager = new TurnManager(gracze);

//         logikaGry = new LogikaGry();
//         logikaGry.TurnManager = turnManager;
//         logikaGry.ListaGraczy = gracze;

//         AddChild(logikaGry);

//         jokerManager = new JokerManager();
//         JokerFactory.StworzJokery();
//         logikaGry.JokerManager = jokerManager;

//         TestSprint();
//         TestPierwszyJoker();
//         TestOdbicie();
//         TestOminMnie();
//     }

//     private void TestSprint()
//     {
//         GD.Print("\n--- TEST: Sprint ---");

//         var gracz = turnManager.AktualnyGracz;

//         for (int i = 0; i < 11; i++)
//             gracz.rekaGracza.Add(new Karta());

//         var sprint = jokerManager.listaJokerow.Find(j => j.Nazwa == "Sprint");

//         GD.Print("Można zagrać? -> " + sprint.CzyPozwalaNaZagranie(null, logikaGry));

//         int przed = turnManager.AktualnyGraczIndex;
//         sprint.Efekt(logikaGry);
//         int po = turnManager.AktualnyGraczIndex;

//         GD.Print($"Index przed: {przed}, index po: {po}");
//     }


//     private void TestPierwszyJoker()
//     {
//         GD.Print("\n--- TEST: Pierwszy Joker ---");

//         var j = jokerManager.listaJokerow.Find(j => j.Nazwa == "Pierwszy joker");

//         turnManager.DlugDobierania = 0;
//         GD.Print("Przed: " + turnManager.DlugDobierania);

//         j.Efekt(logikaGry);

//         GD.Print("Po: " + turnManager.DlugDobierania);
//     }


//     private void TestOdbicie()
//     {
//         GD.Print("\n--- TEST: Odbicie ---");

//         var j = jokerManager.listaJokerow.Find(j => j.Nazwa == "Odbicie");

//         turnManager.DlugDobierania = 3;
//         int kierunekPrzed = turnManager.KierunekGry;

//         var karta = new Karta();
//         karta.Wartosc = "ZmianaKierunku";

//         GD.Print("Można zagrać? -> " + j.CzyPozwalaNaZagranie(karta, logikaGry));
//         j.Efekt(logikaGry);

//         GD.Print($"Kierunek przed: {kierunekPrzed}, po: {turnManager.KierunekGry}");
//     }


//     private void TestOminMnie()
//     {
//         GD.Print("\n--- TEST: Omiń mnie! ---");

//         var j = jokerManager.listaJokerow.Find(j => j.Nazwa == "Omiń mnie!");

//         turnManager.DlugDobierania = 2;

//         var karta = new Karta();
//         karta.Wartosc = "Stop";

//         GD.Print("Można zagrać? -> " + j.CzyPozwalaNaZagranie(karta, logikaGry));

//         int przed = turnManager.AktualnyGraczIndex;
//         j.Efekt(logikaGry);
//         int po = turnManager.AktualnyGraczIndex;

//         GD.Print($"Gracz przed: {przed}, gracz po: {po}");
//     }

// }
