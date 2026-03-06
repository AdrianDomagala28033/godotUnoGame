using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class Signaling : Node
{
    private WebSocketPeer socket = new WebSocketPeer();
    private string Intencja = "";
    private bool CzyPolaczono = false;
    private string kodDolaczenia = "";
    private string adresSerwera = "ws://127.0.0.1:8080";
    private Dictionary<int, WebRtcPeerConnection> polaczenia = new Dictionary<int, WebRtcPeerConnection>();
    public WebRtcMultiplayerPeer rtcPeer = new WebRtcMultiplayerPeer();
    public int MojeId = 1;

    public override void _Ready()
    {

    }
    public override void _Process(double delta)
    {
        socket.Poll();
        if (socket.GetReadyState() == WebSocketPeer.State.Open)
        {
            if(CzyPolaczono == false)
            {
                CzyPolaczono = true;
                string powitanie = $"{{\"akcja\": \"{Intencja}\"}}";
                if(Intencja == "host")
                {
                    socket.SendText(powitanie);
                }
                else if(Intencja == "join")
                {
                    powitanie = $"{{\"akcja\": \"{Intencja}\", \"kod\": \"{kodDolaczenia}\"}}";
                    socket.SendText(powitanie);
                }
            }
            while(socket.GetAvailablePacketCount() > 0)
            {
                byte[] paczka = socket.GetPacket();
                string wiadomosc = Encoding.UTF8.GetString(paczka);
                GD.Print(wiadomosc);
                var dane = Json.ParseString(wiadomosc).AsGodotDictionary();
                if((string)dane["akcja"] == "twoj_kod")
                {
                    string wyciagnietyKod = (string)dane["kod"];
                    GD.Print("Serwer nadał mi kod: " + wyciagnietyKod);
                    rtcPeer.CreateServer();
                    Multiplayer.MultiplayerPeer = rtcPeer;
                    var networkManager = GetNode<NetworkManager>("/root/NetworkManager");
                    networkManager.Rpc(nameof(NetworkManager.ZarejestrujNowegoGracza), 1, networkManager.NazwaGraczaLokalnego, false);
                    GameServer gameServer = new GameServer();
                    gameServer.NumerRundy = 1;
                    gameServer.Name = "GameServer";
                    networkManager.AktualnyKodPokoju = wyciagnietyKod;
                    networkManager.AddChild(gameServer);
                    networkManager.ZglosPolaczenie();
                }
                else if((string)dane["akcja"] == "polaczono")
                {
                    GD.Print("Udało się dołączyć!");
                    MojeId = (int)(GD.Randi() % 10000) + 2;
                    rtcPeer.CreateClient(MojeId);
                    Multiplayer.MultiplayerPeer = rtcPeer;
                    socket.SendText($"{{\"akcja\": \"gotowy\", \"od_kogo\": {MojeId}, \"do_kogo\": 1}}");
                }
                else if((string)dane["akcja"] == "blad")
                {
                    socket.Close();
                    CzyPolaczono = false;
                    GD.Print("Błąd: Zły kod!");
                }
                else if((string)dane["akcja"] == "oferta")
                {
                    if (!dane.ContainsKey("do_kogo") || (int)(double)dane["do_kogo"] != MojeId) continue;
                    int id = (int)(double)dane["od_kogo"];
                    string sdp = (string)dane["sdp"];
                    string type = (string)dane["typ"];
                    if (!polaczenia.ContainsKey(id))
                    {
                        UtworzPolaczenie(id);
                    }
                    polaczenia[id].SetRemoteDescription(type, sdp);
                }
                else if((string)dane["akcja"] == "ice")
                {
                    if (!dane.ContainsKey("do_kogo") || (int)(double)dane["do_kogo"] != MojeId) continue;
                    int id = (int)(double)dane["od_kogo"];
                    string media = (string)dane["media"];
                    int index = (int)(double)dane["index"];
                    string nazwa = (string)dane["name"];
                    polaczenia[id].AddIceCandidate(media, index, nazwa);
                }
                else if ((string)dane["akcja"] == "gotowy")
                {
                    if (Multiplayer.IsServer()) 
                    {
                        int idNowego = (int)(double)dane["od_kogo"];
                        UtworzPolaczenie(idNowego);
                        polaczenia[idNowego].CreateOffer();
                    }
                }
            }
        }
    }
    public void UtworzPokoj()
    {
        socket.Close();
        Intencja = "host";
        socket.ConnectToUrl(adresSerwera);
    }
    public void DolaczDoPokoju(string kod)
    {
        socket.Close();
        Intencja = "join";
        kodDolaczenia = kod;
        socket.ConnectToUrl(adresSerwera);
    }
    private void UtworzPolaczenie(int idGracza)
    {
        WebRtcPeerConnection nowyKabel = new WebRtcPeerConnection();
        nowyKabel.Initialize(new Godot.Collections.Dictionary());
        nowyKabel.SessionDescriptionCreated += (type, sdp) =>
        {
           OnOfertaStworzona(type, sdp, idGracza); 
        };
        nowyKabel.IceCandidateCreated += (media, index, name) =>
        {
            OnIceStworzony(media, index, name, idGracza);
        };
        polaczenia[idGracza] = nowyKabel;
        rtcPeer.AddPeer(nowyKabel, idGracza);
    }

    private void OnIceStworzony(string media, long index, string name, int idGracza)
    {
        var slownik = new Godot.Collections.Dictionary();
        slownik["akcja"] = "ice";
        slownik["media"] = media;
        slownik["index"] = index;
        slownik["name"] = name;
        slownik["od_kogo"] = MojeId;
        slownik["do_kogo"] = idGracza;
        socket.SendText(Json.Stringify(slownik));
    }

    private void OnOfertaStworzona(string type, string sdp, int idGracza)
    {
        var kabel = polaczenia[idGracza];
        kabel.SetLocalDescription(type, sdp);
        var slownik = new Godot.Collections.Dictionary();
        slownik["akcja"] = "oferta";
        slownik["typ"] = type;
        slownik["sdp"] = sdp;
        slownik["od_kogo"] = MojeId;
        slownik["do_kogo"] = idGracza;
        socket.SendText(Json.Stringify(slownik));
    }
}