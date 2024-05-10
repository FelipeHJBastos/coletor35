using org.cesar.dmplight.watchComm.api;
using org.cesar.dmplight.watchComm.impl;
using System;
using System.Data;
using System.Runtime.InteropServices;



internal class Program
{
    private static void Main(string[] args)
    {
        var ip = "192.168.11.230";
        var key = "";
        var versaoFirmware = "01.00.0000";
        var chaveRSA = "D9E8BBE449F94F85D225DD404D0581182F52C297E2390F43A9913B29CD51B01DF9D1889ADBEA57528DD15A7BAF7FBAC893499CA9BF4C09A5D0DB9D409818C1CA7ED667D569EF4A44AF1DE5D5DB62F72B2F02FE64A8AEAB2B04005D55121BDDA96A1127142EFC15F173023DB9272F4E74B9E3B70DD45E067646048F91AD75C303";
        var expoenteRSA = "010001";
        var user = "login";
        var password = "senha";

        TCPComm tcpComm = new TCPComm(ip, 3000);
        tcpComm.SetTimeOut(10000000);

        var watchComm = new WatchComm(
            WatchProtocolType.REPC, //modelo do relogio
            tcpComm, //ip no formato necessario
            1, //sla
            key, //sla
            WatchConnectionType.ConnectedMode,
            versaoFirmware,
            chaveRSA,
            expoenteRSA,
            user,
            password
        );

        Console.WriteLine("----------------------------TESTE----------------------------");

        watchComm.OpenConnection();

        var status = watchComm.GetPrintPointStatus();

        Console.WriteLine(status);

        watchComm.CloseConnection();
    }
}