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
	public List<List<Karta>> ReceGraczy = new List<List<Karta>>();
	private List<Karta> stosZagranych = new List<Karta>();
	private BotGracz botAI = new BotGracz();
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

	public Karta GornaKartaNaStosie { get; private set; }
	public int DlugDobierania { get { return turnManager.DlugDobierania; } }
	public TurnManager TurnManager { get; set; }
	public string WymuszonyKolor { get; set; }
	public Vector2 PozycjaStosuZagranych { get; set; }
	public WyborKoloru InstancjaWyboruKoloru { get; set; }
	public UnoRules UnoManager { get; set; }

	public override void _Ready()
	{
		uIManager = GetNode<UIManager>("UIManager");
		uIManager.Inicjalizuj(this);

		turnManager = new TurnManager(iloscGraczy);
		turnManager.OnTuraRozpoczeta += OnTuraRozpoczeta;

		

		for (int i = 0; i < iloscGraczy; i++)
			ReceGraczy.Add(new List<Karta>());

		for (int i = 1; i < iloscGraczy; i++)
		{
			List<Karta> rekaBota = ReceGraczy[i];
			BotController nowyController = new BotController();
			nowyController.Inicjalizuj(this, i, rekaBota);
			kontroleryBotow.Add(nowyController);
			AddChild(nowyController);
		}

		stanUnoGraczy = new bool[iloscGraczy];
		talia = new DeckManager(this, SzablonKarty);
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

		stosDobierania.InputPickable = true;

		OnTuraRozpoczeta(turnManager.AktualnyGraczIndex);
	}

	private void _OnTaliaPrzetasowano()
	{
		_licznikPrzetasowanWRundzie++;
		OnTaliaPrzetasowano?.Invoke(_licznikPrzetasowanWRundzie);
	}

	private void OnTuraRozpoczeta(int indexGracza)
	{
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
				ReceGraczy[indexGracza].Add(karta);
				if (indexGracza != 0)
					OnAktualizujLicznikBota?.Invoke(indexGracza, ReceGraczy[indexGracza].Count);
			}
		}
		OnRozmiescKarty?.Invoke(ReceGraczy[0]);
	}

	private void DobierzJednaKarte(int indexGracza)
	{
		if (talia.PobierzTalie().Count == 0)
		{
			talia.PrzetasujStosZagranych(stosZagranych, GornaKartaNaStosie);
		}

		if (talia.PobierzTalie().Count == 0) return;

		Karta kartaDoDobrania = talia.PobierzTalie()[talia.PobierzTalie().Count - 1];
		talia.PobierzTalie().RemoveAt(talia.PobierzTalie().Count - 1);

		ReceGraczy[indexGracza].Add(kartaDoDobrania);
		OnKartaDobrano?.Invoke(indexGracza);
		if (indexGracza != 0)
			OnAktualizujLicznikBota?.Invoke(indexGracza, ReceGraczy[indexGracza].Count);
		else
			OnRozmiescKarty?.Invoke(ReceGraczy[0]);
	}

	private void ObslozKlikniecieKarty(Karta kliknietaKarta)
	{
		SprobujZagracKarte(kliknietaKarta, 0);
	}

	public void SprobujZagracKarte(Karta karta, int indexGracza)
	{
		if (karta == null) return;
		if (turnManager.AktualnyGraczIndex != indexGracza) return;
		if (!unoManager.CzyRuchJestLegalny(karta, turnManager.DlugDobierania)) return;

		if (karta.Kolor != "DzikaKarta")
			wymuszonyKolor = null;

		List<Karta> rekaAktywnegoGracza = ReceGraczy[indexGracza];
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
				string wybranyKolor = botAI.WybierzKolor(ReceGraczy[indexGracza - 1]);
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
	}

	public void SprobujDobracKarte(int indexGracza)
	{
		GD.Print($"Kliknięcie na stos przez gracza {indexGracza}. Aktualny gracz: {turnManager.AktualnyGraczIndex}. Panel koloru widoczny: {instancjaWyboruKoloru.Visible}");
		if (turnManager.AktualnyGraczIndex != indexGracza)
			return;
		if (instancjaWyboruKoloru.Visible)
			return;
		if (turnManager.DlugDobierania > 0)
		{
			for (int i = 0; i < turnManager.DlugDobierania; i++)
				DobierzJednaKarte(indexGracza);
			turnManager.DlugDobierania = 0;
		}
		else
			DobierzJednaKarte(indexGracza);

		OnRozmiescKarty?.Invoke(ReceGraczy[0]);
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
}
