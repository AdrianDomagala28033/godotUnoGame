using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

public partial class GameClient : Node3D
{
#region pola
    [Export] public Marker3D PozycjaStosuDobierania;
    [Export] public Marker3D PozycjaStosuZagranych;
    [Export] public Marker3D SpawnGraczGlowny;
    [Export] private PackedScene SzablonKarty;
	[Export] private PackedScene SzablonWyboruKoloru;
    // [Export] private PackedScene SzablonDebugShop;
    // [Export] private PackedScene SzablonDebugCardMenu;
    [Export] public UIManager UIManager;
    public NetworkManager NetworkManager {get; set;}
    public DeckManager DeckManager {get; set;}
    public List<Gracz> ListaGraczy {get; set;}
    public int DlugDobierania {get; set;}
    public Karta GornaKartaNaStosie {get; set;}
    public string WymuszonyKolor {get; set;}
    public WyborKoloru InstancjaWyboruKoloru {get; set;}
    public DebugShop InstancjaDebugShop {get; set;}
    public DebugCardMenu InstancjaDebugCardMenu {get; set;}
    public List<Karta> ZaznaczoneKarty {get; set;} = new List<Karta>();
    public StosDobierania stosDobierania {get; set;}
    public List<Karta> WizualnyStosZagrania {get; set;} = new List<Karta>();
    private int licznikZagranychKart = 0;
    public long AktualnyGraczTuryId { get; private set; } = -1;
#endregion
#region eventy
	public event Action OnReceZmienione;
    public event Action<int> OnKartaDobrano;
    public event Action<int> OnTaliaPrzetasowano;
    public event Action<int> OnRundaZakoczona;
    public event Action<string> OnKolorZostalWybrany;
    public event Action<string> OnKolorZmieniony;
    public event Action<string> OnKolorDoUstawienia;
    //public event Action<DaneJokera> OnJokerZdobyty;
    public event Action<List<Karta>> OnRozmiescKarty;
    public event Action<Karta, int> OnKartaZagrana;
    public event Action<int, int> OnAktualizujLicznikBota;
    public event Action<Karta, Vector3, int> OnDodajKarteNaStos;
    public event Action<long> OnTuraUstawiona;
    public event Action OnListaGraczyZmieniona;
    #endregion
    public override void _Ready()
    {
        ListaGraczy = new List<Gracz>();
        NetworkManager = GetNode<NetworkManager>("/root/NetworkManager");

        NetworkManager.OnKartyOdebrane += HandleKartyOdebrane;
        NetworkManager.OnTuraUstawiona += HandleTuraUstawiona;
        NetworkManager.OnStosZaktualizowany += HandleStosZaktualizowany;
        NetworkManager.LiczbaKartZmieniona += HandleLiczbaKartZmieniona;
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

        UIManager = GetNode<UIManager>("WarstwaUI/UIManager");

        DeckManager = new DeckManager();

        UIManager.Inicjalizuj(this);

        InstancjaWyboruKoloru = (WyborKoloru)SzablonWyboruKoloru.Instantiate();
        AddChild(InstancjaWyboruKoloru);
        InstancjaWyboruKoloru.Hide();
        InstancjaWyboruKoloru.KolorWybrany += (wybranyKolor) =>
        {
            GD.Print($"[CLIENT] Kliknięto kolor: {wybranyKolor}. Wysyłam do serwera.");
            NetworkManager.RpcId(1, nameof(NetworkManager.ObsluzWyborKoloru), wybranyKolor);
        };

        // InstancjaDebugShop = (DebugShop)SzablonDebugShop.Instantiate();
        // InstancjaDebugShop.Inicjalizuj(this);
        // AddChild(InstancjaDebugShop);

        // if (SzablonDebugCardMenu != null)
        // {
        //     InstancjaDebugCardMenu = (DebugCardMenu)SzablonDebugCardMenu.Instantiate();
        //     InstancjaDebugCardMenu.Inicjalizuj(this);
        //     AddChild(InstancjaDebugCardMenu);
        // }

        stosDobierania = GetNode<StosDobierania>("StosDobierania");
        stosDobierania.InputEvent += ObsluzKlikniecieStosu;
        stosDobierania.AktualizujWyglad(108);

        var scoreboard = GetNode<Scoreboard>("WarstwaUI/Scoreboard");
        scoreboard.Inicjalizuj(this);

    }

    private void HandleLiczbaKartZmieniona(long idGracza, int nowaIlosc)
    {
        var graczWizualny = ListaGraczy.Find(g => g.IdGracza == idGracza);
        if(graczWizualny != null && !graczWizualny.CzyToGraczLokalny)
        {
            graczWizualny.rekaGracza.Clear();
            for (int i = 0; i < nowaIlosc; i++)
                graczWizualny.rekaGracza.Add(new Karta());
            UIManager?.AktualizujPrzeciwnikow();
        }
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
                nowaKarta.Scale = Vector3.One * 0.8f;
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
        AktualnyGraczTuryId = idGracza;
        if (!IsInstanceValid(this)) return;
        UIManager?.AktualizujPrzeciwnikow(); 
        UIManager?.PokazTureGracza(idGracza == ListaGraczy.Find(g => g.CzyToGraczLokalny)?.IdGracza);
    }
    private void HandleStosZaktualizowany(string kolor, string wartosc)
{
    if (!IsInstanceValid(this)) return;

    licznikZagranychKart++;

    Karta nowaKarta = SzablonKarty.Instantiate<Karta>();
    nowaKarta.Kolor = kolor;
    nowaKarta.Wartosc = wartosc;
    nowaKarta.InputRayPickable = false;
    AddChild(nowaKarta);

    Vector3 baza = (PozycjaStosuZagranych != null) ? PozycjaStosuZagranych.Position : Vector3.Zero;

    float wysokosc = licznikZagranychKart * 0.001f; 
    float losowyX = (float)GD.RandRange(-0.05, 0.05);
    float losowyZ = (float)GD.RandRange(-0.05, 0.05);
    Vector3 celPozycja = baza + new Vector3(losowyX, wysokosc, losowyZ);
    float losowyObrot = (float)GD.RandRange(-180, 180);
    Vector3 celRotacja = new Vector3(90, losowyObrot, 0); 

    nowaKarta.Position = celPozycja + new Vector3(0, 1.0f, 0); 

    nowaKarta.RotationDegrees = new Vector3(-90, losowyObrot + 90, 0);

    var sprite = nowaKarta.GetNodeOrNull<Sprite3D>("Sprite3D");
    if (sprite != null)
    {
        sprite.RenderPriority = licznikZagranychKart; 
        sprite.NoDepthTest = false; 
    }

    var tween = CreateTween().SetParallel(true);
    tween.TweenProperty(nowaKarta, "position", celPozycja, 0.4f)
         .SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);

    tween.TweenProperty(nowaKarta, "rotation_degrees", celRotacja, 0.35f)
         .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

    WizualnyStosZagrania.Add(nowaKarta);
    GornaKartaNaStosie = nowaKarta;

    if (WizualnyStosZagrania.Count > 20)
    {
        var kartaNaDnie = WizualnyStosZagrania[0];
        WizualnyStosZagrania.RemoveAt(0);
        kartaNaDnie.QueueFree();
    }

    if (kolor != "DzikaKArta")
        WymuszonyKolor = null;

    GD.Print($"[CLIENT] Karta na stosie (Index: {licznikZagranychKart})");
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
    private void ObsluzKlikniecieStosu(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            GD.Print("[CLIENT] Kliknięto w stos dobierania!");
            NetworkManager.RpcId(1, nameof(NetworkManager.DobierzKarty));
        }
    }
}
