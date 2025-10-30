using Godot;
using System;
using System.Collections.Generic;

public partial class LogikaGry : Node2D
{
	#region pola klasy
	[Export]
	private PackedScene SzablonKarty;
	[Export]
	private PackedScene SzablonWyboruKoloru;
	private WyborKoloru instancjaWyboruKoloru;
	private List<Karta> talia = new List<Karta>();
	private List<Karta> rekaGracza = new List<Karta>();
	private List<List<Karta>> receBotow = new List<List<Karta>>();
	private Random _random = new Random();
	private List<Karta> stosZagranych = new List<Karta>();
	private Karta gornaKartaNaStosie;
	private int licznikZIndexStosu = 100;
	private int iloscGraczy = 4;
	private int aktualnyGraczIndex = 0;
	private int kierunekGry = 1;
	private int dlugDobierania = 0;
	private BotGracz botAI = new BotGracz();
	

#endregion

	public override void _Ready()
	{
		for (int i = 0; i < iloscGraczy - 1; i++)
		{
			receBotow.Add(new List<Karta>());
		}
		StworzTalie();
		PotasujTalie();
		RozdajKartyGraczowi(7);

		WystawPierwszaKarte();

		instancjaWyboruKoloru = (WyborKoloru)SzablonWyboruKoloru.Instantiate();
		AddChild(instancjaWyboruKoloru);
		instancjaWyboruKoloru.Hide();
		instancjaWyboruKoloru.KolorWybrany += _OnKolorZostalWybrany;
	}
	private void _OnStosDobieraniaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (aktualnyGraczIndex != 0)
			{
				return;
			}
			if (dlugDobierania > 0)
			{
				for (int i = 0; i < dlugDobierania; i++)
				{
					DobierzKarteDlaGracza(false);
				}
				dlugDobierania = 0;
				RozmiescKartyWRece();
			}
			else
			{
				DobierzKarteDlaGracza(true);
			}
			ZakonczTure();
		}
	}
	private void _OnKolorZostalWybrany(string wybranyKolor)
	{
		gornaKartaNaStosie.Kolor = wybranyKolor;
		ZakonczTure();
	}
#region obsługa podstawowych mechanik
	private void StworzTalie()
	{
		string[] kolory = { "Czerwony", "Niebieski", "Zielony", "Zolty" };
		for (int i = 0; i <= 9; i++)
		{
			foreach (string kolor in kolory)
			{
				StworzKarte(kolor, i.ToString());
				if (i != 0) StworzKarte(kolor, i.ToString());
			}
		}
		string[] specjalne = { "Stop", "+2", "ZmianaKierunku" };
		foreach (string wartosc in specjalne)
		{
			foreach (string kolor in kolory)
			{
				StworzKarte(kolor, wartosc);
				StworzKarte(kolor, wartosc);
			}
		}
		for (int i = 0; i < 4; i++)
		{
			StworzKarte("DzikaKarta", "ZmianaKoloru");
			StworzKarte("DzikaKarta", "+4");
		}
		GD.Print($"Stworzono talię Uno! Liczba kart: {talia.Count}");
	}
	private void StworzKarte(string kolor, string wartosc)
	{
		Karta nowaKarta = (Karta)SzablonKarty.Instantiate();
		nowaKarta.Kolor = kolor;
		nowaKarta.Wartosc = wartosc;
		talia.Add(nowaKarta);

		nowaKarta.OnKartaKliknieta += ObslozKlikniecieKarty;
	}
	private void WystawPierwszaKarte()
	{
		Karta startowaKarta = talia[talia.Count - 1];
		talia.RemoveAt(talia.Count - 1);

		gornaKartaNaStosie = startowaKarta;
		stosZagranych.Add(startowaKarta);

		AddChild(startowaKarta);
		Vector2 srodekStolu = new Vector2(GetViewportRect().Size.X / 2, 350);
		startowaKarta.ZagrajNaStol(srodekStolu, licznikZIndexStosu);
	}
	private void PotasujTalie()
	{
		int n = talia.Count;
		while (n > 1)
		{
			n--;
			int k = _random.Next(n + 1);
			Karta karta = talia[k];
			talia[k] = talia[n];
			talia[n] = karta;
		}
	}
	private void RozdajKartyGraczowi(int ilosc)
	{
		for (int i = 0; i < ilosc; i++)
		{
			if (talia.Count > 0)
			{
				Karta kartaCzlowiek = talia[talia.Count - 1];
				talia.RemoveAt(talia.Count - 1);
				rekaGracza.Add(kartaCzlowiek);
				AddChild(kartaCzlowiek);
			}
			for(int j = 0; j < receBotow.Count; j++)
			{
				if(talia.Count > 0)
				{
					Karta kartaBot = talia[talia.Count - 1];
					talia.RemoveAt(talia.Count - 1);
					receBotow[j].Add(kartaBot);
					kartaBot.InputPickable = false;
					kartaBot.Hide();
				}
			}
		}
		RozmiescKartyWRece();
	}
	private void ObslozKlikniecieKarty(Karta kliknietaKarta)
	{
		if(aktualnyGraczIndex != 0)
		{
			kliknietaKarta.AnulujZagranie();
			return;
		}
		if (CzyRuchJestLegalny(kliknietaKarta, gornaKartaNaStosie, dlugDobierania))
		{
			rekaGracza.Remove(kliknietaKarta);
			stosZagranych.Add(kliknietaKarta);
			gornaKartaNaStosie = kliknietaKarta;
			licznikZIndexStosu++;

			Vector2 srodekStolu = new Vector2(GetViewportRect().Size.X / 2, 350);
			kliknietaKarta.ZagrajNaStol(srodekStolu, licznikZIndexStosu);
			RozmiescKartyWRece();
			if(rekaGracza.Count == 0)
			{
				GD.Print("Koniec gry!");
				GetTree().Paused = true;
				return;
			}
			ZastosujEfektKarty(kliknietaKarta, true);
			ZakonczTure();
		}
		else
		{
			GD.Print("Nielegalny ruch");
			kliknietaKarta.AnulujZagranie();
		}
	}
	private bool CzyRuchJestLegalny(Karta kartaDoZagrania, Karta gornaKarta, int dlug)
	{
		if(dlug > 0)
		{
			return kartaDoZagrania.Wartosc == "+2" || kartaDoZagrania.Wartosc == "+4";
		}
		if (kartaDoZagrania.Kolor == "DzikaKarta")
		{
			return true;
		}
		if (kartaDoZagrania.Kolor == gornaKartaNaStosie.Kolor)
		{
			return true;
		}
		if (kartaDoZagrania.Wartosc == gornaKartaNaStosie.Wartosc)
		{
			return true;
		}
		return false;
	}
	private void DobierzKarteDlaGracza(bool odswiezRekeNaKoniec = true)
	{
		if (talia.Count == 0)
		{
			PrzetasujStosZagranych();
		}
		Karta kartaDoDobrania = talia[talia.Count - 1];
		talia.RemoveAt(talia.Count - 1);

		AddChild(kartaDoDobrania);
		rekaGracza.Add(kartaDoDobrania);

		kartaDoDobrania.Show();
		kartaDoDobrania.InputPickable = true;
		kartaDoDobrania.ZIndex = 10;
		if (odswiezRekeNaKoniec)
		{
			RozmiescKartyWRece();
		}
	}
	private void DobierzKarteDlaBota(int indexRekiBota)
	{
		if (talia.Count == 0) PrzetasujStosZagranych();
		if (talia.Count == 0) return;

		Karta kartaBot = talia[talia.Count - 1];
		talia.RemoveAt(talia.Count - 1);
		receBotow[indexRekiBota].Add(kartaBot);
	}
	private void PrzetasujStosZagranych()
	{
		foreach (Karta karta in stosZagranych)
		{
			talia.Add(karta);
			karta.Hide();
		}
		stosZagranych.Clear();
		stosZagranych.Add(gornaKartaNaStosie);
		PotasujTalie();
	}
	private void RozmiescKartyWRece()
	{
		int iloscKart = rekaGracza.Count;
		float odstep = 80;
		float pozycjaStartowaX = (GetViewportRect().Size.X / 2) - (iloscKart * odstep / 2) + (odstep / 2);
		float pozycjaY = 600;

		for (int i = 0; i < iloscKart; i++)
		{
			Karta karta = rekaGracza[i];
			Vector2 nowaPozycja = new Vector2(pozycjaStartowaX + (i * odstep), pozycjaY);

			karta.CreateTween().TweenProperty(karta, "position", nowaPozycja, 0.2);
			karta.UstawOryginalnaPozycje(pozycjaY);
			karta.ZIndex = 10 + i;
		}
	}
	#endregion
	private void ZakonczTure()
	{
		if (instancjaWyboruKoloru.Visible)
		{
			return;
		}
		aktualnyGraczIndex += kierunekGry;
		if (aktualnyGraczIndex >= iloscGraczy)
		{
			aktualnyGraczIndex = 0;
		}
		else if (aktualnyGraczIndex < 0)
		{
			aktualnyGraczIndex = iloscGraczy - 1;
		}
		if (aktualnyGraczIndex == 0)
		{
			if (dlugDobierania > 0)
			{
				GD.Print($"Masz dług {dlugDobierania} kart! Zagrywasz +2/+4 albo dobierasz.");
			}
		}
		else
		{
			Timer timerBota = new Timer();
			timerBota.WaitTime = 1.0;
			timerBota.OneShot = true;
			timerBota.Timeout += TuraBota;
			AddChild(timerBota);
			timerBota.Start();
		}
		if (dlugDobierania > 0 && aktualnyGraczIndex != 0)
		{
			for (int i = 0; i < dlugDobierania; i++)
			{
				if (talia.Count == 0) PrzetasujStosZagranych();
				if (talia.Count > 0)
				{
					Karta dobranaPrzezBota = talia[talia.Count - 1];
					talia.RemoveAt(talia.Count - 1);
					dobranaPrzezBota.QueueFree();
				}
			}
			dlugDobierania = 0;
			ZakonczTure();
		}
		else if(aktualnyGraczIndex != 0)
		{
			Timer timerBota = new Timer();
		}
	}
	private void TuraBota()
	{
		int indexRekiBota = aktualnyGraczIndex - 1;
		List<Karta> rekaBota = receBotow[indexRekiBota];
		if(dlugDobierania > 0)
		{
			Karta decyzjaBota = botAI.WybierzKarteDoZagrania(rekaBota, gornaKartaNaStosie, dlugDobierania, (k, g, d) => CzyRuchJestLegalny(k, g, d));
			if(decyzjaBota != null)
			{
				ZagrajKarteBota(decyzjaBota, indexRekiBota);
			}
			else
			{
				GD.Print($"Bot {aktualnyGraczIndex} płaci dług {dlugDobierania} kart.");
				for (int i = 0; i < dlugDobierania; i++)
				{
					DobierzKarteDlaBota(indexRekiBota);
				}
				dlugDobierania = 0;
				ZakonczTure();
			}
		}
		else
		{
			Karta decyzjaBota = botAI.WybierzKarteDoZagrania(rekaBota, gornaKartaNaStosie, dlugDobierania, (k, g, d) => CzyRuchJestLegalny(k, g, d));
			if (decyzjaBota != null)
			{
				ZagrajKarteBota(decyzjaBota, indexRekiBota);
			}
			else
			{
				GD.Print($"Bot {aktualnyGraczIndex} dobiera kartę.");
				DobierzKarteDlaBota(indexRekiBota);
				ZakonczTure();
			}
		}
	}

	private void ZagrajKarteBota(Karta kartaDoZagrania, int indexRekiBota)
	{
		GD.Print($"Bot {aktualnyGraczIndex} zagrywa: {kartaDoZagrania.Kolor} {kartaDoZagrania.Wartosc}");
		receBotow[indexRekiBota].Remove(kartaDoZagrania);
		stosZagranych.Add(kartaDoZagrania);
		gornaKartaNaStosie = kartaDoZagrania;
		licznikZIndexStosu++;

		AddChild(kartaDoZagrania);
		Vector2 srodekStolu = new Vector2(GetViewportRect().Size.X / 2, 350);
		kartaDoZagrania.ZagrajNaStol(srodekStolu, licznikZIndexStosu);
		kartaDoZagrania.Show();

		if (receBotow[indexRekiBota].Count == 0)
		{
			GD.Print($"KONIEC GRY! Bot {aktualnyGraczIndex} WYGRAŁ!");
			GetTree().Paused = true;
			return;
		}
		ZastosujEfektKarty(kartaDoZagrania, false);
		if(kartaDoZagrania.Kolor == "DzikaKarta")
		{
			string wybranyKolor = botAI.WybierzKolor(receBotow[indexRekiBota]);
			gornaKartaNaStosie.Kolor = wybranyKolor;
			ZakonczTure();
		}
		else
		{
			ZakonczTure();
		}
	}
	#region karty atakujace

	private void ZastosujEfektKarty(Karta zagranaKarta, bool jestGraczemLudzkim)
	{
		switch (zagranaKarta.Wartosc)
		{
			case "Stop":
				aktualnyGraczIndex += kierunekGry;
				break;
			case "ZmianaKierunku":
				kierunekGry *= -1;
				break;
			case "+2":
				dlugDobierania += 2;
				break;
			case "ZmianaKoloru":
				if (jestGraczemLudzkim)
				{
					instancjaWyboruKoloru.Show();
				}
				break;
			case "+4":
				dlugDobierania += 4;
				if (jestGraczemLudzkim)
				{
					instancjaWyboruKoloru.Show();
				}
				break;
		}
	}

#endregion
}
