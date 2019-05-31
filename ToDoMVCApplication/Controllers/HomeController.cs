﻿using System;
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
                if (loginSuccessful)
                {
                    User.Name = ds.Tables[0].Rows[0]["Name"].ToString();
                    User.EmailId = ds.Tables[0].Rows[0]["EmailId"].ToString();
                    User.Id = Convert.ToInt32(ds.Tables[0].Rows[0]["Id"].ToString());
                    IEnumerable<Task> tn = GetAllTaskForLoggedInUser(User.Id);
                    return PartialView("ToDoMainPage", tn);
                }
            }
            
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            return Json(new {User, result = false}, JsonRequestBehavior.AllowGet);
        }

        public List<Task> GetAllTaskForLoggedInUser(int UserId) {
            List<Task> task = new List<Task>();
            sqlite_conn.Open();
            String sql = "Select * from Task WHERE userId = @UserId";
            
            using (SQLiteCommand cmd = new SQLiteCommand(sql, sqlite_conn))
            {
                cmd.Parameters.AddWithValue("@UserId", UserId);
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        task.Add(new Task
                        {
                            TaskId = Convert.ToInt32(rdr["Id"]),
                            TaskDescription = Convert.ToString(rdr["TaskDescription"]),
                            TaskName = Convert.ToString(rdr["TaskName"]),
                            DeleteInd = Convert.ToBoolean(rdr["deleteInd"]),
                            ImportantInd = Convert.ToBoolean(rdr["importantInd"]),
                            ModifiedDateTime = Convert.ToDateTime(rdr["modifiedDateTime"]),
                            CreateDateTime = Convert.ToDateTime(rdr["createdDateTime"]),
                        });
                        
                    }
                }
            }
            if (task.Count() == 0)
            {
                task.Add(new Task
                {
                    TaskId = 0,
                    TaskDescription = "TestTask",
                    TaskName = "Task1",
                    DeleteInd = false,
                    ImportantInd = false,
                    CreateDateTime = DateTime.Now,
                    ModifiedDateTime = DateTime.Now,

                });
            }
            sqlite_conn.Close();
            IEnumerable<Task> en = task;
            return task;
        }

        public ActionResult AddUpdateTask(Task task, int UserId = 1)
        {
            try
            {
                sqlite_conn.Open();
                SQLiteCommand sqlite_cmd;
                sqlite_cmd = sqlite_conn.CreateCommand();
                String sql = "Select * from Users WHERE Id = @TaskId";

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
                else {
                    addNewTask(task, UserId);
                }
                }
          

            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
            IEnumerable<Task> tn = GetAllTaskForLoggedInUser(UserId);
            return PartialView("ToDoMainPage", tn);
        }


        static void updatetask(Task task) {
            sqlite_conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            String sql = "update task set name = @taskName, description = @description, modifieddatetime = @modifieddate, DeleteInd = @DeleteInd, importantInd = @importantInd, userId = @UserId Where id = @TaskId";
            sqlite_cmd.Parameters.AddWithValue("@taskId", task.TaskId);
            sqlite_cmd.Parameters.AddWithValue("@taskName", task.TaskName);
            sqlite_cmd.Parameters.AddWithValue("@description", task.TaskDescription);
            sqlite_cmd.Parameters.AddWithValue("@modifiedDate", task.ModifiedDateTime);
            sqlite_cmd.Parameters.AddWithValue("@DeleteInd", task.DeleteInd);
            sqlite_cmd.Parameters.AddWithValue("@importantInd", task.ImportantInd);
            sqlite_cmd.CommandText = sql;
            sqlite_cmd.ExecuteNonQuery();
            sqlite_conn.Close();
        }

        static void addNewTask(Task task, int UserId)
        {
            sqlite_conn.Open();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqlite_conn.CreateCommand();
            String sql = "INSERT INTO TASK(id,name,description,modifieddatetime,deleteInd,importantind,createddatetime,UserId) VALUES (@TaskId,@taskName,@description,@modifiedDate,@DeleteInd,@importantInd,@createdDate,@UserId)";
            sqlite_cmd.Parameters.AddWithValue("@taskId", task.TaskId);
            sqlite_cmd.Parameters.AddWithValue("@taskName", task.TaskName);
            sqlite_cmd.Parameters.AddWithValue("@description", task.TaskDescription);
            sqlite_cmd.Parameters.AddWithValue("@modifiedDate", task.ModifiedDateTime);
            sqlite_cmd.Parameters.AddWithValue("@DeleteInd", task.DeleteInd);
            sqlite_cmd.Parameters.AddWithValue("@importantInd", task.ImportantInd);
            sqlite_cmd.Parameters.AddWithValue("@createdDate", DateTime.Now);
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
            sqlite_cmd.CommandText  = "insert into Users (id, name, username, emailId, password) values (1, 'Rohit','Tyson','rohit@gmail.com','1234');";
            sqlite_cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}