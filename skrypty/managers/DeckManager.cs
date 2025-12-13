using Godot;
using System;
using System.Collections.Generic;

public class DeckManager
{
	private Node _parent;
	private PackedScene SzablonKarty;
	private Random _random = new Random();
	public List<Karta> talia = new List<Karta>();
	public event Action OnTaliaPrzetasowano;

	public DeckManager(Node parent, PackedScene szablonKarty)
	{
		_parent = parent;
		SzablonKarty = szablonKarty;
	}

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
		Karta nowaKarta = (Karta)SzablonKarty.Instantiate();
		nowaKarta.Kolor = kolor;
		nowaKarta.Wartosc = wartosc;
		talia.Add(nowaKarta);
	}

	public void PotasujTalie()
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

	public void PrzetasujStosZagranych(List<Karta> aktualnyStos, Karta gornaKarta)
	{
		aktualnyStos.Remove(gornaKarta);
		foreach (Karta karta in aktualnyStos)
		{
			talia.Add(karta);
			karta.ZrestartujStanKarty();
			karta.Hide();
			karta.InputPickable = true;
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

	public List<Karta> PobierzTalie() => talia;
}
