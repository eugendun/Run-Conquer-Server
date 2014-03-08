using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Run_Conquer_Server.Controllers;
using Run_Conquer_Server.Models;
using UrlHelper = System.Web.Http.Routing.UrlHelper;

namespace Run_Conquer_Server_Tests.Tests
{
    [TestClass]
    public class GameInstanceControllerTest
    {
        private readonly GameInstanceController _controller;
        private readonly JsonMediaTypeFormatter _formatter;

        #region constructors

        public GameInstanceControllerTest()
        {
            HttpRequestMessage request = GetRequestMessage();
            var urlHelper = new Mock<UrlHelper>(request);
            _controller = new GameInstanceController { Request = request, Url = urlHelper.Object };

            _formatter = new JsonMediaTypeFormatter {
                SerializerSettings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                }
            };
        }

        #endregion

        #region helper

        private HttpRequestMessage GetRequestMessage()
        {
            HttpConfiguration config = GetConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:3010");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(new HttpRoute());
            return request;
        }

        private HttpConfiguration GetConfiguration()
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                "DefaultApi",
                "api/GameInstance/PostGameInstance/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                );
            return config;
        }

        private GameInstance GetNewGameInstance(bool withPlayers = false)
        {
            var map = new Map();
            var teamA = new Team { Color = "red", Name = "Red Team" };
            var teamB = new Team { Color = "blue", Name = "Blue Team" };
            var game = new GameInstance { Map = map, Teams = new Collection<Team> { teamA, teamB } };

            if(withPlayers) {
                var playerA = new Player { Id = DateTime.Now.GetHashCode() };
                var playerB = new Player { Id = (DateTime.Now.Add(TimeSpan.FromSeconds(1))).GetHashCode() };
                (game.Teams.Where(team => team.Color == "red")).Single().Players.Add(playerA);
                (game.Teams.Where(team => team.Color == "blue")).Single().Players.Add(playerB);
            }

            return game;
        }

        #endregion

        #region controller tests with pure object

        [TestMethod]
        public void CreateGameInstance()
        {
            GameInstance game = GetNewGameInstance();

            Assert.AreEqual(0, game.Id);
            Assert.IsNotNull(game.Map);
            Assert.AreNotEqual(game, game.Map.GameInstance);
            Assert.AreEqual(2, game.Teams.Count);

            HttpResponseMessage result = _controller.PostGameInstance(game);

            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.IsNotNull(result);

            var content = ((ObjectContent)result.Content).Value as GameInstance;
            Assert.IsNotNull(content);

            Assert.AreNotEqual(0, game.Id);
            Assert.AreEqual(game, game.Map.GameInstance);
            Assert.AreEqual(2, game.Teams.Count);

            using(var db = new GameModelContainer()) {
                Assert.IsTrue(db.GameInstanceSet.ToList().Exists(g => g.Id == game.Id));
                Assert.IsTrue(db.MapSet.ToList().Exists(m => m.Id == game.Map.Id));
                foreach(Team team in game.Teams) {
                    Assert.IsTrue(db.TeamSet.ToList().Exists(t => t.Id == team.Id));
                }
            }
        }

        [TestMethod]
        public void DeleteGameInstance()
        {
            using(var db = new GameModelContainer()) {
                GameInstance game = GetNewGameInstance(true);
                db.GameInstanceSet.Add(game);
                db.SaveChanges();

                CollectionAssert.Contains(db.GameInstanceSet.ToList(), game);
                CollectionAssert.Contains(db.MapSet.ToList(), game.Map);
                foreach(Team team in game.Teams) {
                    CollectionAssert.Contains(db.TeamSet.ToList(), team);
                }

                _controller.DeleteGameInstance(game.Id);
                CollectionAssert.DoesNotContain(db.GameInstanceSet.ToList(), game);
                foreach(Team team in game.Teams) {
                    CollectionAssert.DoesNotContain(db.TeamSet.ToList(), team);
                }
            }
        }

        [TestMethod]
        public void ChangeGameInstance()
        {
            using(var db = new GameModelContainer()) {
                GameInstance game = GetNewGameInstance(true);
                db.GameInstanceSet.Add(game);
                db.SaveChanges();

                GameInstance getGame = _controller.GetGameInstance(game.Id);
                Player playerRed = (from t in getGame.Teams
                                    from p in t.Players
                                    where t.Color == "red"
                                    select p).First();

                Assert.AreEqual(0, playerRed.Position.x);
                Assert.AreEqual(0, playerRed.Position.y);

                playerRed.Position.x = 33;
                playerRed.Position.y = 33;

                Player playerBlue = (from t in getGame.Teams
                                     from p in t.Players
                                     where t.Color == "blue"
                                     select p).First();

                Assert.IsNotNull(playerBlue);

                Team teamBlue = (from t in getGame.Teams
                                 where t.Color == "blue"
                                 select t).First();

                teamBlue.Players.Remove(playerBlue);

                _controller.PutGameInstance(getGame.Id, getGame);

                getGame = _controller.GetGameInstance(game.Id);
                playerRed = (from t in getGame.Teams
                             from p in t.Players
                             where t.Color == "red"
                             select p).First();
                Assert.AreEqual(33, playerRed.Position.x);
                Assert.AreEqual(33, playerRed.Position.y);

                teamBlue = (from t in getGame.Teams
                            where t.Color == "blue"
                            select t).First();

                Assert.AreEqual(0, teamBlue.Players.Count);
            }
        }

        #endregion

        #region controller tests with json objects

        [TestMethod]
        public void DefaultGameInstanceJsonDeserialization()
        {
            string gameJson =
                "{Map:{},Teams:[" +
                "{Color:'red',Name:'Red Team'}," +
                "{Color:'blue',Name:'Blue Team'}" +
                "]}";

            var game = SerializationHelper.Deserialize<GameInstance>(_formatter, gameJson);
            Assert.IsNotNull(game);
            Assert.AreEqual(0, game.Id);
            Assert.IsNotNull(game.Map);
            Assert.AreEqual(2, game.Teams.Count);

            Team teamRed = game.Teams.SingleOrDefault(team => team.Color == "red");
            Assert.IsNotNull(teamRed);
            Assert.AreEqual("Red Team", teamRed.Name);
            Assert.AreEqual(0, teamRed.Id);

            Team teamBlue = game.Teams.SingleOrDefault(team => team.Color == "blue");
            Assert.IsNotNull(teamBlue);
            Assert.AreEqual("Blue Team", teamBlue.Name);
            Assert.AreEqual(0, teamBlue.Id);

            IEnumerable<Player> players = game.Teams.SelectMany(team => team.Players);
            Assert.AreEqual(0, players.Count());
        }

        [TestMethod]
        public void ChangeGameInstanceJsonDeserialization()
        {
            GameInstance game = GetNewGameInstance(true);
            using(var db = new GameModelContainer()) {
                db.GameInstanceSet.Add(game);
                db.SaveChanges();
            }

            var teamRed = (from team in game.Teams
                           where team.Color == "red"
                           select team).FirstOrDefault();
            var teamBlue = (from team in game.Teams
                            where team.Color == "blue"
                            select team).FirstOrDefault();
            Assert.IsNotNull(teamRed);
            Assert.IsNotNull(teamBlue);

            var playerBlue = teamBlue.Players.FirstOrDefault();
            Assert.IsNotNull(playerBlue);

            #region json game instance (modified)

            var json = "{" +
                       "'$id':'1'," +
                       "'Map':" +
                            "{" +
                            "'$id':'2'," +
                            "'GameInstance':{'$ref':'1'}," +
                            "'Id':" + game.Map.Id + "}," +
                            "'Teams':[" +
                                "{" +
                                    "'$id':'3'," +
                                    "'GameInstance':{'$ref':'1'}," +
                                    "'Players':[]," +
                                    "'Id':" + teamRed.Id + "," +
                                    "'Color':'red'," +
                                    "'Name':'Red Team'," +
                                    "'GameInstanceId':" + teamRed.GameInstanceId +
                                "}," +
                                "{" +
                                    "'$id':'4'," +
                                    "'GameInstance':{'$ref':'1'}," +
                                    "'Players':[" +
                                        "{" +
                                            "'$id':'5'," +
                                            "'Team':{'$ref':'4'}," +
                                            "'Id':" + playerBlue.Id + "," +
                                            "'TeamId':" + playerBlue.TeamId + "," +
                                            "'Position':{'$id':'6','x':0.2,'y':0.0}" +
                                        "}" +
                                    "]," +
                                    "'Id':" + teamBlue.Id + "," +
                                    "'Color':'blue'," +
                                    "'Name':'Blue Team'," +
                                    "'GameInstanceId':" + teamBlue.GameInstanceId +
                                "}" +
                            "]," +
                            "'Id':" + game.Id +
                       "}";

            #endregion

            var desGame = SerializationHelper.Deserialize<GameInstance>(_formatter, json);
            Assert.AreEqual(0, desGame.Teams.Where(t => t.Color == "red").SelectMany(t => t.Players).Count());
            _controller.PutGameInstance(game.Id, desGame);

            using(var db = new GameModelContainer()) {
                var dbgame = db.GameInstanceSet.FirstOrDefault(g => g.Id == game.Id);

                Assert.IsNotNull(dbgame);
                Assert.AreEqual(0, dbgame.Teams.Where(t => t.Color == "red").SelectMany(t => t.Players).Count());
                Assert.AreEqual(1, dbgame.Teams.Where(t => t.Color == "blue").SelectMany(t => t.Players).Count());
            }

            #region json add new player

            json = "{" +
           "'$id':'1'," +
           "'Map':" +
                "{" +
                "'$id':'2'," +
                "'GameInstance':{'$ref':'1'}," +
                "'Id':" + game.Map.Id + "}," +
                "'Teams':[" +
                    "{" +
                        "'$id':'3'," +
                        "'GameInstance':{'$ref':'1'}," +
                        "'Players':[" +
                            "{" +
                                //"'$id':'12'," +
                                //"'Team':{'$ref':'3'}," +
                                "'Id':" + "1111111" + "," +
                                //"'TeamId':" + teamRed.Id + "," +
                                "'Position':{'$id':'13','x':0.2,'y':1.0}" + 
                            "}" +
                        "]," +
                        "'Id':" + teamRed.Id + "," +
                        "'Color':'red'," +
                        "'Name':'Red Team'," +
                        "'GameInstanceId':" + teamRed.GameInstanceId +
                    "}," +
                    "{" +
                        "'$id':'4'," +
                        "'GameInstance':{'$ref':'1'}," +
                        "'Players':[" +
                            "{" +
                                "'$id':'5'," +
                                "'Team':{'$ref':'4'}," +
                                "'Id':" + playerBlue.Id + "," +
                                "'TeamId':" + playerBlue.TeamId + "," +
                                "'Position':{'$id':'6','x':0.2,'y':0.0}" +
                            "}" +
                        "]," +
                        "'Id':" + teamBlue.Id + "," +
                        "'Color':'blue'," +
                        "'Name':'Blue Team'," +
                        "'GameInstanceId':" + teamBlue.GameInstanceId +
                    "}" +
                "]," +
                "'Id':" + game.Id +
           "}";

            #endregion

            desGame = SerializationHelper.Deserialize<GameInstance>(_formatter, json);
            Assert.AreEqual(1, desGame.Teams.Where(t => t.Color == "red").SelectMany(t => t.Players).Count());
            _controller.PutGameInstance(game.Id, desGame);

            using(var db = new GameModelContainer()) {
                var dbgame = db.GameInstanceSet.FirstOrDefault(g => g.Id == game.Id);

                Assert.IsNotNull(dbgame);
                Assert.AreEqual(1, dbgame.Teams.Where(t => t.Color == "red").SelectMany(t => t.Players).Count());
                Assert.AreEqual(1, dbgame.Teams.Where(t => t.Color == "blue").SelectMany(t => t.Players).Count());
            }

        }

        #endregion
    }
}