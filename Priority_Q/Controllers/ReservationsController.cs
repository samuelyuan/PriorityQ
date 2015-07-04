using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Priority_Q.Models;
using Microsoft.AspNet.Identity;

namespace Priority_Q.Views
{
    public class ReservationsController : Controller
    {
        private ReservationDBContext db = new ReservationDBContext();

        // GET: Reservations/Confirm/?tableID=XX&timeSlot=XX&daySlot=XX
        public ActionResult Confirm(int? tableID, String timeSlot, String daySlot)
        {
            //Find the table that we want to reserve
            TableDBContext tableDB = new TableDBContext();
            Table desiredTable = tableDB.Tables.Find(tableID);

            Restaurant restaurant = (new RestaurantDBContext()).Restaurants.Find(desiredTable.RestaurantId);
            ViewBag.RestaurantId = desiredTable.RestaurantId;
            if (!Request.IsAuthenticated || restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            //Find the reservation
            Reservation reservation = new Reservation();
            reservation.TableId = desiredTable.ID;
            reservation.DaySlot = daySlot;
            reservation.HourSlot = DateTime.Parse(timeSlot).Hour;
            reservation.MinuteSlot = DateTime.Parse(timeSlot).Minute;

            return View(reservation);
        }

        // Post:  Reservations/AssignTable/?tableID=XX&timeSlot=XX&daySlot=XX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm([Bind(Include = "ID,TableId,DaySlot,HourSlot,MinuteSlot,CustomerName")] Reservation reservation)
        {
            //Find the table that we want to reserve
            TableDBContext tableDB = new TableDBContext();
            Table desiredTable = tableDB.Tables.Find(reservation.TableId);

            if (ModelState.IsValid)
            {
                //Add the reservation to the database
                db.Reservations.Add(reservation);
                db.SaveChanges();
                return RedirectToAction("ViewTables", "Restaurants", new { id = desiredTable.RestaurantId });
            }

            return View(reservation);
        }

        // GET: Reservations/Manage/5
        public ActionResult Manage(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }

            TableDBContext tableDB = new TableDBContext();
            ViewBag.RestaurantId = tableDB.Tables.Find(reservation.TableId).RestaurantId;

            Restaurant restaurant = (new RestaurantDBContext()).Restaurants.Find(ViewBag.RestaurantId);
            if (!Request.IsAuthenticated || restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservations.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Reservation reservation = db.Reservations.Find(id);
            db.Reservations.Remove(reservation);
            db.SaveChanges();

            var table = (new TableDBContext()).Tables.Find(reservation.TableId);
            var restaurantId = table.RestaurantId;

            return RedirectToAction("ViewTables", "Restaurants", new { id = restaurantId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
