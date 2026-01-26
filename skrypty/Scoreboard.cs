using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Scoreboard : Control
{
    [Export] private VBoxContainer wyniki;
    [Export] private Button przycisk;
    [Export] private Label labelOczekiwania;
    public bool wyswietlaRankingOgolny = false;
    public GameClient gameClient;
    public int numerRundy {get; set;}
    private bool blokadaZmianySceny = false;

    public override void _Ready()
    {
        if (przycisk == null)
            przycisk = GetNode<Button>("Panel/LayoutGlowny/Button");
        if(labelOczekiwania == null)
            labelOczekiwania = GetNodeOrNull<Label>("Panel/LabelOczekiwania");
        this.Hide();
    }
    public override void _ExitTree()
    {
        if (gameClient != null && gameClient.NetworkManager != null)
        {
            gameClient.NetworkManager.OnListaGraczyZmieniona -= AktualizujLicznikGotowych;
        }
    }
    public override void _EnterTree()
    {
        blokadaZmianySceny = false;
    }
    public void Inicjalizuj(GameClient client)
    {
        gameClient = client;
        gameClient.NetworkManager.OnListaGraczyZmieniona += AktualizujLicznikGotowych;
        AktualizujLicznikGotowych();
    }

    private void _on_button_pressed()
    {
        GD.Print("[SCOREBOARD] Kliknięto przycisk. Stan ogólny: " + wyswietlaRankingOgolny);
        if (!wyswietlaRankingOgolny)
        {
            wyswietlaRankingOgolny = true;
            WyswietlWyniki();
        }
        else
        {
            if(przycisk != null)
            {
                przycisk.Disabled = true;
                przycisk.Text = "Czekaj...";
            }
            gameClient.NetworkManager.Rpc(nameof(NetworkManager.ZglosGotowosc));
        }
    }
    public void AktualizujLicznikGotowych()
    {
        if (!GodotObject.IsInstanceValid(gameClient) || !GodotObject.IsInstanceValid(labelOczekiwania)) 
            return;
        int gotowi =  gameClient.NetworkManager.ListaGraczy.Count(g => g.CzyGotowy);
        int wszyscy = gameClient.ListaGraczy.Count;
        labelOczekiwania.Text = $"Oczekiwanie na graczy: {gotowi}/{wszyscy}";
        if (Multiplayer.IsServer())
        {
            if(wszyscy > 0 && gotowi == wszyscy && !blokadaZmianySceny)
            {
                blokadaZmianySceny = true;
                GD.Print("[SERVER] Wszyscy gotowi na Scoreboardzie! Zarządzam przejście do Sklepu.");
                foreach(var g in gameClient.NetworkManager.ListaGraczy) g.CzyGotowy = false;
                long[] kolejka = gameClient.NetworkManager.ListaGraczy.OrderBy(g => g.Miejsce).Select(g => g.Id).ToArray();
                string[] oferta = new string[] { 
                    "Joker Zmiana Koloru", 
                    "Joker +4", 
                    "Joker Blokada" 
                };
                gameClient.NetworkManager.Rpc(nameof(NetworkManager.PrzejdzDoSklepu), "res://sceny/rozgrywka/Sklep.tscn", kolejka, oferta);
            }
        }
    }

    public void WyswietlWyniki()
    {
        this.Show();
        if (przycisk != null) przycisk.Disabled = false;
        foreach (var child in wyniki.GetChildren())
        {
            child.QueueFree();
        }
        HBoxContainer naglowek = new HBoxContainer();
        Label info = new Label();
        List<Gracz> graczePosortowani;
        string typWyswietlania;
        if (!wyswietlaRankingOgolny)
        {
            info.Text = $"Koniec rundy {numerRundy}";
            graczePosortowani = gameClient.ListaGraczy.OrderBy(g => g.Miejsce).ToList();
            typWyswietlania = "miejsce";
            if (przycisk != null) przycisk.Text = "Pokaż ranking ogólny";
        }
        else
        {
            info.Text = $"Klasyfikacja generalna"; 
            graczePosortowani = gameClient.ListaGraczy.OrderByDescending(g => g.Wynik).ToList();
            typWyswietlania = "wynik";
            if (przycisk != null) przycisk.Text = "Przejdź do sklepu";
            AktualizujLicznikGotowych();
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
            var daneZSieci = gameClient.NetworkManager.ListaGraczy.Find(g => g.Id == gracz.IdGracza);
            string status = (daneZSieci != null && daneZSieci.CzyGotowy) ? "[Gotowy] " : "";
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
        if(labelOczekiwania != null) labelOczekiwania.Text = "";
        this.Hide();
    }
}
