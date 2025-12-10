using Godot;
using System;

public partial class JokerSlot : TextureRect
{
    private Joker joker;
    public event Action<string, string> OnHover;
    public event Action OnHoverExit;

    public void Inicjalizuj(Joker joker)
    {
        this.joker = joker;
    }
    public override void _GuiInput(InputEvent @event)
    {
        if(@event is InputEventMouseMotion)
        {
            
        }
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
