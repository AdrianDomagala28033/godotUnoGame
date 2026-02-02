using Godot;
using System;
using System.Collections.Generic;

public partial class Karta : Area3D
{
	[Export]
	public string Kolor { get; set; }
	[Export]
	public string Wartosc { get; set; }
	public event Action<Karta, int> OnKartaKliknieta;
	private Sprite3D _sprite;
	private Vector3 oryginalnaPozycja;
	private Tween _aktywnaAnimacja;
	private bool jestZagrywana = false;
	private CollisionShape3D ksztaltKolizji;
	private bool czyZaznaczona;
	public Vector3 DomyslnaRotacja { get; set; }
	private int _bazowyPriorytet = 0;
	public bool CzyPodKursorem { get; private set; } = false;


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

	public void UstawOryginalnaPozycje(Vector3 pozycja)
	{
		oryginalnaPozycja = pozycja;
	}
	public void ZagrajNaStol(Vector3 pozycjaCelu)
	{
		ZatrzymajAktywnaAnimacje();
		jestZagrywana = true;
		InputRayPickable = false;
		_aktywnaAnimacja = CreateTween();
		_aktywnaAnimacja.TweenProperty(this, "global_position", pozycjaCelu, 0.3);
	}
	public override void _Ready()
	{
		_sprite = GetNode<Sprite3D>("Sprite3D");
		_sprite.Centered = true;
		_sprite.PixelSize = 0.01f;
		_sprite.DoubleSided = true;
		this.InputEvent += _OnInputEvent;
		this.MouseEntered += _OnMouseEntered;
		this.MouseExited += _OnMouseExited;
		ksztaltKolizji = GetNode<CollisionShape3D>("CollisionShape3D");
		string sciezkaDoTekstury;
		if (Kolor == "Rewers")
		{
			sciezkaDoTekstury = "res://grafika/karty_grafika/back/rewers.png";
		}
		else
		{
			string nazwaFolderu = mapowanieKolorow[Kolor];
			string nazwaWartosci = mapowanieWartosci[Wartosc];

			sciezkaDoTekstury = $"res://grafika/karty_grafika/{nazwaFolderu}/{nazwaWartosci}_{nazwaFolderu}.png";
		}
		_sprite.Texture = (Texture2D)ResourceLoader.Load(sciezkaDoTekstury);
	}
	private void _OnInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && (mouseEvent.ButtonIndex == MouseButton.Left ||  mouseEvent.ButtonIndex == MouseButton.Right) && !jestZagrywana)
		{
			ZatrzymajAktywnaAnimacje();
			OnKartaKliknieta?.Invoke(this, (int)mouseEvent.ButtonIndex);
		}
	}
	private void _OnMouseEntered()
	{
		CzyPodKursorem = true;
        if (jestZagrywana) return;
        ZatrzymajAktywnaAnimacje();
		if (_sprite != null) 
		{
			_sprite.RenderPriority = 100;
			_sprite.AlphaCut = Sprite3D.AlphaCutMode.OpaquePrepass;
		}
        
        _aktywnaAnimacja = CreateTween();
        _aktywnaAnimacja.SetParallel(true);
        
        Vector3 cel = oryginalnaPozycja + new Vector3(0, 0.8f, 0.2f);
		_aktywnaAnimacja.TweenProperty(
			this,
			"rotation_degrees",
			new Vector3(-55, 0, 0),
			0.1f
		);
        
        _aktywnaAnimacja.TweenProperty(this, "global_position", cel, 0.1f).SetTrans(Tween.TransitionType.Sine);
        _aktywnaAnimacja.TweenProperty(this, "scale", Vector3.One * 1f, 0.1f);
    }
	private void _OnMouseExited()
	{
		CzyPodKursorem = false;
		if (!czyZaznaczona)
		{
			if (jestZagrywana) return;
			ZatrzymajAktywnaAnimacje();
			
			_aktywnaAnimacja = CreateTween();
			_aktywnaAnimacja.SetParallel(true);

			_aktywnaAnimacja.TweenProperty(this, "global_position", oryginalnaPozycja, 0.15f).SetTrans(Tween.TransitionType.Sine);
			_aktywnaAnimacja.TweenProperty(this, "rotation_degrees", DomyslnaRotacja, 0.15f);
			_aktywnaAnimacja.TweenProperty(this, "scale", Vector3.One * 0.8f, 0.15f);
		}
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
		ksztaltKolizji.Scale = new Vector3(skala, 1, 1);
    }
	public void ZrestartujStanKarty()
	{
		InputRayPickable = true;
		jestZagrywana = false;
		ZatrzymajAktywnaAnimacje();
		RotationDegrees = DomyslnaRotacja;
		Scale = Vector3.One * 0.6f;
		Show();
	}

    public void UstawZaznaczenie(bool czyZaznaczona)
	{
		this.czyZaznaczona = czyZaznaczona;
		ZatrzymajAktywnaAnimacje();
		_aktywnaAnimacja = CreateTween();
		
		if (this.czyZaznaczona)
			_aktywnaAnimacja.TweenProperty(this, "global_position", oryginalnaPozycja + new Vector3(0, 0.8f, 0), 0.1);
		else
			_aktywnaAnimacja.TweenProperty(this, "global_position", oryginalnaPozycja, 0.1);
	}
	public void UstawKolejnoscRysowania(int indeks)
    {
        _bazowyPriorytet = indeks;
        if (_sprite != null)
        {
            _sprite.RenderPriority = _bazowyPriorytet;
        }
    }

}
