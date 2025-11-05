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
		Karta gornaKarta = logikaGry.GornaKartaNaStosie;
		int dlug = logikaGry.DlugDobierania;
		Karta decyzja = mozgAI.WybierzKarteDoZagrania(rekaBota, gornaKarta, dlug, (k, g, d) => logikaGry.CzyRuchJestLegalny(k, g, d));
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
