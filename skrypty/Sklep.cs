using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Sklep : Control
{
    [Export] private Label KtoWybiera {get; set;}
    [Export] private HBoxContainer ListaJokerow {get; set;}
    [Export] private Button ButtonGotowosci {get; set;}
    public NetworkManager NetworkManager {get; set;}

    public override void _Ready()
    {
        NetworkManager = GetNodeOrNull<NetworkManager>("/root/NetworkManager");
        if(NetworkManager != null)
        {
            GD.Print($"[SKLEP] Załadowano. Pierwszy wybiera ID: {(NetworkManager.KolejkaDraftu != null && NetworkManager.KolejkaDraftu.Length > 0 ? NetworkManager.KolejkaDraftu[0] : 0)}");
            GenerujOferte(NetworkManager.OfertaJokerow);
            AktualizujStanPrzyciskow();
            ButtonGotowosci.Pressed += ZglosGotowosc;
            NetworkManager.OnListaGraczyZmieniona += AktualizujLicznikGotowych;
            AktualizujLicznikGotowych();
        }
    }
    public override void _ExitTree()
    {
        if (NetworkManager != null)
        {
            NetworkManager.OnListaGraczyZmieniona -= AktualizujLicznikGotowych;
        }
    }

    private void GenerujOferte(string[] oferta)
    {
        if (oferta == null) return;
        foreach (string nazwa in oferta)
        {
            Button btn = new Button();
            btn.Pressed += () => OnJokerWybrany(nazwa);
            btn.Text = nazwa;
            btn.CustomMinimumSize = new Vector2(150, 60);
            ListaJokerow.AddChild(btn);
        }
    }

    private void OnJokerWybrany(string nazwa)
    {
        NetworkManager.RpcId(1, nameof(NetworkManager.ObsluzWyborJokera), nazwa );
    }


    public void AktualizujStanPrzyciskow()
    {
        if (NetworkManager.KolejkaDraftu != null && NetworkManager.KolejkaDraftu.Length > 0)
        {
            bool czyMojaKolej = Multiplayer.GetUniqueId() == NetworkManager.KolejkaDraftu[0];
            var obecnyGracz = NetworkManager.ListaGraczy.Find(g => g.Id == NetworkManager.KolejkaDraftu[0]);
            if(obecnyGracz != null)
                KtoWybiera.Text = $"Wybiera gracz {obecnyGracz.Nazwa}";
            foreach (var node in ListaJokerow.GetChildren())
            {
                if(node is Button btn)
                {
                    bool czyDostepny = NetworkManager.OfertaJokerow.Contains(btn.Text);
                    btn.Disabled = !czyMojaKolej || !czyDostepny;
                }
            }
        }
        else
        {
            KtoWybiera.Text = $"Wszyscy gracze wybrali, oczekiwanie na gotowość wszystkich graczy...";
            foreach (var node in ListaJokerow.GetChildren())
            {
                if(node is Button btn)
                    btn.Disabled = true;
            }
        }
    }
    public void ZglosGotowosc()
    {
        NetworkManager.Rpc(nameof(NetworkManager.ZglosGotowosc));
    }
    private void AktualizujLicznikGotowych()
    {
        if (NetworkManager.ListaGraczy.FirstOrDefault(g => g.Id == Multiplayer.GetUniqueId()).CzyGotowy)
        {
            int gotowi =  NetworkManager.ListaGraczy.Count(g => g.CzyGotowy);
            int wszyscy = NetworkManager.ListaGraczy.Count;
            ButtonGotowosci.Text = $"{gotowi}/{wszyscy}";
            if (Multiplayer.IsServer())
            {
                if(wszyscy > 0 && gotowi == wszyscy)
                {
                    GD.Print("[SERVER] Wszyscy gotowi na Scoreboardzie! Zarządzam przejście do Sklepu.");
                    foreach(var g in NetworkManager.ListaGraczy) g.CzyGotowy = false;
                    NetworkManager.Rpc(nameof(NetworkManager.WyjdzZeSklepu), "res://sceny/rozgrywka/stol_gry.tscn");
                }
            }
        }
        else
        {
            ButtonGotowosci.Text = "Dalej";
        }
    }
}
