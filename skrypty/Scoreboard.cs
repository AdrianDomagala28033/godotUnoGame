using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Scoreboard : Control
{
    [Export] private VBoxContainer wyniki;
    //[Export] private Button przycisk;
    public event Action onGuzikKlikniety;

    public override void _Ready()
    {
        var guzik = GetNode<Button>("Panel/LayoutGlowny/Button");
        guzik.Pressed += ObslugaKlikniecia;
        this.Hide();
    }

    private void ObslugaKlikniecia()
    {
        onGuzikKlikniety?.Invoke();
    }


    public void WyswietlWyniki(List<Gracz> listaGraczy, int numerRundy)
    {
        this.Show();
        foreach (var child in wyniki.GetChildren())
        {
            child.QueueFree();
        }
        HBoxContainer naglowek = new HBoxContainer();
        Label info = new Label();
        info.Text = $"Koniec rundy {numerRundy}";
        naglowek.AddChild(info);
        wyniki.AddChild(naglowek);
        List<Gracz> graczePosortowani = listaGraczy.OrderBy(g => g.Miejsce).ToList();
        foreach (Gracz gracz in graczePosortowani)
        {
            HBoxContainer dane = new HBoxContainer();
            Label wynik = new Label();
            wynik.Text = $"{gracz.Nazwa} --------------------------- miejsce {gracz.Miejsce}";
            dane.AddChild(wynik);
            wyniki.AddChild(dane);
        }
    }
}
