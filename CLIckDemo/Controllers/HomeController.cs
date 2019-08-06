using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CLickDemo.DAL;
using CLickDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Data.Entity.Validation;

/*
 * The Home Controller runs the home page and hangles logic for
 * interacting with College and FavCollege objects as well as interacting with database.
 * @author Xiaoxin Kern
 */

namespace CLickDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserDbContext dbContext;

        //inject Database context in the constructor
        public HomeController(UserDbContext context)
        {
            dbContext = context;
        }

        //Get Home Page
        public IActionResult Index()
        {
            return View("Form");
        }

        //Search for colleges based on user input
        [HttpPost]
        public async Task<ActionResult> SearchAsync()
        {
            //obtain values for fields from search from
            string universityName = Request.Form["Name"];
            string universityCity = Request.Form["City"];
            string universityState = Request.Form["State"];
            string searchTerms = "";
           
            //concatenate search parameters
            if(universityName != null && universityName.Length > 0)
            {
                searchTerms += "school.name=" + universityName.Replace(" ", "%20") + "&";
            }
            if(universityCity != null && universityCity.Length >0)
            {
                searchTerms += "school.city=" + universityCity + "&";

            }
            if(universityState != null && universityState.Length > 0)
            {
                searchTerms += "school.state=" + universityState + "&";
            }
            string responseString = await GetHttpResponseAsync(searchTerms);
            List<College> colleges = ConvertHttpResponseToObjects(responseString);
           
            foreach(College college in colleges)
            {
                if(!dbContext.Colleges.Contains(college))
                {
                    dbContext.Colleges.Add(college);

                }
                
            }
            dbContext.SaveChanges();

            return View("Display", colleges);
        }


        //receive response string resulting from an api call to the server
        [NonAction]
        public async Task<string> GetHttpResponseAsync(string searchTerms)
        {   
            string responseString = await SendAPICall(searchTerms);
            return responseString;
        }

        //send an API request to the college score card server
        //to get information about queried colleges
        [NonAction]
        public async Task<String> SendAPICall(string searchTerms)
        {
            string domainString = "https://api.data.gov/ed/collegescorecard/v1/schools.json?";
            string fieldsRequested = "_fields=id,school.name,school.city,school.state,school.zip,school.school_url,latest.admissions.admission_rate.overall,latest.cost.tuition.in_state,latest.cost.tuition.out_of_state,latest.admissions.sat_scores.average.overall&";
            string API_KEY = "api_key=2eam7HE8mzboXqjcYzxBs2Bs0RYPRTV6ODlRrst7";
            string urlLink = domainString + searchTerms + fieldsRequested + API_KEY;
            string result = null;
            using (var httpClient = new HttpClient())
            {
                 result = await httpClient.GetStringAsync(urlLink);
            }
            return result;
        }
        
        //deserialize the response string sent by the server
        [NonAction]
        public List<College> ConvertHttpResponseToObjects(string response)
        {

            JObject jsonResponse = JObject.Parse(response);
            JArray jsonArray = (JArray)jsonResponse["results"];
            string resultString = jsonArray.ToString();
            List<College> colleges = JsonConvert.DeserializeObject<List<College>>(resultString);
            return colleges;
            
        }
        
        //Get details about a college and display them to User
        public ActionResult Details(int? id)
        {
            College college = dbContext.Colleges.Find(id); 
            if(college != null)
            {
                return View(college);
            }
           else
            {
                return View("No details found");
            }
            
         
        }

        //Get: Save page to confirm if the user wants to user a college
        [HttpGet]
        [Authorize]
        public ActionResult Save(int id)
        {
            
            College collegeToSave = dbContext.Colleges.Find(id);
            return View(collegeToSave);
        }

        //save a college to database upon clicking the save button
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult SaveConfirm(int id)
        {
            College college = dbContext.Colleges.Find(id);
            string userName = User.Identity.Name;

            if (college != null)
            {
                FavCollege favCollege = new FavCollege()
                {
                    Id = college.Id,
                    Name = college.Name,
                    City = college.City,
                    State = college.State,
                    ZipCode = college.ZipCode,
                    Website = college.Website,
                    AdmissionRate = college.AdmissionRate,
                    Tuition_In_State = college.Tuition_In_State,
                    Tuition_Out_State = college.Tuition_Out_State,
                    AverageSAT = college.AverageSAT,
                    UserName = userName
                };

                IEnumerable<FavCollege> listOfCollegesWithTheSameId = (from c in dbContext.FavColleges
                                                                       where c.Id == favCollege.Id && c.UserName.Equals(userName)
                                                                       select c).ToList();
                if(listOfCollegesWithTheSameId == null || listOfCollegesWithTheSameId.Count() == 0)
                {
                    dbContext.FavColleges.Add(favCollege);
                    dbContext.SaveChanges();
                }
      
            }
            

            IEnumerable<FavCollege> favCollegesForDisplay = (from savedCollege in dbContext.FavColleges
                                                            where String.Equals(savedCollege.UserName, userName)
                                                            select savedCollege).ToList();

            return View("ManageFavColleges", favCollegesForDisplay);

            
           
        }

        //obtain saved favorite colleges for a logged-in user
        //and display the list to the user
        [Authorize]
        public ActionResult ManageFavColleges()
        {
            string userName = User.Identity.Name;

            IEnumerable<FavCollege> favCollegesForDisplay = (from savedCollege in dbContext.FavColleges
                                                            where String.Equals(savedCollege.UserName, userName)
                                                            select savedCollege).ToList();
            return View(favCollegesForDisplay);
           
        }
        
        //Get Delete page to ask if the user wants to confirm deletion
        [HttpGet]
        [Authorize]
        public ActionResult Delete(int?id, bool?saveChangesError=false)
        {
            if (id == null)
            {
                return BadRequest();
            }
            if(saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed.Try again, and if the problem persists see your system administrator.";

            }
            string userName = User.Identity.Name;
            FavCollege favCollegeToDelete = dbContext.FavColleges.FirstOrDefault(c => c.Id == id && c.UserName.Equals(userName));
           
            if(favCollegeToDelete == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            return View(favCollegeToDelete);

        }

        //Post: Delete a selected college from Table FavColleges
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            string userName = User.Identity.Name;
            try
            {
                FavCollege favCollegeToRemove = dbContext.FavColleges.Single(c => c.Id == id && c.UserName.Equals(userName));
                dbContext.FavColleges.Remove(favCollegeToRemove);
                dbContext.SaveChanges();
            }
            catch(InvalidOperationException exception) //need to log this exception
            {
                return RedirectToAction(nameof(HomeController.Delete), "Home", new { id = id, saveChangesError = true } );
            }
           
            IEnumerable<FavCollege> favColleges = (from c in dbContext.FavColleges
                                                   where c.UserName.Equals(userName)
                                                   select c).ToList();
            return View("ManageFavColleges", favColleges);
        }

    }

   
}