using Godot;
using System;
using System.Collections.Generic;

public class TurnManager
{
    public int AktualnyGraczIndex { get; private set; }
    public int KierunekGry { get; private set; } = 1;
    public int IloscGraczy { get; private set; }
    public int DlugDobierania { get; set; }
    public int DodatkoweRuchy { get; set; } = 0;
    public List<Gracz> ListaGraczy { get; private set; }
    

    public event Action<int> OnTuraRozpoczeta;
    public event Action<int> OnTuraZakonczona;

    public TurnManager(List<Gracz> listaGraczy)
    {
        ListaGraczy = listaGraczy;
        IloscGraczy = listaGraczy.Count;
        AktualnyGraczIndex = 0;
    }

    public Gracz AktualnyGracz => ListaGraczy[AktualnyGraczIndex];

    public void UstawWybranegoGracza(int index)
    {
        if (index >= 0 && index <= ListaGraczy.Count)
            AktualnyGraczIndex = index;
    }

    public void ZmienKierunek()
    {
        KierunekGry *= -1;
    }

    public void PominTure()
    {
        AktualnyGraczIndex += KierunekGry;
        UporzadkujIndex();
    }

    public void ZakonczTure()
    {
        OnTuraZakonczona?.Invoke(AktualnyGraczIndex);
        AktualnyGraczIndex += KierunekGry;
        UporzadkujIndex();
        OnTuraRozpoczeta?.Invoke(AktualnyGraczIndex);
    }
    public void RozpocznijTure()
    {
        OnTuraRozpoczeta?.Invoke(AktualnyGraczIndex);
    }
    public void UporzadkujIndex()
    {
        if (AktualnyGraczIndex >= IloscGraczy)
            AktualnyGraczIndex = 0;
        else if (AktualnyGraczIndex < 0)
            AktualnyGraczIndex = IloscGraczy - 1;
    }

}
