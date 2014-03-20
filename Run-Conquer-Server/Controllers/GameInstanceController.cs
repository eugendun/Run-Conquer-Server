using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Run_Conquer_Server.Models;

namespace Run_Conquer_Server.Controllers
{
    public class GameInstanceController : ApiController
    {
        private readonly GameModelContainer _db = new GameModelContainer();

        // GET api/GameInstance/GetGameInstances
        [HttpGet]
        public IEnumerable<GameInstance> GetGameInstances()
        {
            return _db.GameInstanceSet;
        }

        // GET api/GameInstance/GetGameInstance/5
        [HttpGet]
        public GameInstance GetGameInstance(int id)
        {
            GameInstance gameinstance = _db.GameInstanceSet.Find(id);
            if (gameinstance == null) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return gameinstance;
        }

        // GET api/GameInstance/GetLastGameInstance
        [HttpGet]
        public GameInstance GetLastGameInstance()
        {
            var lastGameInstanceId =  (from game in _db.GameInstanceSet
                                       select game.Id).Max();

            GameInstance gameInstance = _db.GameInstanceSet.Find(lastGameInstanceId);
            if (gameInstance == null) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            return gameInstance;
        }

        // PUT api/GameInstance/PutGameInstance/5
        [HttpPost]
        public HttpResponseMessage PutGameInstance(int id, GameInstance gameinstance)
        {
            if (!ModelState.IsValid) {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != gameinstance.Id) {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var game = _db.GameInstanceSet.Find(id);

            if (game == null) {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("GameInstance not found!"));
            }

            if (gameinstance.Map != null) {
                if (game.Map == null) {
                    game.Map = new Map { LatLon = gameinstance.Map.LatLon, Zoom = gameinstance.Map.Zoom, Size = gameinstance.Map.Size };
                } else {
                    game.Map.LatLon = gameinstance.Map.LatLon;
                    game.Map.Size = gameinstance.Map.Size;
                    game.Map.Zoom = gameinstance.Map.Zoom;
                }
            }

            var players = gameinstance.Players;
            foreach (var player in players) {
                var dbPlayer = _db.PlayerSet.Find(player.Id);
                if (dbPlayer == null) {
                    game.Players.Add(new Player { Id = player.Id, Position = player.Position });
                } else {
                    if (dbPlayer.GameInstance != game) {
                        dbPlayer.GameInstance = game;
                    }
                    dbPlayer.Position = player.Position;
                }
            }

            // TODO Map should be removed and created
            // TODO Remove removed players
            // TODO Add new players
            // TODO Update all players position

            try {
                _db.SaveChanges();
            } catch (DbUpdateConcurrencyException ex) {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, game);
            response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = gameinstance.Id }));
            return response;
        }

        // POST api/GameInstance/PostGameInstance
        [HttpPost]
        public HttpResponseMessage PostGameInstance(GameInstance gameinstance)
        {
            if (ModelState.IsValid) {
                _db.GameInstanceSet.Add(gameinstance);
                _db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, gameinstance);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = gameinstance.Id }));
                return response;
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}