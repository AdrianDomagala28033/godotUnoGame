using Godot;
using System;

[GlobalClass]
public partial class EfektBlokadyDlugu : EfektJokera
{
    public override void Wykonaj(GameServer server)
    {
        server.turnManager.KierunekGry *= -1;
        GD.Print($"[JOKER] Zablokowano atak. Nowy: {server.turnManager.KierunekGry}");
        if(server.turnManager.DlugDobierania > 0)
        {
            GD.Print("[JOKER] Przekazanie ataku! Kończę turę gracza i przekazuję dług dalej.");
            server.turnManager.ZakonczTure();
            long idOfiary = server.turnManager.AktualnyGraczId;
            server.networkManager.Rpc(nameof(NetworkManager.UstawTure), idOfiary);
            server.AktywujJokeryNaStartTury(idOfiary);
        }
    }
    public override bool CzyPozwalaNaZagranie(GameServer server, DaneKarty kartaDoZagrania)
    {
        if(kartaDoZagrania.Wartosc == "Stop" || kartaDoZagrania.Wartosc == "10")
            return true;
        else
        return false;
    }

}
