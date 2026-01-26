using Godot;
using System;
using System.Collections.Generic;

class InitialStatePacket
{
    public int IndexGracza {get; set;}
    public int KierunekGry {get; set;}
    public int IloscGraczy {get; set;}
    public int DlugDobierania {get; set;}
    public DaneKarty gornaKarta {get; set;}
    public List<DaneKarty> rekaGracza {get; set;}
}