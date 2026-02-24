using Godot;
using System;
using System.Collections.Generic;

public class TurnManager
{
    public List<long> ListaGraczyId { get; private set; }
    private Dictionary<long, DaneGracza> bazaDanychGraczy;
    public int AktualnyGraczIndex { get; private set; }
    public int KierunekGry { get; set; } = 1;
    public int IloscGraczy => ListaGraczyId.Count;
    public int DlugDobierania { get; set; }
    public int DodatkoweRuchy { get; set; } = 0;
    public long AktualnyGraczId => ListaGraczyId[AktualnyGraczIndex];

    public event Action<int> OnTuraRozpoczeta;
    public event Action<int> OnTuraZakonczona;
    public event Action<int> OnKierunekZmieniony;

    public TurnManager(Dictionary<long, DaneGracza> bazaDanych)
    {
        bazaDanychGraczy = bazaDanych;
        ListaGraczyId = new List<long>(bazaDanych.Keys);
        AktualnyGraczIndex = 0;
    }
    private bool CzyUkonczyl(long id)
    {
        return bazaDanychGraczy[id].RekaGracza.Count == 0;
    }
    public void UstawWybranegoGracza(long id)
    {
        int index = ListaGraczyId.IndexOf(id);
        if(index != - 1)
            AktualnyGraczIndex = index;
    }

    public void ZmienKierunek()
    {
        KierunekGry *= -1;
        OnKierunekZmieniony?.Invoke(KierunekGry);
    }

    public void PominTure()
    {
        AktualnyGraczIndex += KierunekGry;
        UporzadkujIndex();
    }

    public void ZakonczTure()
    {
        OnTuraZakonczona?.Invoke(AktualnyGraczIndex);
        int bezpiecznik = 0;
        do
        {
            AktualnyGraczIndex += KierunekGry;
            UporzadkujIndex();   
        } while (CzyUkonczyl(AktualnyGraczId) && bezpiecznik <= IloscGraczy);
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
