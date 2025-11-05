using Godot;
using System;

public class CardEffectHandler
{
    private TurnManager turnManager;
    private WyborKoloru wyborKoloru;
    private BotGracz botAI;
    private Action zakonczTureCallback;
    private Action<string> ustawWymuszonyKolor;

    public CardEffectHandler(TurnManager turnManager, WyborKoloru wyborKoloru, BotGracz botAI, Action zakonczTureCallback, Action<string> ustawWymuszonyKolor)
    {
        this.turnManager = turnManager;
        this.wyborKoloru = wyborKoloru;
        this.botAI = botAI;
        this.zakonczTureCallback = zakonczTureCallback;
        this.ustawWymuszonyKolor = ustawWymuszonyKolor;
    }

    public void ZastosujEfektKarty(Karta zagranaKarta, bool jestGraczemLudzkim, System.Collections.Generic.List<Karta> rekaBota)
    {
        switch (zagranaKarta.Wartosc)
        {
            case "Stop":
                turnManager.PominTure();
                zakonczTureCallback?.Invoke();
                break;

            case "ZmianaKierunku":
                turnManager.ZmienKierunek();
                zakonczTureCallback?.Invoke();
                break;

            case "+2":
                turnManager.DlugDobierania += 2;
                zakonczTureCallback?.Invoke();
                break;

            case "+4":
                turnManager.DlugDobierania += 4;
                if (jestGraczemLudzkim)
                {
                    wyborKoloru.Show();
                }
                else
                {
                    string wybranyKolor = botAI.WybierzKolor(rekaBota);
                    ustawWymuszonyKolor?.Invoke(wybranyKolor);
                    GD.Print($"Bot wybrał kolor: {wybranyKolor}");
                    zakonczTureCallback?.Invoke();
                }
                break;

            case "ZmianaKoloru":
                if (jestGraczemLudzkim)
                {
                    wyborKoloru.Show();
                }
                else
                {
                    string wybranyKolor = botAI.WybierzKolor(rekaBota);
                    ustawWymuszonyKolor?.Invoke(wybranyKolor);
                    GD.Print($"Bot wybrał kolor: {wybranyKolor}");
                    zakonczTureCallback?.Invoke();
                }
                break;

            default:
                zakonczTureCallback?.Invoke();
                break;
        }
    }
}
