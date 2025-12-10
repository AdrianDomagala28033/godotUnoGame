using Godot;
using System;

public partial class UnoRules : Node
{
	private LogikaGry logikaGry;
	private TurnManager turnManager;
	private UIManager uIManager;
	//private JokerManager jokerManager;

	public UnoRules(LogikaGry logikaGry, TurnManager turnManager, UIManager uIManager)
	{
		this.logikaGry = logikaGry;
		this.turnManager = turnManager;
		this.uIManager = uIManager;
		turnManager.OnTuraRozpoczeta += ObsluzPoczatekTury;
	}
	public bool CzyRuchJestLegalny(Karta kartaDoZagrania, int dlug)
	{
		Gracz aktualnyGracz = logikaGry.TurnManager.AktualnyGracz;
		if (logikaGry.JokerManager.CzyJokerPozwalaNaZagranie(kartaDoZagrania, logikaGry, aktualnyGracz))
			return true;
		if (dlug > 0)
			return kartaDoZagrania.Wartosc == "+2" || kartaDoZagrania.Wartosc == "+4";
		if (kartaDoZagrania.Kolor == "DzikaKarta")
			return true;
		if (logikaGry.WymuszonyKolor != null)
			return kartaDoZagrania.Kolor == logikaGry.WymuszonyKolor;
		if (kartaDoZagrania.Kolor == logikaGry.GornaKartaNaStosie.Kolor)
			return true;
		if (kartaDoZagrania.Wartosc == logikaGry.GornaKartaNaStosie.Wartosc)
			return true;
		return false;
	}
	public void ZastosujEfektKarty(Karta zagranaKarta, bool jestGraczemLudzkim)
	{
		//var popupManager = GetNode<PopupManager>("/root/PopupManager");
		switch (zagranaKarta.Wartosc)
		{
			case "Stop":
				turnManager.PominTure();
				//popupManager.PokazWiadomosc("STOP!", logikaGry.PozycjaStosuZagranych);
				break;
			case "ZmianaKierunku":
				turnManager.ZmienKierunek();
				//popupManager.PokazWiadomosc("Zmiana kierunku!", logikaGry.PozycjaStosuZagranych);
				break;
			case "+2":
				turnManager.DlugDobierania += 2;
				uIManager.UstawDlug(turnManager.DlugDobierania);
				//popupManager.PokazWiadomosc("+2!", logikaGry.PozycjaStosuZagranych);
				break;
			case "ZmianaKoloru":
				if (jestGraczemLudzkim)
					logikaGry.InstancjaWyboruKoloru.Show();
				break;
			case "+4":
				turnManager.DlugDobierania += 4;
				if (jestGraczemLudzkim)
					logikaGry.InstancjaWyboruKoloru.Show();
				break;
		}
	}
	public void ObsluzPoczatekTury(int indexGracza)
    {
        foreach (Joker joker in logikaGry.ListaGraczy[indexGracza].PosiadaneJokery)
		{
			joker.Efekt(logikaGry);
		}
		uIManager.UstawDlug(logikaGry.DlugDobierania);
    }
}
