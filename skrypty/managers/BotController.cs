using Godot;
using System;
using System.Collections.Generic;

public partial class BotController : Node
{
	private LogikaGry logikaGry;
	private int indexBota;
	private List<Gracz> listaGraczy;
	public void Inicjalizuj(LogikaGry logikaGry, List<Gracz> listaGraczy)
    {
		this.logikaGry = logikaGry;
		this.listaGraczy = listaGraczy;

    }
	public void RozpocznijTure()
	{
		if (listaGraczy == null || listaGraczy.Count == 0)
    {
        logikaGry.SprobujDobracKarte(indexBota);
        return;
    }
		Karta gornaKarta = logikaGry.GornaKartaNaStosie;
		int dlug = logikaGry.DlugDobierania;
		Karta decyzja = listaGraczy[indexBota].WybierzKarteDoZagrania(listaGraczy[indexBota].rekaGracza, dlug, (k, d) => logikaGry.UnoManager.CzyRuchJestLegalny(k, d));
		if (decyzja != null)
        {
			logikaGry.SprobujZagracKarte(decyzja, indexBota);
        }
        else
        {
			logikaGry.SprobujDobracKarte(indexBota);
        }
    }

    internal void UstawIndexBota(int i)
    {
		indexBota = i;
    }

}
