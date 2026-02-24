using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class NetworkManager : Node
{
    public string Adres {get; set;} = "127.0.0.1";
    public int Port {get; set;} = 8910;
    public ENetMultiplayerPeer Peer {get; set;}
    public List<DaneGracza> ListaGraczy = new List<DaneGracza>();
    public string NazwaGraczaLokalnego = "gracz";
    public long[] KolejkaDraftu {get; set;}
    public string[] OfertaJokerow {get; set;}
    public event Action OnPolaczono;
    public event Action OnDolaczono;
    public event Action OnListaGraczyZmieniona;
    public event Action<long> OnKartyOdebrane;
    public event Action<long> OnTuraUstawiona;
    public event Action<string, string> OnStosZaktualizowany;
    [Signal] public delegate void PokazDraftEventHandler(string[] idJokerow);
    [Signal] public delegate void JokeryZmienioneEventHandler(string[] aktualneJokery);
    [Signal] public delegate void LiczbaKartZmienionaEventHandler(long idGracza, int nowaIlosc);

    public override void _Ready()
    {
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.PeerConnected += OnPeerConnected;
        JokerManager.ZaladujJokery();
    }
    #region obsluga lobby i menu
    public void HostujGre(string nazwa)
    {
        if(GetNodeOrNull<Node>("GameServer") != null)
            GetNode<Node>("GameServer").QueueFree();
        Peer = new ENetMultiplayerPeer();
        var wynik = Peer.CreateServer(Port);
        if (wynik == Error.Ok)
        {
            OnPolaczono?.Invoke();
            GD.Print("Udało sie hostować gre");
            Multiplayer.MultiplayerPeer = Peer;
            Rpc(nameof(ZarejestrujNowegoGracza), 1, nazwa, false);
            GameServer gameServer = new GameServer();
            gameServer.Name = "GameServer";
            gameServer.NumerRundy = 1;
            this.AddChild(gameServer);
        }
    }
    public void DolaczDoGry(string kodDolaczenia, string nazwaGracza)
    {
        Peer = new ENetMultiplayerPeer();
        NazwaGraczaLokalnego = nazwaGracza;
        var wynik = Peer.CreateClient(kodDolaczenia, Port);
        if(wynik == Error.Ok)
        {
            Multiplayer.MultiplayerPeer = Peer; 
        }
    }
    private void OnConnectedToServer()
    {
        long id = Multiplayer.GetUniqueId();
        string nazwa = NazwaGraczaLokalnego;
        RpcId(1, nameof(ZadanieDolaczenia), id, nazwa);
        GD.Print("Hurra! Udało się połączyć z serwerem!");
        OnPolaczono?.Invoke();
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public void ZadanieDolaczenia(long id, string nazwa)
    {
        if (!Multiplayer.IsServer()) return;
        Rpc(nameof(ZarejestrujNowegoGracza), id, nazwa, false);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ZarejestrujNowegoGracza(long id, string nazwa, bool gotowosc)
    {
        if (ListaGraczy.Any(g => g.Id == id)) return;
        ListaGraczy.Add(new DaneGracza(id, nazwa, gotowosc));
        OnDolaczono?.Invoke();
        GD.Print($"Dodano gracza ID: {id}");
    }
    public void OnPeerConnected(long id)
    {
        if (Multiplayer.IsServer())
        {
            foreach (DaneGracza gracz in ListaGraczy)
            {
                RpcId(id, nameof(ZarejestrujNowegoGracza), gracz.Id, gracz.Nazwa, gracz.CzyGotowy);
            }
        }
        GD.Print("Dołączył gracz o ID: " + id);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ZaladujGre(string sciezka)
    {
        ZmienScene(sciezka);
        if (Multiplayer.IsServer())
        {
            var timer = GetTree().CreateTimer(0.5f);
            timer.Timeout += () =>
            {
                var server = GetNode<GameServer>("GameServer");
                server.PrzygotujRozgrywke(ListaGraczy);
            };
        }
    }
    #endregion
    #region przesył informacji
    public void WyslijGotowosc()
    {
        RpcId(1, nameof(ObsluzZgloszenieGotowosci));
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ObsluzZgloszenieGotowosci()
    {
        long idNadawcy = Multiplayer.GetRemoteSenderId();
        if (idNadawcy == 0) idNadawcy = 1;

        var gracz = ListaGraczy.FirstOrDefault(g => g.Id == idNadawcy);
        if(gracz != null)
        {
            bool nowyStan = !gracz.CzyGotowy;
            ZaktualizujStanGotowosciClienta(idNadawcy, nowyStan);
            Rpc(nameof(ZaktualizujStanGotowosciClienta), idNadawcy, nowyStan);
            foreach (var peer in Multiplayer.GetPeers())
                if (peer != 1)
                    RpcId(peer, nameof(ZaktualizujStanGotowosciClienta), idNadawcy, nowyStan);
            
            GD.Print($"[SERVER] Gracz {idNadawcy} zmienił gotowość na: {nowyStan}");
        }
        else
            GD.PrintErr($"[SERVER] BŁĄD! Nie znaleziono gracza o ID: {idNadawcy}");
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    public void ZaktualizujStanGotowosciClienta(long idGracza, bool czyGotowy)
    {
        var gracz = ListaGraczy.FirstOrDefault(g => g.Id == idGracza);
        if (gracz == null)
        {
            GD.PrintErr($"[NetworkManager] Błąd! Próba ustawienia gotowości dla nieznanego gracza ID: {idGracza}");
            return;
        }

        gracz.CzyGotowy = czyGotowy;
        OnListaGraczyZmieniona?.Invoke();
    }

    private bool SprawdzCzyWszyscyGotowi()
    {
        if(ListaGraczy.All(g => g.CzyGotowy))
            return true;
        else
            return false;
    }
    public void ZmienScene(string sciezka)
    {
            GD.Print($"[NETWORK] Zmieniam scenę na: {sciezka}");
            GetTree().ChangeSceneToFile(sciezka);
    }
#region rozgrywka
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void OdbierzKarty(string[] kolory, string[] wartosci)
    {
        foreach(DaneGracza gracz in ListaGraczy)
        {
            if(Multiplayer.GetUniqueId() == gracz.Id)
            {
                gracz.RekaGracza.Clear();
                for (int i = 0; i < kolory.Length; i++)
                {
                    gracz.RekaGracza.Add(new DaneKarty(kolory[i], wartosci[i]));
                }
                OnKartyOdebrane?.Invoke(gracz.Id);
            }
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void UstawTure(long id)
    {
        OnTuraUstawiona?.Invoke(id);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ZaktualizujStos(string kolor, string wartosc)
    {
        OnStosZaktualizowany?.Invoke(kolor, wartosc);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ZagrajKarty(string[] kolory, string[] wartosci)
    {
        GD.Print($"[NETWORK] Otrzymano RPC ZagrajKarty. IsServer: {Multiplayer.IsServer()}");
        if (Multiplayer.IsServer())
        {
            long idGracza = Multiplayer.GetRemoteSenderId();
            if(idGracza == 0)
                idGracza = 1;
            GD.Print($"[NETWORK] Przekazuję do GameServer. ID Gracza: {idGracza}");
            var server = GetNode<GameServer>("GameServer");
            server.ObsluzZagranieKart(idGracza, kolory, wartosci);
            GD.Print($"Gracz {idGracza} chce zagrac {kolory.Length} kart");
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PokazWyborKoloru()
    {
        GD.Print("[NETWORK] Otrzymano rozkaz: Pokaż Wybór Koloru!");
        var client = GetNodeOrNull<GameClient>("/root/StolGry");
        if(client != null)
        {
        var lokalnyGracz = client.ListaGraczy.Find(g => g.IdGracza == Multiplayer.GetUniqueId());

        if (lokalnyGracz != null && lokalnyGracz.rekaGracza.Count > 0)
        {
            GD.Print("[NETWORK] Mam karty, więc pokazuję wybór koloru.");
            client.InstancjaWyboruKoloru.Show();
        }
        else
        {
            GD.Print("[NETWORK] Nie mam kart (koniec gry), ignoruję żądanie wyboru koloru.");
        }
    }
        else
        {
            GD.PrintErr("[NETWORK] BŁĄD! Nie znaleziono węzła GameClient na scenie!");
            foreach(Node child in GetTree().Root.GetChildren())
                GD.Print($"[TREE] Dziecko Roota: {child.Name}");
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ObsluzWyborKoloru(string wybranyKolor)
    {
        if (Multiplayer.IsServer())
        {
            long idNadawcy = Multiplayer.GetRemoteSenderId();
            if (idNadawcy == 0) idNadawcy = 1;
            GD.Print($"[NETWORK] Gracz {idNadawcy} wybrał kolor: {wybranyKolor}");
            var server = GetNode<GameServer>("GameServer");
            server.UstawWybranyKolor(idNadawcy, wybranyKolor);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ZaktualizujDlug(int nowyDlug)
    {
        var client = GetNodeOrNull<GameClient>("/root/StolGry");
        if(client != null)
            client.UIManager.UstawDlug(nowyDlug);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ZaktualizujKolor(string kolor)
    {
        var client = GetNodeOrNull<GameClient>("/root/StolGry");
        if(client != null)
            client.UIManager.UstawKolor(kolor);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ZaktualizujKierunekGry(bool czyZgodnie)
    {
        var client = GetNodeOrNull<GameClient>("/root/StolGry");
        if(client != null && client.UIManager != null)
        {
            client.UIManager.UstawKierunekStrzalek(czyZgodnie);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void DobierzKarty()
    {
        if (Multiplayer.IsServer())
        {
            long idNadawcy = Multiplayer.GetRemoteSenderId();
            if(idNadawcy == 0) idNadawcy = 1;
            var server = GetNode<GameServer>("GameServer");
            server.ObsluzDobranie(idNadawcy, server.turnManager.DlugDobierania);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ZaktualizujLiczbeKartGracza(long idGracza, int nowaIlosc)
    {
        var gracz = ListaGraczy.FirstOrDefault(g => g.Id == idGracza);
        if(gracz != null)
        {
            gracz.RekaGracza.Clear();
            for (int i = 0; i < nowaIlosc; i++)
                gracz.RekaGracza.Add(new DaneKarty("Rewers", "0"));
            EmitSignal(SignalName.LiczbaKartZmieniona, idGracza, nowaIlosc);
            GD.Print($"[NETWORK] Zaktualizowano liczbę kart gracza {idGracza} na: {nowaIlosc}");
        }
    }
#endregion
#region sklep i tablica wynikow
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PokazTabliceWynikow(long[] idGraczy, int[] miejscaGraczy, int[] wynikGraczy, int numerRundy)
    {
        var client = GetTree().Root.GetNode<GameClient>("StolGry");
        var scoreboard = GetTree().Root.GetNode<Scoreboard>("StolGry/WarstwaUI/Scoreboard");
        for (int i = 0; i < idGraczy.Length; i++)
        {
            long id = idGraczy[i];
            if(client != null)
            {
                Gracz graczWizualny = client.ListaGraczy.Find(g => g.IdGracza == id);
                if(graczWizualny != null)
                {
                    graczWizualny.Miejsce = miejscaGraczy[i];
                    graczWizualny.Wynik = wynikGraczy[i];
                    graczWizualny.CzyUkonczyl = true;
                }
            }
            DaneGracza daneGracza = ListaGraczy.Find(g => g.Id == id);
            if(daneGracza != null)
            {
                daneGracza.Miejsce = miejscaGraczy[i];
                daneGracza.Wynik = wynikGraczy[i];
            }
        }
        scoreboard.numerRundy = numerRundy;
        scoreboard.gameClient = client;
        if(client != null && client.InstancjaWyboruKoloru != null) 
             client.InstancjaWyboruKoloru.Hide();
        scoreboard.WyswietlWyniki();
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void PrzejdzDoSklepu(string sciezka, long[] kolejkaDraftu, string[] ofertaJokerow)
    {
        OfertaJokerow = ofertaJokerow; 
        KolejkaDraftu = kolejkaDraftu;
        foreach(var g in ListaGraczy) g.CzyGotowy = false;
        if (Multiplayer.IsServer())
        {
            var server = GetNode<GameServer>("GameServer");
            if(server != null)
                server.KolejkaDraftu = new Queue<long>(kolejkaDraftu);
            else
                GD.PrintErr("[NETWORK] Uwaga: Nie znaleziono GameServer przy przejściu do sklepu!"); 
        }
            GD.Print($"[NETWORK] Zmieniam scenę na: {sciezka}");
            ZmienScene(sciezka);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ObsluzWyborJokera(string nazwa)
    {
        if(!Multiplayer.IsServer()) return;
        long idNadawcy = Multiplayer.GetRemoteSenderId();
        var server = GetNode<GameServer>("GameServer");
        if(server.KolejkaDraftu.Count > 0 && server.KolejkaDraftu.Peek() == idNadawcy)
        {
            GD.Print($"Gracz {idNadawcy} wybrał: {nazwa}");
            if (server.ListaGraczy.ContainsKey(idNadawcy))
            {
                server.ListaGraczy[idNadawcy].PosiadaneJokery.Add(nazwa);
                Rpc(nameof(NadajJokeraGraczowi), idNadawcy, nazwa);
            }
            List<string> NiewybraneJokery = new List<string>(OfertaJokerow);
            OfertaJokerow = NiewybraneJokery.Where(j => j != nazwa).ToArray();
            server.KolejkaDraftu.Dequeue();
            Rpc(nameof(ZaktualizujStanSklepu), server.KolejkaDraftu.ToArray(), OfertaJokerow);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void NadajJokeraGraczowi(long idGracza, string idJokera)
    {
        var gracz = ListaGraczy.Find(g => g.Id == idGracza);
        if(gracz != null)
        {
            gracz.PosiadaneJokery.Add(idJokera);
            EmitSignal(SignalName.JokeryZmienione, gracz.PosiadaneJokery.ToArray());
            GD.Print($"[NETWORK] Zsynchronizowano: Gracz {gracz.Nazwa} ma teraz jokera {idJokera}");
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void ZaktualizujStanSklepu(long[] kolejka, string[] oferta)
    {
        KolejkaDraftu = kolejka;
        OfertaJokerow = oferta;
        EmitSignal(SignalName.PokazDraft, oferta);
        var sklep = GetTree().Root.GetNodeOrNull<Sklep>("Sklep");
        if(sklep != null)
            sklep.AktualizujStanPrzyciskow();
        else
            GD.PrintErr("[NETWORK] Błąd: Nie znaleziono węzła 'Sklep' na scenie!");
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void WyjdzZeSklepu(string sciezka)
    {
        foreach(var g in ListaGraczy)
        {
            g.CzyGotowy = false;
            g.RekaGracza.Clear();
        }
        if (Multiplayer.IsServer())
        {
            var server = GetNode<GameServer>("GameServer");
            server.NumerRundy++;
        }
        ZaladujGre(sciezka);
    }
    #region debug menu
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void Debug_PoprosOJokera(string idJokera)
    {
        if (Multiplayer.IsServer())
        {
            long idGracza = Multiplayer.GetRemoteSenderId();
            if(idGracza == 0) idGracza = 1;
            var server = GetNode<GameServer>("GameServer");
            server.Debug_PrzyznajJokera(idJokera, idGracza);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void Debug_PoprosOKarte(string kolor, string wartosc)
    {
        if (Multiplayer.IsServer())
        {
            long idNadawcy = Multiplayer.GetRemoteSenderId();
            if (idNadawcy == 0) idNadawcy = 1; // Fix dla Hosta

            var server = GetNodeOrNull<GameServer>("GameServer");
            if (server != null)
            {
                server.Debug_DodajKarteGraczowi(idNadawcy, kolor, wartosc);
            }
        }
    }
    #endregion
    #endregion
    #endregion
}