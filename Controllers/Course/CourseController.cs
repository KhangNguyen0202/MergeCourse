using BrainStormEra.Models;
using BrainStormEra.Views.Course;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrainStormEra.Controllers.Course
{
    public class CourseController : Controller
    {


        private readonly SwpMainFpContext _context;
        private readonly ILogger<CourseController> _logger;
        public CourseController(SwpMainFpContext context, ILogger<CourseController> logger)
        {
            _context = context;
            _logger = logger;
        }



        public ActionResult Details(int id)
        {
            return View();
        }




        public ActionResult CreateCourse()
        {
            var viewmodel = new CreateCourseViewModel
            {
                CourseCategories = _context.CourseCategories.ToList()
            };

            return View(viewmodel);
        }




        [HttpPost]
        public ActionResult CreateCourse(CreateCourseViewModel viewmodel)
        {
            var userId = Request.Cookies["user_id"];
            var lastCourse = _context.Courses.OrderByDescending(c => c.CourseId).FirstOrDefault();
            var newCourseId = lastCourse == null ? "CO001" : "CO" + (int.Parse(lastCourse.CourseId.Substring(2)) + 1).ToString("D3");

            // CheckDuplicate() namecourse
            var existingCourse = _context.Courses.FirstOrDefault(c => c.CourseName == viewmodel.CourseName && c.CourseId != viewmodel.CourseId);
            if (existingCourse != null)
            {
                ModelState.AddModelError("CourseName", "The Course Name already exists. Please enter a different name.");
                viewmodel.CourseCategories = _context.CourseCategories.ToList();
                return View(viewmodel);
            }
            var obj = new Models.Course
            {
                CourseId = newCourseId,
                CourseName = viewmodel.CourseName,
                CourseDescription = viewmodel.CourseDescription,
                CourseStatus = 1,
                CreatedBy = userId,
                CoursePicture = viewmodel.CoursePicture.FileName,
                Price = viewmodel.Price,
            };

            var selectedCategory = _context.CourseCategories.FirstOrDefault(c => c.CourseCategoryId == viewmodel.CourseCategoryId);

            if (selectedCategory != null)
            {
                // Gán danh mục cho khóa học
                obj.CourseCategories = new List<CourseCategory> { selectedCategory };
            }

            _context.Courses.Add(obj);
            _context.SaveChanges();
            viewmodel.CourseCategories = _context.CourseCategories.ToList();
            return RedirectToAction("ManagementCourse");

        }








        // EditCourse
        [HttpGet]
        public ActionResult EditCourse()
        {
            var courseId = HttpContext.Request.Cookies["CourseId"];

            var course = _context.Courses
                    .Include(c => c.CourseCategories)
                    .FirstOrDefault(c => c.CourseId == courseId);

            if (course == null)
            {
                return RedirectToAction("ManagementCourse");
            }
            var viewModel = new EditCourseViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseCategories = _context.CourseCategories.ToList(),
                CourseCategoryId = course.CourseCategories.FirstOrDefault()?.CourseCategoryId, // Chọn Category hiện tại nếu có
                CourseDescription = course.CourseDescription,
                CoursePictureFile = course.CoursePicture,
                Price = course.Price
            };
            return View(viewModel);
        }
        //

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCourse(EditCourseViewModel viewmodel)
        {
            if (ModelState.IsValid)
            {
                var course = _context.Courses
                    .Include(c => c.CourseCategories)
                    .FirstOrDefault(c => c.CourseId == viewmodel.CourseId);

                if (course == null)
                {
                    return RedirectToAction("ManagementCourse");
                }
                var existingCourse = _context.Courses.FirstOrDefault(c => c.CourseName == viewmodel.CourseName);
                if (existingCourse != null)
                {
                    ModelState.AddModelError("CourseName", "The Course Name already exists. Please enter a different name.");
                    viewmodel.CourseCategories = _context.CourseCategories.ToList();
                }
                course.CourseName = viewmodel.CourseName;
                course.CourseDescription = viewmodel.CourseDescription;
                course.Price = viewmodel.Price;


                // Kiểm tra xem người dùng có chọn thay đổi ảnh không
                if (viewmodel.CoursePicture != null && viewmodel.CoursePicture.Length > 0)
                {
                    // Người dùng chọn ảnh mới, cập nhật đường dẫn ảnh
                    course.CoursePicture = Path.GetFileName(viewmodel.CoursePicture.FileName);
                }
                else
                {
                    // Nếu người dùng không chọn ảnh mới, giữ lại ảnh hiện tại
                    course.CoursePicture = course.CoursePicture;
                }
                course.CourseCategories.Clear();
                var selectedCategory = _context.CourseCategories.FirstOrDefault(c => c.CourseCategoryId == viewmodel.CourseCategoryId);
                if (selectedCategory != null)
                {
                    course.CourseCategories.Add(selectedCategory);
                }
                _context.SaveChanges();
                return RedirectToAction("ManagementCourse");
            }

            viewmodel.CourseCategories = _context.CourseCategories.ToList();
            return View(viewmodel);
        }







        // GET: ManagementCourse
        public ActionResult ManagementCourse()
        {
            var userId = Request.Cookies["user_id"];
            var user_role = Request.Cookies["user_role"];


            switch (user_role)
            {
                // Role Instructor
                case "2":
                    var instructorCourses = _context.Courses
                        .Include(c => c.CourseCategories)
                        .Include(c => c.Enrollments)
                        .Include(c => c.CreatedByNavigation) // Include thông tin người tạo từ bảng Account
                        .Where(c => c.CourseStatus == 2 && c.CreatedBy == userId)  // Lọc khóa học có status là 2 và được tạo bởi người dùng hiện tại
                        .OrderByDescending(c => c.CourseCreatedAt)
                        .Select(course => new ManagementCourseViewModel
                        {
                            CourseId = course.CourseId,
                            CourseName = course.CourseName,
                            CourseDescription = course.CourseDescription,
                            CourseStatus = course.CourseStatus,
                            CoursePicture = course.CoursePicture,
                            Price = course.Price,
                            CourseCreatedAt = course.CourseCreatedAt,
                            CreatedBy = course.CreatedBy,  // Lấy thông tin người tạo
                            CourseCategories = course.CourseCategories.ToList(),
                            StarRating = (byte?)Math.Round(
                                _context.Feedbacks
                                .Where(f => f.CourseId == course.CourseId)
                                .Average(f => (double?)f.StarRating) ?? 0)
                        })
                        .ToList();

                    return View("ManagementCourse", instructorCourses);

                // Default case: Khi không khớp vai trò nào
                default:
                    var Course = _context.Courses
                        .Include(c => c.CourseCategories)
                        .Include(c => c.Enrollments)
                        .Where(c => c.CourseStatus == 2)  // Lọc khóa học có status là 2
                        .OrderByDescending(c => c.CourseCreatedAt)
                        .Select(course => new ManagementCourseViewModel
                        {
                            CourseId = course.CourseId,
                            CourseName = course.CourseName,
                            CourseDescription = course.CourseDescription,
                            CourseStatus = course.CourseStatus,
                            CoursePicture = course.CoursePicture,
                            Price = course.Price,
                            CourseCreatedAt = course.CourseCreatedAt,
                            CreatedBy = course.CreatedBy,  // Lấy thông tin người tạo
                            CourseCategories = course.CourseCategories.ToList(),
                            StarRating = (byte?)Math.Round(
                                _context.Feedbacks
                                .Where(f => f.CourseId == course.CourseId)
                                .Average(f => (double?)f.StarRating) ?? 0)
                        })
                        .ToList();

                    return View("ManagementCourse", Course);
            }
        }



        public ActionResult ConfirmDelete()
        {
            var courseId = HttpContext.Request.Cookies["CourseId"];

            var course = _context.Courses
                   .Include(c => c.CourseCategories)
                   .FirstOrDefault(c => c.CourseId == courseId);
            var viewModel = new DeleteCourseViewModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseCategories = _context.CourseCategories.ToList(),
                CourseCategoryId = course.CourseCategories.FirstOrDefault()?.CourseCategoryId, // Chọn Category hiện tại nếu có
                CourseDescription = course.CourseDescription,
                CoursePictureFile = course.CoursePicture,
                Price = course.Price
            };
            return View("DeleteCourse", viewModel);
        }


        [HttpPost]
        public ActionResult DeleteCourse()
        {

            var courseID = HttpContext.Request.Cookies["CourseId"];


            var course = _context.Courses.Find(courseID);

            if (course != null)
            {

                _context.Courses.Remove(course);
                _context.SaveChanges();


                var userRole = HttpContext.Request.Cookies["user_role"];


                if (userRole == "1")
                {
                    return RedirectToAction("CourseAcceptance", "Course");
                }
                else if (userRole == "2")
                {
                    return RedirectToAction("ManagementCourse");
                }

            }

            return RedirectToAction("ErrorPage", "Home");
        }



        [HttpGet]
        public IActionResult CourseDetail(int page = 1, int pageSize = 4)
        {
            try
            {
                var courseId = Request.Cookies["CourseId"];

                if (string.IsNullOrEmpty(courseId))
                {
                    _logger.LogWarning("Course ID is null or empty.");
                    return View("ErrorPage", "Course ID not found in cookies.");
                }

                // Lấy thông tin khóa học
                var course = _context.Courses
                             .Include(c => c.Chapters)
                             .ThenInclude(ch => ch.Lessons)
                             .FirstOrDefault(c => c.CourseId == courseId);

                if (course == null)
                {
                    _logger.LogError($"Course not found with ID: {courseId}");
                    return View("ErrorPage", "Course not found.");
                }

                // Tính toán số lượng học viên đã đăng ký (enrollments)
                var learnersCount = _context.Enrollments
                                    .Where(e => e.CourseId == courseId && e.Approved == true)
                                    .Count();
                ViewBag.LearnersCount = learnersCount;

                // Lấy danh sách feedback (comment và rating) theo phân trang
                var feedbacks = _context.Feedbacks
                                .Where(f => f.CourseId == courseId && f.HiddenStatus == false)
                                .Include(f => f.User)
                                .OrderByDescending(f => f.FeedbackDate) // Sắp xếp feedback mới nhất lên trên
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

                // Tính tổng số comment và rating trung bình
                var totalComments = _context.Feedbacks.Count(f => f.CourseId == courseId && f.HiddenStatus == false);
                var averageRating = totalComments > 0 ? _context.Feedbacks
                                                        .Where(f => f.CourseId == courseId && f.HiddenStatus == false)
                                                        .Average(f => f.StarRating) : 0;

                ViewBag.TotalComments = totalComments;
                ViewBag.AverageRating = averageRating;
                ViewBag.Comments = feedbacks;

                // Phân trang
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalComments / pageSize);

                // Lấy thông tin người tạo khóa học
                var createdBy = _context.Accounts.FirstOrDefault(a => a.UserId == course.CreatedBy);
                if (createdBy != null)
                {
                    ViewBag.CreatedBy = createdBy.FullName;
                }
                else
                {
                    ViewBag.CreatedBy = "Unknown";
                }

                // Tính phần trăm mỗi mức rating (1-5 sao)
                var ratingPercentages = new Dictionary<int, double>();

                for (int i = 1; i <= 5; i++)
                {
                    var count = feedbacks.Count(f => f.StarRating == i);
                    ratingPercentages[i] = feedbacks.Count > 0 ? (double)count / feedbacks.Count() : 0;
                }

                ViewBag.RatingPercentages = ratingPercentages;




                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the course.");
                return View("ErrorPage", "An unexpected error occurred.");
            }
        }



        [HttpGet]
        public ActionResult CourseAcceptance()
        {
            var pendingCourses = _context.Courses
                .Where(c => c.CourseStatus == 1 || c.CourseStatus == 2)
                .Include(c => c.CourseCategories)
                .OrderBy(c => c.CourseStatus)
                .Select(course => new ManagementCourseViewModel
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseDescription = course.CourseDescription,
                    CourseStatus = course.CourseStatus,
                    CoursePicture = course.CoursePicture,
                    Price = course.Price,
                    CourseCreatedAt = course.CourseCreatedAt,
                    CreatedBy = course.CreatedBy,
                    CourseCategories = course.CourseCategories.ToList(),
                    StarRating = (byte?)Math.Round(
                        _context.Feedbacks
                            .Where(f => f.CourseId == course.CourseId)
                            .Average(f => (double?)f.StarRating) ?? 0)
                })
                .ToList();

            return View("CourseAcceptance", pendingCourses);
        }






        [HttpGet]
        public IActionResult ReviewCourse(int page = 1, int pageSize = 4)
        {
            try
            {
                var courseId = Request.Cookies["CourseId"];

                if (string.IsNullOrEmpty(courseId))
                {
                    _logger.LogWarning("Course ID is null or empty.");
                    return View("ErrorPage", "Course ID not found in cookies.");
                }

                // Lấy thông tin khóa học
                var course = _context.Courses
                                     .Include(c => c.Chapters)
                                     .ThenInclude(ch => ch.Lessons)
                                     .FirstOrDefault(c => c.CourseId == courseId);

                if (course == null)
                {
                    _logger.LogError($"Course not found with ID: {courseId}");
                    return View("ErrorPage", "Course not found.");
                }

                // Tính toán số lượng học viên đã đăng ký (enrollments)
                var learnersCount = _context.Enrollments
                                            .Where(e => e.CourseId == courseId && e.Approved == true)
                                            .Count();
                ViewBag.LearnersCount = learnersCount;

                // Lấy danh sách feedback (comment và rating) theo phân trang
                var feedbacks = _context.Feedbacks
                                        .Where(f => f.CourseId == courseId && f.HiddenStatus == false)
                                        .Include(f => f.User)
                                        .OrderByDescending(f => f.FeedbackDate) // Sắp xếp feedback mới nhất lên trên
                                        .Skip((page - 1) * pageSize)
                                        .Take(pageSize)
                                        .ToList();

                // Tính tổng số comment và rating trung bình
                var totalComments = _context.Feedbacks.Count(f => f.CourseId == courseId && f.HiddenStatus == false);
                var averageRating = totalComments > 0 ? _context.Feedbacks
                                                        .Where(f => f.CourseId == courseId && f.HiddenStatus == false)
                                                        .Average(f => f.StarRating) : 0;

                ViewBag.TotalComments = totalComments;
                ViewBag.AverageRating = averageRating;
                ViewBag.Comments = feedbacks;

                // Phân trang
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalComments / pageSize);

                // Lấy thông tin người tạo khóa học
                var createdBy = _context.Accounts.FirstOrDefault(a => a.UserId == course.CreatedBy);
                if (createdBy != null)
                {
                    ViewBag.CreatedBy = createdBy.FullName;
                }
                else
                {
                    ViewBag.CreatedBy = "Unknown";
                }

                // Truyền CourseStatus vào ViewBag
                ViewBag.CourseStatus = course.CourseStatus;

                // Tính phần trăm mỗi mức rating (1-5 sao)
                var ratingPercentages = new Dictionary<int, double>();

                for (int i = 1; i <= 5; i++)
                {
                    var count = feedbacks.Count(f => f.StarRating == i);
                    ratingPercentages[i] = feedbacks.Count > 0 ? (double)count / feedbacks.Count() : 0;
                }

                ViewBag.RatingPercentages = ratingPercentages;

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the course.");
                return View("ErrorPage", "An unexpected error occurred.");
            }
        }


        [HttpPost]
        public IActionResult ChangeStatus(string courseId, int status)
        {
            var course = _context.Courses.FirstOrDefault(c => c.CourseId == courseId);
            if (course != null)
            {
                course.CourseStatus = status;
                _context.SaveChanges();
                // Redirect về trang trước hoặc trang danh sách mà không có ID trên URL
                return RedirectToAction("CourseAcceptance", "Course");
            }
            return View("Error");
        }




    }
}
