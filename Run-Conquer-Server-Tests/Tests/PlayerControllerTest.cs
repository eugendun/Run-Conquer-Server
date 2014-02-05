using Microsoft.VisualStudio.TestTools.UnitTesting;
using Run_Conquer_Server.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;

namespace Run_Conquer_Server_Tests
{
    [TestClass]
    public class PlayerControllerTest
    {
        public static string serverUrl = "http://192.168.178.25";
        public static string serverPort = "3010";

        public JsonMediaTypeFormatter formatter;

        [TestInitialize]
        public void Initialize()
        {
            formatter = new JsonMediaTypeFormatter();
            formatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize;
            formatter.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
        }

        public string GetCallUrl(string apiCall)
        {
            Debug.WriteLine(serverPort + ":" + serverPort + "/" + apiCall);
            return serverUrl + ":" + serverPort + "/api/" + apiCall;
        }

        [TestMethod]
        //[AspNetDevelopmentServer("RuncConquer", @"D:\Source\Repos\Run-Conquer-Server\Run-Conquer-Server")]
        public void TestGetPlayers()
        {

            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            byte[] rawResult = client.DownloadData(GetCallUrl("Player/GetPlayers"));
            var result = Encoding.Unicode.GetString(rawResult);
        }

        [TestMethod]
        public void TestPutPlayer()
        {
            var player = new Player();
            var map = new Map();
            var game = new GameInstance();
            game.Map = map;
            player.Id = 2;
            game.Players.Add(player);

            var json = new JsonMediaTypeFormatter();
            string serPlayer = SerializationHelper.Serialize(json, player);
            byte[] data = Encoding.ASCII.GetBytes(serPlayer);

            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            byte[] result = client.UploadData(GetCallUrl("Player/PostPlayer"), data);

        }

        [TestMethod]
        public void TestDeleteAllPlayers()
        {
            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            //client.DownloadData(GetCallUrl("Player/DeleteAllPlayers"));

            byte[] rawResult = client.DownloadData(GetCallUrl("Player/GetPlayers"));
            var result = Encoding.ASCII.GetString(rawResult);
            var players = SerializationHelper.Deserialize<List<Player>>(formatter, result);
        }
    }
}
