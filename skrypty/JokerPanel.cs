using Godot;
using System;

public partial class JokerPanel : PanelContainer
{
    [Export] private PackedScene szablonSlotu;
    private HBoxContainer slotyContainer;
    private Button przyciskRozwin;
    private AnimationPlayer animacja;
    private Label infoLabel;
    private Control infoPanel;
    private bool jestRozwiniete = false;
    private NetworkManager NetworkManager {get; set;}

    public override void _Ready()
    {
        slotyContainer = GetNode<HBoxContainer>("Uklad/SlotyContainer");
        przyciskRozwin = GetNode<Button>("Uklad/PrzyciskRozwin");
        animacja = GetNode<AnimationPlayer>("Uklad/WysunPanel");
        infoLabel = GetNode<Label>("Uklad/InfoPanel/InfoLabel");
        infoPanel = GetNode<Control>("Uklad/InfoPanel");

        przyciskRozwin.Pressed += PrzelaczWidok;
        infoPanel.Hide();
        przyciskRozwin.Text = "JOKERY ▼";
        jestRozwiniete = false;
        NetworkManager = GetTree().Root.GetNode<NetworkManager>("NetworkManager");
        NetworkManager.JokeryZmienione += OdswiezJokery;
        OdswiezJokery(new string[0]);
    }
    public override void _ExitTree()
    {
        if (NetworkManager != null)
        {
            NetworkManager.JokeryZmienione -= OdswiezJokery;
        }
    }
    private void PrzelaczWidok()
    {
        if (jestRozwiniete)
        {
            animacja.PlayBackwards("WysunPanel");
            przyciskRozwin.Text = "JOKERY ▼";
        }
        else
        {
            animacja.Play("WysunPanel");
            przyciskRozwin.Text = "JOKERY ▲";

        }
        jestRozwiniete = !jestRozwiniete;
    }
    public void DodajJokeraDoWidoku(DaneJokera joker)
    {
        JokerSlot slot = (JokerSlot)szablonSlotu.Instantiate();
        slotyContainer.AddChild(slot);
        slot.Inicjalizuj(joker);

        slot.OnHover += PokazOpis;
        slot.OnHoverExit += UkryjOpis;
    }
    private void PokazOpis(string nazwa, string opis)
    {
        infoLabel.Text = $"{nazwa}\n{opis}";
        infoPanel.Show();
    }
    private void UkryjOpis()
    {
        infoPanel.Hide();
    }
    public void OdswiezJokery(string[] noweJokery)
    {
        foreach (var joker in slotyContainer.GetChildren())
        {
            joker.QueueFree();
        }
        long idGracza = Multiplayer.GetUniqueId();
        var daneGracza = NetworkManager.ListaGraczy.Find(g => g.Id == idGracza);
        if(daneGracza == null) return;
        var jokeryGracza = daneGracza.PosiadaneJokery;
        foreach (var joker in jokeryGracza)
        {
            var daneJokera = JokerManager.PobierzJokera(joker);
            JokerSlot slot = (JokerSlot)szablonSlotu.Instantiate();
            slot.Inicjalizuj(daneJokera);
            slot.OnHover += PokazOpis;
            slot.OnHoverExit += UkryjOpis;
            slotyContainer.AddChild(slot);
        }
    }
}
