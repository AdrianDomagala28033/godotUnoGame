using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : Control
{
	private LogikaGry logikaGry;
	[Export] private Label etykietaTuryGracza;
	[Export] private JokerPanel jokerPanel;
	private StatusPanel statusPanel;
	private Control koniecRundyPanel;
	private UiBota _uiBot1;
	private UiBota _uiBot2;
	private UiBota _uiBot3;
	private Node2D stosKart;

	public override void _Ready()
	{
		statusPanel = GetNodeOrNull<StatusPanel>("StatusPanel");
		koniecRundyPanel = GetNodeOrNull<Control>("KoniecRundyPanel");
		if (koniecRundyPanel != null)
			koniecRundyPanel.Hide();

		_uiBot1 = GetNode<UiBota>("InterfejsGry/UIBota");
		_uiBot2 = GetNode<UiBota>("InterfejsGry/UIBota2");
		_uiBot3 = GetNode<UiBota>("InterfejsGry/UIBota3");

		stosKart = GetNode<Node2D>("/root/StolGry/StosKart");
	}
    public void Inicjalizuj(LogikaGry gra)
	{
		logikaGry = gra;
		logikaGry.OnKartaZagrana += HandleKartaZagrano;
		logikaGry.OnKartaDobrano += HandleKartaDobrano;
		logikaGry.OnTaliaPrzetasowano += HandleTaliaPrzetasowano;
		logikaGry.OnRundaZakoczona += HandleRundaZakonczona;
		logikaGry.OnKolorZostalWybrany += HandleKolorWybrany;
		logikaGry.OnKolorZmieniony += UstawKolor;
		logikaGry.OnKartaZagrana += PokazKarteNaStosie;
		logikaGry.OnKolorDoUstawienia += HandleKolorDoUstawienia;
		logikaGry.OnDodajKarteNaStos += DodajKarteNaStos;
		logikaGry.OnJokerZdobyty += HandleJokerZdobyty;

		logikaGry.OnRozmiescKarty += (reka) => RozmiescKartyWRece(reka);
		logikaGry.OnDodajKarteNaStos += (karta, pos, z, idx) => DodajKarteNaStos(karta, pos, z, idx);
		logikaGry.OnAktualizujLicznikBota += (idx, count) => AktualizujUILicznikBota(idx, count);
		logikaGry.OnKolorDoUstawienia += (kolor) => UstawKolor(kolor);
		logikaGry.OnReceZmienione += () => {
			if (logikaGry != null && logikaGry.ListaGraczy[0].rekaGracza != null && logikaGry.ListaGraczy[0].rekaGracza.Count > 0)
				RozmiescKartyWRece(logikaGry.ListaGraczy[0].rekaGracza);
		};
		logikaGry.OnJokerZdobyty += (joker) => jokerPanel.DodajJokeraDoWidoku(joker);
	}
	private void HandleJokerZdobyty(Joker joker)
    {
        GD.Print($"[UI] Dostałem info o jokerze: {joker.Nazwa}. Dodaję do panelu.");
    }
    private void HandleKolorDoUstawienia(string kolor)
    {
        if (statusPanel != null)
			statusPanel.CallDeferred("UstawKolor", kolor);
    }

    private void PokazKarteNaStosie(Karta karta, int indexGracza)
	{
		if (karta == null)
		{
			GD.PrintErr("[UIManager] PokazKarteNaStosie: karta == null");
			return;
		}
	}

	public void DodajKarteNaStos(Karta karta, Vector2 pozycjaStosu, int zIndex, int indexGracza)
	{
		if (karta.GetParent() == null)
			AddChild(karta);

		karta.Show();
		karta.ZIndex = zIndex;
		karta.CallDeferred("ZagrajNaStol", pozycjaStosu, zIndex);
	}

	private void HandleKartaZagrano(Karta karta, int graczIndex)
	{
		if (statusPanel == null) return;
		if (karta == null) return;
		statusPanel.CallDeferred("UstawKolor", karta.Kolor);
		statusPanel.CallDeferred("UstawDlug", logikaGry.DlugDobierania);
	}

	private void HandleKartaDobrano(int graczIndex)
	{
		if (statusPanel == null) return;
		statusPanel.CallDeferred("UstawDlug", logikaGry.DlugDobierania);
		if (logikaGry != null && logikaGry.ListaGraczy[0].rekaGracza != null && logikaGry.ListaGraczy[0].rekaGracza.Count > 0)
			RozmiescKartyWRece(logikaGry.ListaGraczy[0].rekaGracza);
	}

	private void HandleTaliaPrzetasowano(int ile)
	{
		var popup = GetNodeOrNull<PopupManager>("root/PopupManager");
		if (popup != null)
			popup.CallDeferred("PokazWiadomosc", $"Talia przetasowana ({ile})", Vector2.Zero);
	}

	private void HandleRundaZakonczona(int zwyciezcaIndex)
	{
		if (koniecRundyPanel != null)
		{
			koniecRundyPanel.CallDeferred("show");
			var label = koniecRundyPanel.GetNodeOrNull<Label>("VBoxContainer/WinnerLabel");
			if (label != null)
				label.CallDeferred("set_text", $"Koniec gry! Wygrał gracz {zwyciezcaIndex}");
		}
	}

	private void HandleKolorWybrany(string nazwaKoloru)
	{
		if (statusPanel == null) return;
		statusPanel.CallDeferred("UstawKolor", nazwaKoloru);
	}

	public void PokazTureGracza(bool widoczna)
	{
		if (etykietaTuryGracza == null)
		{
			GD.PrintErr("[UIManager] Brak referencji do etykietaTuryGracza!");
			return;
		}
		if (widoczna)
			etykietaTuryGracza.Show();
		else
			etykietaTuryGracza.Hide();
	}

	public void UstawDlug(int dlug)
	{
		statusPanel?.CallDeferred("UstawDlug", dlug);
	}

	public void UstawKolor(string nazwaKoloru)
	{
		statusPanel?.CallDeferred("UstawKolor", nazwaKoloru);
	}

	public void AktualizujUILicznikBota(int indexBota, int iloscKart)
	{
		switch (indexBota)
		{
			case 1: _uiBot1?.AktualizujLicznik(iloscKart); break;
			case 2: _uiBot2?.AktualizujLicznik(iloscKart); break;
			case 3: _uiBot3?.AktualizujLicznik(iloscKart); break;
		}
	}

	private void RozmiescKartyWRece(List<Karta> rekaGracza)
	{
		if (rekaGracza == null) return;
		int iloscKart = rekaGracza.Count;
		float szerokoscKarty = 150;
		float szerokoscEkranu = GetViewportRect().Size.X;
		float margines = 100;

		float maxDostepnaSzerokosc = szerokoscEkranu - (2 * margines);
		float wymaganaSzerokosc = iloscKart * szerokoscKarty;

		float odstep;
		float skala;

		if (iloscKart == 0) return;

		if (wymaganaSzerokosc > maxDostepnaSzerokosc)
		{
			odstep = (maxDostepnaSzerokosc - szerokoscKarty) / (iloscKart - 1);
			odstep = Mathf.Max(odstep, 20);

			skala = maxDostepnaSzerokosc / wymaganaSzerokosc;
			skala = Mathf.Min(skala, 1.0f);
		}
		else
		{
			odstep = szerokoscKarty * 0.9f;
			skala = 1.0f;
		}

		float szerokoscCalejReki = (iloscKart - 1) * odstep + szerokoscKarty;
		float pozycjaStartowaX = (szerokoscEkranu / 2) - (szerokoscCalejReki / 2);
		float pozycjaY = GetViewportRect().Size.Y + 50;

		for (int i = 0; i < iloscKart; i++)
		{
			Karta karta = rekaGracza[i];
			if (karta == null) continue;

			if (karta.GetParent() == null)
			{
				AddChild(karta);
			}

			Vector2 nowaPozycja = new Vector2(pozycjaStartowaX + (i * odstep), pozycjaY);
			karta.UstawSkale(skala);
			karta.Show();
			karta.InputPickable = true;
			karta.CreateTween().TweenProperty(karta, "position", nowaPozycja, 0.2);
			karta.UstawOryginalnaPozycje(pozycjaY);
			karta.ZIndex = 10 + i;
		}
	}

	private void RozmiescKartyWRece()
	{
		if (logikaGry == null) return;
		if (logikaGry.ListaGraczy[0].rekaGracza == null || logikaGry.ListaGraczy[0].rekaGracza.Count == 0) return;
			RozmiescKartyWRece(logikaGry.ListaGraczy[0].rekaGracza);
	}
}
