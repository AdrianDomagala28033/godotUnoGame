using Godot;
using System;

[GlobalClass]
public partial class EfektLimituDobierania : EfektJokera
{
    [Export] public int ProgKart { get; set; }
    [Export] public int MaxDlug { get; set; }
    public override void Wykonaj(GameServer server) {}
    public override void NaPoczatkuTury(GameServer server, long idGracza)
    {
        if(!server.ListaGraczy.ContainsKey(idGracza)) return;
        var gracz = server.ListaGraczy[idGracza];
        if(gracz.RekaGracza.Count <= ProgKart)
        {
            if(server.turnManager.DlugDobierania >= MaxDlug)
            {
                int staryDlug = server.turnManager.DlugDobierania;
                server.turnManager.DlugDobierania = MaxDlug;
                server.networkManager.Rpc(nameof(NetworkManager.ZaktualizujDlug), server.turnManager.DlugDobierania);
                GD.Print($"[JOKER LIMIT] Gracz {idGracza} uratowany przez limit! Zredukowano {staryDlug} -> {MaxDlug}");
            }  
        }   
    }
}