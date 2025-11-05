using Godot;
using System;
using System.Collections.Generic;

public class TurnManager
{
    public int AktualnyGraczIndex { get; private set; }
    public int KierunekGry { get; private set; } = 1;
    public int IloscGraczy { get; private set; }
    public int DlugDobierania { get; set; }

    public event Action<int> OnTuraRozpoczeta;
    public event Action<int> OnTuraZakonczona;

    public TurnManager(int iloscGraczy)
    {
        IloscGraczy = iloscGraczy;
        AktualnyGraczIndex = 0;
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

	private void UporzadkujIndex()
	{
		if (AktualnyGraczIndex >= IloscGraczy)
			AktualnyGraczIndex = 0;
		else if (AktualnyGraczIndex < 0)
			AktualnyGraczIndex = IloscGraczy - 1;
	}

}
