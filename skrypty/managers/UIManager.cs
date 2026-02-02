using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : Control
{
	private GameClient gameClient;
	[Export] private JokerPanel jokerPanel;
	[Export] private Label3D LewyNapis;
	[Export] private Label3D PrawyNapis;
	[Export] private ProfilPrzeciwnika ProfilLewy;
    [Export] private ProfilPrzeciwnika ProfilGorny;
    [Export] private ProfilPrzeciwnika ProfilPrawy;
	private StatusPanel statusPanel;
	private Control koniecRundyPanel;
	private Marker3D stosKart;
	private float aktualnyScrollIndex = 0.0f;
	private int docelowyScrollIndex = 0;
	private float katNaKarte = 6.0f;
	private int maxWidocznychKart = 7;

	public override void _Ready()
	{
		statusPanel = GetNodeOrNull<StatusPanel>("StatusPanel");
		koniecRundyPanel = GetNodeOrNull<Control>("KoniecRundyPanel");
		if (koniecRundyPanel != null)
			koniecRundyPanel.Hide();


		stosKart = GetNode<Marker3D>("/root/StolGry/StosKart");

		var interfejs = GetNodeOrNull<Control>("InterfejsGry");
		if (interfejs != null) 
			interfejs.MouseFilter = Control.MouseFilterEnum.Ignore;

		if (jokerPanel != null)
        	jokerPanel.MouseFilter = Control.MouseFilterEnum.Pass;
		if(LewyNapis != null && PrawyNapis != null)
		{
			LewyNapis.Visible = false;
			PrawyNapis.Visible = false;
		}
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
		gameClient.OnKartaZagrana += (karta, id) => AktualizujPrzeciwnikow();
		gameClient.OnKartaDobrano += (id) => AktualizujPrzeciwnikow();
		gameClient.OnTuraUstawiona += (id) => AktualizujPrzeciwnikow();
		gameClient.OnListaGraczyZmieniona += AktualizujPrzeciwnikow;

		gameClient.OnRozmiescKarty += (reka) => RozmiescKartyWRece(reka);
		gameClient.OnDodajKarteNaStos += (karta, pos, idx) => DodajKarteNaStos(karta, pos, idx);
		gameClient.OnKolorDoUstawienia += (kolor) => UstawKolor(kolor);
		gameClient.OnReceZmienione += () => {
			if (gameClient != null && gameClient.ListaGraczy[0].rekaGracza != null && gameClient.ListaGraczy[0].rekaGracza.Count > 0)
				RozmiescKartyWRece(gameClient.ListaGraczy[0].rekaGracza);
		};
	}

	public override void _Input(InputEvent @event)
	{
		var mojaReka = gameClient.ListaGraczy.Find(g => g.CzyToGraczLokalny)?.rekaGracza;
		if (mojaReka == null || mojaReka.Count == 0) return;

		if (@event is InputEventMouseButton mb && mb.Pressed)
		{
			if (mb.ButtonIndex == MouseButton.WheelDown)
				docelowyScrollIndex++;
			else if (mb.ButtonIndex == MouseButton.WheelUp)
				docelowyScrollIndex--;
			float polowaReki = (mojaReka.Count - 1) / 2.0f;
			docelowyScrollIndex = (int)Mathf.Clamp(docelowyScrollIndex, -polowaReki, polowaReki);
			AktualizujLicznikKart(mojaReka.Count);
		}
	}
	public override void _Process(double delta)
	{
		aktualnyScrollIndex = Mathf.Lerp(aktualnyScrollIndex, (float)docelowyScrollIndex, (float)delta * 10.0f);

		var graczLokalny = gameClient.ListaGraczy.Find(g => g.CzyToGraczLokalny);
		if (graczLokalny != null && graczLokalny.rekaGracza.Count > 0)
			RozmiescKartyWRece(graczLokalny.rekaGracza);
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

	public void DodajKarteNaStos(Karta karta, Vector3 pozycjaStosu, int indexGracza)
	{
		if (karta.GetParent() == null)
			AddChild(karta);

		karta.UstawSkale(0.8f);
		karta.Show();
		karta.CallDeferred("ZagrajNaStol", pozycjaStosu);
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
		float stalaPozycjaStrzalek = (maxWidocznychKart / 2.0f) + 0.8f;
		if (rekaGracza == null || rekaGracza.Count == 0) return;
		
		int iloscKart = rekaGracza.Count;
		AktualizujLicznikKart(iloscKart);
		Vector3 centrumMarkera = gameClient.SpawnGraczGlowny.GlobalPosition;

		float szerokoscLuku = -10.0f;
		float glebokoscLuku = -15.0f;

		float limitWidocznosciStopnie = (maxWidocznychKart * katNaKarte) / 2.0f;
		float srodekReki = (iloscKart - 1) / 2.0f;
		float? lewaKrawedzIndex = null;
		float? prawaKrawedzIndex = null;
		int idxPierwszej = -1;
		int idxOstatniej = -1;

		for(int i = 0; i < iloscKart; i++)
		{
			Karta karta = rekaGracza[i];
			if (karta.CzyPodKursorem) continue;
			if(karta == null) continue;
			float wirtualnyIndex = (i - srodekReki) - aktualnyScrollIndex;
			float aktualnyKatStopnie = wirtualnyIndex * katNaKarte;

			
			if (Mathf.Abs(aktualnyKatStopnie) > limitWidocznosciStopnie)
			{
				if (karta.Visible)
				{
					karta.Visible = false;
					karta.InputRayPickable = false;
				} 
				continue; 
			}
			if (lewaKrawedzIndex == null)
			{
				lewaKrawedzIndex = wirtualnyIndex;
				idxPierwszej = i;
			}
			prawaKrawedzIndex = wirtualnyIndex;
    		idxOstatniej = i;
			if (!karta.Visible)
			{
				karta.Visible = true;
				karta.InputRayPickable = true;	
			}
			
			float aktualnyKatRadiany = Mathf.DegToRad(aktualnyKatStopnie);
			float x = Mathf.Sin(aktualnyKatRadiany) * szerokoscLuku;
			float z = -(1.0f - Mathf.Cos(aktualnyKatRadiany)) * glebokoscLuku;

			float y = 0; 

			Vector3 nowaPozycja = centrumMarkera + new Vector3(x, y, z);
			Vector3 nowaRotacja = new Vector3(-60, aktualnyKatStopnie * 0.5f, 0);

			if(karta.GetParent() == null)
				gameClient.SpawnGraczGlowny.GetParent().AddChild(karta);

			karta.InputRayPickable = true;
			karta.GlobalPosition = nowaPozycja;
			karta.RotationDegrees = nowaRotacja;
			
			karta.UstawOryginalnaPozycje(nowaPozycja);
			karta.DomyslnaRotacja = nowaRotacja;
			karta.UstawSkale(0.8f);
		}
		if (LewyNapis != null)
		{
			if (lewaKrawedzIndex.HasValue && idxPierwszej > 0)
			{
				LewyNapis.Visible = true;
				int ileUkrytych = idxPierwszej;
            	LewyNapis.Text = $"+{ileUkrytych}";
				UstawObiektNaLuku(LewyNapis, lewaKrawedzIndex.Value - 1.0f, 0f);
			}
			else
				LewyNapis.Visible = false;
		}
		if (PrawyNapis != null)
		{
			if (prawaKrawedzIndex.HasValue && idxOstatniej < iloscKart - 1)
			{
				PrawyNapis.Visible = true;
				int ileUkrytych = (iloscKart - 1) - idxOstatniej;
				PrawyNapis.Text = $"+{ileUkrytych}";
				UstawObiektNaLuku(PrawyNapis, prawaKrawedzIndex.Value + 1.0f, 0f);
			}
			else
				PrawyNapis.Visible = false;
		}
	}
	private void RozmiescKartyWRece()
	{
		if (gameClient == null) return;
		if (gameClient.ListaGraczy[0].rekaGracza == null || gameClient.ListaGraczy[0].rekaGracza.Count == 0) return;
		RozmiescKartyWRece(gameClient.ListaGraczy[0].rekaGracza);
	}
	private void UstawObiektNaLuku(Node3D obiekt, float wirtualnyIndex, float yOffset)
	{
		float szerokoscLuku = -10.0f;
		float glebokoscLuku = -15.0f;
		float aktualnyKatStopnie = wirtualnyIndex * katNaKarte;
		float aktualnyKatRadiany = Mathf.DegToRad(aktualnyKatStopnie);
		float x = Mathf.Sin(aktualnyKatRadiany) * szerokoscLuku;
		float z = -(1.0f - Mathf.Cos(aktualnyKatRadiany)) * glebokoscLuku;
		z -= Mathf.Abs(wirtualnyIndex) * 0.15f;
		Vector3 centrum = gameClient.SpawnGraczGlowny.GlobalPosition;
		obiekt.GlobalPosition = centrum + new Vector3(x, yOffset, z);
		obiekt.RotationDegrees = new Vector3(-60, aktualnyKatStopnie * 0.5f, 0);
	}
	private void AktualizujLicznikKart(int ilosscKart)
	{
		if(LewyNapis == null || PrawyNapis == null) return;
		if(ilosscKart <= maxWidocznychKart)
		{
			LewyNapis.Visible = false;
			PrawyNapis.Visible = false;
			return;
		}
		float polowaReki = (ilosscKart - 1) / 2.0f;
		LewyNapis.Visible = docelowyScrollIndex > -polowaReki;
		PrawyNapis.Visible = docelowyScrollIndex < polowaReki;
	}
	public void UstawKierunekStrzalek(bool czyZgodnie)
    {
        if (statusPanel != null)
        {
            statusPanel.UstawKierunek(czyZgodnie);
        }
    }
	public void AktualizujPrzeciwnikow()
	{
		if(gameClient == null || gameClient.ListaGraczy == null) return;
		var graczLokalny = gameClient.ListaGraczy.Find(g => g.CzyToGraczLokalny);
		if(graczLokalny == null) return;

		if (ProfilLewy != null) ProfilLewy.Visible = false;
		if (ProfilGorny != null) ProfilGorny.Visible = false;
		if (ProfilPrawy != null) ProfilPrawy.Visible = false;

		int mojIndex = gameClient.ListaGraczy.IndexOf(graczLokalny);
		int liczbaGraczy = gameClient.ListaGraczy.Count;
		long idGraczaTury = gameClient.AktualnyGraczTuryId;

		for (int i = 0; i < liczbaGraczy; i++)
		{
			var innyGracz = gameClient.ListaGraczy[i];
			if (innyGracz.CzyToGraczLokalny) continue;
			int offset = (i - mojIndex + liczbaGraczy) % liczbaGraczy;
			ProfilPrzeciwnika profilPrzeciwnika = null;
			if(liczbaGraczy == 2)
				profilPrzeciwnika = ProfilGorny;
			else 
			{
				if (offset == 1) profilPrzeciwnika = ProfilLewy;
				else if (offset == 2) profilPrzeciwnika = ProfilGorny;
				else if (offset == 3) profilPrzeciwnika = ProfilPrawy;
			}
			if (profilPrzeciwnika != null)
			{
				profilPrzeciwnika.Visible = true;
				bool czyJegoTura = (innyGracz.IdGracza == idGraczaTury);
				profilPrzeciwnika.UstawDane(innyGracz.Nazwa, czyJegoTura);
				profilPrzeciwnika.AktualizujKarty(innyGracz.LiczbaKart);
			}
		}
	}
}
