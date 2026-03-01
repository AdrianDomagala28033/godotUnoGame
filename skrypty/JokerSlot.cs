using Godot;
using System;

public partial class JokerSlot : TextureRect
{
    private DaneJokera joker;


    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
    }

    public void Inicjalizuj(DaneJokera joker)
    {
        this.joker = joker;
        if (joker.Ikona != null) 
            Texture = joker.Ikona;
        TooltipText = $"{joker.Nazwa}\n{joker.RzadkoscJokera}\n{joker.Opis}";
    }
}
