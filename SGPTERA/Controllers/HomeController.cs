using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGPTERA.Models;
using System.Diagnostics;
using TeracromDatabase;

namespace SGPTERA.Controllers
{
    
    public class HomeController : AuthController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DapperContext _dapperContext;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, DapperContext dapperContext) : base(configuration, httpContextAccessor, dapperContext)
        {
            _logger = logger;
            _dapperContext = dapperContext;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
