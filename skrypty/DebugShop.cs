using Godot;
using System;

public partial class DebugShop : CanvasLayer
{
    
    private GameClient logikaGry;
    private Control grid;

    public void Inicjalizuj(GameClient logika)
    {
        this.logikaGry = logika;
        WygenerujPrzyciski();
    }
    public override void _Ready()
    {
        grid = GetNode<Control>("ScrollContainer/Grid");
        this.Hide();
    }
    public void WygenerujPrzyciski()
    {
        // var jokery = JokerFactory.StworzJokery();
        // foreach(var joker in jokery)
        // {
        //     Button btn = new Button();
        //     btn.Text = $"{joker.Nazwa}\n({joker.RzadkoscJokera})";
        //     btn.CustomMinimumSize = new Vector2(200, 100);
        //     var j = joker;
        //     btn.Pressed += () => KupJokera(j);
        //     grid.AddChild(btn);
        // }
    }
    private void KupJokera(Joker joker)
    {
        GD.Print($"[DEBUG] Dodano jokera: {joker.Nazwa}");
        //logikaGry.PrzyznajJokeraGraczowi(joker);
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event is InputEventKey key && key.Pressed && key.Keycode == Key.F1)
        {
            if (Visible) Hide();
            else Show();
        }
    }

}
