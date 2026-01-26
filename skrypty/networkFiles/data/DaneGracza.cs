using System;
using System.Collections.Generic;

public class DaneGracza
{
    public long Id {get; set;}
    public string Nazwa {get; set;}
    public bool CzyGotowy {get; set;}
    public List<DaneKarty> RekaGracza {get; set;} = new List<DaneKarty>();
    public int Wynik {get; set;} = 0;
    public List<Joker> PosiadaneJokery {get; set;} = new List<Joker>();
    public bool CzyUkonczyl {get; set;} = false;
    public int Miejsce {get; set;}

    public DaneGracza(long id, string nazwa, bool gotowosc)
    {
        this.Id = id;
        this.Nazwa = nazwa;
        CzyGotowy = gotowosc;
        CzyUkonczyl = false;

    }
    public void DodajJokera(Joker joker)
    {
        PosiadaneJokery.Add(joker);
    }
    public void PrzypiszPunktyZaMiejsce(Dictionary<long, DaneGracza> listaGraczy)
    {
        this.Wynik += (listaGraczy.Count - Miejsce) * 10;
    }
    public void DodajPunkty(int iloscPkt)
    {
        this.Wynik += iloscPkt;
    }
}