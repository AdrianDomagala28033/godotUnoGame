using Godot;
using System;

public partial class MainMenu : Control
{
    [Export] public PackedScene ElementListyGraczaScena;
    public Control menuStartowe;
    public Control panelHosta;
    public Control panelDolaczania;
    public Control lobby;

    public LineEdit inputNazwaHosta;
    public LineEdit inputNazwaGracza;
    public LineEdit inputKodDolaczenia;
    public LineEdit inputPoleWpisywania;

    public Button guzikZatwierdzHost;
    public Button guzikZatwierdzDolacz;
    
    public Button guzikPowrotHost;
    public Button guzikPowrotDolacz;

    public Button guzikGotowy;
    public Button guzikStart;
    public Button guzikWyslijWiadomosc;

    public VBoxContainer kontenerGraczy;

    public Button stworzGre;
    public Button dolaczDoGry;
    public Button otworzOpcje;
    public Button wyjdz;
    public NetworkManager networkManager;

    public override void _Ready()
    {
        #region przypisanie obiektow
        stworzGre = GetNode<Button>("MenuStartowe/HostButton");
        dolaczDoGry = GetNode<Button>("MenuStartowe/JoinButton");
        otworzOpcje = GetNode<Button>("MenuStartowe/OptionsButton");
        wyjdz = GetNode<Button>("MenuStartowe/QuitButton");

        menuStartowe = GetNode<Control>("MenuStartowe");
        panelHosta = GetNode<Control>("PanelHosta");
        panelDolaczania = GetNode<Control>("PanelDolaczania");
        lobby = GetNode<Control>("PanelLobby");

        inputNazwaHosta = GetNode<LineEdit>("PanelHosta/VBoxContainer/InputNazwaHosta");
        inputNazwaGracza = GetNode<LineEdit>("PanelDolaczania/VBoxContainer/InputNazwaGracza");
        inputKodDolaczenia = GetNode<LineEdit>("PanelDolaczania/VBoxContainer/InputKodDolaczenia");

        inputPoleWpisywania = GetNode<LineEdit>("PanelLobby/HBoxContainer/Chat/HBoxContainer/InputPoleWpisywania");

        guzikZatwierdzHost = GetNode<Button>("PanelHosta/VBoxContainer/RozpocznijGreButton");
        guzikZatwierdzDolacz = GetNode<Button>("PanelDolaczania/VBoxContainer/DolaczButton");
        
        guzikPowrotHost = GetNode<Button>("PanelHosta/VBoxContainer/PowrotButton");
        guzikPowrotDolacz = GetNode<Button>("PanelDolaczania/VBoxContainer/PowrotButton");

        guzikGotowy = GetNode<Button>("PanelLobby/HBoxContainer/ListaGraczy/HBoxContainer/GotowyButton");
        guzikStart = GetNode<Button>("PanelLobby/HBoxContainer/ListaGraczy/HBoxContainer/StartButton");

        guzikWyslijWiadomosc = GetNode<Button>("PanelLobby/HBoxContainer/Chat/HBoxContainer/WyslijWiadomoscButton");

        kontenerGraczy = GetNode<VBoxContainer>("PanelLobby/HBoxContainer/ListaGraczy/KontenerGraczy");

        networkManager = GetNode<NetworkManager>("/root/NetworkManager");
        #endregion

        #region obsluga eventow
        stworzGre.Pressed += HandleOtworzPanelHosta;
        dolaczDoGry.Pressed += HandleOtworzPanelDolaczania;
        otworzOpcje.Pressed += HandleOtworzOpcje;
        wyjdz.Pressed += HandleWyjdz;

        guzikZatwierdzHost.Pressed += HandleStworzLobby;
        guzikPowrotHost.Pressed += HandlePowrotDoMenu;

        guzikZatwierdzDolacz.Pressed += HandleDolaczDoGry;
        guzikPowrotDolacz.Pressed += HandlePowrotDoMenu;

        networkManager.OnDolaczono += AktualizujListe;

        networkManager.OnPolaczono += PokazLobby;
        guzikGotowy.Pressed += HandleGotowy;
        guzikStart.Pressed += HandleStworzGre;

        networkManager.OnListaGraczyZmieniona += AktualizujListe;

        #endregion
        
        panelHosta.Hide();
        panelDolaczania.Hide();
        lobby.Hide();
        
    }

    #region lobby
    private void HandleGotowy()
    {
        networkManager.Rpc(nameof(NetworkManager.ZglosGotowosc)); 
    }
    private void PokazLobby()
    {
        panelHosta.Hide();
        panelDolaczania.Hide();
        menuStartowe.Hide();
        lobby.Show();
        AktualizujListe();
    }
    public void AktualizujListe()
    {
        int iloscGotowychGraczy = 0;
        foreach (Node child in kontenerGraczy.GetChildren())
        {
            child.QueueFree();
        }
        foreach (DaneGracza gracz in networkManager.ListaGraczy)
        {
            Control nowyElement = (Control)ElementListyGraczaScena.Instantiate();
            Label labelNazwa = nowyElement.GetNode<Label>("NazwaGracza");
            Label labelStatus = nowyElement.GetNode<Label>("StatusGotowosci");

            labelNazwa.Text = gracz.Nazwa;
            labelStatus.Text = gracz.CzyGotowy ? "GOTOWY" : "CZEKA...";

            kontenerGraczy.AddChild(nowyElement);
            if(gracz.Id == Multiplayer.GetUniqueId())
            {
                guzikGotowy.Modulate = gracz.CzyGotowy ? Colors.Green : Colors.White;
                guzikGotowy.Text = gracz.CzyGotowy ? "NIE GOTOWY" : "GOTOWY";
            }
            if (gracz.CzyGotowy)
            {
                iloscGotowychGraczy++;
            }
        }
        if(networkManager.ListaGraczy.Count > 0 && iloscGotowychGraczy == networkManager.ListaGraczy.Count && Multiplayer.IsServer())
            guzikStart.Visible = true;
        else
            guzikStart.Visible = false;
    }
    private void HandleStworzGre()
    {
        foreach (var gracz in networkManager.ListaGraczy)
        {
            gracz.CzyGotowy = false;
            networkManager.Rpc(nameof(NetworkManager.ZaktualizujStanGotowosciClienta), gracz.Id, false);   
        }
        networkManager.Rpc(nameof(NetworkManager.ZaladujGre), "res://sceny/rozgrywka/stol_gry.tscn");
    }
#endregion
#region widok tworzenia i dolaczania do gry
    private void HandlePowrotDoMenu()
    {
        panelHosta.Hide();
        panelDolaczania.Hide();
        menuStartowe.Show();
    }
    private void HandleDolaczDoGry()
    {
        string nazwaGracza = inputNazwaGracza.Text;
        string kodDolaczenia = inputKodDolaczenia.Text;
        if(string.IsNullOrEmpty(kodDolaczenia))
            inputKodDolaczenia.Text = "127.0.0.1";
        if(!string.IsNullOrEmpty(nazwaGracza) && !string.IsNullOrEmpty(kodDolaczenia))
        {
            networkManager.DolaczDoGry(kodDolaczenia, nazwaGracza);
        }
    }
    private void HandleOtworzPanelHosta()
    {
        menuStartowe.Hide();
        panelHosta.Show();
    }
    private void HandleOtworzPanelDolaczania()
    {
        menuStartowe.Hide();
        panelDolaczania.Show();
    }
    private void HandleOtworzOpcje()
    {
        GD.Print("Chcę otworzyć opcje");
    }
    private void HandleWyjdz()
    {
        GD.Print("Chcę wyjść");
    }
    private void HandleStworzLobby()
    {
        string nazwaHosta = inputNazwaHosta.Text;
        if(!string.IsNullOrEmpty(nazwaHosta))
        {
            if(networkManager.Peer == null)
            {
                networkManager.HostujGre(nazwaHosta);
                guzikZatwierdzHost.Text = "Rozpocznij gre";
            }
        }
    }
    #endregion
    public override void _ExitTree()
    {
        if (networkManager != null)
        {
            networkManager.OnDolaczono -= AktualizujListe;
            networkManager.OnPolaczono -= PokazLobby;
            networkManager.OnListaGraczyZmieniona -= AktualizujListe;
        }
    }
}
