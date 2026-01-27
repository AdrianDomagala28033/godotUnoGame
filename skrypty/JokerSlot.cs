using Godot;
using System;

public partial class JokerSlot : TextureRect
{
    private DaneJokera joker;
    public event Action<string, string> OnHover;
    public event Action OnHoverExit;

    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        MouseFilter = MouseFilterEnum.Stop;
    }

    public void Inicjalizuj(DaneJokera joker)
    {
        this.joker = joker;
        if (joker.Ikona != null) 
            Texture = joker.Ikona;
    }
    public void OnMouseEntered()
    {
        if(joker != null)
        {
            OnHover?.Invoke(joker.Nazwa, joker.Opis);
        }
    }
    public void OnMouseExited()
    {
        if(joker != null)
        {
            OnHoverExit?.Invoke();
        }
    }
}
