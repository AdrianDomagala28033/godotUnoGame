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
    private bool blokadaZmianySceny = false;

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
            ButtonGotowosci.Disabled = true;
        }
    }
    public override void _ExitTree()
    {
        if (NetworkManager != null)
        {
            NetworkManager.OnListaGraczyZmieniona -= AktualizujLicznikGotowych;
        }
    }
    public override void _EnterTree()
    {
        blokadaZmianySceny = false;
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
            long mojeId = Multiplayer.GetUniqueId();
            long idWybierajacego = NetworkManager.KolejkaDraftu[0];           
            bool czyMojaKolej = mojeId == idWybierajacego;

            GD.Print($"[SKLEP DEBUG] Moje ID: {mojeId} vs Wybiera: {idWybierajacego}. Czy moja kolej? {czyMojaKolej}");

            var obecnyGracz = NetworkManager.ListaGraczy.Find(g => g.Id == idWybierajacego);
            if(obecnyGracz != null)
                KtoWybiera.Text = $"Wybiera gracz {obecnyGracz.Nazwa}";
            
            foreach (var node in ListaJokerow.GetChildren())
            {
                if(node is Button btn)
                {
                    bool czyDostepny = NetworkManager.OfertaJokerow.Contains(btn.Text);
                    GD.Print($"[SKLEP DEBUG] Joker {btn.Text} - Dostępny: {czyDostepny}, Disabled będzie: {!czyMojaKolej || !czyDostepny}");
                    btn.Disabled = !czyMojaKolej || !czyDostepny;
                }
            }
        }
        else
        {
            GD.Print("[SKLEP ERROR] Kolejka draftu jest pusta lub null!");
            KtoWybiera.Text = $"Wszyscy gracze wybrali...";
            ButtonGotowosci.Disabled = false;
            foreach (var node in ListaJokerow.GetChildren())
            {
                if(node is Button btn)
                    btn.Disabled = true;
            }
        }
    }
    public void ZglosGotowosc()
    {
        NetworkManager.WyslijGotowosc();
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
                if(wszyscy > 0 && gotowi == wszyscy && !blokadaZmianySceny)
                {
                    blokadaZmianySceny = true;
                    GD.Print("[SERVER] Wszyscy gotowi na Scoreboardzie! Zarządzam przejście do Sklepu.");
                    foreach(var g in NetworkManager.ListaGraczy) g.CzyGotowy = false;
                    NetworkManager.Rpc(nameof(NetworkManager.WyjdzZeSklepu), "res://sceny/rozgrywka/StolGry.tscn");
                }
            }
        }
        else
        {
            ButtonGotowosci.Text = "Dalej";
        }
    }
}
