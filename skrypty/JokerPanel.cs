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
    public void DodajJokeraDoWidoku(Joker joker)
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

}
