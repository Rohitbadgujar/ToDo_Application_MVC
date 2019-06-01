# ToDo_Application_MVC

Platform Used: MS Visual Studio Community 2019 Version 16.1.1
Database Used: In Memory SQlite
Github Repository Link: https://github.com/Rohitbadgujar/ToDo_Application_MVC
Application is built in C# .Net using MVC framework. 
•	MVC framework provides separate layers for view, controller and Model. These layers are loosely coupled which helped me to code easily. It also provides ability to reusable partial view which helps to avoid redundant code.
•	It also provides Razor syntax which lets you embed server-based code into web pages using C#
•	I created Separate Classes for Users and Task with one-to-many relationship e.g. One User can have many tasks. 
•	I have used [HTTP] verbs to store and retrive data securely. 
•	I have used HTML, Bootstrap, CSS, JQuery to build rich webpage which supports all recent browsers.
•	Created separate form for Sign-in and Sign-up. Validated duplicate User.
•	On Successful Sign-in. Partial view is displayed to show Tasks and provides the functionality to Add new Task, Mark as Completed, Important or Delete.
•	Completed task are shown by strike-through text. Validation message is added wherever required. 
•	I have also added search task functionality based on the task name.
•	The user also has the option to log out from the application.
•	The application supports multiple users.
 On given extra time I would make the application
•	more secure by encrypting/decrypting the password. 
•	Create repository to perform SQL operation using EntityFramework
•	Integrate Captcha while sign in to protect the website against bots.
•	Make use of appropriate design pattern. 
•	Edit User details, forgot password functionally.
•	Auto suggest task on User history. 
•	Creating notification alerts when task is not marked completed for long time.
I guess SQLite doesn’t support Store procedure therefore, I haven’t used any. 
Things you should know:
Sign up before you sign-in into the application. 

