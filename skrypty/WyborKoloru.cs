using Godot;
using System;

public partial class WyborKoloru : Node3D
{
	[Signal]
	public delegate void KolorWybranyEventHandler(string kolor);

    public override void _Ready()
    {
		foreach (Node dziecko in GetChildren())
        {
            string nazwa = dziecko.Name.ToString();
            
            if (nazwa.Contains("Cwiartka"))
            {
                var area = dziecko.GetNodeOrNull<Area3D>("Area3D");

                if (area != null)
                {
                    string kolor = "";
                    if (nazwa.Contains("Czerwona")) kolor = "Czerwony";
                    else if (nazwa.Contains("Niebieska")) kolor = "Niebieski";
                    else if (nazwa.Contains("Zielona")) kolor = "Zielony";
                    else if (nazwa.Contains("Zolta")) kolor = "Zolty";

                    if (kolor != "")
                    {
                        area.InputEvent += (camera, @event, position, normal, shapeIdx) => 
                            _OnPrzyciskKoloruNacisniety(@event, kolor);
                        
                        GD.Print($"[WyborKoloru] Sukces! Podłączono: {kolor}");
                    }
                }
                else
                {
                    GD.PrintErr($"[WyborKoloru] Ćwiartka {nazwa} nie ma Area3D!");
                }
            }
        }
    }

    private void PodlaczCwiartke(string sciezka, string kolor)
    {
        var area = GetNodeOrNull<Area3D>(sciezka);
		if(area != null)
		{
			area.InputEvent += (camera, @event, position, normal, shapeIdx) => 
                _OnPrzyciskKoloruNacisniety(@event, kolor);
		}
		else
		{
			GD.PrintErr($"[WyborKoloru] Błąd! Nie znaleziono: {sciezka}");
		}
    }


    private void _OnPrzyciskKoloruNacisniety(InputEvent @event, string kolor)
    {
		if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.KolorWybrany, kolor);   
            this.Hide();
        }
    }
}
