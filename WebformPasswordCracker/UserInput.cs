using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace WebformPasswordCracker
{

    /**
     * Class used to handle all user input and setup
     */
    class UserInput
    {
        public static int formType = 0;
        public static string formTypeStr = "";

        //Networking info generated from user input
        public static string ip = "";
        public static string host = "";
        public static string addOnPath = "";
        public static string fullPath = "";
        public static string requestType = "";

        //User defined settings
        public static string usernameField = "";
        public static string passwordField = "";
        public static string invalidPasswordText = "";

        //Word list file path, userListPath and single user name
        public static string wordListPath = "";
        public static string userListPath = "";
        public static string username = "";

        public static string cookies = "";

        public static bool showAttempts = false;

        public static List<string> keyNameList = new List<string>();


        //General post form specific booleans
        public static string customFormVariables = "";
        public static string rawFormVariables = "";

        /**
         * Helper method for user responses
         * Allows for a question to be passed and returns the users response
         */
        public static string getUserResponse(string question)
        {
            Console.Write(question);
            return Console.ReadLine();
        }

        /**
         * User Setup For The Attack (eg. Target, Word list, etc..)
         */
        public static void userSetup()
        {
            Console.Clear();
            resetConfig();

            //Called to display the tile of the program
            coolTitle();
            Console.WriteLine("\n                                   ** This program is for Educational Purposes Only **\n " +
                              "                          ** I Will not be held accountable for any elicit use of this software **");

            Console.WriteLine("\nWelcome to PassCrack, this is a program that allows a user to easily run word lists against web forms\n");
            
            Console.WriteLine("Select A Form Type...");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1) ASP/ASPX\n");
            //Console.WriteLine("2) General POST Form\n");
            //Console.WriteLine("3) General GET Form\n");
            Console.ResetColor();
            try
            {
                formType = Int32.Parse(getUserResponse("Select A Form Type (1-...): "));
            }
            catch (Exception e) { }
            Console.ForegroundColor = ConsoleColor.Cyan;
        

            //Convert selected form to string for display
            switch (formType)
            {
                case 1:
                    formTypeStr = "\"ASP/ASPX\"";
                    break;
                case 2:
                    formTypeStr = "\"General POST Form\"";
                    break;
                case 3:
                    formTypeStr = "\"General GET Form\"";
                    break;
                default:
                    formTypeStr = "\"General POST Form\"";
                    break;
            }

            //Print what the selected form is
            Console.WriteLine(String.Format("You Selected: {0}\n", formTypeStr));
            Console.ResetColor();


            //Resolve the host name passed to an IP address, if host doesn't resolve tell the user the URL was bad and then restart
            try
            {
                host = UserInput.getUserResponse("Base URL To Login Page (eg. www.example.com): ");
                IPAddress testIp = null;
                if (!IPAddress.TryParse(host, out testIp)){
                    ip = (Dns.GetHostEntry(host).AddressList[0]).ToString();
                }
                else
                {
                    ip = host;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(String.Format("Host name Resolved to {0}", ip));
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid URL : " + e.Message);
                Console.ResetColor();
                Thread.Sleep(3000);
                Console.Clear();
                userSetup();
            }

            //Add the attack URI to the resolved IP
            addOnPath = UserInput.getUserResponse("Input the add on path (eg /login/...): ");
            requestType = UserInput.getUserResponse("Input the request type (http/https if login form is https use https): ");


            //Combine the base URL and the full URI and confirm that it is correct
            fullPath = requestType + "://" + host + addOnPath;
            if (UserInput.getUserResponse(String.Format("Is this path correct: {0} (y/n): ", fullPath)) == "n")
            {
                Console.Clear();
                userSetup();
            }

            //Inform the user it is getting the response from the URL to verify that its not a 404 or something similar
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Getting Response From URL...");
            Console.ResetColor();

            //Wrap block in try catch to avoid 404 errors
            try
            {
                //As well check the response length to see if it is greater than a certain character limit to verify that something was actually returned
                if (Requests.getResponse().Length > 20)
                {
                    //If it is inform the user it most likely found a valid response
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Response Received!");
                    Console.ResetColor();
                }

                //However for some strange reason if the character length was less than 20 inform the user and ask if they wish to see the response
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("An Unknown Error Occurred");
                    Console.ResetColor();
                    if (UserInput.getUserResponse("Would you like to see the response? (y/n): ") == "y")
                    {
                        Console.WriteLine(Requests.getResponse());
                        if (UserInput.getUserResponse("Would you like to continue if not? (y/n)") == "n")
                        {
                            Console.Clear();
                            userSetup();
                        }

                    }
                    else
                    {
                        Console.Clear();
                        userSetup();
                    }
                }
            }

            //If a 404 or similar exception was returned tell the user the exception and return to the menu
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error Getting Response From Server: " + e.Message);
                Console.ResetColor();
                Thread.Sleep(2000);
                Console.Clear();
                userSetup();
            }

            //Field names for the user name and password
            usernameField = UserInput.getUserResponse("Input the name of the user field on the form (unsaniztized): ");
            passwordField = UserInput.getUserResponse("Input the name of the password field on the form (unsaniztized): ");

            //The user selected a general post form
            if (formType == 2 || formType == 3)
            {
                rawFormVariables = UserInput.getUserResponse("Input Any Custom Parameters: ");
                customFormVariables = buildCustomOptions(rawFormVariables);
                cookies = getUserResponse("Input any cookies that need to be sent in the request (can be left blank): ");

            }

            //Ask the user for the invalid login response
            invalidPasswordText = UserInput.getUserResponse("Input the name of the error element ID: ");

            //Ask for a path to a password list and escape all back slashes
            wordListPath = sanitizePath(UserInput.getUserResponse("Path to password list file: "));

            //Asks the user if they want to use a user list or a single user name
            if (UserInput.getUserResponse("Would you like to use a user list or a single user name? (list / single): ") == "list")
            {
                userListPath = sanitizePath(UserInput.getUserResponse("Path to user list file: "));
            }

            //If 'list' isn't typed default to single
            else
            {
                username = UserInput.getUserResponse("User name: ");
            }

            //Set weather or not we will show the attempts
            if(UserInput.getUserResponse("Would you like to show attempts? (y/n): ") == "y")
            {
                showAttempts = true;
            }


            //Display the selected options
            printConfig();

            //Confirm that the settings are correct and start the attack
            if (UserInput.getUserResponse("All configured settings values are listed above, are you sure you want to proceed? (y/n): ") == "y")
            {
                Cracker.beginAttack();
            }

            //If not then reset
            else
            {
                Console.Clear();
                userSetup();
            }
        }

 
        /**
         * One of two methods used to pull dynamic keys from websites to make assembling requests easier for users
         * Also I don't know why but they work in to separate methods, but its 2am it works I'm not gonna question it
         */
        private static List<string> parseCustomOptions(string input)
        {
            //Loops through all they getKey calls in the custom option and then assigns them to a list
            List<string> keyList = new List<string>();
            int length = 0;
            try
            {
               
                while (input.IndexOf("getKey(", length) != -1)
                {

                    int startOfkey = input.IndexOf("getKey(", length) + 7;
                    int endOfKey = input.IndexOf(")", startOfkey);

                    keyList.Add(Requests.getKeyValue(input.Substring(startOfkey, endOfKey - startOfkey)));
                    keyNameList.Add(input.Substring(startOfkey, endOfKey - startOfkey));
                    length += endOfKey;

                }
            }
            catch(Exception e) { }
            return keyList;

        }

        /**
         * Second method that actually parses out the getKey call and builds the custom request
         */
        public static string buildCustomOptions(string input)
        {
            int occurance = 0;
            int length = 0;
            try
            {
                List<string> keys = parseCustomOptions(input);
                while (input.IndexOf("getKey(", length) != -1)
                {

                    int startOfkey = input.IndexOf("getKey(", length);
                    int endOfKey = input.IndexOf(")", startOfkey)+1;


                    input = input.Remove(startOfkey, (endOfKey - startOfkey));
                    
                    input = input.Insert(startOfkey, keys[occurance]);

                    length += endOfKey;
                    occurance++;

                }
            }
            catch(Exception e) { }
           
            return input;
        }

        /**
        * SUPER Unnecessary cool randomly colored title
        */
        private static void coolTitle()
        {

            Random rand = new Random();
            string[] title =
            {
              "      _____                     ",
              "     |  __ \\                   ",
              "     | |__) |_ _ ___ ___        ",
              "     |  ___/ _` / __/ __|       ",
              "     | |  | (_| \\__ \\__ \\    ",
              "     |_|   \\__,_|___/___/      ",
              " _____                 _        ",
              "/ ____|               | |       ",
              "| |     _ __ __ _  ___| | __    ",
              "| |    | '__/ _` |/ __| |/ /    ",
              "| |____| | | (_| | (__|   <     ",
              "\\_____ |_|  \\__,_|\\___|_|\\_\\"
            };
            foreach (string line in title)
            {
                Console.ForegroundColor = (ConsoleColor)rand.Next(10, 16);
                Console.Write("                                              ");
                Console.Write(line);
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        /**
        * Print out all the variables
        */
        private static void printConfig()
        {
            Console.WriteLine("");
            Console.WriteLine(String.Format("Selected Form: {0}", formTypeStr));
            Console.WriteLine(String.Format("IP: {0}", ip));
            Console.WriteLine(String.Format("Host: {0}", host));
            Console.WriteLine(String.Format("Full Path: {0}", fullPath));
            Console.WriteLine(String.Format("User name Field: {0}", usernameField));
            Console.WriteLine(String.Format("Password Field: {0}", passwordField));
            Console.WriteLine(String.Format("Invalid Login: {0}", invalidPasswordText));
            Console.WriteLine(String.Format("Word list Path: {0}", wordListPath));
            Console.WriteLine(String.Format("Show Attempts: {0}", showAttempts));
  
            //Check if it is using userlists or usernames
            if (userListPath.Length > 0)
            {
                Console.WriteLine("User List Path: " + userListPath);
            }
            else
            {
                Console.WriteLine("User Name: " + username);
            }

            if (formType == 2)
            {
                Console.WriteLine(String.Format("Form Parameters: {0}=USERNAME&{1}=PASSWORD&{2}", usernameField, passwordField, customFormVariables));
            }
            else if (formType == 3)
            {
                Console.WriteLine(String.Format("URL: {0}", fullPath + "?" + usernameField + "=USERNAME&" + passwordField + "=PASSWORD&" + customFormVariables));
            }

            if(formType == 2 || formType == 3)
            {
                if(cookies.Length > 0)
                {
                    Console.WriteLine("Cookie: " + cookies);
                }
            }
        }

        /**
         * Clear the variables
         */
        private static void resetConfig()
        {
            ip = "";
            addOnPath = "";
            fullPath = "";
            usernameField = "";
            passwordField = "";
            invalidPasswordText = "";
            wordListPath = "";
            userListPath = "";
            username = "";
        }

        /**
         * Method used to escape backslashes in a path
         */
        private static string sanitizePath(string path)
        {
            string tempPath = "";
            foreach (char letter in path)
            {
                if (letter != '\\')
                {
                    tempPath += letter;
                }
                else
                {
                    tempPath += letter + "\\";
                }
            }
            return tempPath;
        }

    }
}
