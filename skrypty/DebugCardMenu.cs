using Godot;
using System;

public partial class DebugCardMenu : CanvasLayer
{
    private GameClient logikaGry;
    private Control grid;

    private string[] kolory = { "Czerwony", "Niebieski", "Zielony", "Zolty" };
    private string[] wartosci = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "Stop", "ZmianaKierunku", "+2" };

    public void Inicjalizuj(GameClient logika)
    {
        logikaGry = logika;
    }
    public override void _Ready()
    {
        grid = GetNode<Control>("ScrollContainer/GridContainer");
        WygenerujPrzyciski();
        Hide();
    }
    private void WygenerujPrzyciski()
    {
        foreach(var kolor in kolory)
        {
            foreach(var wartosc in wartosci)
            {
                StworzPrzycisk(kolor, wartosc);
            }
        }
        StworzPrzycisk("DzikaKarta", "ZmianaKoloru");
        StworzPrzycisk("DzikaKarta", "+4");
    }

    private void StworzPrzycisk(string kolor, string wartosc)
    {
        Button btn = new Button();
        btn.Text = $"{kolor}\n{wartosc}";
        btn.CustomMinimumSize = new Vector2(120,60);

        Color bg = new Color(0.5f, 0.5f, 0.5f);
        if (kolor == "Czerwony") bg = new Color(0.8f, 0.2f, 0.2f);
        if (kolor == "Niebieski") bg = new Color(0.2f, 0.2f, 0.8f);
        if (kolor == "Zielony") bg = new Color(0.2f, 0.8f, 0.2f);
        if (kolor == "Zolty") bg = new Color(0.8f, 0.8f, 0.2f);
        if (kolor == "DzikaKarta") bg = new Color(0.8f, 0.8f, 0.8f);

        btn.Modulate = bg;

        btn.Pressed += () =>
        {
            GD.Print($"[DEBUG] Proszę o kartę: {kolor} {wartosc}");
            logikaGry.NetworkManager.Rpc(nameof(NetworkManager.Debug_PoprosOKarte), kolor, wartosc);
        };
        grid.AddChild(btn);
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.F2)
        {
            if (Visible) Hide(); else Show();
        }
    }
    
}
