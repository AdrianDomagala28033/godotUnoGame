using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Scoreboard : Control
{
    [Export] private VBoxContainer wyniki;
    //[Export] private Button przycisk;
    public event Action onGuzikKlikniety;
    public bool wyswietlaRankingOgolny = false;
    public LogikaGry logikaGry;
    public int numerRundy;

    public override void _Ready()
    {
        var guzik = GetNode<Button>("Panel/LayoutGlowny/Button");
        guzik.Pressed += ObslugaKlikniecia;
        this.Hide();
    }

    private void ObslugaKlikniecia()
    {
        if (!wyswietlaRankingOgolny)
        {
            wyswietlaRankingOgolny = true;
            WyswietlWyniki();
        }
        else
            onGuzikKlikniety?.Invoke();
    }


    public void WyswietlWyniki()
    {
        this.Show();
        foreach (var child in wyniki.GetChildren())
        {
            child.QueueFree();
        }
        HBoxContainer naglowek = new HBoxContainer();
        Label info = new Label();
        List<Gracz> graczePosortowani;
        string typWyswietlania;
        var guzik = GetNode<Button>("Panel/LayoutGlowny/Button");
        if (!wyswietlaRankingOgolny)
        {
            info.Text = $"Koniec rundy {numerRundy}";
            graczePosortowani = logikaGry.ListaGraczy.OrderBy(g => g.Miejsce).ToList();
            typWyswietlania = "miejsce";
        }
        else
        {
            info.Text = $"Klasyfikacja generalna"; 
            graczePosortowani = logikaGry.ListaGraczy.OrderByDescending(g => g.Wynik).ToList();
            typWyswietlania = "wynik";
            guzik.Text = "Rozpocznij następną turę";
        }
        naglowek.AddChild(info);
        wyniki.AddChild(naglowek);
        WypelnijListe(graczePosortowani, typWyswietlania);
    }
    public void WypelnijListe(List<Gracz> listaGraczy, string parametr)
    {
        foreach (Gracz gracz in listaGraczy)
        {
            HBoxContainer dane = new HBoxContainer();
            Label wynik = new Label();
            if(parametr == "miejsce")
                wynik.Text = $"{gracz.Miejsce}. {gracz.Nazwa} --------------------------- miejsce {gracz.Miejsce}";
            else if(parametr == "wynik")
                wynik.Text = $"{gracz.Miejsce}. {gracz.Nazwa} --------------------------- {gracz.Wynik} pkt";
            dane.AddChild(wynik);
            wyniki.AddChild(dane);
        }
    }
    public void ZresetujStan()
    {
        wyswietlaRankingOgolny = false;
        this.Hide();
    }
}
