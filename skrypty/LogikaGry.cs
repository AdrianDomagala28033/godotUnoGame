using Godot;
using System;
using System.Collections.Generic;

public partial class LogikaGry : Node2D
{
	[Export] private PackedScene SzablonKarty;
	[Export] private PackedScene SzablonWyboruKoloru;
	private WyborKoloru instancjaWyboruKoloru;
	private DeckManager talia;
	private TurnManager turnManager;
	private List<Karta> stosZagranych = new List<Karta>();
	private List<BotController> kontroleryBotow = new List<BotController>();
	private int licznikZIndexStosu = 100;
	private int iloscGraczy = 4;
	private string wymuszonyKolor = null;
	private Vector2 pozycjaStosuZagranych = new Vector2(650, 375);
	private Vector2 pozycjaStosuDobierania = new Vector2(810, 375);
	private int _licznikPrzetasowanWRundzie;
	private bool[] stanUnoGraczy;
	private UIManager uIManager;
	private UnoRules unoManager;

	public event Action<string> OnKolorZostalWybrany;
	public event Action<string> OnKolorZmieniony;
	public event Action<Karta, int> OnKartaZagrana;
	public event Action<int> OnKartaDobrano;
	public event Action<int> OnTaliaPrzetasowano;
	public event Action<int> OnRundaZakoczona;
	public event Action<List<Karta>> OnRozmiescKarty;
	public event Action<Karta, Vector2, int, int> OnDodajKarteNaStos;
	public event Action<int, int> OnAktualizujLicznikBota;
	public event Action<string> OnKolorDoUstawienia;
	public event Action OnReceZmienione;
	public event Action<Joker> OnJokerZdobyty;

	public Karta GornaKartaNaStosie { get; private set; }
	public int DlugDobierania { get { return turnManager.DlugDobierania; } }
	public TurnManager TurnManager { get { return turnManager; } }
	public string WymuszonyKolor { get; set; }
	public Vector2 PozycjaStosuZagranych { get; set; }
	public WyborKoloru InstancjaWyboruKoloru { get; set; }
	public UnoRules UnoManager { get; set; }
	public JokerManager JokerManager { get; set; }
	public DeckManager Talia { get; set; }
	public List<Gracz> ListaGraczy { get; set; }
	public UIManager UIManager { get; set; }

	public override void _Ready()
	{
		ListaGraczy = new List<Gracz>();
		var graczCzlowiek = new Gracz("ty", true, 0);
		ListaGraczy.Add(graczCzlowiek);


		for (int i = 1; i < iloscGraczy; i++)
		{
			var bot = new Gracz($"Bot {i}", false, i);
			ListaGraczy.Add(bot);

			BotController nowyController = new BotController();
			nowyController.Inicjalizuj(this, ListaGraczy);
			nowyController.UstawIndexBota(i);
			kontroleryBotow.Add(nowyController);
			AddChild(nowyController);
		}

		uIManager = GetNode<UIManager>("UIManager");
		uIManager.Inicjalizuj(this);

		turnManager = new TurnManager(ListaGraczy);
		turnManager.OnTuraRozpoczeta += OnTuraRozpoczeta;


		stanUnoGraczy = new bool[iloscGraczy];
		talia = new DeckManager(this, SzablonKarty);
		this.Talia = talia;
		talia.StworzTalie();
		talia.PotasujTalie();
		talia.OnTaliaPrzetasowano += _OnTaliaPrzetasowano;

		foreach (var k in talia.PobierzTalie())
			k.OnKartaKliknieta += ObslozKlikniecieKarty;

		RozdajKartyGraczowi(7);
		WystawPierwszaKarte();

		instancjaWyboruKoloru = (WyborKoloru)SzablonWyboruKoloru.Instantiate();
		AddChild(instancjaWyboruKoloru);
		instancjaWyboruKoloru.Hide();
		instancjaWyboruKoloru.KolorWybrany += _OnKolorZostalWybrany;
		InstancjaWyboruKoloru = instancjaWyboruKoloru;

		unoManager = new UnoRules(this, turnManager, uIManager);
		this.UnoManager = unoManager;

		Area2D stosDobierania = GetNode<Area2D>("StosDobierania");
		stosDobierania.Position = pozycjaStosuDobierania;

		JokerManager = new JokerManager();

		stosDobierania.InputPickable = true;

		OnTuraRozpoczeta(turnManager.AktualnyGraczIndex);

		var debugScene = GD.Load<PackedScene>("res://sceny/DebugShop.tscn");
		var debugShop = (DebugShop)debugScene.Instantiate();
		AddChild(debugShop);
		debugShop.Inicjalizuj(this);

		var debugCardsScene = GD.Load<PackedScene>("res://sceny/DebugCardMenu.tscn");
		if (debugCardsScene != null)
		{
			var debugCards = (DebugCardMenu)debugCardsScene.Instantiate();
			AddChild(debugCards);
			debugCards.Inicjalizuj(this);
		}

	}

	private void _OnTaliaPrzetasowano()
	{
		_licznikPrzetasowanWRundzie++;
		OnTaliaPrzetasowano?.Invoke(_licznikPrzetasowanWRundzie);
	}

	private void OnTuraRozpoczeta(int indexGracza)
	{
		string kto = indexGracza == 0 ? "TWÓJ RUCH" : $"RUCH BOTA {indexGracza}";
    	GD.Print($"\n================ {kto} ================");
		uIManager.PokazTureGracza(false);

		if (indexGracza == 0)
		{
			uIManager.PokazTureGracza(true);
			if (turnManager.DlugDobierania > 0)
				uIManager.UstawDlug(turnManager.DlugDobierania);
		}
		else
		{
			int indexKontrolera = indexGracza - 1;

			Timer timerBota = new Timer();
			timerBota.WaitTime = 1.0;
			timerBota.OneShot = true;
			timerBota.Timeout += kontroleryBotow[indexKontrolera].RozpocznijTure;
			AddChild(timerBota);
			timerBota.Start();
		}
	}

	private void _OnKolorZostalWybrany(string wybranyKolor)
	{
		wymuszonyKolor = wybranyKolor;
		GornaKartaNaStosie.Kolor = wybranyKolor;
		instancjaWyboruKoloru.Hide();
		OnKolorZostalWybrany?.Invoke(wybranyKolor);
		OnKolorDoUstawienia?.Invoke(wymuszonyKolor);
		turnManager.ZakonczTure();
	}

	private void WystawPierwszaKarte()
	{
		Karta startowaKarta = null;
		bool czyLegalnaKartaStartowa = false;

		while (!czyLegalnaKartaStartowa)
		{
			if (talia.PobierzTalie().Count == 0) return;

			startowaKarta = talia.PobierzTalie()[talia.PobierzTalie().Count - 1];
			talia.PobierzTalie().RemoveAt(talia.PobierzTalie().Count - 1);

			if (startowaKarta.Kolor != "DzikaKarta" &&
				startowaKarta.Wartosc != "Stop" &&
				startowaKarta.Wartosc != "+2" &&
				startowaKarta.Wartosc != "ZmianaKierunku")
				czyLegalnaKartaStartowa = true;
			else
				talia.PobierzTalie().Add(startowaKarta);
		}
		GornaKartaNaStosie = startowaKarta;
		OnKolorDoUstawienia?.Invoke(startowaKarta.Kolor);
		stosZagranych.Add(startowaKarta);
		OnDodajKarteNaStos?.Invoke(startowaKarta, pozycjaStosuZagranych, licznikZIndexStosu, 0);

		if (startowaKarta.Wartosc == "ZmianaKierunku" || startowaKarta.Wartosc == "Stop")
		{
			unoManager.ZastosujEfektKarty(startowaKarta, false);
			turnManager.ZakonczTure();
		}
	}

	private void RozdajKartyGraczowi(int ilosc)
	{
		for (int i = 0; i < ilosc; i++)
		{
			for (int indexGracza = 0; indexGracza < iloscGraczy; indexGracza++)
			{
				if (talia.PobierzTalie().Count == 0)
					continue;

				Karta karta = talia.PobierzTalie()[talia.PobierzTalie().Count - 1];
				talia.PobierzTalie().RemoveAt(talia.PobierzTalie().Count - 1);
				ListaGraczy[indexGracza].rekaGracza.Add(karta);
				if (indexGracza != 0)
					OnAktualizujLicznikBota?.Invoke(indexGracza, ListaGraczy[indexGracza].rekaGracza.Count);
			}
		}
		OnRozmiescKarty?.Invoke(ListaGraczy[0].rekaGracza);
	}

	private void DobierzJednaKarte(Gracz gracz)
	{
		if (talia.PobierzTalie().Count == 0)
		{
			talia.PrzetasujStosZagranych(stosZagranych, GornaKartaNaStosie);
		}

		if (talia.PobierzTalie().Count == 0) return;

		Karta kartaDoDobrania = talia.PobierzTalie()[talia.PobierzTalie().Count - 1];
		talia.PobierzTalie().RemoveAt(talia.PobierzTalie().Count - 1);

		gracz.rekaGracza.Add(kartaDoDobrania);
		OnKartaDobrano?.Invoke(gracz.Index);
		if (!gracz.JestCzlowiekiem)
			OnAktualizujLicznikBota?.Invoke(gracz.Index, gracz.rekaGracza.Count);
		else
			OnRozmiescKarty?.Invoke(ListaGraczy[0].rekaGracza);
	}

	private void ObslozKlikniecieKarty(Karta kliknietaKarta)
	{
		SprobujZagracKarte(kliknietaKarta, 0);
	}

	public void SprobujZagracKarte(Karta karta, int indexGracza)
	{
		string kto = indexGracza == 0 ? "GRACZ (Ty)" : $"BOT {indexGracza}";
    	GD.Print($"--- [RUCH] {kto} próbuje zagrać: {karta.Kolor} {karta.Wartosc} ---");
		if (karta == null) return;
		if (turnManager.AktualnyGraczIndex != indexGracza) return;
		if (!unoManager.CzyRuchJestLegalny(karta, turnManager.DlugDobierania)) return;

		if (karta.Kolor != "DzikaKarta")
			wymuszonyKolor = null;

		List<Karta> rekaAktywnegoGracza = ListaGraczy[indexGracza].rekaGracza;
		rekaAktywnegoGracza.Remove(karta);

		int iloscKartPoZagraniu = rekaAktywnegoGracza.Count;
		stanUnoGraczy[indexGracza] = (iloscKartPoZagraniu == 1);

		if (indexGracza != 0)
			OnAktualizujLicznikBota?.Invoke(indexGracza, rekaAktywnegoGracza.Count);
		else
			OnRozmiescKarty?.Invoke(rekaAktywnegoGracza);

		if (iloscKartPoZagraniu == 0)
		{
			GetTree().Paused = true;
			return;
		}

		OnKartaZagrana?.Invoke(karta, indexGracza);
		stosZagranych.Add(karta);
		GornaKartaNaStosie = karta;
		licznikZIndexStosu++;
		OnDodajKarteNaStos?.Invoke(karta, pozycjaStosuZagranych, licznikZIndexStosu, indexGracza);
		if(JokerManager != null)
			JokerManager.SprawdzAktywacje(karta, this, ListaGraczy[indexGracza]);

		bool jestCzlowiekiem = (indexGracza == 0);
		unoManager.ZastosujEfektKarty(karta, jestCzlowiekiem);

		if (karta.Kolor == "DzikaKarta")
		{
			if (jestCzlowiekiem)
			{
				instancjaWyboruKoloru.Show();
			}
			else
			{
				string wybranyKolor = ListaGraczy[indexGracza].WybierzKolor(ListaGraczy[indexGracza].rekaGracza);
				wymuszonyKolor = wybranyKolor;
				GornaKartaNaStosie.Kolor = wybranyKolor;
				OnKolorZmieniony?.Invoke(wybranyKolor);
				turnManager.ZakonczTure();
			}
		}
		else
		{
			turnManager.ZakonczTure();
		}
		GD.Print($"[SUKCES] {kto} zagrał kartę. Zostaje mu kart: {(indexGracza == 0 ? ListaGraczy[indexGracza].rekaGracza.Count - 1 : ListaGraczy[indexGracza].rekaGracza.Count)}");
	}

	public void SprobujDobracKarte(int indexGracza)
	{
		string kto = indexGracza == 0 ? "GRACZ (Ty)" : $"BOT {indexGracza}";
    	GD.Print($"--- [RUCH] {kto} decyduje się dobrać kartę ---");
		if (turnManager.AktualnyGraczIndex != indexGracza)
			return;
		if (instancjaWyboruKoloru.Visible)
			return;
		if (turnManager.DlugDobierania > 0)
		{
			for (int i = 0; i < turnManager.DlugDobierania; i++)
				DobierzJednaKarte(ListaGraczy[indexGracza]);
			GD.Print($"[AKCJA] {kto} płaci karę: dobiera {turnManager.DlugDobierania} kart.");
			turnManager.DlugDobierania = 0;
		}
        else
        {
            GD.Print($"[AKCJA] {kto} dobiera 1 kartę z talii.");
			DobierzJednaKarte(ListaGraczy[indexGracza]);
        }

		OnRozmiescKarty?.Invoke(ListaGraczy[0].rekaGracza);
		turnManager.ZakonczTure();
	}
	private void _OnStosDobieraniaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (instancjaWyboruKoloru.Visible)
			return;
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			SprobujDobracKarte(0);
		}
	}
	public void PrzyznajJokeraGraczowi(Joker nowyJoker)
    {
        ListaGraczy[0].DodajJokera(nowyJoker);
		OnJokerZdobyty?.Invoke(nowyJoker);
    }
	public void Debug_DodajKarte(string kolor, string wartosc)
    {
        Karta nowaKarta = (Karta)SzablonKarty.Instantiate();
		nowaKarta.Kolor = kolor;
		nowaKarta.Wartosc = wartosc;
		ListaGraczy[0].rekaGracza.Add(nowaKarta);
		AddChild(nowaKarta);
		nowaKarta.OnKartaKliknieta += ObslozKlikniecieKarty;
		nowaKarta.Show();
		nowaKarta.InputPickable = true;
		nowaKarta.ZIndex = 10;
		OnRozmiescKarty?.Invoke(ListaGraczy[0].rekaGracza);
		GD.Print($"[CHEAT] Dodano kartę: {kolor} {wartosc}");
    }
}
