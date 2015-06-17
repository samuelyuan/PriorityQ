using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Priority_Q.Models;

namespace Priority_Q.Controllers
{
    public class NewsInfosController : Controller
    {
        private NewsInfoDBContext db = new NewsInfoDBContext();

        // GET: NewsInfoes
        public ActionResult Index()
        {
            return View(db.NewsInfos.ToList());
        }

        // GET: NewsInfoes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsInfo newsInfo = db.NewsInfos.Find(id);
            if (newsInfo == null)
            {
                return HttpNotFound();
            }
            return View(newsInfo);
        }

        // GET: NewsInfoes/Create
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsInfo newsInfo = new NewsInfo();
            newsInfo.RestaurantId = id.Value;

            return View(newsInfo);
        }

        // POST: NewsInfoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,RestaurantId,Content,Date")] NewsInfo newsInfo)
        {
            if (ModelState.IsValid)
            {
                newsInfo.Date = DateTime.Now.ToString("MM/dd/yyyy h:mm tt");
                db.NewsInfos.Add(newsInfo);
                db.SaveChanges();
                return RedirectToAction("ViewTables", "Restaurants", new { id = newsInfo.RestaurantId });
            }

            return View(newsInfo);
        }

        // GET: NewsInfoes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsInfo newsInfo = db.NewsInfos.Find(id);
            if (newsInfo == null)
            {
                return HttpNotFound();
            }
            return View(newsInfo);
        }

        // POST: NewsInfoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,RestaurantId,Content,Date")] NewsInfo newsInfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(newsInfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ViewTables", "Restaurants", new { id = newsInfo.RestaurantId });
            }
            return View(newsInfo);
        }

        // GET: NewsInfoes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsInfo newsInfo = db.NewsInfos.Find(id);
            if (newsInfo == null)
            {
                return HttpNotFound();
            }
            return View(newsInfo);
        }

        // POST: NewsInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NewsInfo newsInfo = db.NewsInfos.Find(id);
            db.NewsInfos.Remove(newsInfo);
            db.SaveChanges();
            return RedirectToAction("ViewTables", "Restaurants", new { id = newsInfo.RestaurantId });
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
