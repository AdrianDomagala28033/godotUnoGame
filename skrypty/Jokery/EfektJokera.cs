using Godot;
using System;

public abstract partial class EfektJokera : Resource
{
    public abstract void Wykonaj(GameServer server);
    
    public virtual bool CzyPozwalaNaZagranie(GameServer server, DaneKarty kartaDoZagrania)
    {
        return false;
    }
    public virtual void PoZagraniuKarty(GameServer server, DaneKarty karta) {}
}
