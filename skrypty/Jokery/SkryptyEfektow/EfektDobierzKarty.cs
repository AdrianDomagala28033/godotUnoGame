using Godot;


[GlobalClass]
public partial class EfektDobierzKarty : EfektJokera
{
    [Export] public int Ilosc {get; set;}
    public override void Wykonaj(GameServer server)
    {
        server.turnManager.DlugDobierania += Ilosc;
        server.networkManager.Rpc(nameof(NetworkManager.ZaktualizujDlug), server.turnManager.DlugDobierania);
        GD.Print($"[JokerEffect] Zwiększono dług o {Ilosc}.");
    }
    public override void PoZagraniuKarty(GameServer server, DaneKarty karta)
    {
        if(karta.Kolor == "DzikaKarta")
            Wykonaj(server);
    }
}