﻿using System;
using System.Collections.Generic;
using System.Data;
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
            return _db.GameInstanceSet.AsEnumerable();
        }

        // GET api/GameInstance/GetGameInstance/5
        [HttpGet]
        public GameInstance GetGameInstance(int id)
        {
            GameInstance gameinstance = _db.GameInstanceSet.Find(id);
            if(gameinstance == null) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return gameinstance;
        }

        // PUT api/GameInstance/PutGameInstance/5
        [HttpPost]
        public HttpResponseMessage PutGameInstance(int id, GameInstance gameinstance)
        {
            if(!ModelState.IsValid) {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if(id != gameinstance.Id) {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            _db.GameInstanceSet.Attach(gameinstance);
            _db.Entry(gameinstance).State = EntityState.Modified;

            try {
                int count = _db.SaveChanges();
            } catch(DbUpdateConcurrencyException ex) {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/GameInstance/PostGameInstance
        [HttpPost]
        public HttpResponseMessage PostGameInstance(GameInstance gameinstance)
        {
            if(ModelState.IsValid) {
                _db.GameInstanceSet.Add(gameinstance);
                _db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, gameinstance);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = gameinstance.Id }));
                return response;
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        // DELETE api/GameInstance/DeleteGameInstance/5
        [HttpGet]
        public HttpResponseMessage DeleteGameInstance(int id)
        {
            GameInstance gameinstance = _db.GameInstanceSet.Find(id);
            if(gameinstance == null) {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            _db.MapSet.Remove(gameinstance.Map);
            var players = gameinstance.Teams.SelectMany(t => t.Players);
            foreach(var player in players) {
                player.Team = null;
            }
            var teams = gameinstance.Teams.ToList();
            foreach(var team in teams) {
                _db.TeamSet.Remove(team);
            }
            _db.GameInstanceSet.Remove(gameinstance);

            try {
                _db.SaveChanges();
            } catch(DbUpdateConcurrencyException ex) {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, gameinstance);
        }

        protected override void Dispose(bool disposing)
        {
            _db.Dispose();
            base.Dispose(disposing);
        }
    }
}