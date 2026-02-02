using Godot;
using System;

public abstract partial class EfektJokera : Resource
{
    [Export] public Godot.Collections.Array<string> FiltrujKolory { get; set; } = new();
    [Export] public Godot.Collections.Array<string> FiltrujWartosci { get; set; } = new();
    public abstract void Wykonaj(GameServer server);
    
    public virtual bool CzyPozwalaNaZagranie(GameServer server, DaneKarty kartaDoZagrania)
    {
        return false;
    }
    public virtual void PoZagraniuKarty(GameServer server, DaneKarty karta)
    {
        if(CzyPasuje(server, karta)) Wykonaj(server);
    }
    public virtual void NaPoczatkuTury(GameServer server, long idGracza) {}
    public bool CzyPasuje(GameServer server, DaneKarty karta)
    {
        bool kolorPasuje = FiltrujKolory.Count == 0 || FiltrujKolory.Contains(karta.Kolor);
        bool wartoscPasuje = FiltrujWartosci.Count == 0 || FiltrujWartosci.Contains(karta.Wartosc);
        return kolorPasuje && wartoscPasuje;
    }
}
