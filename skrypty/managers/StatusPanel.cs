using Godot;
using System;

public partial class StatusPanel : Node3D
{
	[Export] private Sprite3D KierunekStrzalki;
	[Export] private Sprite3D WybranyKolor;
	[Export] private Label3D Label3D;
	private float mnoznikKierunku = 1.0f;
    private float predkoscObrotu = -1.0f;

    public override void _Ready()
	{
		if(Label3D != null) Label3D.Text = "0";
		UstawKolor("brak");
    }
    public override void _Process(double delta)
    {
		if(KierunekStrzalki != null && KierunekStrzalki.Visible)
		{
			KierunekStrzalki.RotateY((float)delta * predkoscObrotu * mnoznikKierunku);
		}
	}

	public void UstawDlug(int dlug)
	{
		if (Label3D == null) return;
		if(dlug > 0)
		{
			Label3D.Text = $"{dlug}";
		}
	}
	public void UstawKolor(string nazwaKoloru)
	{
		if (WybranyKolor == null)
        {
            GD.PrintErr("[StatusPanel] Brak przypisanego Sprite3D 'WybranyKolor'!");
            return;
        }
		Color nowyKolor;

		switch (nazwaKoloru.ToLower())
        {
            case "czerwony": nowyKolor = Colors.Red; break;
            case "zielony": nowyKolor = Colors.Green; break;
            case "niebieski": nowyKolor = Colors.Blue; break;
            case "zolty": nowyKolor = Colors.Yellow; break;
            
            default: nowyKolor = new Color(0.2f, 0.2f, 0.2f); break;
        }
		WybranyKolor.Modulate = nowyKolor;
	}
	public void UstawKierunek(bool kierunekLewy)
	{
		mnoznikKierunku = kierunekLewy ? 1.0f : -1.0f;
		if(KierunekStrzalki != null)
		{
			float scaleX = kierunekLewy ? 1 : -1;
            KierunekStrzalki.Scale = new Vector3(scaleX, 1, 1);
		}
		
	}
}
