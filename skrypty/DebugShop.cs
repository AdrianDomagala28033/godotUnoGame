using Godot;
using System;

public partial class DebugShop : CanvasLayer
{
    
    private GameClient gameClient;
    private Control grid;
    

    public void Inicjalizuj(GameClient gameClient)
    {
        this.gameClient = gameClient;
    }
    public override void _Ready()
    {
        grid = GetNode<Control>("ScrollContainer/Grid");
        WygenerujPrzyciski();
        this.Hide();
    }
    public void WygenerujPrzyciski()
    {
        var jokery = JokerManager.PobierzWszystkieId();
        foreach(var j in jokery)
        {
            var joker = JokerManager.PobierzJokera(j);
            Button btn = new Button();
            btn.Text = $"{joker.Nazwa}\n({joker.RzadkoscJokera})";
            btn.CustomMinimumSize = new Vector2(200, 100);
            btn.Pressed += () => KupJokera(joker);
            grid.AddChild(btn);
        }
    }
    private void KupJokera(DaneJokera joker)
    {
        GD.Print($"[DEBUG] Dodano jokera: {joker.Nazwa}");
        gameClient.NetworkManager.Rpc(nameof(NetworkManager.Debug_PoprosOJokera), joker.Id);
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
