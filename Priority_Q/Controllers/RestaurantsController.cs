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
using System.Globalization;

namespace Priority_Q.Controllers
{
    public class RestaurantsController : Controller
    {
        private RestaurantDBContext db = new RestaurantDBContext();

        //------------HELPER FUNCTIONS-------------------

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

        //Join the two databases based off of restaurant id
        private IEnumerable<Priority_Q.Models.Table> GetTables(int? restaurantID)
        {
            TableDBContext tableDB = new TableDBContext();
            return tableDB.Tables.Where(i => i.RestaurantId == restaurantID);
        }

        //set some basic properties for the view
        private void ViewBagSetRestaurantInfo(int? id)
        {
            ViewBag.RestaurantId = id;
            ViewBag.OwnsRestaurant = (db.Restaurants.Find(id).UserID == User.Identity.GetUserId());
            ViewBag.RestaurantName = db.Restaurants.Find(id).Name;
            ViewBag.RestaurantLocation = db.Restaurants.Find(id).City;
        }

        private String ConvertIntTo24Hour(int number)
        {
            String convertToString = number.ToString().PadLeft(2, '0');
            DateTime convertToDateTime = DateTime.ParseExact(convertToString, "HH", CultureInfo.CurrentCulture);
            return convertToDateTime.ToString("h:mm tt");
        }

        //----------------------------------------------

        // GET: Restaurants
        public ActionResult Index(string searchString)
        {
            Restaurant[] restaurantArray = db.Restaurants.ToArray();

            if (!String.IsNullOrEmpty(searchString))
            {
                restaurantArray = db.Restaurants.Where(s => s.Name.Contains(searchString)
                                       || s.City.Contains(searchString)).ToArray();
            }

            ViewBag.CurrentHour = Int32.Parse(DateTime.Now.ToString("HH"));

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
        public ActionResult Create([Bind(Include = "ID,Name,StreetAddress,City,PhoneNumber,NumTables,UserID,OpeningHourStart,OpeningHourEnd")] Restaurant restaurant)
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
            ViewBagSetRestaurantInfo(id);

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
            IEnumerable<Priority_Q.Models.Table> tables = GetTables(id);
            ViewBagSetRestaurantInfo(id);
            ViewBag.TotalTables = tables.Count();

            IEnumerable<Priority_Q.Models.Table> availableTables = tables.Where(table => table.IsOccupied == false);
            ViewBag.AvailableTablesCount = availableTables.Count();
            IEnumerable<Priority_Q.Models.Table> occupiedTables = tables.Where(table => table.IsOccupied == true);
            ViewBag.OccupiedTablesCount = occupiedTables.Count();

            //Find all reservations for each table
            ReservationDBContext reservationDB = new ReservationDBContext();
            ViewBag.AllTimeSlots = new List<int>[tables.Count()];
            ViewBag.ReservationIds = new List<int>[tables.Count()];
            ViewBag.NumReservations = new int[tables.Count()];
            int tableCounter = 0;
            foreach (var table in tables)
            {
                IEnumerable<Priority_Q.Models.Reservation> reservations = reservationDB.Reservations.Where(reservation => reservation.TableId == table.ID);

                ViewBag.AllTimeSlots[tableCounter] = new List<int>();
                ViewBag.ReservationIds[tableCounter] = new List<int>();
                foreach (var reservation in reservations)
                {
                    ViewBag.AllTimeSlots[tableCounter].Add(reservation.TimeSlot);
                    ViewBag.ReservationIds[tableCounter].Add(reservation.ID);
                }
                ViewBag.NumReservations[tableCounter] = reservations.Count();
                tableCounter++;
            }

            //Find all customer belonging to a restaurant 
            CustomerDBContext customerDB = new CustomerDBContext();
            IEnumerable<Priority_Q.Models.Customer> customers = customerDB.Customers.Where(i => i.RestaurantID == id);
            ViewBag.NumCustomers = customers.Count();
            ViewBag.CustomerData = customers;

            //find the most recent news item for a restaurant (usually the last element)
            NewsInfoDBContext newsInfoDB = new NewsInfoDBContext();
            IEnumerable<Priority_Q.Models.NewsInfo> newsInfos = newsInfoDB.NewsInfos.Where(i => i.RestaurantId == id);
            ViewBag.MostRecentNews = "";
            ViewBag.MostRecentDate = "";
            ViewBag.NumNewsItems = newsInfos.Count();
            if (newsInfos.Count() > 0)
            {
                ViewBag.MostRecentNews = newsInfos.Last().Content;
                ViewBag.MostRecentDate = newsInfos.Last().Date;
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
            IEnumerable<Priority_Q.Models.Table> tables = GetTables(id);
            ViewBagSetRestaurantInfo(id);
           
            ViewBag.TotalTables = tables.Count();
            IEnumerable<Priority_Q.Models.Table> availableTables = tables.Where(table => table.IsOccupied == false);
            ViewBag.AvailableTables = availableTables.Count();

            return View(tables);
        }

        // GET: Restaurants/ReserveTables/5?GroupSizeList=XX&&TimeSlotList=XX
        public ActionResult ReserveTables(int? id, int? GroupSizeList, int? TimeSlotList)
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

            //Find all tables belonging to a restaurant 
            IEnumerable<Priority_Q.Models.Table> tables = GetTables(id);
            ViewBagSetRestaurantInfo(id);

            ViewBag.TableCount = tables.Count();

            //add options for group size
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "1 person", Value = "1" });
            for (var i = 2; i <= 20; i++)
            {
                items.Add(new SelectListItem { Text = i.ToString() + " people", Value = i.ToString() });
            }
            ViewBag.GroupSizeList = items;
            if (GroupSizeList == null)
                ViewBag.GroupSize = 0;
            else
                ViewBag.GroupSize = GroupSizeList;

            //add time slots
            items = new List<SelectListItem>();
            int restaurantOpeningHour = restaurant.OpeningHourStart;
            int restaurantClosingHour = restaurant.OpeningHourEnd;
            for (var i = restaurantOpeningHour; i < restaurantClosingHour; i++)
            {
                items.Add(new SelectListItem { Text = ConvertIntTo24Hour(i), Value = i.ToString() });
            }
            ViewBag.TimeSlotList = items;
            if (TimeSlotList == null)
                ViewBag.TimeSlot = 0;
            else
                ViewBag.TimeSlot = TimeSlotList;

            //Find all reservations for each table
            ReservationDBContext reservationDB = new ReservationDBContext();
            ViewBag.AllTimeSlots = new List<int>[tables.Count()];
            ViewBag.NumReservations = new int[tables.Count()];
            int tableCounter = 0;
            foreach (var table in tables)
            {
                IEnumerable<Priority_Q.Models.Reservation> reservations = reservationDB.Reservations.Where(reservation => reservation.TableId == table.ID);

                ViewBag.AllTimeSlots[tableCounter] = new List<int>();
                foreach (var reservation in reservations)
                {
                    ViewBag.AllTimeSlots[tableCounter].Add(reservation.TimeSlot);
                }
                ViewBag.NumReservations[tableCounter] = reservations.Count();
                tableCounter++;
            }

            return View(tables);
        }

        // GET:  Restaurants/AssignReservation/?tableID=XX&timeSlot=XX
        public ActionResult AssignReservation(int? tableID, int? timeSlot)
        {
            TableDBContext tableDB = new TableDBContext();
            Table emptyTable = tableDB.Tables.Find(tableID);

            ReservationDBContext reservationDB = new ReservationDBContext();
            Reservation reservation = new Reservation();
            reservation.TableId = emptyTable.ID;
            reservation.TimeSlot = timeSlot.Value;

            reservationDB.Reservations.Add(reservation);
            reservationDB.SaveChanges();

            return RedirectToAction("ViewTables", "Restaurants", new { id = emptyTable.RestaurantId });
        }

        // GET:  Restaurants/AssignTable/?tableID=XX&customerID=XX
        public ActionResult AssignTable(int? tableID, int? customerID)
        {
            TableDBContext tableDB = new TableDBContext();
            Table emptyTable = tableDB.Tables.Find(tableID);

            //Find all customer belonging to a restaurant 
            CustomerDBContext customerDB = new CustomerDBContext();
            Customer topCustomer = customerDB.Customers.Find(customerID);

            int restaurantID = emptyTable.RestaurantId;

            //make sure user is authorized before proceeding
            if (!IsAuthorized(db.Restaurants.Find(restaurantID)))
                return RedirectToAction("Index", "Restaurants");

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
        public ActionResult Edit([Bind(Include = "ID,Name,StreetAddress,City,PhoneNumber,NumTables,UserID,OpeningHourStart,OpeningHourEnd")] Restaurant restaurant)
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
