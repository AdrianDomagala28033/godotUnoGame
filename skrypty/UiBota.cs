using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class UiBota : Control
{
    private Label _licznikLabel;
    private ColorRect awatar;
    [Export]
    private PackedScene SzablonRewersu;
    [Export]
    private Node2D wezelWachlarza;
    private List<Node2D> kartyWizualne = new List<Node2D>();
	
	public override void _Ready()
    {
        wezelWachlarza = GetNode<Node2D>("WachlarzKart");
        _licznikLabel = GetNode<Label>("VBoxContainer/IloscKart");
    }

    public void AktualizujLicznik(int ilosc)
    {
        _licznikLabel.Text = $"{ilosc} kart";
        while (kartyWizualne.Count < ilosc)
        {
            Node2D nowaKarta = (Node2D)SzablonRewersu.Instantiate();
            wezelWachlarza.AddChild(nowaKarta);
            kartyWizualne.Add(nowaKarta);
        }
        while (kartyWizualne.Count > ilosc)
        {
            Node2D kartaDoUsuniecia = kartyWizualne[0];
            kartyWizualne.RemoveAt(0);
            kartaDoUsuniecia.QueueFree();
        }
        RozmiescWachlarz();
    }

    private void RozmiescWachlarz()
    {
        int iloscKart = kartyWizualne.Count;
        if (iloscKart == 0) return;

        float odstep = 30;
        float rotacja = 5;

        float calkowitaSzerokosc = (iloscKart - 1) * odstep;
        float calkowitaRotacja = (iloscKart - 1) * rotacja;

        float pozycjaStartowaX = -calkowitaSzerokosc / 2;
        float rotacjaStartowa = -calkowitaRotacja / 2;

        for (int i = 0; i < kartyWizualne.Count; i++)
        {
            Node2D karta = kartyWizualne[i];
            karta.Position = new Vector2(pozycjaStartowaX + (i * odstep), 0);
            karta.RotationDegrees = rotacjaStartowa + (i * rotacja);
            karta.ZIndex = i;

        }
    }


    public async Task UstawAktywny(bool jestAktywny)
    {
        if (jestAktywny)
        {
            awatar.Color = new Color("ffffff");
        }
        else
        {
            awatar.Color = new Color("666666");
        }
    }
    
}
