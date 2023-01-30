using System.Net;
using WowPacketParser.Enums;
using WowPacketParser.Live;
using WowPacketParser.Loading;
using WowPacketParser.Parsing;

async Task ReadLoop(IPacketReader reader, WebSocketServer server)
{
    var count = 0;
    for (;;)
    {
        if (reader.GetCurrentSize() == reader.GetTotalSize())
        {
            await Task.Delay(1000);
        }
        else
        {
            var packet = reader.Read(count++, "");
            Handler.Parse(packet);
            server.Send(packet);
        }
    }
}

var host = "127.0.0.1";
var port = 8888;
var file = args.Length > 0 ? args[1] : "/azerothcore/logs/World.pkt";
for (int i = 1; i < args.Length; i++)
{
    string opt = args[i];
    if (!opt.StartsWith("--", StringComparison.CurrentCultureIgnoreCase)) break;
    var _ = opt.Substring(2) switch
    {
        "host" => (host = args[i + 1]) == "",
        "port" => (port = int.Parse(args[i + 1])) == 0,
        _ => throw new ArgumentException($"Unknown option: {opt}")
    };
}

if (!File.Exists(file))
{
    throw new ArgumentException($"Could not find file {file}");
}

var server = new WebSocketServer(IPAddress.Parse(host), port);
Task.Run(server.Start);
// Console.WriteLine("Press any key to start reading the PKT file");
// Console.ReadKey();
var fs = new FileStream(file, FileMode.Open, FileAccess.Read);
var reader = new BinaryReader(fs);
Task.Run(() => ReadLoop(new PacketReader(SniffType.Pkt, reader), server));
Console.ReadKey();