using Godot;
using System;

[GlobalClass]
public partial class EfektSzansy : EfektJokera
{
    [Export] public float SzansaProcentowa { get; set; } = 5.0f;
    [Export] public EfektJokera EfektSukcesu { get; set; }
    private static readonly Random rng = new Random();
    public override void Wykonaj(GameServer server)
    {
        ProbujUruchomic(server, server.Multiplayer.GetRemoteSenderId());
    }
    public override void NaPoczatkuTury(GameServer server, long idGracza)
    {
        ProbujUruchomic(server, idGracza);
    }

    private void ProbujUruchomic(GameServer server, long idGracza)
    {
        double los = rng.NextDouble() * 100.0;
        if(los <= SzansaProcentowa)
        {
            GD.Print($"[SZANSA] Sukces! ({los:F2} <= {SzansaProcentowa})");
            EfektSukcesu?.NaPoczatkuTury(server, idGracza);
            EfektSukcesu?.Wykonaj(server);
        }
    }
}