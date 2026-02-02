using Godot;
using System;

[GlobalClass]
public partial class EfektOdbicia : EfektJokera
{
    public override void Wykonaj(GameServer server)
    {
        server.turnManager.KierunekGry *= -1;
        GD.Print($"[JOKER] Zmieniono kierunek gry. Nowy: {server.turnManager.KierunekGry}");
        if(server.turnManager.DlugDobierania > 0)
        {
            GD.Print("[JOKER] Odbicie ataku! Kończę turę gracza i przekazuję dług dalej.");
            server.turnManager.ZakonczTure();
            long idOfiary = server.turnManager.AktualnyGraczId;
            server.networkManager.Rpc(nameof(NetworkManager.UstawTure), idOfiary);
            server.AktywujJokeryNaStartTury(idOfiary);
        }
    }
    public override bool CzyPozwalaNaZagranie(GameServer server, DaneKarty kartaDoZagrania)
    {
        if(kartaDoZagrania.Wartosc == "ZmianaKierunku" || kartaDoZagrania.Wartosc == "12")
            return true;
        else
        return false;
    }

}
