using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using System.Data;

namespace ToDoMVCApplication.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "(localdb)\\MSSQLLocalDB";
                builder.UserID = "";
                builder.Password = "";
                builder.InitialCatalog = "tododb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    Console.WriteLine("\nQuery data example:");
                    Console.WriteLine("=========================================\n");

                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * ");
                    sb.Append("FROM Users ");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return View();
        }

        public ActionResult SignUp(Models.User User)
        {
            try
            {

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "(localdb)\\MSSQLLocalDB";
                builder.UserID = "";
                builder.Password = "";
                builder.InitialCatalog = "tododb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    {
                        String sql = "Insert INTO Users (username,name,emailid,password) values (@username,@name,@emailId,@password)";

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            connection.Open();
                            command.Parameters.AddWithValue("@username", User.UserName);
                            command.Parameters.AddWithValue("@name", User.Name);
                            command.Parameters.AddWithValue("@emailId", User.EmailId);
                            command.Parameters.AddWithValue("@password", User.Password);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return Json(new { returnvalue = "true" });
        }
        public ActionResult SignIn(Models.User User)
        {
            /*try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "(localdb)\\MSSQLLocalDB";
                builder.UserID = "";
                builder.Password = "";
                builder.InitialCatalog = "tododb";

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    String sql = "Select * from Users WHERE username = @username AND password = @pwd";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        connection.Open();
                        command.Parameters.AddWithValue("@username", User.UserName);
                        command.Parameters.AddWithValue("@pwd", User.Password);
                        //SqlDataReader reader = command.ExecuteReader();

                        DataSet ds = new DataSet();
                        SqlDataAdapter da = new SqlDataAdapter(command);
                        da.Fill(ds);
                        connection.Close();

                        bool loginSuccessful = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
                        User.Name = ds.Tables[0].Rows[0]["Name"].ToString();
                        User.EmailId = ds.Tables[0].Rows[0]["EmailId"].ToString();
                        User.Id = Convert.ToInt32(ds.Tables[0].Rows[0]["Id"].ToString());
                        if (loginSuccessful)
                        {
                            return PartialView("ToDoMainPage", User);
                        }
                    
                    }
                }
            }
            
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            } */
            return PartialView("ToDoMainPage", User);
        }
    }
}