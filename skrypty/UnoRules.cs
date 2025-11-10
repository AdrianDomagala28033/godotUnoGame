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
	}
	public bool CzyRuchJestLegalny(Karta kartaDoZagrania, int dlug)
	{
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
				turnManager.ZakonczTure();
				//popupManager.PokazWiadomosc("STOP!", logikaGry.PozycjaStosuZagranych);
				break;
			case "ZmianaKierunku":
				turnManager.ZmienKierunek();
				turnManager.ZakonczTure();
				//popupManager.PokazWiadomosc("Zmiana kierunku!", logikaGry.PozycjaStosuZagranych);
				break;
			case "+2":
				turnManager.DlugDobierania += 2;
				uIManager.UstawDlug(turnManager.DlugDobierania);
				turnManager.ZakonczTure();
				//popupManager.PokazWiadomosc("+2!", logikaGry.PozycjaStosuZagranych);
				break;
			case "ZmianaKoloru":
				if (jestGraczemLudzkim)
					logikaGry.InstancjaWyboruKoloru.Show();
				else
					turnManager.ZakonczTure();
							break;
			case "+4":
				turnManager.DlugDobierania += 4;
				GD.Print($"[LogikaGry] +4: DlugDobierania nowy = {turnManager.DlugDobierania}");
				if (jestGraczemLudzkim)
					logikaGry.InstancjaWyboruKoloru.Show();
				else
					turnManager.ZakonczTure();
				break;
		}
	}
}
