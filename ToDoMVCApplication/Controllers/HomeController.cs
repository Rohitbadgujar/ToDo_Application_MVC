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
            sqlite_conn = new SQLiteConnection("Data Source=tododatabasetest");
            if (!System.IO.File.Exists("./tododatabasetest"))
            {
                SQLiteConnection.CreateFile("./tododatabasetest");
            }
            CreateTable(sqlite_conn);
            ReadData(sqlite_conn);
            ReadTaskData(sqlite_conn);
            return View();
        }

        public ActionResult MainPage()
        {
            return View("Index");
        }

        [HttpPost]
        public ActionResult SignUp(Models.User User)
        {

            bool check = CheckIfUserExits(sqlite_conn, User);
            if (!check)
            {
                try
                {
                    sqlite_conn.Open();
                    SQLiteCommand sqlite_cmd;
                    sqlite_cmd = sqlite_conn.CreateCommand();
                    String sql = "Insert INTO Users (username,name,emailid,password) values (@username,@name,@emailId,@password)";
                    sqlite_cmd.Parameters.AddWithValue("@username", User.UserName);
                    sqlite_cmd.Parameters.AddWithValue("@name", User.Name);
                    sqlite_cmd.Parameters.AddWithValue("@emailId", User.EmailId);
                    sqlite_cmd.Parameters.AddWithValue("@password", User.Password);
                    sqlite_cmd.CommandText = sql;
                    sqlite_cmd.ExecuteNonQuery();
                    sqlite_conn.Close();
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.ToString());

                }
            }

            return Json(new { returnvalue = "true", alreadyExist = check });
        }
        [HttpPost]
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
                if (loginSuccessful)
                {
                    User.Name = ds.Tables[0].Rows[0]["Name"].ToString();
                    User.EmailId = ds.Tables[0].Rows[0]["EmailId"].ToString();
                    User.Id = Convert.ToInt32(ds.Tables[0].Rows[0]["Id"].ToString());
                    IEnumerable<Task> tn = GetAllTaskForLoggedInUser(User.Id);
                    Session["userName"] = User.Name;
                    ViewBag.UserName = Session["userName"] as String;
                    return PartialView("ToDoMainPage", tn);
                }
            }

            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                sqlite_conn.Close();
            }
            return Json(new { User, result = false }, JsonRequestBehavior.AllowGet);
        }

        public List<Task> GetAllTaskForLoggedInUser(int UserId)
        {
            List<Task> task = new List<Task>();
            sqlite_conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            //String sql = "Select * from Task WHERE userId = @UserId";
            String sql = "Select * from Task ";
            DataSet ds = new DataSet();
            sqlite_cmd.CommandText = sql;
            sqlite_cmd.Parameters.AddWithValue("@UserId", UserId);
            SQLiteDataAdapter da = new SQLiteDataAdapter(sqlite_cmd);

            da.Fill(ds);
            foreach (DataRow rdr in ds.Tables[0].Rows)
            {
                task.Add(new Task
                {
                    TaskId = Convert.ToInt32(rdr["Id"]),
                    TaskDescription = Convert.ToString(rdr["description"]),
                    TaskName = Convert.ToString(rdr["name"]),
                    DeleteInd = Convert.ToBoolean(rdr["deleteInd"]),
                    ImportantInd = Convert.ToBoolean(rdr["importantInd"]),
                    ModifiedDateTime = Convert.ToString(rdr["modifiedDateTime"]),
                    CreateDateTime = Convert.ToString(rdr["createdDateTime"]),

                    CompletedInd = Convert.ToBoolean(rdr["completedInd"]),
                });
            }
            sqlite_conn.Close();

            if (task.Count() == 0)
            {
                task.Add(new Task
                {
                    TaskId = 99,
                    TaskDescription = "TestTask",
                    TaskName = "Task1",
                    DeleteInd = false,
                    ImportantInd = false,
                    CreateDateTime = DateTime.Now.ToString(),
                    ModifiedDateTime = DateTime.Now.ToString(),

                });
            }
            sqlite_conn.Close();
            IEnumerable<Task> en = task;
            return task;
        }
        [HttpPost]
        public ActionResult AddUpdateTask(Task task, int UserId = 1)
        {
            try
            {
                sqlite_conn.Open();
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();
                String sql = "Select * from Task WHERE Id = @TaskId";

                DataSet ds = new DataSet();
                sqlite_cmd.CommandText = sql;
                sqlite_cmd.Parameters.AddWithValue("@TaskId", task.TaskId);
                SQLiteDataAdapter da = new SQLiteDataAdapter(sqlite_cmd);

                da.Fill(ds);
                sqlite_conn.Close();
                bool updateInd = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
                if (updateInd)
                {
                    updatetask(task);
                }
                else
                {
                    addNewTask(task, UserId);
                }
            }


            catch (SqlException e)
            {
                sqlite_conn.Close();
                Console.WriteLine(e.ToString());
            }
            // ReadTaskData(sqlite_conn);
            IEnumerable<Task> tn = GetAllTaskForLoggedInUser(UserId);
            ViewBag.UserName = Session["userName"] as String; ;
            return PartialView("ToDoMainPage", tn);
        }


        static void updatetask(Task task)
        {
            sqlite_conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            String sql = "update task set name = @taskName, description = @description, modifieddatetime = @modifieddate, DeleteInd = @DeleteInd, importantInd = @importantInd, completedInd = @completedInd Where id = @TaskId";
            sqlite_cmd.Parameters.AddWithValue("@taskId", task.TaskId);
            sqlite_cmd.Parameters.AddWithValue("@taskName", task.TaskName);
            sqlite_cmd.Parameters.AddWithValue("@description", task.TaskDescription);
            sqlite_cmd.Parameters.AddWithValue("@modifiedDate", DateTime.Now.ToString());
            sqlite_cmd.Parameters.AddWithValue("@DeleteInd", task.DeleteInd);
            sqlite_cmd.Parameters.AddWithValue("@importantInd", task.ImportantInd);
            sqlite_cmd.Parameters.AddWithValue("@completedInd", task.CompletedInd);
            sqlite_cmd.CommandText = sql;
            sqlite_cmd.ExecuteNonQuery();
            sqlite_conn.Close();
        }
        static void addNewTask(Task task, int UserId)
        {
            sqlite_conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            String sql = "INSERT INTO TASK(name,description,modifieddatetime,deleteInd,importantind,createddatetime,UserId,completedInd) VALUES (@taskName,@description,@modifiedDate,@DeleteInd,@importantInd,@createdDate,@UserId,@completedInd)";
            sqlite_cmd.Parameters.AddWithValue("@taskName", task.TaskName);
            sqlite_cmd.Parameters.AddWithValue("@description", task.TaskDescription);
            sqlite_cmd.Parameters.AddWithValue("@modifiedDate", DateTime.Now.ToString());
            sqlite_cmd.Parameters.AddWithValue("@DeleteInd", task.DeleteInd);
            sqlite_cmd.Parameters.AddWithValue("@importantInd", task.ImportantInd);
            sqlite_cmd.Parameters.AddWithValue("@createdDate", DateTime.Now.ToString());
            sqlite_cmd.Parameters.AddWithValue("@completedInd", task.CompletedInd);
            sqlite_cmd.Parameters.AddWithValue("@UserId", UserId);
            sqlite_cmd.CommandText = sql;
            sqlite_cmd.ExecuteNonQuery();
            sqlite_conn.Close();
        }

        static void CreateTable(SQLiteConnection conn)
        {
            conn.Open();
            SQLiteCommand sqlite_cmd;
            try
            {
                string CreatetableTask = "CREATE Table [Task] ([Id] INTEGER PRIMARY KEY AUTOINCREMENT, [name] TEXT NOT NULL, [description] TEXT NULL , [createddatetime] TEXT NULL , [modifieddatetime] TEXT NULL , [deleteInd] NUMERIC NOT NULL , [importantInd] NUMERIC NOT NULL ,[completedInd] NUMERIC NOT NULL,  [userId] INTEGER NOT NULL)";
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = CreatetableTask;
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                sqlite_conn.Close();
            }
            try
            {
                string CreatetableUser = "CREATE TABLE  [Users] ( [Id] INTEGER PRIMARY KEY AUTOINCREMENT, [username] NCHAR(100) NOT NULL,[name]     NCHAR(100) NOT NULL,[emailId]  NCHAR(100) NOT NULL, [password] NCHAR(20)  NOT NULL,[deleteInd] BIT NOT NULL DEFAULT 0); ";
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = CreatetableUser;
                sqlite_cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                conn.Close();
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
        static void ReadTaskData(SQLiteConnection conn)
        {
            conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Task";
            sqlite_cmd.Prepare();
            DataSet ds = new DataSet();
            SQLiteDataAdapter da = new SQLiteDataAdapter(sqlite_cmd);
            da.Fill(ds);
            conn.Close();

            bool addDummy = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
            if (!addDummy)
            {
                InsertTaskDummyRecord(conn);
            }
        }
        static bool CheckIfUserExits(SQLiteConnection conn, User user)
        {
            sqlite_conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            String sql = "Select * from Users WHERE username = @username or emailId = @emailId";

            DataSet ds = new DataSet();
            sqlite_cmd.CommandText = sql;
            sqlite_cmd.Parameters.AddWithValue("@username", user.UserName);
            sqlite_cmd.Parameters.AddWithValue("@emailId", user.EmailId);
            SQLiteDataAdapter da = new SQLiteDataAdapter(sqlite_cmd);

            da.Fill(ds);
            sqlite_conn.Close();
            bool exitsInd = ((ds.Tables.Count > 0) && (ds.Tables[0].Rows.Count > 0));
            if (exitsInd)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static void InsertTaskDummyRecord(SQLiteConnection conn)
        {
            //SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            conn.Open();

            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "insert into Task (name, description, modifieddatetime, createddatetime,deleteInd,importantInd,Userid,completedInd) values ('Task100','Tyson','11/11/2019','11/11/2019',0,0,2,0);insert into Task (name, description, modifieddatetime, createddatetime,deleteInd,importantInd,Userid,completedInd) values ('Task200','Tyson2','10/11/2019','10/11/2019',0,0,2,0);";
            sqlite_cmd.ExecuteNonQuery();
            conn.Close();
        }
        static void InsertDummyRecord(SQLiteConnection conn)
        {
            //SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            conn.Open();

            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "insert into Users (name, username, emailId, password) values ('Rohit','test','rohit@gmail.com','pwd123');insert into Users (name, username, emailId, password) values ('Rohit2','admin','rohit@gmail.com','1234');";
            sqlite_cmd.ExecuteNonQuery();
            conn.Close();
        }

    }

}