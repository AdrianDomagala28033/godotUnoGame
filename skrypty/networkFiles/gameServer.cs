using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;

public partial class GameServer : Node
{
    public Dictionary<long, DaneGracza> ListaGraczy = new Dictionary<long, DaneGracza>();
    public DeckManager deckManager;
    public List<DaneKarty> stosZagranych = new List<DaneKarty>();
    private long aktualnyGraczId;
    private int kierunekGry = 1;
    public TurnManager turnManager {get; set;}
    public UnoRules unoRules {get; set;}
    public NetworkManager networkManager {get; set;}
    public string WymuszonyKolor {get; set;} = null;
    public DaneKarty GornaKartaNaStosie
    {
        get
        {
            if(stosZagranych.Count > 0)
                return stosZagranych[stosZagranych.Count - 1];
            else
                return null;
        }
    }
    public Queue<long> KolejkaDraftu {get; set;}
    public int NumerRundy {get; set;}


    public override void _Ready()
    {
        deckManager = new DeckManager();
        networkManager = GetParent<NetworkManager>();
        GD.Print("GameServer gotowy do działania.");
    }
    public void PrzygotujRozgrywke(List<DaneGracza> gracze)
    {
        GD.Print("Serwer przygotowuje rozgrywkę...");
        if(NumerRundy == 1)
        {
            ListaGraczy.Clear();
            foreach(DaneGracza gracz in gracze)
            {
                DaneGracza kopiaDlaSerwera = new DaneGracza(gracz.Id, gracz.Nazwa, gracz.CzyGotowy);
                ListaGraczy.Add(gracz.Id, kopiaDlaSerwera);
            }
            turnManager = new TurnManager(ListaGraczy);
            turnManager.OnKierunekZmieniony += HandleKierunekZmieniony;
            unoRules = new UnoRules(this, turnManager);
            AddChild(unoRules);
        }
        else
        {
            foreach (DaneGracza gracz in ListaGraczy.Values)
            {
                gracz.CzyUkonczyl = false;
                gracz.Miejsce = 0;
                gracz.RekaGracza.Clear();
            }
            stosZagranych.Clear();
            turnManager.DlugDobierania = 0;
            WymuszonyKolor = null;
            kierunekGry = 1;
        }
        deckManager.StworzTalie();
        deckManager.PotasujTalie();
        RozdajKartyGraczowi();
        do
        {
            WystawPierwszaKarte();
        }while(stosZagranych[stosZagranych.Count - 1].Kolor == "DzikaKarta");
        aktualnyGraczId = LosujGracza();
        turnManager.UstawWybranegoGracza(aktualnyGraczId);
        networkManager.Rpc(nameof(NetworkManager.UstawTure), aktualnyGraczId);
        networkManager.Rpc(nameof(NetworkManager.ZaktualizujKolor), stosZagranych[stosZagranych.Count - 1].Kolor);
        AktywujJokeryNaStartTury(aktualnyGraczId);
        GD.Print($"Rozgrywka rozpoczęta! Zaczyna gracz: {aktualnyGraczId}");
    }

    private void RozdajKartyGraczowi()
    {
        foreach(DaneGracza gracz in ListaGraczy.Values)
        {
            for (int i = 0; i < 7; i++)
                gracz.RekaGracza.Add(deckManager.WydajKarte());
            WyslijDaneKarty(gracz.Id);
            networkManager.Rpc(nameof(NetworkManager.ZaktualizujLiczbeKartGracza), gracz.Id, gracz.RekaGracza.Count);
        } 
        
    }
    private void WyslijDaneKarty(long id)
    {
        List<string> kolory = new List<string>();
        List<string> wartosci = new List<string>();
        foreach (DaneKarty karta in ListaGraczy[id].RekaGracza)
        {
            kolory.Add(karta.Kolor);
            wartosci.Add(karta.Wartosc);
        }
        networkManager.RpcId(id, nameof(NetworkManager.OdbierzKarty), kolory.ToArray(), wartosci.ToArray());
    }
    private long LosujGracza()
    {
        Random rng = new Random();
        List<long> idGraczy = new List<long>(ListaGraczy.Keys);
        return idGraczy[rng.Next(idGraczy.Count)];
    }
    private void WystawPierwszaKarte()
    {
        DaneKarty pierwszaKarta = deckManager.WydajKarte();
        stosZagranych.Add(pierwszaKarta);

        networkManager.Rpc(nameof(NetworkManager.ZaktualizujStos), pierwszaKarta.Kolor, pierwszaKarta.Wartosc);
    }
    public void ObsluzZagranieKart(long idGracza, string[] kolory, string[] wartosci)
    {
        GD.Print($"[SERVER] Odebrałem żądanie od {idGracza}. Kart: {kolory.Length}");
        if(aktualnyGraczId != idGracza)
        {
            GD.Print($"[SERVER] To nie tura gracza {idGracza}! Aktualna: {aktualnyGraczId}");
            return;
        }  
        List<DaneKarty> kopiaReki = new List<DaneKarty>(ListaGraczy[idGracza].RekaGracza);
        List<DaneKarty> kartyDoZagrania = new List<DaneKarty>();
        for (int i = 0; i < kolory.Length; i++)
        {
            DaneKarty kartaWRece = kopiaReki.Find(karta => karta.Kolor == kolory[i] && karta.Wartosc.Trim() == wartosci[i]);
            if(kartaWRece != null)
            {
                kartyDoZagrania.Add(kartaWRece);
                kopiaReki.Remove(kartaWRece);   
            }
            else
            {
                GD.Print($"[SERVER ERROR] Nie znaleziono karty w ręce gracza! Szukano: {kolory[i]} {wartosci[i]}");
                return;
            }
        }
        if (unoRules.CzyRuchJestLegalny(kartyDoZagrania, turnManager.DlugDobierania))
        {
            foreach (DaneKarty karta in kartyDoZagrania)
            {
                ListaGraczy[idGracza].RekaGracza.Remove(karta);
                stosZagranych.Add(karta);
                unoRules.ZastosujEfektKarty(karta, true);
                if(ListaGraczy[idGracza].RekaGracza.Count == 0)
                {
                    GD.Print($"[SERVER] Gracz {idGracza} pozbył się wszystkich kart!");
                    ListaGraczy[idGracza].CzyUkonczyl = true;
                    ListaGraczy[idGracza].Miejsce = ListaGraczy.Count(g => g.Value.CzyUkonczyl);

                    if(ListaGraczy.Count(g => !g.Value.CzyUkonczyl) == 1)
                    {
                        ZakonczRunde();
                    }
                }
            }
            DaneKarty ostatnia = kartyDoZagrania[kartyDoZagrania.Count - 1];
            WyslijDaneKarty(idGracza);
            networkManager.Rpc(nameof(NetworkManager.ZaktualizujStos), ostatnia.Kolor, ostatnia.Wartosc);
            if(ListaGraczy.Count(g => !g.Value.CzyUkonczyl) > 1)
            {
                if(ostatnia.Kolor != "DzikaKarta" && ostatnia.Wartosc != "+4")
                {
                    turnManager.ZakonczTure();
                    aktualnyGraczId = turnManager.AktualnyGraczId;
                    networkManager.Rpc(nameof(NetworkManager.UstawTure), aktualnyGraczId);
                    WymuszonyKolor = null;
                    AktywujJokeryNaStartTury(aktualnyGraczId);
                }
                else
                {
                    GD.Print($"[SERVER] Zagrano Dziką Kartę. Czekam na wybór koloru od gracza {idGracza}");
                    networkManager.RpcId(idGracza, nameof(NetworkManager.PokazWyborKoloru));
                }
            }
            int ileKartMaHost = ListaGraczy[1].RekaGracza.Count;
            GD.Print($"[DEBUG] Nowa tura dla: {aktualnyGraczId}. Host (ID 1) ma kart: {ileKartMaHost}");
            networkManager.Rpc(nameof(NetworkManager.ZaktualizujLiczbeKartGracza), idGracza, ListaGraczy[idGracza].RekaGracza.Count);
            networkManager.Rpc(nameof(NetworkManager.ZaktualizujKolor), ostatnia.Kolor);
        }
        networkManager.Rpc(nameof(NetworkManager.ZaktualizujDlug), turnManager.DlugDobierania);
    }

    public void UstawWybranyKolor(long idNadawcy, string wybranyKolor)
    {
        WymuszonyKolor = wybranyKolor;
        networkManager.Rpc(nameof(NetworkManager.ZaktualizujKolor), wybranyKolor);
        GD.Print($"[SERVER] Kolor zmieniony na: {wybranyKolor}. Kończę turę gracza {idNadawcy}.");
        turnManager.ZakonczTure();
        aktualnyGraczId = turnManager.AktualnyGraczId;
        networkManager.Rpc(nameof(NetworkManager.UstawTure), aktualnyGraczId);
        AktywujJokeryNaStartTury(aktualnyGraczId);
    }
    private void HandleKierunekZmieniony(int nowyKierunek)
    {
        var networkManager = GetParent<NetworkManager>();
        if(networkManager != null)
        {
            bool czyZgodnie = (nowyKierunek == 1);
            networkManager.Rpc(nameof(NetworkManager.ZaktualizujKierunekGry), czyZgodnie);
        }
    }

    public void ObsluzDobranie(long idNadawcy, int iloscDobierania)
    {
        if(aktualnyGraczId != idNadawcy) return;
        if(turnManager.DlugDobierania == 0)
        {
            if(deckManager.talia.Count == 0) deckManager.PrzetasujStosZagranych(stosZagranych, stosZagranych[stosZagranych.Count - 1]);  
            ListaGraczy[aktualnyGraczId].RekaGracza.Add(deckManager.WydajKarte());
        }
        else
        {
            for (int i = 0; i < iloscDobierania; i++)
            {
                if(deckManager.talia.Count == 0) deckManager.PrzetasujStosZagranych(stosZagranych, stosZagranych[stosZagranych.Count - 1]);  
                ListaGraczy[aktualnyGraczId].RekaGracza.Add(deckManager.WydajKarte());
            }
            turnManager.DlugDobierania = 0;
            networkManager.Rpc(nameof(NetworkManager.ZaktualizujDlug), turnManager.DlugDobierania);
        }
        WyslijDaneKarty(idNadawcy);
        networkManager.Rpc(nameof(NetworkManager.ZaktualizujLiczbeKartGracza), idNadawcy, ListaGraczy[idNadawcy].RekaGracza.Count);
        turnManager.ZakonczTure();
        aktualnyGraczId = turnManager.AktualnyGraczId;
        networkManager.Rpc(nameof(NetworkManager.UstawTure), aktualnyGraczId);
        AktywujJokeryNaStartTury(aktualnyGraczId);
    }
    private void ZakonczRunde()
    {
        long[] idGraczy = new long[ListaGraczy.Count];
        int[] miejscaGraczy = new int[ListaGraczy.Count];
        int[] wynikGraczy = new int[ListaGraczy.Count];
        var przegrany = ListaGraczy.Values.FirstOrDefault(g => !g.CzyUkonczyl);
        int i = 0;

        if(przegrany != null)
        {
            przegrany.CzyUkonczyl = true;
            przegrany.Miejsce = ListaGraczy.Count;
            GD.Print($"[SERVER] Runda zakończona. Przegrany (ostatnie miejsce): {przegrany.Nazwa}");
        }
        foreach (var gracz in ListaGraczy.Values)
        {
            gracz.PrzypiszPunktyZaMiejsce(ListaGraczy);
            GD.Print($"[WYNIKI] {gracz.Nazwa}: Miejsce {gracz.Miejsce}, Wynik całkowity: {gracz.Wynik}");
            idGraczy[i] = gracz.Id;
            miejscaGraczy[i] = gracz.Miejsce;
            wynikGraczy[i] = gracz.Wynik;
            i++;
        }
        foreach (var gracz in ListaGraczy.Values)
        {
            gracz.CzyGotowy = false;
            networkManager.Rpc(nameof(NetworkManager.ZaktualizujStanGotowosciClienta),gracz.Id, false);
        }
        networkManager.Rpc(nameof(NetworkManager.PokazTabliceWynikow), idGraczy, miejscaGraczy, wynikGraczy, NumerRundy);
    }

    public void RozpocznijFazeSklepu()
    {
        GD.Print("[SERVER] Wszyscy gotowi! Rozpoczynam fazę sklepu.");
        long[] kolejka = ListaGraczy.Values.OrderBy(g => g.Miejsce).Select(g => g.Id).ToArray();
        KolejkaDraftu = new Queue<long>(kolejka);
        string[] oferta = GenerujOferteSklepu(ListaGraczy.Count + 1);
        networkManager.Rpc(nameof(NetworkManager.PrzejdzDoSklepu), "res://sceny/rozgrywka/Sklep.tscn", kolejka, oferta);
    }
    private string[] GenerujOferteSklepu(int ilosc)
    {
        string[] pula = JokerManager.PobierzWszystkieId();
        if(pula.Length == 0) return new string[0];
        Random rng = new Random();
        return pula.OrderBy(x => rng.Next()).Take(ilosc).ToArray();
    }
    public void Debug_PrzyznajJokera(string idJokera, long idGracza)
    {
        var gracz = ListaGraczy.Values.FirstOrDefault(g => g.Id == idGracza);
        if(gracz == null) return;
        gracz.PosiadaneJokery.Add(idJokera);
        networkManager.Rpc(nameof(NetworkManager.NadajJokeraGraczowi), gracz.Id, idJokera);
    }

    public void Debug_DodajKarteGraczowi(long idGracza, string kolor, string wartosc)
    {
        if (ListaGraczy.ContainsKey(idGracza))
        {
            DaneKarty nowaKarta = new DaneKarty(kolor, wartosc);
            ListaGraczy[idGracza].RekaGracza.Add(nowaKarta);
            
            GD.Print($"[SERVER DEBUG] Dodałem kartę {kolor} {wartosc} dla gracza {idGracza}");
            WyslijDaneKarty(idGracza);
            networkManager.Rpc(nameof(NetworkManager.ZaktualizujLiczbeKartGracza), idGracza, ListaGraczy[idGracza].RekaGracza.Count);
        }
    }
    public void AktywujJokeryNaStartTury(long idGracza)
    {
        if (ListaGraczy.ContainsKey(idGracza))
        {
            foreach (string idJokera in ListaGraczy[idGracza].PosiadaneJokery)
            {
                var joker = JokerManager.PobierzJokera(idJokera);
                if(joker.Efekty != null)
                    joker.Efekty[0].NaPoczatkuTury(this, idGracza);
            }
        }
    }
}