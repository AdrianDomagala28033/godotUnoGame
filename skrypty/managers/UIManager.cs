using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : Control
{
	private GameClient gameClient;
	[Export] private Label etykietaTuryGracza;
	[Export] private JokerPanel jokerPanel;
	private StatusPanel statusPanel;
	private Control koniecRundyPanel;
	private Node2D stosKart;

	public override void _Ready()
	{
		statusPanel = GetNodeOrNull<StatusPanel>("StatusPanel");
		koniecRundyPanel = GetNodeOrNull<Control>("KoniecRundyPanel");
		if (koniecRundyPanel != null)
			koniecRundyPanel.Hide();


		stosKart = GetNode<Node2D>("/root/StolGry/StosKart");

		var interfejs = GetNodeOrNull<Control>("InterfejsGry");
		if (interfejs != null) 
			interfejs.MouseFilter = Control.MouseFilterEnum.Ignore;

		if (jokerPanel != null)
        	jokerPanel.MouseFilter = Control.MouseFilterEnum.Pass;
	}
    public void Inicjalizuj(GameClient gra)
	{
		gameClient = gra;
		gameClient.OnKartaZagrana += HandleKartaZagrano;
		gameClient.OnKartaDobrano += HandleKartaDobrano;
		gameClient.OnTaliaPrzetasowano += HandleTaliaPrzetasowano;
		gameClient.OnRundaZakoczona += HandleRundaZakonczona;
		gameClient.OnKolorZostalWybrany += HandleKolorWybrany;
		gameClient.OnKolorZmieniony += UstawKolor;
		gameClient.OnKartaZagrana += PokazKarteNaStosie;
		gameClient.OnKolorDoUstawienia += HandleKolorDoUstawienia;
		gameClient.OnDodajKarteNaStos += DodajKarteNaStos;
		//gameClient.OnJokerZdobyty += HandleJokerZdobyty;

		gameClient.OnRozmiescKarty += (reka) => RozmiescKartyWRece(reka);
		gameClient.OnDodajKarteNaStos += (karta, pos, z, idx) => DodajKarteNaStos(karta, pos, z, idx);
		gameClient.OnKolorDoUstawienia += (kolor) => UstawKolor(kolor);
		gameClient.OnReceZmienione += () => {
			if (gameClient != null && gameClient.ListaGraczy[0].rekaGracza != null && gameClient.ListaGraczy[0].rekaGracza.Count > 0)
				RozmiescKartyWRece(gameClient.ListaGraczy[0].rekaGracza);
		};
		//gameClient.OnJokerZdobyty += (joker) => jokerPanel.DodajJokeraDoWidoku(joker);
	}
	// private void HandleJokerZdobyty(DaneJokera joker)
    // {
    //     GD.Print($"[UI] Dostałem info o jokerze: {joker.Nazwa}. Dodaję do panelu.");
    // }
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
		statusPanel.CallDeferred("UstawDlug", gameClient.DlugDobierania);
	}

	private void HandleKartaDobrano(int graczIndex)
	{
		if (statusPanel == null) return;
		statusPanel.CallDeferred("UstawDlug", gameClient.DlugDobierania);
		if (gameClient != null && gameClient.ListaGraczy[0].rekaGracza != null && gameClient.ListaGraczy[0].rekaGracza.Count > 0)
			RozmiescKartyWRece(gameClient.ListaGraczy[0].rekaGracza);
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
		if (gameClient == null) return;
		if (gameClient.ListaGraczy[0].rekaGracza == null || gameClient.ListaGraczy[0].rekaGracza.Count == 0) return;
			RozmiescKartyWRece(gameClient.ListaGraczy[0].rekaGracza);
	}
}
