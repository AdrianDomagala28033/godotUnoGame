using Godot;
using System;

public partial class TloManager : MeshInstance3D
{
    public static TloManager Instancja { get; private set; }
    private ShaderMaterial material;
    public override void _Ready()
    {
        Instancja = this;
        var mat = GetActiveMaterial(0) as ShaderMaterial;
        if(mat != null)
            material = mat;
    }
    public void ZmienRozmiarOka(float promienWewnetrzny, float promienZewnetrzny, float czasTrwania)
    {
        if(material == null) return;
        Tween tween = CreateTween();
        tween.TweenProperty(material, "shader_parameter/promien_wewnetrzny", promienWewnetrzny, czasTrwania);
        tween.Parallel().TweenProperty(material, "shader_parameter/promien_zewnetrzny", promienZewnetrzny, czasTrwania);

        tween.TweenProperty(material, "shader_parameter/promien_wewnetrzny", 0.17, czasTrwania);
        tween.Parallel().TweenProperty(material, "shader_parameter/promien_zewnetrzny", 0.3, czasTrwania);
    }
    public void ZmienKolory(Color kolor1, Color kolor2, float czasTrwania)
    {
        if(material == null) return;
        Tween tween = CreateTween();
        tween.TweenProperty(material, "shader_parameter/color1", kolor1, czasTrwania);
        tween.Parallel().TweenProperty(material, "shader_parameter/color2", kolor2, czasTrwania);
    }
    public void ZmienKolorOka(Color kolor, float czasTrwania)
    {
        if(material == null) return;
        Tween tween = CreateTween();
        tween.TweenProperty(material, "shader_parameter/kolor_oka", kolor, czasTrwania);
    }
    public void ZmienKierunek(float nowyKierunek, float czasTrwania)
    {
        if(material == null) return;
        Tween tween = CreateTween();
        tween.TweenProperty(material, "shader_parameter/kierunek", nowyKierunek, czasTrwania);
    }
}
