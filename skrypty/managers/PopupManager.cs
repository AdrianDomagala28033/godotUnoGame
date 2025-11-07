using Godot;
using System;

public partial class PopupManager : Node
{
	[Export]
	private PackedScene SzablonPopupWiadomosc;
	public void PokazWiadomosc(string tekst, Vector2 pozycjaSwiata)
    {
		Label popup = (Label)SzablonPopupWiadomosc.Instantiate();
		popup.Text = tekst;

		AddChild(popup);

		if (GetViewport().GetCamera2D() != null)
		{
			popup.GlobalPosition = GetViewport().GetCamera2D().GetScreenCenterPosition() + (pozycjaSwiata - GetViewport().GetCamera2D().GlobalPosition) * GetViewport().GetCamera2D().Zoom;
		}
		else
		{
			popup.GlobalPosition = pozycjaSwiata;
		}
		popup.GetNode<AnimationPlayer>("AnimationPlayer").Play("float_and_fade");
    }
}
