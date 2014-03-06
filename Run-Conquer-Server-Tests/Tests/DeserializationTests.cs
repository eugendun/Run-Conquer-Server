using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Run_Conquer_Server.Models;

namespace Run_Conquer_Server_Tests.Tests
{
    [TestClass]
    public class DeserializationTests
    {
        //private static readonly Uri WebApiUri = new Uri("http://192.168.178.25:3010");
        private static readonly Uri WebApiUri = new Uri("http://localhost:3010");
        private static readonly GameModelContainer Db = new GameModelContainer();

        [TestMethod]
        public void AddNewPlayer()
        {
            int uniqueDeviceId = "aksdm129ß0oasdk".GetHashCode();
            var player = new Player { Id = 304 };

            var formatter = new JsonMediaTypeFormatter {
                SerializerSettings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                }
            };

            var appiCall = new Uri(WebApiUri, "api/Player/PostPlayer");

            var client = new HttpClient(new HttpClientHandler());
            var content = new ObjectContent<Player>(player, formatter);
            Task<HttpResponseMessage> response = client.PostAsync(appiCall, content);
            Assert.AreEqual(HttpStatusCode.Created, response.Result.StatusCode, "Player creation request failed!");

            Player dbPlayer = Db.PlayerSet.Find(player.Id);
            Assert.IsNotNull(dbPlayer, "No player was found in the database that should be created by the request!");
        }

        [TestMethod]
        public void AddNewPlayerInJson()
        {
            const string playerJson = "{\"Position\":{\"x\":1,\"y\":2}}";
            var addPlayerUri = new Uri(WebApiUri, "api/Player/PostPlayer");
            var client = new HttpClient(new HttpClientHandler());
            var content = new StringContent(playerJson, Encoding.UTF8, "application/json");
            var response = client.PostAsync(addPlayerUri, content);

            Assert.AreEqual(HttpStatusCode.Created, response.Result.StatusCode);
        }

        [TestMethod]
        public void DeletePlayer()
        {
            Player player = Db.PlayerSet.Add(new Player());
            Db.SaveChanges();

            var deleteUri = new Uri(WebApiUri, string.Format("api/Player/DeletePlayer/{0}", player.Id));
            var client = new HttpClient(new HttpClientHandler());
            Task<HttpResponseMessage> response = client.GetAsync(deleteUri);
            Assert.AreEqual(HttpStatusCode.OK, response.Result.StatusCode, "Player delete request failed!");

            var dbPlayer = Db.PlayerSet.Find(player.Id);
            Assert.IsNull(dbPlayer, string.Format("Player (Id:{0}) found that has to be removed from the database!", dbPlayer.Id));
        }
    }
}