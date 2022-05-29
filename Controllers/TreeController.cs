using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TreeTest_MVC_Core.Models;

namespace TreeTest_MVC_Core.Controllers
{
    public class TreeController : Controller
    {
        private readonly ILogger<TreeController> _logger;

        public TreeController(ILogger<TreeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetNodesJsTree(int storage = 0)
        {

            List<TreeViewData> treeviewdataList = new List<TreeViewData>();

            //using (IDbConnection db = new SqlConnection("Server=.\\SQLEXPRESS;Database=Phisicon;Trusted_Connection=True;TrustServerCertificate=True"))
            using (IDbConnection db = new SqlConnection("Server=(localdb)\\mssqllocaldb;Database=Phisicon;Trusted_Connection=True;"))
            {
                treeviewdataList
                   .AddRange(
                      await db.QueryAsync<TreeViewData>(@"
                                select  
                                    Id, 
                                    Title as text, 
                                    '#' as parent 
                                from 
                                    Courses 
                                where 
                                    Grade = case when @Grade = 0 then Grade else @Grade end
                                  and 
                                    Subject = case when @Subject = '' then Subject else @Subject end
                                  and 
                                    Genre = case when @Genre = '' then Genre else @Genre end
                                order by Title",
                                new { Grade = 0, Subject = "География", Genre = "" }
                      )
                   );

                treeviewdataList
                    .AddRange(
                        await db.QueryAsync<TreeViewData>(@"
                                select 
	                                m.Id, 
                                    Num + ' ' + m.Title as text,
                                    case
                                    when isNull(ParentId, 0) = 0
                                    then cast(CourseId as nvarchar)
                                    else cast(ParentId as nvarchar)
                                    end as parent 
                                from 
                                    Modules m
                                join 
                                    Courses c on m.CourseId = c.Id
                                where 
                                    Grade = case when @Grade = 0 then Grade else @Grade end
                                  and 
                                    Subject = case when @Subject = '' then Subject else @Subject end
                                  and 
                                    Genre = case when @Genre = '' then Genre else @Genre end
                                order by [Order]",
                                new { Grade = 0, Subject = "География", Genre = "" }
                        )
                    );
            }

            return Json(treeviewdataList);

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

