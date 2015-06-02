﻿using System;
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
    public class RestaurantsController : Controller
    {
        private RestaurantDBContext db = new RestaurantDBContext();

        // GET: Restaurants
        public ActionResult Index()
        {
            Restaurant[] restaurantArray = db.Restaurants.ToArray();;
            for (int i = 0; i < restaurantArray.Count(); i++)
            {
                int restaurantId = restaurantArray[i].ID;
               
                //the number of tables for a given restaurant can change, so update
                TableDBContext tableDB = new TableDBContext();
                IEnumerable<Priority_Q.Models.Table> allTables = tableDB.Tables.Where(table => table.RestaurantId == restaurantId);
                restaurantArray[i].NumTables = allTables.Count();

                IEnumerable<Priority_Q.Models.Table> availableTables = allTables.Where(table => table.IsOccupied == false);
                restaurantArray[i].AvailableTables = availableTables.Count();

                //Find all customer belonging to a restaurant 
                CustomerDBContext customerDB = new CustomerDBContext();
                IEnumerable<Priority_Q.Models.Customer> customers = customerDB.Customers.Where(cust => cust.RestaurantID == restaurantId);

                //If there are available tables and customers are still in the priority queue, remove them one at a time
                if (availableTables.Count() > 0 && customers.Count() > 0)
                {
                    foreach (Table table in availableTables)
                    {
                        //if table is unoccupied, fill in the seat with the highest priority customer
                        if (table.IsOccupied == false)
                        {
                            //fill in seat
                            Table emptyTable = tableDB.Tables.Find(table.ID);
                            emptyTable.IsOccupied = true;
                            tableDB.Entry(emptyTable).State = EntityState.Modified;
                            tableDB.SaveChanges();
                            restaurantArray[i].AvailableTables--;

                            //remove customer from queue
                            Customer topCustomer = customers.First();
                            topCustomer = customerDB.Customers.Find(topCustomer.ID);
                            customerDB.Customers.Remove(topCustomer);
                            customerDB.SaveChanges();
                        }
                    }
                }
            }

            return View(restaurantArray.ToList());
        }

        // GET: Restaurants/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }
            return View(restaurant);
        }

        // GET: Restaurants/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Restaurants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Location,NumTables,UserID")] Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                restaurant.UserID = User.Identity.GetUserId();
                db.Restaurants.Add(restaurant);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(restaurant);
        }

        // GET: Restaurants/ViewPriorityQueue/5
        public ActionResult ViewPriorityQueue(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Restaurant restaurant = db.Restaurants.Find(id);

            //Find all customer belonging to a restaurant 
            CustomerDBContext customerDB = new CustomerDBContext();
            IEnumerable<Priority_Q.Models.Customer> customers = customerDB.Customers.Where(i => i.RestaurantID == id);
            ViewBag.RestaurantID = id;
            ViewBag.OwnsRestaurant = (db.Restaurants.Find(id).UserID == User.Identity.GetUserId());
            return View(customers);
        }

        // GET: Restaurants/ViewTables/5
        public ActionResult ViewTables(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Restaurant restaurant = db.Restaurants.Find(id);

            //Find all tables belonging to a restaurant 
            TableDBContext tableDB = new TableDBContext();
            IEnumerable<Priority_Q.Models.Table> tables = tableDB.Tables.Where(i => i.RestaurantId == id);
            ViewBag.RestaurantId = id;
            ViewBag.OwnsRestaurant = (db.Restaurants.Find(id).UserID == User.Identity.GetUserId());
            return View(tables);
        }

        // GET: Restaurants/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }

            //if the user isn't logged in, they shouldn't be able to edit restaurants!
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Restaurants");
         
            //if the logged in user doesn't own the restaurant, they shouldn't be able to edit other people's data
            if (restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Location,NumTables")] Restaurant restaurant)
        {
            if (ModelState.IsValid)
            {
                //while editing, make sure the user id is attached to the restaurant
                //that way the user doesn't lose control by accident
                restaurant.UserID = User.Identity.GetUserId();

                db.Entry(restaurant).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(restaurant);
        }

        // GET: Restaurants/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            if (restaurant == null)
            {
                return HttpNotFound();
            }

            //if the user isn't logged in, they shouldn't be able to delete restaurants!
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Restaurants");

            //if the logged in user doesn't own the restaurant, they shouldn't be able to delete other people's data!
            if (restaurant.UserID != User.Identity.GetUserId())
                return RedirectToAction("Index", "Restaurants");

            return View(restaurant);
        }

        // POST: Restaurants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Restaurant restaurant = db.Restaurants.Find(id);
            db.Restaurants.Remove(restaurant);
            db.SaveChanges();
            return RedirectToAction("Index");
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
