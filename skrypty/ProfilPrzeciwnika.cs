using Godot;
using System;

[Tool]
public partial class ProfilPrzeciwnika : Node3D
{
    [ExportGroup("Referencje UI")]
    [Export] private Label3D NazwaGracza;
    [Export] private Node3D AwatarGracza;
    [Export] private Sprite3D TurnMarker;
    [Export] private Node3D KartyContainer;
    
    [ExportGroup("Ustawienia")]
    [Export] private PackedScene SzablonKarty;
    [Export] private int MaxWidocznychKart = 7;
    [Export] private float RozstawKart = 0.4f;
    [Export] private float SkalaKarty = 0.45f;
    [Export] public bool OdwrocLicznik = false;
    private Vector3 _pozycjaNazwy = new Vector3(0, 0.4f, 0);
    [Export] public Vector3 PozycjaNazwy 
    { 
        get => _pozycjaNazwy; 
        set 
        { 
            _pozycjaNazwy = value; 
            if (NazwaGracza != null) NazwaGracza.Position = _pozycjaNazwy;
        } 
    }
    private Vector3 _pozycjaAwataru = new Vector3(0, 0.4f, 0);
    [Export] public Vector3 PozycjaAwataru 
    { 
        get => _pozycjaAwataru; 
        set 
        { 
            _pozycjaAwataru = value;
            if (AwatarGracza != null) AwatarGracza.Position = _pozycjaAwataru;
        } 
    }

    private int ostatniaIloscKart = -1;

    public override void _Ready()
    {
        if(TurnMarker != null) TurnMarker.Visible = false;
        if (NazwaGracza != null)
        {
            NazwaGracza.HorizontalAlignment = HorizontalAlignment.Center;
            NazwaGracza.VerticalAlignment = VerticalAlignment.Bottom;
            NazwaGracza.Position = PozycjaNazwy; 
        }
        if (AwatarGracza != null) AwatarGracza.Position = PozycjaAwataru;
        if (Engine.IsEditorHint())
        {
            UstawDane("Gracz (Edytor)", false);
            AktualizujKarty(8);
        }
    }

    public void UstawDane(string nazwa, bool czyJegoTura)
    {
        if(NazwaGracza != null && nazwa != null) NazwaGracza.Text = nazwa;
        if(NazwaGracza != null) NazwaGracza.Position = PozycjaNazwy; 
        
        if(TurnMarker != null)
        {
            TurnMarker.Visible = czyJegoTura;
            TurnMarker.Position = new Vector3(PozycjaNazwy.X, PozycjaNazwy.Y + 0.3f, PozycjaNazwy.Z);
            
            if (czyJegoTura)
                TurnMarker.Modulate = new Color(1, 1, 0, 1);
        }
    }

    public void AktualizujKarty(int faktycznaIloscKart)
    {
        if(faktycznaIloscKart == ostatniaIloscKart && !Engine.IsEditorHint()) return;
        ostatniaIloscKart = faktycznaIloscKart;
        if (KartyContainer == null) return;
        foreach (Node child in KartyContainer.GetChildren())
            child.QueueFree();
        if (faktycznaIloscKart == 0) return;
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
        for (int i = 0; i < kartyDoWyswietlania; i++)
            StworzKarteWizualna(startX + (i * RozstawKart), i, kartyDoWyswietlania, false);
        if (czyJestNadmiar)
        {
            float pozycjaX = startX + (kartyDoWyswietlania * RozstawKart);
            StworzKarteWizualna(pozycjaX, kartyDoWyswietlania, kartyDoWyswietlania, true, nadmiar);
        }
    }

    private void StworzKarteWizualna(float x, int index, int totalCards, bool czyToLicznik, int iloscNadmiaru = 0)
    {
        var kartaNode = SzablonKarty.Instantiate() as Karta;
        if (kartaNode == null)
            return;

        kartaNode.Kolor = "Rewers";
        kartaNode.Wartosc = "";
        kartaNode.InputRayPickable = false;
        KartyContainer.AddChild(kartaNode);
        float yOffset = 0.02f + ((totalCards - index) * 0.005f);
        kartaNode.Position = new Vector3(x, yOffset, 0);
        kartaNode.RotationDegrees = new Vector3(-90, 0, 0);
        kartaNode.Scale = Vector3.One * SkalaKarty;
        
        if (czyToLicznik)
        {
            var sprite = kartaNode.GetNodeOrNull<Sprite3D>("Sprite3D");
            if (sprite != null) 
            {
                sprite.Modulate = new Color(0.4f, 0.4f, 0.4f);
            }
            
            var label = new Label3D();
            label.Text = $"+{iloscNadmiaru}";
            label.FontSize = 128;
            label.OutlineRenderPriority = 10;
            label.RenderPriority = 100;
            label.Billboard = BaseMaterial3D.BillboardModeEnum.Disabled;
            label.DoubleSided = true;
            label.RotationDegrees = Vector3.Zero;
            if (OdwrocLicznik)
                label.RotationDegrees = new Vector3(0, 180, 0);
            
            label.Position = new Vector3(0.1f, 0, 0.05f); 
            
            kartaNode.AddChild(label);
        }
    }
}