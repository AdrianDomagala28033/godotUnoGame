using Godot;
using System;
using System.Collections.Generic;

public partial class UnoRules : Node
{
	private GameServer gameServer;
	public TurnManager turnManager;
	//private JokerManager jokerManager;

	public UnoRules(GameServer gameServer, TurnManager turnManager)
	{
		this.gameServer = gameServer;
		this.turnManager = turnManager;
		turnManager.OnTuraRozpoczeta += ObsluzPoczatekTury;
	}
	public bool CzyRuchJestLegalny(List<DaneKarty> kartyDoZagrania, int dlug)
	{
		if (!CzyZestawJestSpojny(kartyDoZagrania))
		{
			GD.Print("[RULES] Zestaw niespójny!");
			return false;
		}
		long idGracza = turnManager.AktualnyGraczId;
		var gracz = gameServer.ListaGraczy[idGracza];
		var karta = kartyDoZagrania[0];
		foreach (string idJokera in gracz.PosiadaneJokery)
		{
			var daneJokera = JokerManager.PobierzJokera(idJokera);
			if(daneJokera != null)
			{
				foreach (var efekt in daneJokera.Efekty)
				{
					if(efekt.CzyPozwalaNaZagranie(gameServer, karta))
					{
						GD.Print($"[RULES] Joker {daneJokera.Nazwa} zezwolił na ruch!");
                    	return true;
					}
				}
			}
		}
		if (dlug > 0)
		{
			bool wynik = kartyDoZagrania[0].Wartosc == "+2" || kartyDoZagrania[0].Wartosc == "+4";
			if (!wynik) GD.Print("[RULES] Trwa dobieranie! Musisz rzucić +2/+4."); // 👈
			return wynik;		
		}
		if (kartyDoZagrania[0].Kolor == "DzikaKarta")
			return true;
		if (gameServer.WymuszonyKolor != null)
			return kartyDoZagrania[0].Kolor == gameServer.WymuszonyKolor;
		if (kartyDoZagrania[0].Kolor == gameServer.GornaKartaNaStosie.Kolor)
			return true;
		if (kartyDoZagrania[0].Wartosc == gameServer.GornaKartaNaStosie.Wartosc)
			return true;
		GD.Print($"[RULES] Ruch nielegalny! Karta: {kartyDoZagrania[0].Kolor} {kartyDoZagrania[0].Wartosc} vs Stół: {gameServer.GornaKartaNaStosie.Kolor} {gameServer.GornaKartaNaStosie.Wartosc}");
		return false;
	}
	public void ZastosujEfektKarty(DaneKarty zagranaKarta, bool jestGraczemLudzkim)
	{
		//var popupManager = GetNode<PopupManager>("/root/PopupManager");
		long idGracza = turnManager.AktualnyGraczId;
		var gracz = gameServer.ListaGraczy[idGracza];
		foreach (string idJokera in gracz.PosiadaneJokery)
		{
			var daneJokera = JokerManager.PobierzJokera(idJokera);
			if(daneJokera != null)
			{
				foreach (var efekt in daneJokera.Efekty)
				{
					efekt.PoZagraniuKarty(gameServer, zagranaKarta);
					gameServer.ListaGraczy[idGracza].DodajPunkty(1);
				}
			}
		}
		switch (zagranaKarta.Wartosc)
		{
			case "Stop":
				turnManager.PominTure();
				gameServer.ListaGraczy[idGracza].DodajPunkty(1);
				//popupManager.PokazWiadomosc("STOP!", logikaGry.PozycjaStosuZagranych);
				break;
			case "ZmianaKierunku":
				turnManager.ZmienKierunek();
				gameServer.ListaGraczy[idGracza].DodajPunkty(1);
				//popupManager.PokazWiadomosc("Zmiana kierunku!", logikaGry.PozycjaStosuZagranych);
				break;
			case "+2":
				turnManager.DlugDobierania += 2;
				gameServer.ListaGraczy[idGracza].DodajPunkty(2);
				//popupManager.PokazWiadomosc("+2!", logikaGry.PozycjaStosuZagranych);
				break;
			case "ZmianaKoloru":
				if (jestGraczemLudzkim)
					gameServer.GetParent<NetworkManager>().RpcId(idGracza, nameof(NetworkManager.PokazWyborKoloru));
				gameServer.ListaGraczy[idGracza].DodajPunkty(1);
				break;
			case "+4":
				turnManager.DlugDobierania += 4;
				if (jestGraczemLudzkim)
					gameServer.GetParent<NetworkManager>().RpcId(idGracza, nameof(NetworkManager.PokazWyborKoloru));
				gameServer.ListaGraczy[idGracza].DodajPunkty(4);
				break;
		}
	}
	public void ObsluzPoczatekTury(int indexGracza)
    {
		long idGracza = turnManager.ListaGraczyId[indexGracza];
		if (gameServer.ListaGraczy.ContainsKey(idGracza))
		{
			// foreach (DaneJokera joker in gameServer.ListaGraczy[idGracza].PosiadaneJokery)
			// {
			// 	if(joker.WarunekAktywacji == WarunekAktywacji.Pasywny)
			// 		joker.Efekt(gameServer);
			// }
		}
    }
	private bool CzyZestawJestSpojny(List<DaneKarty> karty)
	{
		foreach (DaneKarty karta in karty)
		{
			if(karty[0].Wartosc == karta.Wartosc)
				continue;
			else
				return false;
		}
		return true;
	}
}
