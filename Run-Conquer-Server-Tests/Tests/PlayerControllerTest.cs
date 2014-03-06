using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Formatting;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Run_Conquer_Server.Models;

namespace Run_Conquer_Server_Tests.Tests
{
    [TestClass]
    public class PlayerControllerTest
    {
        private const string ServerUrl = "http://192.168.178.25";
        private const string ServerPort = "3010";

        private JsonMediaTypeFormatter _formatter;

        [TestInitialize]
        public void Initialize()
        {
            _formatter = new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.None
                }
            };
        }

        private static string GetCallUrl(string apiCall)
        {
            Debug.WriteLine(ServerPort + ":" + ServerPort + "/" + apiCall);
            return ServerUrl + ":" + ServerPort + "/api/" + apiCall;
        }

        [TestMethod]
        //[AspNetDevelopmentServer("RuncConquer", @"D:\Source\Repos\Run-Conquer-Server\Run-Conquer-Server")]
        public void TestGetPlayers()
        {
            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            byte[] rawResult = client.DownloadData(GetCallUrl("Player/GetPlayers"));
            string result = Encoding.Unicode.GetString(rawResult);
        }

        [TestMethod]
        public void TestPutPlayer()
        {
            var player = new Player();
            var map = new Map();
            var game = new GameInstance();
            game.Map = map;
            player.Id = 2;
            //game.Players.Add(player);

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
            string result = Encoding.ASCII.GetString(rawResult);
            var players = SerializationHelper.Deserialize<List<Player>>(_formatter, result);
        }

        [TestMethod]
        public void TestGameModelInstance()
        {
            var playerA = new Player {Position = new PositionType {x = 1.0, y = 2.0}};
            var playerB = new Player {Position = new PositionType {x = 4.0, y = 4.0}};
            var teamA = new Team {Color = "red", Name = "Red Team"};
            var teamB = new Team {Color = "blue", Name = "Blue Team"};
            teamA.Players.Add(playerA);
            teamB.Players.Add(playerB);

            var map = new Map();
            var game = new GameInstance {Map = map};
            game.Teams.Add(teamA);
            game.Teams.Add(teamB);

            string serGame = SerializationHelper.Serialize(_formatter, game);
            Debug.WriteLine(serGame);

            var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

            string jsontest =
                "{Map:{},Teams:[" +
                "{Color:'red',Name:'Red Team',Players:[{Position:{x:1.0,y:2.0}}]}," +
                "{Color:'blue',Name:'Blue Team',Players:[{Position:{x:4.0,y:4.0}}]}" +
                "]}";

            byte[] data = Encoding.ASCII.GetBytes(jsontest);
            client.UploadData(GetCallUrl("GameInstance/PostGameInstance"), data);
        }
    }
}