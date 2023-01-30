using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WowPacketParser.Enums.Version;
using WowPacketParser.Misc;

namespace WowPacketParser.Live;

public class WebSocketServer
{
    private const string Guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    private readonly IPAddress _host;
    private readonly int _port;
    private readonly Socket _serverSocket;
    private Socket? _clientSocket;

    public WebSocketServer(IPAddress host, int port)
    {
        _host = host;
        _port = port;
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
    }

    public void Send(Packet packet)
    {
        if (_clientSocket is null || !_clientSocket.Connected) return;
        try
        {
            var message = JsonSerializer.Serialize(new
            {
                opCode = packet.Opcode,
                name = Opcodes.GetOpcodeName(packet.Opcode, packet.Direction, false),
                direction = packet.Direction.ToString(),
                length = packet.Length,
                connectionIndex = packet.ConnectionIndex,
                endpointAddress = packet.EndPoint.Address.ToString(),
                endpointPort = packet.EndPoint.Port,
                number = packet.Number,
                timestamp = Math.Floor(packet.Time.Subtract(DateTime.UnixEpoch).TotalSeconds),
                message = packet.Writer.ToString()
            });
            _clientSocket.Send(GetFrameFromString(message));
        }
        catch (Exception)
        {
            _clientSocket = null;
        }
    }


    public void Start()
    {
        _serverSocket.Bind(new IPEndPoint(_host, _port));
        _serverSocket.Listen(1);
        _serverSocket.BeginAccept(OnAccept, null);
    }

    private void OnAccept(IAsyncResult result)
    {
        byte[] buffer = new byte[1024];
        try
        {
            _clientSocket = null;
            var headerResponse = "";
            if (_serverSocket.IsBound)
            {
                _clientSocket = _serverSocket.EndAccept(result);
                var i = _clientSocket.Receive(buffer);
                headerResponse = (Encoding.UTF8.GetString(buffer)).Substring(0, i);
                // write received data to the console
                // Console.WriteLine(headerResponse);
                // Console.WriteLine("=====================");
            }

            if (_clientSocket == null) return;
            /* Handshaking and managing ClientSocket */
            var key = headerResponse.Replace("ey:", "`")
                .Split('`')[1] // dGhlIHNhbXBsZSBub25jZQ== \r\n .......
                .Replace("\r", "").Split('\n')[0] // dGhlIHNhbXBsZSBub25jZQ==
                .Trim();

            // key should now equal dGhlIHNhbXBsZSBub25jZQ==
            var test1 = AcceptKey(ref key);

            var newLine = "\r\n";

            var response = "HTTP/1.1 101 Switching Protocols" + newLine
                                                              + "Upgrade: websocket" + newLine
                                                              + "Connection: Upgrade" + newLine
                                                              + "Sec-WebSocket-Accept: " + test1 + newLine + newLine
                //+ "Sec-WebSocket-Protocol: chat, superchat" + newLine
                //+ "Sec-WebSocket-Version: 13" + newLine
                ;

            _clientSocket.Send(Encoding.UTF8.GetBytes(response));
            Task.Run(async () =>
            {
                await _clientSocket.ReceiveAsync(new byte[1]);
                Console.WriteLine("Client message. Closing.");
                _serverSocket.BeginAccept(OnAccept, null);
            });
        }
        catch (SocketException exception)
        {
            Console.WriteLine("ServerSocket exception: " + exception.Message);
        }
    }

    public T[] SubArray<T>(T[] data, int index, int length)
    {
        var result = new T[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }

    private static string AcceptKey(ref string key)
    {
        var longKey = key + Guid;
        var hashBytes = ComputeHash(longKey);
        return Convert.ToBase64String(hashBytes);
    }

    private static readonly SHA1 Sha1 =  SHA1.Create();

    private static byte[] ComputeHash(string str)
    {
        return Sha1.ComputeHash(Encoding.ASCII.GetBytes(str));
    }

    //Needed to decode frame
    // public string GetDecodedData(byte[] buffer, int length)
    // {
    //     byte b = buffer[1];
    //     int dataLength = 0;
    //     int totalLength = 0;
    //     int keyIndex = 0;
    //
    //     if (b - 128 <= 125)
    //     {
    //         dataLength = b - 128;
    //         keyIndex = 2;
    //         totalLength = dataLength + 6;
    //     }
    //
    //     if (b - 128 == 126)
    //     {
    //         dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
    //         keyIndex = 4;
    //         totalLength = dataLength + 8;
    //     }
    //
    //     if (b - 128 == 127)
    //     {
    //         dataLength = (int)BitConverter.ToInt64(
    //             new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] },
    //             0);
    //         keyIndex = 10;
    //         totalLength = dataLength + 14;
    //     }
    //
    //     if (totalLength > length)
    //         throw new Exception("The buffer length is small than the data length");
    //
    //     byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };
    //
    //     int dataIndex = keyIndex + 4;
    //     int count = 0;
    //     for (int i = dataIndex; i < totalLength; i++)
    //     {
    //         buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
    //         count++;
    //     }
    //
    //     return Encoding.ASCII.GetString(buffer, dataIndex, dataLength);
    // }

    //function to create  frames to send to client 
    /// <summary>
    /// Enum for opcode types
    /// </summary>
    public enum EOpcodeType
    {
        /* Denotes a continuation code */
        Fragment = 0,

        /* Denotes a text code */
        Text = 1,

        /* Denotes a binary code */
        Binary = 2,

        /* Denotes a closed connection */
        ClosedConnection = 8,

        /* Denotes a ping*/
        Ping = 9,

        /* Denotes a pong */
        Pong = 10
    }

    /// <summary>Gets an encoded websocket frame to send to a client from a string</summary>
    /// <param name="message">The message to encode into the frame</param>
    /// <param name="opcode">The opcode of the frame</param>
    /// <returns>Byte array in form of a websocket frame</returns>
    public static byte[] GetFrameFromString(string message, EOpcodeType opcode = EOpcodeType.Text)
    {
        byte[] response;
        var bytesRaw = Encoding.Default.GetBytes(message);
        var frame = new byte[10];

        long indexStartRawData;
        long length = bytesRaw.Length;

        frame[0] = (byte)(128 + (int)opcode);
        if (length <= 125)
        {
            frame[1] = (byte)length;
            indexStartRawData = 2;
        }
        else if (length <= 65535)
        {
            frame[1] = 126;
            frame[2] = (byte)((length >> 8) & 255);
            frame[3] = (byte)(length & 255);
            indexStartRawData = 4;
        }
        else
        {
            frame[1] = 127;
            frame[2] = (byte)((length >> 56) & 255);
            frame[3] = (byte)((length >> 48) & 255);
            frame[4] = (byte)((length >> 40) & 255);
            frame[5] = (byte)((length >> 32) & 255);
            frame[6] = (byte)((length >> 24) & 255);
            frame[7] = (byte)((length >> 16) & 255);
            frame[8] = (byte)((length >> 8) & 255);
            frame[9] = (byte)(length & 255);

            indexStartRawData = 10;
        }

        response = new byte[indexStartRawData + length];

        long i, reponseIdx = 0;

        //Add the frame bytes to the reponse
        for (i = 0; i < indexStartRawData; i++)
        {
            response[reponseIdx] = frame[i];
            reponseIdx++;
        }

        //Add the data bytes to the response
        for (i = 0; i < length; i++)
        {
            response[reponseIdx] = bytesRaw[i];
            reponseIdx++;
        }

        return response;
    }
}