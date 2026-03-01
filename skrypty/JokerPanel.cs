using Godot;
using System;

public partial class JokerPanel : Control
{
    [Export] private PackedScene szablonSlotu;
    [Export] private Button buttonWysun;
    private VBoxContainer slotyContainer;
    private NetworkManager NetworkManager {get; set;}
    private bool jestWysuniety = false;
    private Tween aktywnaAnimacja;

    public override void _Ready()
    {
        slotyContainer = GetNode<VBoxContainer>("SlotyContainer");
        NetworkManager = GetTree().Root.GetNode<NetworkManager>("NetworkManager");
        NetworkManager.JokeryZmienione += OdswiezJokery;
        OdswiezJokery(new string[0]);
        buttonWysun.Pressed += HandleKliknietoButton;
    }
    public void HandleKliknietoButton()
    {
        aktywnaAnimacja = CreateTween();
        if (!jestWysuniety)
        {
            aktywnaAnimacja.TweenProperty(this, "global_position",  new Vector2(0, 0), 0.5);
            buttonWysun.Text = "<\n<\n<\n";
            jestWysuniety = true;
        }
        else
        {
            aktywnaAnimacja.TweenProperty(this, "global_position",  new Vector2(-150, 0), 0.5);
            buttonWysun.Text = ">\n>\n>\n";
            jestWysuniety = false;
        }
    }
    public override void _ExitTree()
    {
        if (NetworkManager != null)
        {
            NetworkManager.JokeryZmienione -= OdswiezJokery;
        }
    }
    public void DodajJokeraDoWidoku(DaneJokera joker)
    {
        JokerSlot slot = (JokerSlot)szablonSlotu.Instantiate();
        slotyContainer.AddChild(slot);
        slot.Inicjalizuj(joker);
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
            slotyContainer.AddChild(slot);
        }
    }
}
