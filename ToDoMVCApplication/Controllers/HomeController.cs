using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Text;
using System.Configuration;
using System.Data;
using ToDoMVCApplication.Models;
using System.Data.SQLite;

namespace ToDoMVCApplication.Controllers
{
    public class HomeController : Controller
    {
        public static SQLiteConnection sqlite_conn;
        public ActionResult Index()
        {
            sqlite_conn = new SQLiteConnection("Data Source=tododatabase.sqlite");
            if (!System.IO.File.Exists("./tododatabase.sqlite"))
            {
                SQLiteConnection.CreateFile("./tododatabase.sqlite");
            }
            CreateTable(sqlite_conn);
            ReadData(sqlite_conn);
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
            try
            {

                sqlite_conn.Open();
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();
                String sql = "Select * from Users WHERE username = @username AND password = @pwd";

                DataSet ds = new DataSet();
                sqlite_cmd.CommandText = sql;
                sqlite_cmd.Parameters.AddWithValue("@username", User.UserName);
                sqlite_cmd.Parameters.AddWithValue("@pwd", User.Password);
                SQLiteDataAdapter da = new SQLiteDataAdapter(sqlite_cmd);

                da.Fill(ds);
                sqlite_conn.Close();

                bool loginSuccessful = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
                User.Name = ds.Tables[0].Rows[0]["Name"].ToString();
                User.EmailId = ds.Tables[0].Rows[0]["EmailId"].ToString();
                User.Id = Convert.ToInt32(ds.Tables[0].Rows[0]["Id"].ToString());
                if (loginSuccessful)
                {
                    return PartialView("ToDoMainPage", User);
                }
            }
            
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            } 
            return PartialView("Exit", User);
        }

        public ActionResult AddUpdateTask(Task task, int UserId)
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
                    //Here we declare the parameter which we have to use in our application  
                    SqlCommand cmd = new SqlCommand();
                    SqlParameter sp1 = new SqlParameter();
                    SqlParameter sp2 = new SqlParameter();
                    SqlParameter sp3 = new SqlParameter();
                    SqlParameter sp4 = new SqlParameter();
                    {
                        cmd.Parameters.Add("@TaskId", SqlDbType.Int).Value = task.TaskId;
                        cmd.Parameters.Add("@TaskName", SqlDbType.VarChar).Value = task.TaskName;
                        cmd.Parameters.Add("@TaskDescription", SqlDbType.VarChar).Value = task.TaskDescription;
                        cmd.Parameters.Add("@Important", SqlDbType.Bit).Value = task.ImportantInd;
                        cmd.Parameters.Add("@DeleteInd", SqlDbType.Bit).Value = task.DeleteInd;
                        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = UserId;
                        cmd = new SqlCommand("AddUpdateTask", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();

                    }
                }
            }

            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            } 
            return PartialView("ToDoMainPage", User);
        }
        static void CreateTable(SQLiteConnection conn)
        {
            conn.Open();
            SQLiteCommand sqlite_cmd;
            try
            {
                string CreatetableTask = "CREATE Table [Task] ([Id] INTEGER NOT NULL, [name] TEXT NOT NULL, [description] TEXT NULL , [createddatetime] NUMERIC NOT NULL , [modifieddatetime] NUMERIC NOT NULL , [deleteInd] NUMERIC NOT NULL , [importantInd] NUMERIC NOT NULL , [userId] INTEGER NOT NULL, CONSTRAINT[PK_Task] PRIMARY KEY([Id]))";
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = CreatetableTask;
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            try {
                string CreatetableUser = "CREATE TABLE IF NOT EXISTS [Users] ( [Id]       INT IDENTITY(1, 1) NOT NULL, [username] NCHAR(100) NOT NULL,[name]     NCHAR(100) NOT NULL,[emailId]  NCHAR(100) NOT NULL, [password] NCHAR(20)  NOT NULL,[deleteInd] BIT NOT NULL DEFAULT 0); ";
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = CreatetableUser;
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            conn.Close();
            // sqlite_cmd.CommandText = Createsql1;
            //sqlite_cmd.ExecuteNonQuery();

        }


        static void ReadData(SQLiteConnection conn)
        {
            conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Users";

            DataSet ds = new DataSet();
            SQLiteDataAdapter da = new SQLiteDataAdapter(sqlite_cmd);
            da.Fill(ds);
            conn.Close();

            bool addDummy = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
            if (!addDummy)
            {
                InsertDummyRecord(conn);
            }
        }
        static void InsertDummyRecord(SQLiteConnection conn) {
            //SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            conn.Open();

            sqlite_cmd = conn.CreateCommand();

            sqlite_cmd.CommandText  = "insert into Users (id, name, username, emailId, password) values (0, 'Rohit','Tyson','rohit@gmail.com','1234');";

            sqlite_cmd.ExecuteNonQuery();

            conn.Close();
        }
    }
}