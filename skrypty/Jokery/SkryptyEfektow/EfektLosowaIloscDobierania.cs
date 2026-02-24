using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class EfektLosowaIloscDobierania : EfektJokera
{
    private static readonly Random rng = new Random();
    public override void Wykonaj(GameServer server)
    {
        
    }
    public override void NaPoczatkuTury(GameServer server, long idGracza)
    {
        GD.Print($"[DEBUG] Joker Hazardzista sprawdza gracza {idGracza}. Dług: {server.turnManager.DlugDobierania}");
        if(server.turnManager.DlugDobierania > 0)
            ZastosujEfektDlaGracza(server, idGracza);
    }
    public override void PoZagraniuKarty(GameServer server, DaneKarty karta)
    {
        
    }
    public void ZastosujEfektDlaGracza(GameServer server, long idGracza)
    {
        int staryDlug = server.turnManager.DlugDobierania;
        int nowyDlug = rng.Next(0, 11);
        server.turnManager.DlugDobierania = nowyDlug;
        server.networkManager.Rpc(nameof(NetworkManager.ZaktualizujDlug), server.turnManager.DlugDobierania);
        GD.Print($"[JOKER HAZARDZISTA] Gracz {idGracza} zaryzykował! Dług zmieniony z {staryDlug} na {nowyDlug}.");
    }

}
