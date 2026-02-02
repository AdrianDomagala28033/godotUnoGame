using Godot;
using System;

public partial class ProfilPrzeciwnika : Node3D
{
    [ExportGroup("Referencje UI")]
    [Export] private Label3D NazwaGracza;
    [Export] private Sprite3D TurnMarker;
    [Export] private Node3D KartyContainer;
    
    [ExportGroup("Ustawienia")]
    [Export] private PackedScene SzablonKarty;
    [Export] private int MaxWidocznychKart = 8;
    [Export] private float RozstawKart = 0.4f;

    private int ostatniaIloscKart = -1;

    public override void _Ready()
    {
        if(TurnMarker != null) TurnMarker.Visible = false;
    }
    public void UstawDane(string nazwa, bool czyJegoTura)
    {
        if(nazwa != null) NazwaGracza.Text = nazwa;
        if(TurnMarker != null)
        {
            TurnMarker.Visible = czyJegoTura;
            if (czyJegoTura)
                TurnMarker.Modulate = new Color(1, 1, 0, 1);
        }
    }
    public void AktualizujKarty(int faktycznaIloscKart)
    {
        GD.PrintErr($"[ProfilPrzeciwnika] {Name}: AktualizujKarty wywołane. Ilość: {faktycznaIloscKart}, Ostatnia: {ostatniaIloscKart}");
        if(faktycznaIloscKart == ostatniaIloscKart) 
        {
            GD.PrintErr($"[ProfilPrzeciwnika] {Name}: Ilość bez zmian, wychodzę.");
            return;
        }
        ostatniaIloscKart = faktycznaIloscKart;
        if (KartyContainer == null)
        {
            GD.PrintErr($"[ProfilPrzeciwnika] {Name}: BŁĄD! KartyContainer jest NULL!");
            return;
        }
        foreach (Node child in KartyContainer.GetChildren())
            child.QueueFree();
        if (faktycznaIloscKart == 0) return;
        if (SzablonKarty == null)
        {
            GD.PrintErr($"[ProfilPrzeciwnika] {Name}: BŁĄD! SzablonKarty jest NULL! Przypisz w Inspektorze.");
            return;
        }
        if (SzablonKarty == null) return;

        int kartyDoWyswietlania = faktycznaIloscKart;
        bool czyJestNadmiar = false;
        int nadmiar = 0;
        if(faktycznaIloscKart > MaxWidocznychKart)
        {
            kartyDoWyswietlania = MaxWidocznychKart;
            czyJestNadmiar = true;
            nadmiar = faktycznaIloscKart - MaxWidocznychKart;
        }
        float totalWidth = ((czyJestNadmiar ? kartyDoWyswietlania + 1 : kartyDoWyswietlania) - 1) * RozstawKart;
        float startX = -totalWidth / 2.0f;
        GD.PrintErr($"[ProfilPrzeciwnika] {Name}: Przystępuję do tworzenia {kartyDoWyswietlania} kart.");
        for (int i = 0; i < kartyDoWyswietlania; i++)
            StworzKarteWizualna(startX + (i * RozstawKart), i, false);
        if (czyJestNadmiar)
        {
            float pozycjaX = startX + (kartyDoWyswietlania * RozstawKart);
            StworzKarteWizualna(pozycjaX, kartyDoWyswietlania, true, nadmiar);
        }
    }

    private void StworzKarteWizualna(float x, int index, bool czyToLicznik, int iloscNadmiaru = 0)
    {
        var kartaNode = SzablonKarty.Instantiate() as Karta;
        if (kartaNode == null)
        {
            GD.PrintErr($"[ProfilPrzeciwnika] {Name}: Błąd instancjonowania karty!");
            return;
        }

        kartaNode.Kolor = "Rewers";
        kartaNode.Wartosc = "";
        kartaNode.InputRayPickable = false;
        KartyContainer.AddChild(kartaNode);
        GD.PrintErr($"[ProfilPrzeciwnika] {Name}: Stworzono kartę {index}. Pozycja Lokalna: {kartaNode.Position}, Globalna: {kartaNode.GlobalPosition}");
        float z = index * 0.005f;
        kartaNode.Position = new Vector3(x, 0.1f, z);
        kartaNode.RotationDegrees = new Vector3(-90, 0, 0);
        kartaNode.Scale = Vector3.One * 0.6f;
        if (czyToLicznik)
        {
            var sprite = kartaNode.GetNodeOrNull<Sprite3D>("Sprite3D");
            if (sprite != null) 
                sprite.Modulate = new Color(0.4f, 0.4f, 0.4f);
            var label = new Label3D();
            label.Text = $"+{iloscNadmiaru}";
            label.FontSize = 128;
            label.OutlineRenderPriority = 10;
            label.RenderPriority = 100;
            label.Billboard = BaseMaterial3D.BillboardModeEnum.Enabled;
            label.Position = new Vector3(0, 0.02f, 0.1f);
        
            kartaNode.AddChild(label);
        }
    }
}
