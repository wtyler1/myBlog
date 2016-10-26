﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using wtyler_Blog.Models;
using PagedList;
using PagedList.Mvc;





namespace wtyler_Blog.Controllers
{
    [RequireHttps]
    public class BlogPostsController : Controller
    {
        
        private ApplicationDbContext db = new ApplicationDbContext();
        private ImageUploadValidator validator = new ImageUploadValidator();

        // GET: BlogPosts
        public ActionResult Index(int? page, string query)
        {
            //Search
            var result = db.Posts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                result = db.Posts.Where(p => p.Body.Contains(query))
                .Union(db.Posts.Where(p => p.Title.Contains(query)))
                .Union(db.Posts.Where(p => p.Comments.Any(c => c.Body.Contains(query))))
                .Union(db.Posts.Where(p => p.Comments.Any(c => c.Author.FirstName.Contains(query))))
                .Union(db.Posts.Where(p => p.Comments.Any(c => c.Author.DisplayName.Contains(query))));
            } 

            int pageSize = 3; // the number of posts shown per page
            int pageNumber = (page ?? 1); // if there's no post set the default page as page 1

            var qpost = result.OrderByDescending(p=>p.Created).ToPagedList(pageNumber, pageSize);
            return View(qpost);
        }

        public ActionResult blogSingle()
        {
            return View(db.Posts.ToList());
        }

        public ActionResult blogDetails(int? id)
        {
            var DetailedPost = db.Posts.Find(id);
            return View(DetailedPost);
        }

        // GET: BlogPosts/Details/5
        public ActionResult Details(string slug, string query)
        {
            //Search
            var result = db.Posts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query))
            {
                result = db.Posts.Where(p => p.Body.Contains(query))
                .Union(db.Posts.Where(p => p.Title.Contains(query)))
                .Union(db.Posts.Where(p => p.Comments.Any(c => c.Body.Contains(query))))
                .Union(db.Posts.Where(p => p.Comments.Any(c => c.Author.FirstName.Contains(query))))
                .Union(db.Posts.Where(p => p.Comments.Any(c => c.Author.DisplayName.Contains(query))));
            }

            if (String.IsNullOrWhiteSpace(slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.FirstOrDefault(p => p.Slug == slug);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Body,MediaURL")] BlogPost blogPost, HttpPostedFileBase Image)
        {
            if (ModelState.IsValid)
            {
                //Add image
                if (validator.IsWebFriendlyImage(Image))
                {
                    var filename = Path.GetFileName(Image.FileName);
                    Image.SaveAs(Path.Combine(Server.MapPath("~/img/uploads/"), filename));
                    blogPost.MediaURL = "~/img/uploads/" + filename;
                }

                var Slug = StringUtilities.URLFriendly(blogPost.Title);
                if(String.IsNullOrWhiteSpace(Slug))
                {
                    ModelState.AddModelError("Title", "Invalid Title");
                    return View(blogPost);
                }
                if(db.Posts.Any(p=>p.Slug==Slug))
                {
                    ModelState.AddModelError("Title", "The Title must be unique.");
                    return View(blogPost);

                }
                blogPost.Created = DateTime.Now;
                blogPost.Slug = Slug;
                db.Posts.Add(blogPost);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        
        [Authorize(Roles = "Moderator,Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Title,Slug,Body,MediaURL")] BlogPost blogPost, HttpPostedFileBase Image)
        {
            if (ModelState.IsValid)
            {
                var Slug = StringUtilities.URLFriendly(blogPost.Title);
                if (string.IsNullOrWhiteSpace(Slug))
                {
                    ModelState.AddModelError("Title", "Invalid title");
                    return View("blogPost");
                }


                if (Image !=null) {

                    if (validator.IsWebFriendlyImage(Image))
                {
                    var filename = Path.GetFileName(Image.FileName);
                    Image.SaveAs(Path.Combine(Server.MapPath("~/img/uploads/"), filename));
                    blogPost.MediaURL = "~/img/uploads/" + filename;
                }
                }


                //if (blogPost.Slug != Slug)
                //{
                //    if (db.Posts.Any(p => p.Slug == Slug))
                //    {
                //        ModelState.AddModelError("Title", "The title must be unique");
                //        return View(blogPost);
                //    }
                //    blogPost.Slug = Slug;

                //}

                blogPost.Updated = DateTime.Now;
                // for modifying the properties IsModified = True for change and False for no change option 
                //db.Entry(blogPost).Property("MediaURL").IsModified = true;
                db.Entry(blogPost).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Moderator,Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BlogPost blogPost = db.Posts.Find(id);
            db.Posts.Remove(blogPost);
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
