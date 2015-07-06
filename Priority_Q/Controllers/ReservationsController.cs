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

        // Post:  Reservations/Confirm/?tableID=XX&timeSlot=XX&daySlot=XX
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

            return RedirectToAction("ReserveTables", "Restaurants", new { id = desiredTable.RestaurantId });
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
