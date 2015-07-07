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
        
        //Join the two databases based off of restaurant id
        private IEnumerable<Priority_Q.Models.Customer> GetCustomers(int? restaurantID)
        {
            CustomerDBContext customerDB = new CustomerDBContext();
            return customerDB.Customers.Where(i => i.RestaurantID == restaurantID);
        }
        
        //Find out how many tables have "x" number of people
        private SortedDictionary<int, int> GetTableCapacityCount(IEnumerable<Priority_Q.Models.Table> tables)
        {
            SortedDictionary<int, int> tableCapacityCount = new SortedDictionary<int, int>();
            foreach (var item in tables)
            {
                if (!item.IsOccupied)
                {
                    if (!tableCapacityCount.ContainsKey(item.MaxCapacity))
                        tableCapacityCount.Add(item.MaxCapacity, 1); //initialize
                    else
                        tableCapacityCount[item.MaxCapacity]++; //add one
                }
            }

            return tableCapacityCount;
        }

        //Get all reservations for each table
        private List<IEnumerable<Priority_Q.Models.Reservation>> GetAllReservations(IEnumerable<Priority_Q.Models.Table> tables)
        {
            ReservationDBContext reservationDB = new ReservationDBContext();
            List<IEnumerable<Priority_Q.Models.Reservation>> allReservations = new List<IEnumerable<Priority_Q.Models.Reservation>>();
            foreach (var table in tables)
            {
                IEnumerable<Priority_Q.Models.Reservation> reservations = reservationDB.Reservations.Where(reservation => reservation.TableId == table.ID);
                allReservations.Add(reservations);
            }
            return allReservations;
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

            //Find all tables belonging to a restaurant 
            List<int> allTableCounts = new List<int>();
            for (var i = 0; i < restaurantArray.Length; i++)
            {
                IEnumerable<Priority_Q.Models.Table> tables = GetTables(restaurantArray[i].ID);
                IEnumerable<Priority_Q.Models.Table> availableTables = tables.Where(table => table.IsOccupied == false);
                allTableCounts.Add(availableTables.Count());
            }
            ViewData["AllTableCounts"] = allTableCounts;

            //Find all customer belonging to a restaurant 
            List<int> allCustomerCounts = new List<int>();
            for (var i = 0; i < restaurantArray.Length; i++)
            {
                IEnumerable<Priority_Q.Models.Customer> customers = GetCustomers(restaurantArray[i].ID);
                allCustomerCounts.Add(customers.Count());
            }
            ViewData["AllCustomerCounts"] = allCustomerCounts;

            return View(restaurantArray.ToList());
        }

        // GET: Restaurants/CustomerView/XX
        public ActionResult CustomerView(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Restaurant restaurant = db.Restaurants.Find(id);
            ViewData["Restaurant"] = restaurant;
            ViewBag.OwnsRestaurant = (restaurant.UserID == User.Identity.GetUserId());

            //Find all tables belonging to a restaurant 
            IEnumerable<Priority_Q.Models.Table> tables = GetTables(id);
            ViewData["AllTables"] = tables;

            IEnumerable<Priority_Q.Models.Table> availableTables = tables.Where(table => table.IsOccupied == false);
            ViewBag.AvailableTablesCount = availableTables.Count();

            ViewData["TableCapacityCount"] = GetTableCapacityCount(tables);

            //Find all customer belonging to a restaurant 
            ViewData["AllCustomers"] = GetCustomers(id);

            //Find the most recent news item for a restaurant (usually the last element)
            NewsInfoDBContext newsInfoDB = new NewsInfoDBContext();
            IEnumerable<Priority_Q.Models.NewsInfo> newsInfos = newsInfoDB.NewsInfos.Where(i => i.RestaurantId == id);
            ViewData["MostRecentNews"] = (newsInfos.Count() > 0) ? newsInfos.Last() : null;

            return View();
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

        // GET: Restaurants/ViewReservations/5
        public ActionResult ViewReservations(int? id, String DaySlotList)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Restaurant restaurant = db.Restaurants.Find(id);
            if (!IsAuthorized(restaurant))
            {
                return RedirectToAction("Index", "Restaurants");
            }
            ViewData["Restaurant"] = restaurant;

            //Find all tables belonging to a restaurant 
            IEnumerable<Priority_Q.Models.Table> tables = GetTables(id);
            ViewData["AllTables"] = tables;

            //add day slots
            List<SelectListItem> items = new List<SelectListItem>();
            DateTime todaysDate = DateTime.Now;
            for (var i = 0; i < 14; i++)
            {
                String currentDateDisplay = todaysDate.AddDays(i).ToString("MMM dd, yyyy");
                String currentDateStored = todaysDate.AddDays(i).ToString("MM/dd/yyyy");
                items.Add(new SelectListItem { Text = currentDateDisplay, Value = currentDateStored });
            }
            ViewBag.DaySlotList = items;
            ViewBag.DaySlot = (DaySlotList == null) ? "" : DaySlotList;

            //Find all reservations for each table
            return View(GetAllReservations(tables));
        }

        // GET: Restaurants/ViewTables/XX
        public ActionResult ViewTables(int? id, int? GroupSizeList, String TimeSlotList, String DaySlotList)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Restaurant restaurant = db.Restaurants.Find(id);

            if (!IsAuthorized(restaurant))
                return RedirectToAction("CustomerView", "Restaurants", new { id = id });

            ViewData["Restaurant"] = restaurant;
            ViewBag.OwnsRestaurant = (restaurant.UserID == User.Identity.GetUserId());

            //Find all tables belonging to a restaurant 
            IEnumerable<Priority_Q.Models.Table> tables = GetTables(id);
            ViewData["AllTables"] = tables;

            IEnumerable<Priority_Q.Models.Table> availableTables = tables.Where(table => table.IsOccupied == false);
            ViewBag.AvailableTablesCount = availableTables.Count();

            //Find all customer belonging to a restaurant 
            ViewData["AllCustomers"] = GetCustomers(id);

            //Find the most recent news item for a restaurant (usually the last element)
            NewsInfoDBContext newsInfoDB = new NewsInfoDBContext();
            IEnumerable<Priority_Q.Models.NewsInfo> newsInfos = newsInfoDB.NewsInfos.Where(i => i.RestaurantId == id);
            ViewData["MostRecentNews"] = (newsInfos.Count() > 0) ? newsInfos.Last() : null;

            //Find all reservations for each table
            List<IEnumerable<Priority_Q.Models.Reservation>> allReservations = GetAllReservations(tables);
            List<List<Priority_Q.Models.Reservation>> todayReservations = new List<List<Reservation>>();
            foreach (var item in allReservations)
            {
                todayReservations.Add(new List<Priority_Q.Models.Reservation>());
                foreach (var reservation in item)
                {
                    //only display reservations for the current day
                    if (DateTime.Now.ToString("MM/dd/yyyy").Equals(reservation.DaySlot))
                    {
                        int tableCounter = todayReservations.Count() - 1;
                        todayReservations[tableCounter].Add(reservation);
                    }   
                }
            }
            ViewData["TodayReservations"] = todayReservations;

            //Remove all old reservations
            foreach (var item in allReservations)
            {
                foreach (var reservation in item)
                {
                    if (DateTime.Now.Date > DateTime.Parse(reservation.DaySlot))
                    {
                        //remove the old entry
                        ReservationDBContext reservationDB = new ReservationDBContext();
                        reservationDB.Reservations.Remove(reservationDB.Reservations.Find(reservation.ID));
                        reservationDB.SaveChanges();
                    }
                }
            }

            return View();
        }

        // GET: Restaurants/ManageTables/5
        public ActionResult ManageTables(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Restaurant restaurant = db.Restaurants.Find(id);
            ViewBagSetRestaurantInfo(id);

            if (!IsAuthorized(db.Restaurants.Find(id)))
                return RedirectToAction("Index", "Restaurants");

            //Find all tables belonging to a restaurant 
            IEnumerable<Priority_Q.Models.Table> tables = GetTables(id);
            ViewBag.TotalTables = tables.Count();

            IEnumerable<Priority_Q.Models.Table> availableTables = tables.Where(table => table.IsOccupied == false);
            ViewBag.AvailableTables = availableTables.Count();

            return View(tables);
        }

        // GET: Restaurants/ReserveTables/5?GroupSizeList=XX&&TimeSlotList=XX&&DaySlotList=XX
        public ActionResult ReserveTables(int? id, int? GroupSizeList, String TimeSlotList, String DaySlotList)
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
            ViewBag.GroupSize = (GroupSizeList == null) ? 0 : GroupSizeList;

            //add time slots
            items = new List<SelectListItem>();
            for (var numHours = restaurant.OpeningHourStart; numHours < restaurant.OpeningHourEnd; numHours++)
            {
                for (var numMinutes = 0; numMinutes < 60; numMinutes += 30)
                {
                    //String currentTime = i + ":" + j.ToString().PadLeft(2, '0');
                    DateTime currentTime = new DateTime().AddHours(numHours).AddMinutes(numMinutes);
                    String currentTimeDisplay = currentTime.ToString("hh:mm tt", new CultureInfo("en-US"));
                    items.Add(new SelectListItem
                    {
                        Text = currentTimeDisplay, 
                        Value = currentTimeDisplay 
                    });
                }
            }
            ViewBag.TimeSlotList = items;
            ViewBag.TimeSlot = (TimeSlotList == null) ? "" : TimeSlotList;

            //add day slots
            items = new List<SelectListItem>();
            DateTime todaysDate = DateTime.Now;
            for (var i = 0; i < 14; i++)
            {
                String currentDateDisplay = todaysDate.AddDays(i).ToString("MMM dd, yyyy");
                String currentDateStored = todaysDate.AddDays(i).ToString("MM/dd/yyyy");
                items.Add(new SelectListItem { Text = currentDateDisplay, Value = currentDateStored });
            }
            ViewBag.DaySlotList = items;
            ViewBag.DaySlot = (DaySlotList == null) ? "" : DaySlotList;

            //Find all reservations for each table
            ViewData["AllReservations"] = GetAllReservations(tables);

            //keep track of time. tables should be reserved at least an hour in advance (this might depend on the restaurant though)
            ViewBag.CurrentHour = Int32.Parse(DateTime.Now.ToString("HH"));

            return View(tables);
        }

        // GET:  Restaurants/AssignTable/?tableID=XX&customerID=XX
        public ActionResult AssignTable(int? tableID, int? customerID)
        {
            TableDBContext tableDB = new TableDBContext();
            Table emptyTable = tableDB.Tables.Find(tableID);
            int restaurantID = emptyTable.RestaurantId;

            //make sure user is authorized before proceeding
            if (!IsAuthorized(db.Restaurants.Find(restaurantID)))
                return RedirectToAction("Index", "Restaurants");

            //refuse to fill table if there's no enough seats
            CustomerDBContext customerDB = new CustomerDBContext();
            Customer topCustomer = customerDB.Customers.Find(customerID);
            if (emptyTable.MaxCapacity < topCustomer.GroupCapacity)
                return RedirectToAction("Index", "Restaurants");

            //Occupy the table
            emptyTable.IsOccupied = true;

            //Modify the table in the database
            tableDB.Entry(emptyTable).State = EntityState.Modified;
            tableDB.SaveChanges();

            //remove customer from queue
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
