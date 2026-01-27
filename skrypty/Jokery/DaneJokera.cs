using Godot;
using System;
public enum RzadkoscJokera
{
    Zwykly,
    Rzadki,
    Legendarny,
}
[GlobalClass]
public partial class DaneJokera : Resource
{
    [Export] public string Id { get; set; }
    [Export] public string Nazwa { get; set; } = "Nowy Joker";
    [Export(PropertyHint.MultilineText)] public string Opis { get; set; } = "";
    [Export] public Texture2D Ikona { get; set; }
    [Export] public RzadkoscJokera RzadkoscJokera { get; set; }
    [Export] public Godot.Collections.Array<EfektJokera> Efekty { get; set; } = new Godot.Collections.Array<EfektJokera>();
}
