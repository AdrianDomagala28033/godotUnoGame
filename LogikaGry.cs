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
    [Export] private UiBota _uiBot1;
    [Export] private UiBota _uiBot2;
    [Export] private UiBota _uiBot3;
    [Export] private Label etykietaTuryGracza;
    private Vector2 pozycjaStosuZagranych = new Vector2(650, 375);
	private Vector2 pozycjaStosuDobierania = new Vector2(810, 375);
	private int _licznikPrzetasowanWRundzie;
	private bool[] stanUnoGraczy;
	#region eventy
	public event Action<Karta, int> OnKartaZagrano;
	public event Action<int> OnKartaDobrano;
	public event Action<int> OnTaliaPrzetasowano;
	public event Action<int> OnRundaZakocznona;
	#endregion
	#region get set
	public Karta GornaKartaNaStosie { get; private set; }
	public int DlugDobierania {get { return turnManager.DlugDobierania; }}
	#endregion

	public override void _Ready()
	{
		turnManager = new TurnManager(iloscGraczy);
		turnManager.OnTuraRozpoczeta += OnTuraRozpoczeta;
		for (int i = 0; i < iloscGraczy; i++)
		{
			ReceGraczy.Add(new List<Karta>());
		}

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

		OnTuraRozpoczeta(turnManager.AktualnyGraczIndex);
		GetNode<Area2D>("StosDobierania").Position = pozycjaStosuDobierania;
	}
	private void _OnTaliaPrzetasowano()
    {
        _licznikPrzetasowanWRundzie++;
        OnTaliaPrzetasowano?.Invoke(_licznikPrzetasowanWRundzie);
    }
	private void OnTuraRozpoczeta(int indexGracza)
	{
		etykietaTuryGracza.Hide();
		_uiBot1.UstawAktywny(false);
		_uiBot2.UstawAktywny(false);
		_uiBot3.UstawAktywny(false);
		if (indexGracza == 0)
		{
			etykietaTuryGracza.Show();
			if (turnManager.DlugDobierania > 0)
				GD.Print($"Masz dług {turnManager.DlugDobierania} kart! Zagrywasz +2/+4 albo dobierasz.");
		}
		else
		{
			if (indexGracza == 1) _uiBot1.UstawAktywny(true);
			if (indexGracza == 2) _uiBot2.UstawAktywny(true);
			if (indexGracza == 3) _uiBot3.UstawAktywny(true);

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
			{
				czyLegalnaKartaStartowa = true;
			}
			else
			{
				talia.PobierzTalie().Add(startowaKarta);
				talia.PobierzTalie();
			}
		}
		GornaKartaNaStosie = startowaKarta;
		stosZagranych.Add(startowaKarta);
		AddChild(startowaKarta);
		if (startowaKarta.Wartosc == "ZmianaKierunku" || startowaKarta.Wartosc == "Stop")
    {
        GD.Print($"Zasada: Karta startowa {startowaKarta.Wartosc} aktywuje efekt.");
        ZastosujEfektKarty(startowaKarta, false);
        turnManager.ZakonczTure();
    }
    
    startowaKarta.ZagrajNaStol(pozycjaStosuZagranych, licznikZIndexStosu);
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
				if (indexGracza == 0)
					AddChild(karta);
				else
				{
					karta.InputPickable = false;
					karta.Hide();
					AktualizujUILicznikBota(indexGracza);
				}
			}
		}
		RozmiescKartyWRece();
	}

	

	public bool CzyRuchJestLegalny(Karta kartaDoZagrania, Karta gornaKarta, int dlug)
	{
		if (dlug > 0)
			return kartaDoZagrania.Wartosc == "+2" || kartaDoZagrania.Wartosc == "+4";
		if (kartaDoZagrania.Kolor == "DzikaKarta")
			return true;
		if (wymuszonyKolor != null)
			return kartaDoZagrania.Kolor == wymuszonyKolor;
		if (kartaDoZagrania.Kolor == GornaKartaNaStosie.Kolor)
			return true;
		if (kartaDoZagrania.Wartosc == GornaKartaNaStosie.Wartosc)
			return true;
		return false;
	}

	private void DobierzJednaKarte(int indexGracza) 
{
    if (talia.PobierzTalie().Count == 0)
    {
        talia.PrzetasujStosZagranych(stosZagranych, GornaKartaNaStosie);
        // Event OnTaliaPrzetasowano jest już podpięty w _Ready, więc to zadziała
    }

    if (talia.PobierzTalie().Count == 0) return; // Zabezpieczenie

    Karta kartaDoDobrania = talia.PobierzTalie()[talia.PobierzTalie().Count - 1];
    talia.PobierzTalie().RemoveAt(talia.PobierzTalie().Count - 1);

		ReceGraczy[indexGracza].Add(kartaDoDobrania);
    if (indexGracza == 0)
    {
        AddChild(kartaDoDobrania);
        kartaDoDobrania.Show();
        kartaDoDobrania.InputPickable = true;
        kartaDoDobrania.ZIndex = 10;
    }
    else
    {
			kartaDoDobrania.InputPickable = false;
			AktualizujUILicznikBota(indexGracza);
    }
    
    OnKartaDobrano?.Invoke(indexGracza);
}

	private void RozmiescKartyWRece()
	{
		List<Karta> rekaGracza = ReceGraczy[0];
		int iloscKart = rekaGracza.Count;
		float szerokoscKarty = 150;
		float szerokoscEkranu = GetViewportRect().Size.X;
		float margines = 100;

		float maxDostepnaSzerokosc = szerokoscEkranu - (2 * margines);
		float wymaganaSzerokosc = iloscKart * szerokoscKarty;

		float odstep;
		float skala;

		if (wymaganaSzerokosc > maxDostepnaSzerokosc)
		{
			odstep = (maxDostepnaSzerokosc - szerokoscKarty) / (iloscKart - 1);
			odstep = Mathf.Max(odstep, 20);

			skala = maxDostepnaSzerokosc / wymaganaSzerokosc;
			skala = Mathf.Min(skala, 1.0f);
		}
		else
		{
			odstep = szerokoscKarty * 0.9f;
			skala = 1.0f;
		}
		float szerokoscCalejReki = (iloscKart - 1) * odstep + szerokoscKarty;
		float pozycjaStartowaX = (szerokoscEkranu / 2) - (szerokoscCalejReki / 2);
		float pozycjaY = 780;

		for (int i = 0; i < iloscKart; i++)
		{
			Karta karta = rekaGracza[i];
			Vector2 nowaPozycja = new Vector2(pozycjaStartowaX + (i * odstep), pozycjaY);

			karta.UstawSkale(skala);

			karta.CreateTween().TweenProperty(karta, "position", nowaPozycja, 0.2);
			karta.UstawOryginalnaPozycje(pozycjaY);
			karta.ZIndex = 10 + i;
		}
	}

	private void ObslozKlikniecieKarty(Karta kliknietaKarta)
	{
		SprobujZagracKarte(kliknietaKarta, 0);
	}

	public void SprobujZagracKarte(Karta karta, int indexGracza)
	{
		if (turnManager.AktualnyGraczIndex != indexGracza)
		{
			if (indexGracza == 0)
				karta.AnulujZagranie();
			return;
		}
		if (!CzyRuchJestLegalny(karta, GornaKartaNaStosie, turnManager.DlugDobierania))
		{
			if (indexGracza == 0)
				karta.AnulujZagranie();
			return;
		}
		if (karta.Kolor != "DzikaKarta")
		{
			wymuszonyKolor = null;
		}
		List<Karta> rekaAktywnegoGracza = ReceGraczy[indexGracza];
		rekaAktywnegoGracza.Remove(karta);
		bool wygrana = false;
		int iloscKartPoZagraniu = rekaAktywnegoGracza.Count;
		if (iloscKartPoZagraniu == 1)
		{
			stanUnoGraczy[indexGracza] = true;
			GD.Print($"Gracz {indexGracza} ma jedną kartę!");
		}
		else
		{
			stanUnoGraczy[indexGracza] = false;
		}
		if (indexGracza == 0)
		{
			RozmiescKartyWRece();
			wygrana = (ReceGraczy[0].Count == 0);
		}
		else
		{
			AktualizujUILicznikBota(indexGracza);
			wygrana = (ReceGraczy[indexGracza].Count == 0);
		}
		if(iloscKartPoZagraniu == 0)
        {
			GD.Print($"KONIEC GRY! Gracz {indexGracza} WYGRAŁ!");
			//OnRundaZakoczona?.Invoke(graczIndex);
			GetTree().Paused = true;
			return;
        }
		OnKartaZagrano?.Invoke(karta, indexGracza);
		stosZagranych.Add(karta);
		GornaKartaNaStosie = karta;
		licznikZIndexStosu++;
		if(indexGracza != 0)
        {
			AddChild(karta);
        }
		karta.CallDeferred("ZagrajNaStol",pozycjaStosuZagranych, licznikZIndexStosu);
		if (indexGracza != 0)
			karta.Show();
		
		bool jestCzlowiekiem = (indexGracza == 0);
		ZastosujEfektKarty(karta, jestCzlowiekiem);
		if (karta.Kolor == "DzikaKarta")
		{
			if (jestCzlowiekiem)
			{

			}
			else
			{
				string wybranyKolor = botAI.WybierzKolor(ReceGraczy[indexGracza - 1]);
				wymuszonyKolor = wybranyKolor;
				GD.Print($"Wybrano kolor {wymuszonyKolor}");
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
		if (turnManager.AktualnyGraczIndex != indexGracza)
			return;
		if (instancjaWyboruKoloru.Visible)
			return;
		if (turnManager.DlugDobierania > 0)
		{
			GD.Print($"Gracz {indexGracza} płaci dług {turnManager.DlugDobierania} kart.");
			for (int i = 0; i < turnManager.DlugDobierania; i++)
				DobierzJednaKarte(indexGracza);
			turnManager.DlugDobierania = 0;
		}
		else
		{
			GD.Print($"Gracz {indexGracza} dobiera 1 kartę.");
			DobierzJednaKarte(indexGracza);
		}
		if (indexGracza == 0)
			RozmiescKartyWRece();
		turnManager.ZakonczTure();
    }

    private void AktualizujUILicznikBota(int indexBota)
	{
		int indexKontrolera = indexBota - 1;
		int iloscKart = ReceGraczy[indexBota].Count;
		if (indexKontrolera == 0) _uiBot1.AktualizujLicznik(iloscKart);
		if (indexKontrolera == 1) _uiBot2.AktualizujLicznik(iloscKart);
		if (indexKontrolera == 2) _uiBot3.AktualizujLicznik(iloscKart);
	}

	private void ZastosujEfektKarty(Karta zagranaKarta, bool jestGraczemLudzkim)
	{
		switch (zagranaKarta.Wartosc)
		{
			case "Stop":
				turnManager.PominTure();
				break;
			case "ZmianaKierunku":
				turnManager.ZmienKierunek();
				break;
			case "+2":
				turnManager.DlugDobierania += 2;
				break;
			case "ZmianaKoloru":
				if (jestGraczemLudzkim)
					instancjaWyboruKoloru.Show();
				break;
			case "+4":
				turnManager.DlugDobierania += 4;
				if (jestGraczemLudzkim)
					instancjaWyboruKoloru.Show();
				break;
		}
	}
}
