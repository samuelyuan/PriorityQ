using System.Data.Entity;
using System.Net;
using System.Collections;
using System.Web.Mvc;
using Priority_Q.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;

namespace Priority_Q.Controllers
{
    public class TablesController : Controller
    {
        private TableDBContext db = new TableDBContext();

        // GET: Tables
        public ActionResult Index()
        {
            return View();
        }

        // GET: Tables/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Table table = db.Tables.Find(id);
            if (table == null)
            {
                return HttpNotFound();
            }
            return View(table);
        }

        // GET: Tables/Create/1
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Table table = new Table();
            table.RestaurantId = id.Value;
            return View(table);
        }

        // POST: Tables/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,RestaurantId,MaxCapacity,IsOccupied,ReservedTimes")] Table table)
        {
            if (ModelState.IsValid)
            {
                db.Tables.Add(table);
                db.SaveChanges();
                return RedirectToAction("ViewTables", "Restaurants", new { id = table.RestaurantId });
            }

            return View(table);
        }

        // GET: Tables/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Table table = db.Tables.Find(id);
            if (table == null)
            {
                return HttpNotFound();
            }

            //if the user isn't logged in, they shouldn't be able to edit tables!
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Restaurants");

            //if the user doesn't own the table, they shouldn't be able to edit tables
            Restaurant restaurant = (new RestaurantDBContext()).Restaurants.Find(table.RestaurantId);
            if (restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            return View(table);
        }

        // POST: Tables/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,RestaurantId,MaxCapacity,IsOccupied,ReservedTimes")] Table table)
        {
            if (ModelState.IsValid)
            {
                db.Entry(table).State = EntityState.Modified;   
                db.SaveChanges();
                return RedirectToAction("ViewTables", "Restaurants", new { id = table.RestaurantId });
            }
            return View(table);
        }

        // GET: Tables/ToggleOccupied/1
        public ActionResult ToggleOccupied(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Table table = db.Tables.Find(id);
            if (table == null)
            {
                return HttpNotFound();
            }

            //if the user isn't logged in, they shouldn't be able to edit tables!
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Restaurants");

            //if the user doesn't own the table, they shouldn't be able to edit tables
            Restaurant restaurant = (new RestaurantDBContext()).Restaurants.Find(table.RestaurantId);
            if (restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            table.IsOccupied = !table.IsOccupied;
            db.Entry(table).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("ViewTables", "Restaurants", new { id = table.RestaurantId });
        }

        // GET: Tables/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Table table = db.Tables.Find(id);
            if (table == null)
            {
                return HttpNotFound();
            }

            //if the user isn't logged in, they shouldn't be able to delete tables!
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Restaurants");

            //if the user doesn't own the table, they shouldn't be able to delete tables
            Restaurant restaurant = (new RestaurantDBContext()).Restaurants.Find(table.RestaurantId);
            if (restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            return View(table);
        }

        // POST: Tables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Table table = db.Tables.Find(id);
            db.Tables.Remove(table);
            db.SaveChanges();
            return RedirectToAction("ViewTables", "Restaurants", new { id = table.RestaurantId });
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
