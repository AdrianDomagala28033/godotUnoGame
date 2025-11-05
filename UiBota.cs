using Godot;
using System;
using System.Threading.Tasks;

public partial class UiBota : Control
{
    private Label _licznikLabel;
    private ColorRect awatar;
	
	public override void _Ready()
    {
		_licznikLabel = GetNode<Label>("VBoxContainer/IloscKart");
    }

    public void AktualizujLicznik(int ilosc)
    {
        _licznikLabel.Text = $"{ilosc} kart";
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
