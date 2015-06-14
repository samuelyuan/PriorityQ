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

namespace Priority_Q.Controllers
{
    public class CustomersController : Controller
    {
        private CustomerDBContext db = new CustomerDBContext();

        // GET: Customers
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Restaurants");
        }

        // GET: Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Customers/Create/1
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = new Customer();
            customer.RestaurantID = id.Value;

            //user isn't logged in
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Restaurants");

            //restaurant doesn't have this customer
            Restaurant restaurant = (new RestaurantDBContext()).Restaurants.Find(customer.RestaurantID);
            if (restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            return View(customer);
        }

        // POST: Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,RestaurantID,Name,GroupCapacity")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                db.SaveChanges();
                return RedirectToAction("ViewTables", "Restaurants", new { id = customer.RestaurantID });
            }

            return View(customer);
        }

        // GET: Customers/AssignTable/5
        public ActionResult AssignTable(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer customer = db.Customers.Find(id);
            ViewBag.CustomerID = customer.ID;
            ViewBag.CustomerName = customer.Name;
            ViewBag.CustomerGroupSize = customer.GroupCapacity;

            RestaurantDBContext RestaurantDB = new RestaurantDBContext();

            //Restaurant restaurant = db.Restaurants.Find(id);
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Restaurants");

            //restaurant doesn't have this customer
            Restaurant restaurant = RestaurantDB.Restaurants.Find(customer.RestaurantID);
            if (restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            //Find all tables belonging to a restaurant 
            TableDBContext tableDB = new TableDBContext();
            IEnumerable<Priority_Q.Models.Table> tables = tableDB.Tables.Where(i => i.RestaurantId == restaurant.ID);
            IEnumerable<Priority_Q.Models.Table> canSeatTables = tables.Where(i => i.MaxCapacity >= ViewBag.CustomerGroupSize); 
            ViewBag.RestaurantId = restaurant.ID;
            ViewBag.OwnsRestaurant = (restaurant.UserID == User.Identity.GetUserId());
            ViewBag.RestaurantName = restaurant.Name;
            ViewBag.RestaurantLocation = restaurant.Location;

            IEnumerable<Priority_Q.Models.Table> availableTables = canSeatTables.Where(table => table.IsOccupied == false);
            ViewBag.AvailableTables = availableTables.Count();

            return View(tables);
        }

        // GET: Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,RestaurantID,Name,GroupCapacity")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            //if the user isn't logged in, they shouldn't be able to delete customers!
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Restaurants");

            //if the restaurant doesn't have this customer, they shouldn't be able to delete customers
            Restaurant restaurant = (new RestaurantDBContext()).Restaurants.Find(customer.RestaurantID);
            if (restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customers.Find(id);
            db.Customers.Remove(customer);
            db.SaveChanges();
            return RedirectToAction("ViewTables", "Restaurants", new { id = customer.RestaurantID });
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
