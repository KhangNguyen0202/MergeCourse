﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrainStormEra.Views;
using BrainStormEra.Views.Chapter;
using BrainStormEra.Models;


namespace BrainStormEra.Controllers.Chapter
{
    public class ChapterController : Controller
    {

        private readonly SwpMainFpContext _context; // Define the context as a private field

        public ChapterController(SwpMainFpContext context)
        {
            _context = context; // Properly assign the injected context to the private field
        }












        public IActionResult EditChapter()
        {
            var chapterId = HttpContext.Request.Cookies["ChapterId"];

            var chapter = _context.Chapters
                .Where(ch => ch.ChapterId == chapterId)
                .OrderBy(ch => ch.ChapterOrder) // Sắp xếp theo ChapterOrder
                .Select(ch => new BrainStormEra.Models.Chapter
                {
                    ChapterId = ch.ChapterId,
                    CourseId = ch.CourseId,

                    ChapterName = ch.ChapterName,
                    ChapterDescription = ch.ChapterDescription,
                    ChapterOrder = ch.ChapterOrder,
                    ChapterStatus = ch.ChapterStatus,
                    ChapterCreatedAt = ch.ChapterCreatedAt
                })
                .FirstOrDefault();

            return View(chapter);
        }


        [HttpPost]
        public IActionResult EditChapter(BrainStormEra.Models.Chapter chapter)
        {
            var existingChapter = _context.Chapters.Find(chapter.ChapterId);


            existingChapter.ChapterName = chapter.ChapterName;
            existingChapter.ChapterDescription = chapter.ChapterDescription;
            _context.SaveChanges();

            return RedirectToAction("ViewChapters");
        }


        [HttpGet]
        public IActionResult ViewChapters()
        {
            var courseId = HttpContext.Request.Cookies["CourseId"];

            var chapters = _context.Chapters
                .Where(ch => ch.CourseId == courseId)
                .OrderBy(ch => ch.ChapterOrder) // Sắp xếp theo ChapterOrder
                .Select(ch => new BrainStormEra.Models.Chapter
                {
                    ChapterId = ch.ChapterId,
                    CourseId = ch.CourseId,
                    ChapterName = ch.ChapterName,
                    ChapterDescription = ch.ChapterDescription,
                    ChapterOrder = ch.ChapterOrder,
                    ChapterStatus = ch.ChapterStatus,
                    ChapterCreatedAt = ch.ChapterCreatedAt
                })
                .ToList();

            return View(chapters);
        }



        [HttpGet]
        public async Task<IActionResult> CreateChapter(string courseId)
        {

            var existingChapters = await _context.Chapters
                .Where(ch => ch.CourseId == courseId)
                .ToListAsync();
            return View(existingChapters);
        }





        // GET DELETE
        [HttpGet]
        public IActionResult DeleteChapter()
        {
            var courseId = HttpContext.Request.Cookies["CourseId"];


            var chapters = _context.Chapters
                .Where(ch => ch.CourseId == courseId)
                .OrderBy(ch => ch.ChapterOrder) // Sắp xếp theo ChapterOrder
                .Select(ch => new BrainStormEra.Models.Chapter
                {
                    ChapterId = ch.ChapterId,
                    CourseId = ch.CourseId,
                    ChapterName = ch.ChapterName,
                    ChapterDescription = ch.ChapterDescription,
                    ChapterOrder = ch.ChapterOrder,
                    ChapterStatus = ch.ChapterStatus,
                    ChapterCreatedAt = ch.ChapterCreatedAt
                })
                .ToList();

            return View(chapters);
        }

        [HttpPost]
        public IActionResult DeleteChapter(List<string> ChapterIds)
        {
            var chaptersToDelete = _context.Chapters.Where(ch => ChapterIds.Contains(ch.ChapterId)).ToList();
            var courseId = chaptersToDelete.FirstOrDefault()?.CourseId;
            // Xóa các chương
            _context.Chapters.RemoveRange(chaptersToDelete);
            _context.SaveChanges();
            return RedirectToAction("DeleteChapter");
        }






        //ADD chapter

        [HttpGet]
        public IActionResult ChapterManagement()
        {
            return View();
        }



        // POST: ChapterManagement
        [HttpPost]
        public IActionResult ChapterManagement(BrainStormEra.Models.Chapter chapter)
        {
            var courseId = HttpContext.Request.Cookies["CourseId"];



            // Kiểm tra trùng tên chương trong cùng khóa học
            var existingChapter = _context.Chapters
                .Where(c => c.CourseId == courseId && c.ChapterName == chapter.ChapterName)
                .FirstOrDefault();

            if (existingChapter != null)
            {
                ModelState.AddModelError("ChapterName", "Chapter name already exists in this course. Please choose a different name.");
                return View(chapter); // Trả về view với thông báo lỗi
            }

            // Lấy chapter cuối cùng của khóa học dựa trên CourseId, sắp xếp theo ChapterOrder
            var lastChapterOrder = _context.Chapters
        .Where(c => c.CourseId == courseId)
        .OrderByDescending(c => c.ChapterOrder)
        .Select(c => c.ChapterOrder)
        .FirstOrDefault();


            var newChapterOrder = (lastChapterOrder == 0 || lastChapterOrder == null) ? 1 : lastChapterOrder + 1;

            // Lấy ChapterId cuối cùng trong cơ sở dữ liệu
            var lastChapter = _context.Chapters.OrderByDescending(c => c.ChapterId).FirstOrDefault();
            // Tạo ChapterId mới dựa trên ChapterId cuối cùng
            var newChapterId = lastChapter == null ? "CH001" : "CH" + (int.Parse(lastChapter.ChapterId.Substring(2)) + 1).ToString("D3");

            // chapter order 
            // Tự động tăng ChapterOrder
            var newChapter = new BrainStormEra.Models.Chapter

            {
                ChapterId = newChapterId,
                ChapterName = chapter.ChapterName,
                ChapterDescription = chapter.ChapterDescription,
                ChapterOrder = newChapterOrder,
                CourseId = courseId,
                ChapterStatus = 0
            };



            _context.Chapters.Add(newChapter);
            _context.SaveChanges();

            return RedirectToAction("ViewChapters");
        }



    }
}
