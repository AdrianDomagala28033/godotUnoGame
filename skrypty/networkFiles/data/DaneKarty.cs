public class DaneKarty
{
    public string Kolor { get; set; }
    public string Wartosc { get; set; }
    public DaneKarty() {}

    public DaneKarty(string kolor, string wartosc)
    {
        Kolor = kolor;
        Wartosc = wartosc;
    }
}