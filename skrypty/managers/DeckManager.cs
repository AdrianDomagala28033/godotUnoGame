using Godot;
using System;
using System.Collections.Generic;

public class DeckManager
{
	private Random _random = new Random();
	public List<DaneKarty> talia = new List<DaneKarty>();
	public event Action OnTaliaPrzetasowano;


	public void StworzTalie()
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
	}

	private void StworzKarte(string kolor, string wartosc)
	{
		DaneKarty nowaKarta = new DaneKarty(kolor, wartosc);
		talia.Add(nowaKarta);
	}

	public void PotasujTalie()
	{
		int n = talia.Count;
		while (n > 1)
		{
			n--;
			int k = _random.Next(n + 1);
			DaneKarty karta = talia[k];
			talia[k] = talia[n];
			talia[n] = karta;
		}
	}

	public void PrzetasujStosZagranych(List<DaneKarty> aktualnyStos, DaneKarty gornaKarta)
	{
		aktualnyStos.Remove(gornaKarta);
		foreach (DaneKarty karta in aktualnyStos)
		{
			talia.Add(karta);
			if (karta.Wartosc == "ZmianaKoloru" || karta.Wartosc == "+4")
			{
				karta.Kolor = "DzikaKarta";
			}
		}
		aktualnyStos.Clear();
		aktualnyStos.Add(gornaKarta);
		PotasujTalie();
		OnTaliaPrzetasowano?.Invoke();
	}
	public DaneKarty WydajKarte()
	{
		DaneKarty karta = talia[0];
		talia.RemoveAt(0);
		return karta;
	}

	public List<DaneKarty> PobierzTalie() => talia;
}
