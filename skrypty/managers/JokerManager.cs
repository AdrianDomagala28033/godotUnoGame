// using Godot;
// using System;
// using System.Collections.Generic;

// public class JokerManager
// {

//     public JokerManager()
//     {
        
//     }
//     public void SprawdzAktywacje(Karta karta, GameClient gameClient, Gracz gracz)
//     {
//         foreach (Joker joker in gracz.PosiadaneJokery)
//         {
//             if (joker.CzySpelniaWarunek(karta))
//             {
//                 GD.Print($"[JokerManager] Aktywacja '{joker.Nazwa}' u gracza {gracz.Index}!");
//                 gracz.DodajPunkty(3);
//                 joker.Efekt?.Invoke(gameClient);
//             }
//         }
//     }
//     public bool CzyJokerPozwalaNaZagranie(Karta karta, GameClient gameClient, Gracz gracz)
//     {
//         foreach (Joker joker in gracz.PosiadaneJokery)
//         {
//             if (joker.CzyPozwalaNaZagranie(karta, gameClient))
//                 return true;
//         }
//         return false;
//     }
// }
