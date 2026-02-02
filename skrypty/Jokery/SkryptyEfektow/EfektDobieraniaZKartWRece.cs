using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class EfektDobieraniaZKartWRece : EfektJokera
{
    [Export] public int Ilosc {get; set;}
    public override void Wykonaj(GameServer server)
    {
        if(Ilosc > 0)
        {
            long idNadawcy = server.Multiplayer.GetRemoteSenderId();
            if(idNadawcy == 0) idNadawcy = 1;
                ZastosujEfektDlaGracza(server, idNadawcy);
        }
    }
    public override void NaPoczatkuTury(GameServer server, long idGracza)
    {
        if(Ilosc < 0)
            ZastosujEfektDlaGracza(server, idGracza);
    }
    public override void PoZagraniuKarty(GameServer server, DaneKarty karta)
    {
        Wykonaj(server);
    }
    public void ZastosujEfektDlaGracza(GameServer server, long idGracza)
    {
        if(Ilosc < 0 && server.turnManager.DlugDobierania <= 0) return;
        if(!server.ListaGraczy.ContainsKey(idGracza)) return;
        int zmianaDlugu = 0;
        foreach (DaneKarty karta in server.ListaGraczy[idGracza].RekaGracza)
            if(CzyPasuje(server, karta))
                zmianaDlugu += Ilosc;
        if(zmianaDlugu != 0)
        {
            server.turnManager.DlugDobierania += zmianaDlugu;
            if(server.turnManager.DlugDobierania < 0) 
                    server.turnManager.DlugDobierania = 0;
            server.networkManager.Rpc(nameof(NetworkManager.ZaktualizujDlug), server.turnManager.DlugDobierania);
            GD.Print($"[JOKER START TURY] Gracz {idGracza} użył jokera. Zmiana długu o: {zmianaDlugu}");
        }
    }

}
