using Godot;
using System;
using System.Collections.Generic;

public partial class BotController : Node
{
	private BotGracz mozgAI = new BotGracz();
	private LogikaGry logikaGry;
	private int indexBota;
	private List<Karta> rekaBota;
	public void Inicjalizuj(LogikaGry logikaGry, int indexBota, List<Karta> rekaBota)
    {
		this.logikaGry = logikaGry;
		this.indexBota = indexBota;
		this.rekaBota = rekaBota;
    }
	public void RozpocznijTure()
	{
		if (rekaBota == null || rekaBota.Count == 0)
    {
        GD.PrintErr($"Bot {indexBota} nie ma kart do zagrania!");
        logikaGry.SprobujDobracKarte(indexBota);
        return;
    }
		Karta gornaKarta = logikaGry.GornaKartaNaStosie;
		int dlug = logikaGry.DlugDobierania;
		Karta decyzja = mozgAI.WybierzKarteDoZagrania(rekaBota, dlug, (k, d) => logikaGry.UnoManager.CzyRuchJestLegalny(k, d));
		if (decyzja != null)
        {
			logikaGry.SprobujZagracKarte(decyzja, indexBota);
        }
        else
        {
			logikaGry.SprobujDobracKarte(indexBota);
        }
    }
}
