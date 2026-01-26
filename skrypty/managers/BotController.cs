using Godot;
using System;
using System.Collections.Generic;

public partial class BotController : Node
{
	private GameClient gameClient;
	private int indexBota;
	private List<Gracz> listaGraczy;
	public void Inicjalizuj(GameClient logikaGry, List<Gracz> listaGraczy)
    {
		this.gameClient = logikaGry;
		this.listaGraczy = listaGraczy;

    }
	// public void RozpocznijTure()
	// {
	// 	if (listaGraczy == null || listaGraczy.Count == 0)
    // {
    //     gameClient.SprobujDobracKarte(indexBota);
    //     return;
    // }
	// 	Karta gornaKarta = gameClient.GornaKartaNaStosie;
	// 	int dlug = gameClient.DlugDobierania;
	// 	Karta decyzja = listaGraczy[indexBota].WybierzKarteDoZagrania(listaGraczy[indexBota].rekaGracza, dlug, (k, d) => gameClient.UnoManager.CzyRuchJestLegalny(k, d));
	// 	if (decyzja != null)
    //     {
	// 		gameClient.SprobujZagracKarte(decyzja, indexBota);
    //     }
    //     else
    //     {
	// 		gameClient.SprobujDobracKarte(indexBota);
    //     }
    // }

    internal void UstawIndexBota(int i)
    {
		indexBota = i;
    }

}
