using Godot;
using System;

public partial class StatusPanel : PanelContainer
{
	private Label labelDlug;
	private Label labelKolor;
	private ColorRect aktualnyKolor;

    public override void _Ready()
	{
		labelDlug = GetNode<Label>("VBoxContainer/LabelDlug");
		labelKolor = GetNode<Label>("VBoxContainer/LabelKolor");
		aktualnyKolor = GetNode<ColorRect>("VBoxContainer/LabelKolor/AktualnyKolor");

		if (labelDlug == null) GD.PrintErr("[StatusPanel] Nie znaleziono LabelDlug pod VBoxContainer!");
		if (labelKolor == null) GD.PrintErr("[StatusPanel] Nie znaleziono LabelKolor pod VBoxContainer!");
		if (aktualnyKolor == null) GD.PrintErr("[StatusPanel] Nie znaleziono AktuanyKolor (ColorRect) pod VBoxContainer!");
    }
	public void UstawDlug(int dlug)
	{
		if (labelDlug == null) return;
		labelDlug.Text = $"Musisz dobrać {dlug} kart";
	}
	public void UstawKolor(string nazwaKoloru)
	{
		if (aktualnyKolor == null)
		{
			GD.PrintErr($"[StatusPanel] UstawKolor: brak aktualnyKolor. nazwaKoloru={nazwaKoloru}");
		}
		Color kolor;

		switch (nazwaKoloru.ToLower())
		{
			case "czerwony": kolor = Colors.Red; break;
			case "zielony": kolor = Colors.Green; break;
			case "niebieski": kolor = Colors.Blue; break;
			case "zolty": kolor = Colors.Yellow; break;
			default: kolor = Colors.Gray; break;
		}
		aktualnyKolor.Color = kolor;
	}
}
