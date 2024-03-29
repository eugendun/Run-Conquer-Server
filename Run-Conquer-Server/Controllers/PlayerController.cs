﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Run_Conquer_Server.Models;

namespace Run_Conquer_Server.Controllers
{
    public class PlayerController : ApiController
    {
        private readonly GameModelContainer db = new GameModelContainer();

        // GET api/Player/GetPlayers
        [HttpGet]
        public IEnumerable<Player> GetPlayers()
        {
            return db.PlayerSet.ToList(); ;
        }

        // GET api/Player/GetPlayer/5
        [HttpGet]
        public Player GetPlayer(int id)
        {
            Player player = db.PlayerSet.Find(id);
            if (player == null) {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return player;
        }

        // POST api/Player/PutPlayer
        [HttpPost]
        public HttpResponseMessage PutPlayer(Player player)
        {
            if (!ModelState.IsValid) {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            var dbPlayer = db.PlayerSet.Find(player.Id);
            if (dbPlayer == null) {
                return PostPlayer(player);
            }

            dbPlayer.Position.x = player.Position.x;
            dbPlayer.Position.y = player.Position.y;

            try {
                db.SaveChanges();
            } catch (DbUpdateConcurrencyException ex) {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/Player/PostPlayer
        [HttpPost]
        public HttpResponseMessage PostPlayer(Player player)
        {
            if (ModelState.IsValid) {
                db.PlayerSet.Add(player);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, player);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = player.Id }));
                return response;
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        // GET api/Player/DeletePlayer/5
        [HttpGet]
        public HttpResponseMessage DeletePlayer(int id)
        {
            Player player = db.PlayerSet.Find(id);
            if (player == null) {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.PlayerSet.Remove(player);

            try {
                db.SaveChanges();
            } catch (DbUpdateConcurrencyException ex) {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, player);
        }

        // GET api/Player/DeleteAllPlayers
        [HttpGet]
        public HttpResponseMessage DeleteAllPlayers()
        {
            List<Player> players = db.PlayerSet.ToList();
            foreach (Player player in players) {
                db.PlayerSet.Remove(player);
            }

            try {
                db.SaveChanges();
            } catch (DbUpdateConcurrencyException ex) {
                return Request.CreateErrorResponse(HttpStatusCode.Conflict, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}