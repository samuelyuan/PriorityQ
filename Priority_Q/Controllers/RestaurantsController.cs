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
    public class RestaurantsController : Controller
    {
        private RestaurantDBContext db = new RestaurantDBContext();

        private Boolean IsAuthorized(Restaurant restaurant)
        {
            //user isn't logged in
            if (!Request.IsAuthenticated)
                return false;

            //logged in user doesn't own the restaurant
            if (restaurant.UserID != User.Identity.GetUserId())
                return false;

            return true;
        }

        // GET: Restaurants
        public ActionResult Index(string searchString)
        {
            Restaurant[] restaurantArray = db.Restaurants.ToArray();

            if (!String.IsNullOrEmpty(searchString))
            {
                restaurantArray = db.Restaurants.Where(s => s.Name.Contains(searchString)
                                       || s.City.Contains(searchString)).ToArray();
            }

            ViewBag.AvailableTablesArray = new int[restaurantArray.Length];
            ViewBag.NumWaitingArray = new int[restaurantArray.Length];
            
            for (int i = 0; i < restaurantArray.Count(); i++)
            {
                int restaurantId = restaurantArray[i].ID;
               
                //the number of tables for a given restaurant can change, so update
                TableDBContext tableDB = new TableDBContext();
                IEnumerable<Priority_Q.Models.Table> allTables = tableDB.Tables.Where(table => table.RestaurantId == restaurantId);
                IEnumerable<Priority_Q.Models.Table> availableTables = allTables.Where(table => table.IsOccupied == false);
                ViewBag.AvailableTablesArray[i] = availableTables.Count();

                //Find all customer belonging to a restaurant 
                CustomerDBContext customerDB = new CustomerDBContext();
                IEnumerable<Priority_Q.Models.Customer> customers = customerDB.Customers.Where(cust => cust.RestaurantID == restaurantId);

                ViewBag.NumWaitingArray[i] = customers.Count();
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
            //user isn't logged in
            if (!Request.IsAuthenticated)
                return RedirectToAction("Index", "Restaurants");

            return View();
        }

        // POST: Restaurants/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,StreetAddress,City,PhoneNumber,NumTables,UserID")] Restaurant restaurant)
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

        // GET: Restaurants/ViewNews/5
        public ActionResult ViewNews(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Find all pieces of news belonging to a restaurant 
            NewsInfoDBContext newsInfoDB = new NewsInfoDBContext();
            IEnumerable<Priority_Q.Models.NewsInfo> newsInfos = newsInfoDB.NewsInfos.Where(i => i.RestaurantId == id);
            ViewBag.RestaurantId = id;
            ViewBag.OwnsRestaurant = (db.Restaurants.Find(id).UserID == User.Identity.GetUserId());
            ViewBag.RestaurantName = db.Restaurants.Find(id).Name;
            ViewBag.RestaurantLocation = db.Restaurants.Find(id).City;

            //reverse because the site should display the most recent posts, which are at the end
            return View(newsInfos.Reverse());
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
            ViewBag.RestaurantName = db.Restaurants.Find(id).Name;
            ViewBag.RestaurantLocation = db.Restaurants.Find(id).City;

            ViewBag.TotalTables = tables.Count();
            IEnumerable<Priority_Q.Models.Table> availableTables = tables.Where(table => table.IsOccupied == false);
            ViewBag.AvailableTablesCount = availableTables.Count();
            IEnumerable<Priority_Q.Models.Table> occupiedTables = tables.Where(table => table.IsOccupied == true);
            ViewBag.OccupiedTablesCount = occupiedTables.Count();

            //Find all customer belonging to a restaurant 
            CustomerDBContext customerDB = new CustomerDBContext();
            IEnumerable<Priority_Q.Models.Customer> customers = customerDB.Customers.Where(i => i.RestaurantID == id);
            ViewBag.NumCustomers = customers.Count();
            ViewBag.CustomerData = customers;

            //find the most recent news item for a restaurant (usually the last element)
            NewsInfoDBContext newsInfoDB = new NewsInfoDBContext();
            IEnumerable<Priority_Q.Models.NewsInfo> newsInfos = newsInfoDB.NewsInfos.Where(i => i.RestaurantId == id);
            if (newsInfos.Count() > 0)
            {
                ViewBag.MostRecentNews = newsInfos.Last().Content;
                ViewBag.MostRecentDate = newsInfos.Last().Date;
            }
            else 
            {
                ViewBag.MostRecentNews = "";
                ViewBag.MostRecentDate = "";
            }
            var tuple = new Tuple<IEnumerable<Priority_Q.Models.Table>, IEnumerable<Priority_Q.Models.Customer>>(tables, customers);
            return View(tuple);
        }

        // GET: Restaurants/ManageTables/5
        public ActionResult ManageTables(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Restaurant restaurant = db.Restaurants.Find(id);

            if (!IsAuthorized(db.Restaurants.Find(id)))
                return RedirectToAction("Index", "Restaurants");

            //Find all tables belonging to a restaurant 
            TableDBContext tableDB = new TableDBContext();
            IEnumerable<Priority_Q.Models.Table> tables = tableDB.Tables.Where(i => i.RestaurantId == id);
            ViewBag.RestaurantId = id;
            ViewBag.OwnsRestaurant = (db.Restaurants.Find(id).UserID == User.Identity.GetUserId());
            ViewBag.RestaurantName = db.Restaurants.Find(id).Name;
            ViewBag.RestaurantLocation = db.Restaurants.Find(id).City;
           
            ViewBag.TotalTables = tables.Count();
            IEnumerable<Priority_Q.Models.Table> availableTables = tables.Where(table => table.IsOccupied == false);
            ViewBag.AvailableTables = availableTables.Count();

            return View(tables);
        }

        // GET:  Restaurants/AssignTable/?tableID=XX&customerID=XX
        public ActionResult AssignTable(int? tableID, int? customerID)
        {
            //the number of tables for a given restaurant can change, so update
            TableDBContext tableDB = new TableDBContext();
            Table emptyTable = tableDB.Tables.Find(tableID);

            //Find all customer belonging to a restaurant 
            CustomerDBContext customerDB = new CustomerDBContext();
            Customer topCustomer = customerDB.Customers.Find(customerID);

            int restaurantID = emptyTable.RestaurantId;

            //fill in seat
            //refuse to fill table if there's no enough seats
            if (emptyTable.MaxCapacity < topCustomer.GroupCapacity)
            {
                return RedirectToAction("Index", "Restaurants");
            }
            emptyTable.IsOccupied = true;
            tableDB.Entry(emptyTable).State = EntityState.Modified;
            tableDB.SaveChanges();

            //remove customer from queue
            topCustomer = customerDB.Customers.Find(topCustomer.ID);
            customerDB.Customers.Remove(topCustomer);
            customerDB.SaveChanges();

            return RedirectToAction("ViewTables", "Restaurants", new { id = restaurantID });
        }


        // GET: Restaurants/ManagePriorityQueue/5
        public ActionResult ManagePriorityQueue(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Restaurant restaurant = db.Restaurants.Find(id);
            if (!IsAuthorized(db.Restaurants.Find(id)))
                return RedirectToAction("Index", "Restaurants");

            //Find all customer belonging to a restaurant 
            CustomerDBContext customerDB = new CustomerDBContext();
            IEnumerable<Priority_Q.Models.Customer> customers = customerDB.Customers.Where(i => i.RestaurantID == id);
            ViewBag.RestaurantID = id;
            ViewBag.OwnsRestaurant = (db.Restaurants.Find(id).UserID == User.Identity.GetUserId());
            ViewBag.RestaurantName = db.Restaurants.Find(id).Name;
            ViewBag.RestaurantLocation = db.Restaurants.Find(id).City;
            ViewBag.NumCustomers = customers.Count();
            return View(customers);
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

            if (!IsAuthorized(restaurant))
                return RedirectToAction("Index", "Restaurants");
        
            return View(restaurant);
        }

        // POST: Restaurants/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,StreetAddress,City,PhoneNumber,NumTables,UserID")] Restaurant restaurant)
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

            if (!IsAuthorized(restaurant))
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
