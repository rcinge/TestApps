using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using NFluent;

namespace ConsoleApp.NetCore2
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new BloggingContext())
            {
                bool initData = false;

                if (initData)
                {
                    var b1 = new Blog { Rating = 10, Url = "http://test.com" };
                    db.Blogs.Add(b1);
                    db.SaveChanges();

                    var p1 = new Post { Blog = b1, BlogId = b1.BlogId, Content = "p1", Title = "t1" };
                    var p2 = new Post { Blog = b1, BlogId = b1.BlogId, Content = "p2", Title = "t2" };
                    var p3 = new Post { Blog = b1, BlogId = b1.BlogId, Content = "p3", Title = "t3" };
                    db.Posts.Add(p1);
                    db.Posts.Add(p2);
                    db.Posts.Add(p3);
                    db.SaveChanges();

                    var pt1 = new PostTag { Post = p1, PostId = p1.PostId, Tag = "foo" };
                    var pt2 = new PostTag { Post = p1, PostId = p1.PostId, Tag = "bar" };
                    var pt3 = new PostTag { Post = p1, PostId = p1.PostId, Tag = "baz" };
                    var pt4 = new PostTag { Post = p2, PostId = p2.PostId, Tag = "quux" };
                    db.PostTags.Add(pt1);
                    db.PostTags.Add(pt2);
                    db.PostTags.Add(pt3);
                    db.PostTags.Add(pt4);
                    db.SaveChanges();
                }

                //
                // Original test from StefH: https://github.com/StefH/System.Linq.Dynamic.Core.TestApps/blob/master/ConsoleApp.NetCore2/Program.cs
                //

                var postList = db.Posts.ToList();

                var realQuery = db.Blogs.GroupJoin(
                    postList,
                    blog => blog.BlogId,
                    post => post.BlogId,
                    (blg, pst) => new { Name = blg.Url, NumberOfPosts = pst.Count() });

                var dynamicQuery = db.Blogs.AsQueryable().GroupJoin(
                    postList,
                    "BlogId",
                    "BlogId",
                    "new(outer.Url as Name, inner.Count() as NumberOfPosts)");

                // Assert
                var realResult = realQuery.ToArray();
                Check.That(realResult).IsNotNull();
                Console.WriteLine(JsonConvert.SerializeObject(realResult));

                var dynamicResult = dynamicQuery.ToDynamicArray();
                Check.That(dynamicResult).IsNotNull();
                Console.WriteLine(JsonConvert.SerializeObject(dynamicResult));

                Console.WriteLine();

                //
                // Real query 2a: left outer join to IEnumerable<PostTag> result
                //

                Console.WriteLine("Real Query 2a");

                var query2a = db.Blogs
                             .Join(db.Posts, b => b.BlogId, p => p.BlogId, (b, p) => new { b, p })
                             .GroupJoin(db.PostTags, bp => bp.p.PostId, pt => pt.PostId, (bp, pt_g) => new { bp, pt_g })
                             .Select(m => new {
                                 m.bp.b,
                                 m.bp.p,
                                 m.pt_g
                             });

                foreach (var result in query2a)
                {
                    Blog b = result.b;
                    Post p = result.p;
                    IEnumerable<PostTag> pt_g = result.pt_g;

                    Console.WriteLine($"{b.BlogId} {b.Url} {p.PostId} {p.Title} {p.Content}");
                    foreach (var pt in pt_g)
                    {
                        Console.WriteLine($"    {pt.PostTagId} {pt.Tag}");
                    }
                }

                Console.WriteLine();

                //
                // Real query 2b: left outer join to flattened .SelectMany() result
                //

                Console.WriteLine("Real Query 2b");

                var query2b = db.Blogs
                    .Join(db.Posts, b => b.BlogId, p => p.BlogId, (b, p) => new { b, p })
                    .GroupJoin(db.PostTags, bp => bp.p.PostId, pt => pt.PostId, (bp, pt_g) => new { bp, pt_g })
                    .SelectMany(m => m.pt_g.DefaultIfEmpty(), (pj, pt) => new { pj.bp, pt });

                foreach (var result in query2b)
                {
                    Blog b = result.bp.b;
                    Post p = result.bp.p;
                    PostTag pt = result.pt;

                    Console.WriteLine($"{b.BlogId} {b.Url} {p.PostId} {p.Title} {p.Content} {pt?.PostTagId} {pt?.Tag}");
                }

                Console.WriteLine();

                //
                // Dynamic Query 3a: left outer join to IEnumerable<PostTag> result - All ToList()
                //

                Console.WriteLine("Dynamic GroupJoin() Query 3a - All ToList()");

                var dynQuery3a_AllList = db.Blogs
                    .Join(db.Posts.ToList(), "BlogId", "BlogId", "new(outer as b, inner as p)", null)
                    .GroupJoin(db.PostTags.ToList(), "p.PostId", "PostId", "new(outer as bp, inner as pt_g)", null)
                    .Select("it");

                foreach (dynamic result in dynQuery3a_AllList)
                {
                    Blog b = (Blog)result.bp.b;
                    Post p = (Post)result.bp.p;
                    IEnumerable<PostTag> pt_g = (IEnumerable<PostTag>)result.pt_g;

                    Console.WriteLine($"{b.BlogId} {b.Url} {p.PostId} {p.Title} {p.Content}");
                    foreach (var pt in pt_g)
                    {
                        Console.WriteLine($"    {pt.PostTagId} {pt.Tag}");
                    }
                }

                Console.WriteLine();

                //
                // Dynamic Query 3a: left outer join to IEnumerable<PostTag> result - Join EF, GroupJoin ToList()
                //

                Console.WriteLine("Dynamic GroupJoin() Query 3a - Join EF, GroupJoin ToList()");

                var dynQuery3a_EFList = db.Blogs
                    .Join(db.Posts, "BlogId", "BlogId", "new(outer as b, inner as p)", null)
                    .GroupJoin(db.PostTags.ToList(), "p.PostId", "PostId", "new(outer as bp, inner as pt_g)", null)
                    .Select("it");

                foreach (dynamic result in dynQuery3a_EFList)
                {
                    Blog b = (Blog)result.bp.b;
                    Post p = (Post)result.bp.p;
                    IEnumerable<PostTag> pt_g = (IEnumerable<PostTag>)result.pt_g;

                    Console.WriteLine($"{b.BlogId} {b.Url} {p.PostId} {p.Title} {p.Content}");
                    foreach (var pt in pt_g)
                    {
                        Console.WriteLine($"    {pt.PostTagId} {pt.Tag}");
                    }
                }

                Console.WriteLine();

                //
                // Dynamic Query 3a: left outer join to IEnumerable<PostTag> result - All EF
                //

                Console.WriteLine("Dynamic GroupJoin() Query 3a - All EF");

                var dynQuery3a_AllEF = db.Blogs
                    .Join(db.Posts, "BlogId", "BlogId", "new(outer as b, inner as p)", null)
                    .GroupJoin(db.PostTags, "p.PostId", "PostId", "new(outer as bp, inner as pt_g)", null)
                    .Select("it");

                foreach (dynamic result in dynQuery3a_AllEF)
                {
                    Blog b = (Blog)result.bp.b;
                    Post p = (Post)result.bp.p;
                    IEnumerable<PostTag> pt_g = (IEnumerable<PostTag>)result.pt_g;

                    Console.WriteLine($"{b.BlogId} {b.Url} {p.PostId} {p.Title} {p.Content}");
                    foreach (var pt in pt_g)
                    {
                        Console.WriteLine($"    {pt.PostTagId} {pt.Tag}");
                    }
                }

                Console.WriteLine();

                //
                // Dynamic Query 3b: left outer join to flattened .SelectMany() result - All ToList()
                //

                Console.WriteLine("Dynamic SelectMany() Query 3b - All ToList()");

                var dynQuery3b_AllList = db.Blogs
                    .Join(db.Posts.ToList(), "BlogId", "BlogId", "new(outer as b, inner as p)", null)
                    .GroupJoin(db.PostTags.ToList(), "p.PostId", "PostId", "new(outer as bp, inner as pt_g)", null)
                    .SelectMany("pt_g.DefaultIfEmpty()", "new(pj.bp as bp, pt as pt)", "pj", "pt", null);

                foreach (dynamic result in dynQuery3b_AllList)
                {
                    Blog b = (Blog)result.bp.b;
                    Post p = (Post)result.bp.p;
                    PostTag pt = (PostTag)result.pt;

                    Console.WriteLine($"{b.BlogId} {b.Url} {p.PostId} {p.Title} {p.Content} {pt?.PostTagId} {pt?.Tag}");
                }

                Console.WriteLine();

                //
                // Dynamic Query 3b: left outer join to flattened .SelectMany() result - Join EF, GroupJoin ToList()
                //

                Console.WriteLine("Dynamic SelectMany() Query 3b - Join EF, GroupJoin ToList()");

                var dynQuery3b_EFList = db.Blogs
                    .Join(db.Posts, "BlogId", "BlogId", "new(outer as b, inner as p)", null)
                    .GroupJoin(db.PostTags.ToList(), "p.PostId", "PostId", "new(outer as bp, inner as pt_g)", null)
                    .SelectMany("pt_g.DefaultIfEmpty()", "new(pj.bp as bp, pt as pt)", "pj", "pt", null);

                foreach (dynamic result in dynQuery3b_EFList)
                {
                    Blog b = (Blog)result.bp.b;
                    Post p = (Post)result.bp.p;
                    PostTag pt = (PostTag)result.pt;

                    Console.WriteLine($"{b.BlogId} {b.Url} {p.PostId} {p.Title} {p.Content} {pt?.PostTagId} {pt?.Tag}");
                }

                Console.WriteLine();

                //
                // Dynamic Query 3b: left outer join to flattened .SelectMany() result - All EF
                //

                Console.WriteLine("Dynamic SelectMany() Query 3b - All EF");

                var dynQuery3b_AllEF = db.Blogs
                    .Join(db.Posts, "BlogId", "BlogId", "new(outer as b, inner as p)", null)
                    .GroupJoin(db.PostTags, "p.PostId", "PostId", "new(outer as bp, inner as pt_g)", null)
                    .SelectMany("pt_g.DefaultIfEmpty()", "new(pj.bp as bp, pt as pt)", "pj", "pt", null);

                foreach (dynamic result in dynQuery3b_AllEF)
                {
                    Blog b = (Blog)result.bp.b;
                    Post p = (Post)result.bp.p;
                    PostTag pt = (PostTag)result.pt;

                    Console.WriteLine($"{b.BlogId} {b.Url} {p.PostId} {p.Title} {p.Content} {pt?.PostTagId} {pt?.Tag}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}