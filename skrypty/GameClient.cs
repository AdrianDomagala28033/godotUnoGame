using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameClient : Node2D
{
#region pola
    [Export] private PackedScene SzablonKarty;
	[Export] private PackedScene SzablonWyboruKoloru;
    public NetworkManager NetworkManager {get; set;}
    public UIManager UIManager {get; set;}
    public DeckManager DeckManager {get; set;}
    // public JokerManager JokerManager {get; set;}
    public List<Gracz> ListaGraczy {get; set;}
    public int DlugDobierania {get; set;}
    public Karta GornaKartaNaStosie {get; set;}
    public string WymuszonyKolor {get; set;}
    public WyborKoloru InstancjaWyboruKoloru {get; set;}
    public List<Karta> ZaznaczoneKarty {get; set;} = new List<Karta>();
#endregion
#region eventy
	public event Action OnReceZmienione;
    public event Action<int> OnKartaDobrano;
    public event Action<int> OnTaliaPrzetasowano;
    public event Action<int> OnRundaZakoczona;
    public event Action<string> OnKolorZostalWybrany;
    public event Action<string> OnKolorZmieniony;
    public event Action<string> OnKolorDoUstawienia;
    public event Action<Joker> OnJokerZdobyty;
    public event Action<List<Karta>> OnRozmiescKarty;
    public event Action<Karta, int> OnKartaZagrana;
    public event Action<int, int> OnAktualizujLicznikBota;
    public event Action<Karta, Vector2, int, int> OnDodajKarteNaStos;
    #endregion
    public override void _Ready()
    {
        ListaGraczy = new List<Gracz>();
        NetworkManager = GetNode<NetworkManager>("/root/NetworkManager");
        NetworkManager.OnKartyOdebrane += HandleKartyOdebrane;
        NetworkManager.OnTuraUstawiona += HandleTuraUstawiona;
        NetworkManager.OnStosZaktualizowany += HandleStosZaktualizowany;
        int index = 0;
        bool czyToGraczLokalny = false;
        foreach(DaneGracza gracz in NetworkManager.ListaGraczy)
        {
            if(gracz.Id == Multiplayer.GetUniqueId())
                czyToGraczLokalny = true;
            else
                czyToGraczLokalny = false;
            ListaGraczy.Add(new Gracz(gracz.Nazwa, gracz.Id, czyToGraczLokalny, index));
            index++;
        }

        UIManager = GetNode<UIManager>("UIManager");

        DeckManager = new DeckManager();
        // JokerManager = new JokerManager();

        UIManager.Inicjalizuj(this);

        InstancjaWyboruKoloru = (WyborKoloru)SzablonWyboruKoloru.Instantiate();
        AddChild(InstancjaWyboruKoloru);
        InstancjaWyboruKoloru.Hide();
        InstancjaWyboruKoloru.KolorWybrany += (wybranyKolor) =>
        {
            GD.Print($"[CLIENT] Kliknięto kolor: {wybranyKolor}. Wysyłam do serwera.");
            NetworkManager.RpcId(1, nameof(NetworkManager.ObsluzWyborKoloru), wybranyKolor);
        };

        Area2D stosDobierania = GetNode<Area2D>("StosDobierania");
        stosDobierania.InputEvent += ObsluzKlikniecieStosu;

        var scoreboard = GetNode<Scoreboard>("CanvasLayer/Scoreboard");
        scoreboard.Inicjalizuj(this);

    }

    private void HandleKartyOdebrane(long idGracza)
    {
        if (!IsInstanceValid(this)) return;
        if(idGracza != Multiplayer.GetUniqueId())
            return;

        var graczWizualny = ListaGraczy.Find(g => g.IdGracza == idGracza);
        var daneGracza = NetworkManager.ListaGraczy.Find(d => d.Id == idGracza);
        if(graczWizualny != null && daneGracza != null)
        {
            List<DaneKarty> daneDoOdhaczenia = new List<DaneKarty>(daneGracza.RekaGracza);
            List<Karta> wizualneDoUsuniecia = new List<Karta>();
            foreach (var wizualnaKarta in graczWizualny.rekaGracza)
            {
                var pasujacaWDanych = daneDoOdhaczenia.Find(k => k.Kolor == wizualnaKarta.Kolor && k.Wartosc == wizualnaKarta.Wartosc);
                if(pasujacaWDanych != null)
                    daneDoOdhaczenia.Remove(pasujacaWDanych);
                else
                    wizualneDoUsuniecia.Add(wizualnaKarta);
            }
            foreach (var doUsuniecia in wizualneDoUsuniecia)
            {
                graczWizualny.rekaGracza.Remove(doUsuniecia);
                doUsuniecia.QueueFree();
            }
            foreach (var noweDane in daneDoOdhaczenia)
            {
                Karta nowaKarta = SzablonKarty.Instantiate<Karta>();
                nowaKarta.Kolor = noweDane.Kolor;
                nowaKarta.Wartosc = noweDane.Wartosc;
                nowaKarta.OnKartaKliknieta += ObsluzKlikniecieKarty;
                AddChild(nowaKarta);
                graczWizualny.rekaGracza.Add(nowaKarta);
            }
            OnRozmiescKarty?.Invoke(graczWizualny.rekaGracza);
        }
    }
    private void HandleTuraUstawiona(long idGracza)
    {
        if (!IsInstanceValid(this)) return;
        foreach (Gracz gracz in ListaGraczy)
        {
            if(gracz.IdGracza == idGracza)
            {
                if(idGracza == Multiplayer.GetUniqueId())
                    UIManager.PokazTureGracza(true);
                else
                    UIManager.PokazTureGracza(false);
            }
        }
    }
    private void HandleStosZaktualizowany(string kolor, string wartosc)
    {
        if (!IsInstanceValid(this)) return;
        if(GornaKartaNaStosie != null)
            GornaKartaNaStosie.QueueFree();

        Karta nowaKarta = SzablonKarty.Instantiate<Karta>();
        nowaKarta.Kolor = kolor;
        nowaKarta.Wartosc = wartosc;

        nowaKarta.Position = new Vector2(650, 375); 
        nowaKarta.ZIndex = 0;
        nowaKarta.InputPickable = false;
        AddChild(nowaKarta);

        GornaKartaNaStosie = nowaKarta;
        if(kolor != "DzikaKArta")
            WymuszonyKolor = null;
        GD.Print($"[CLIENT] Nowa karta na stosie: {kolor} {wartosc}");
    }
    private bool CzyMogeZaznaczyc(Karta karta)
    {
        //TODO: sprawdz jokera
        if(ZaznaczoneKarty.Count == 0)
            return true;
        else if(ZaznaczoneKarty[0].Wartosc == karta.Wartosc)
            return true;
        else
            return false;
    }
    public void ObsluzKlikniecieKarty(Karta kartaKliknieta, int indexPrzycisku)
    {
        if(indexPrzycisku == 1)
        {
            string[] kolory;
            string[] wartosci;
            if(ZaznaczoneKarty.Count > 0)
            {
                kolory = new string[ZaznaczoneKarty.Count];
                wartosci= new string[ZaznaczoneKarty.Count];
                for (int i = 0; i < ZaznaczoneKarty.Count; i++)
                {
                    kolory[i] = ZaznaczoneKarty[i].Kolor;
                    wartosci[i] = ZaznaczoneKarty[i].Wartosc;
                }
                GD.Print($"[CLIENT] Wysyłam żądanie zagrania {kolory.Length} kart.");
                NetworkManager.RpcId(1, nameof(NetworkManager.ZagrajKarty), kolory, wartosci);
                ZaznaczoneKarty.Clear();
            }
            else if(ZaznaczoneKarty.Count == 0)
            {
                kolory = new string[1];
                wartosci = new string[1];
                kolory[0] = kartaKliknieta.Kolor;
                wartosci[0] = kartaKliknieta.Wartosc;
                GD.Print($"[CLIENT] Wysyłam żądanie zagrania {kolory.Length} kart.");
                NetworkManager.RpcId(1, nameof(NetworkManager.ZagrajKarty), kolory, wartosci);
                ZaznaczoneKarty.Clear();
            }
        }
        else
        {
            if(ZaznaczoneKarty.Contains(kartaKliknieta))
            {
                kartaKliknieta.UstawZaznaczenie(false);
                ZaznaczoneKarty.Remove(kartaKliknieta);
            }
            else
            {
                if (CzyMogeZaznaczyc(kartaKliknieta))
                {
                    ZaznaczoneKarty.Add(kartaKliknieta);
                    kartaKliknieta.UstawZaznaczenie(true);
                }
            }
        }
    }
    private void ObsluzKlikniecieStosu(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            GD.Print("[CLIENT] Kliknięto w stos dobierania!");
            NetworkManager.RpcId(1, nameof(NetworkManager.DobierzKarty));
        }
    }
}
