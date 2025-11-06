using Godot;
using System;
using System.Collections.Generic;

public partial class Karta : Area2D
{
	[Export]
	public string Kolor { get; set; }
	[Export]
	public string Wartosc { get; set; }
	public event Action<Karta> OnKartaKliknieta;
	private Sprite2D _sprite;
	private float oryginalnaPozycjaY;
	private Tween _aktywnaAnimacja;
	private bool jestZagrywana = false;
	private CollisionShape2D ksztaltKolizji;


	private Dictionary<string, string> mapowanieKolorow = new Dictionary<string, string>(){
		{"Czerwony", "red"},
		{ "Niebieski", "blue" },
		{ "Zielony", "green" },
		{ "Zolty", "yellow" },
		{ "DzikaKarta", "wild" }
	};
	private Dictionary<string, string> mapowanieWartosci = new Dictionary<string, string>(){
		{ "0", "0" }, { "1", "1" }, { "2", "2" }, { "3", "3" }, { "4", "4" },
		{ "5", "5" }, { "6", "6" }, { "7", "7" }, { "8", "8" }, { "9", "9" },
		{ "Stop", "10" },
		{ "+2", "11" },
		{ "ZmianaKierunku", "12" },
		{ "ZmianaKoloru", "0" },
		{ "+4", "1" }
	};

	public void UstawOryginalnaPozycje(float pozycjaY)
	{
		oryginalnaPozycjaY = pozycjaY;
	}
	public void ZagrajNaStol(Vector2 pozycjaCelu, int nowyZIndex)
	{
		ZatrzymajAktywnaAnimacje();
		jestZagrywana = true;
		InputPickable = false;
		ZIndex = nowyZIndex;
		_aktywnaAnimacja = CreateTween();
		_aktywnaAnimacja.TweenProperty(this, "position", pozycjaCelu, 0.3);
	}
	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		ksztaltKolizji = GetNode<CollisionShape2D>("CollisionShape2D");
		string sciezkaDoTekstury;
		if (Kolor == "Rewers")
		{
			sciezkaDoTekstury = "res://karty_grafika/back/rewers.png";
		}
		else
		{
			string nazwaFolderu = mapowanieKolorow[Kolor];
			string nazwaWartosci = mapowanieWartosci[Wartosc];

			sciezkaDoTekstury = $"res://karty_grafika/{nazwaFolderu}/{nazwaWartosci}_{nazwaFolderu}.png";
		}
		_sprite.Texture = (Texture2D)ResourceLoader.Load(sciezkaDoTekstury);
	}
	private void _OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && !jestZagrywana)
		{
			ZatrzymajAktywnaAnimacje();
			OnKartaKliknieta?.Invoke(this);
		}
	}
	private void _OnMouseEntered()
	{
		if (jestZagrywana) return;
		ZatrzymajAktywnaAnimacje();
		_aktywnaAnimacja = CreateTween();
		_aktywnaAnimacja.TweenProperty(this, "position:y", oryginalnaPozycjaY - 40f, 0.1);
	}
	private void _OnMouseExited()
	{
		if (jestZagrywana) return;
		ZatrzymajAktywnaAnimacje();
		_aktywnaAnimacja = CreateTween();
		_aktywnaAnimacja.TweenProperty(this, "position:y", oryginalnaPozycjaY, 0.1);
	}
	private void ZatrzymajAktywnaAnimacje()
	{
		if (_aktywnaAnimacja != null && _aktywnaAnimacja.IsRunning())
		{
			_aktywnaAnimacja.Kill();
		}
		_aktywnaAnimacja = null;
	}
	public void AnulujZagranie()
	{
		_OnMouseExited();
	}
	public void UstawSkale(float skala)
    {
		ksztaltKolizji.Scale = new Vector2(skala, 1);
    }

}
