using Godot;
using System;

public partial class WyborKoloru : CanvasLayer
{
	[Signal]
	public delegate void KolorWybranyEventHandler(string kolor);

    public override void _Ready()
    {
		GetNode<Button>("ColorRect/HBoxContainer/ButtonCzerwony").Pressed += () => _OnPrzyciskKoloruNacisniety("Czerwony");
		GetNode<Button>("ColorRect/HBoxContainer/ButtonNiebieski").Pressed += () => _OnPrzyciskKoloruNacisniety("Niebieski");
		GetNode<Button>("ColorRect/HBoxContainer/ButtonZielony").Pressed += () => _OnPrzyciskKoloruNacisniety("Zielony");
		GetNode<Button>("ColorRect/HBoxContainer/ButtonZolty").Pressed += () => _OnPrzyciskKoloruNacisniety("Zolty");
    }

    private void _OnPrzyciskKoloruNacisniety(string kolor)
    {
		GD.Print($"Wybrano kolor: {kolor}");
		EmitSignal(SignalName.KolorWybrany, kolor);
		this.Hide();
    }
}
