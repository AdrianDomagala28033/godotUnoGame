using System.Collections.Generic;
using Godot;

public partial class StosDobierania : Area3D
{
    private Node3D _wizualizacja;
    public void AktualizujWyglad(int iloscKart)
    {
        if (iloscKart <= 0)
        {
            Visible = false;
            ProcessMode = ProcessModeEnum.Disabled;
        }
        else
        {
            Visible = true;
            ProcessMode = ProcessModeEnum.Inherit;
        }
        
        // Tutaj kiedyś dodamy skalowanie wysokości (te bajery, o których mówiłeś)
        // Na razie zostawiamy stały klocek.
    }
}