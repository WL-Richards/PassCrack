using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Web;
using System.Threading;
using System.Collections;

namespace WebformPasswordCracker
{

    /**
     * Class used to handle all requests to the server
     */
    class Requests
    {
        
        //Global variables for holding ASP/ASPX specific forms
        private static string __VIEWSTATE = "";
        private static string __VIEWSTATEGENERATOR = "";
        private static string __EVENTVALIDATION = "";

        private static string responseStr = "";

        /**
         * If non HTTPS tell the requests to ignore certificate validations
         */
        public static void setCertIgnore()
        {
            //Tell the web requests to ignore the certificates
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        }

        /**
         * Method used to grab dynamic values makes it easy for users to implement into requests
         */
        public static string getKeyValue(string keyName)
        {
            //Gets the response from the website
            string response = getResponse();

            string valueDelimiter = "value=";

            int NamePosition = response.IndexOf(keyName)+1;
            int ValuePosition = response.IndexOf(valueDelimiter, NamePosition);
            
            int StartPosition = ValuePosition + valueDelimiter.Length+1; 
            int EndPosition = response.IndexOf("/", StartPosition)-2;

            return response.Substring(StartPosition, EndPosition - StartPosition);
        }

        /**
         * Gets the response from the server
         */
        public static string getResponse()
        {
            string responseStr = "";
            WebRequest request = WebRequest.Create(UserInput.fullPath);

            WebResponse response = request.GetResponse();
            using (Stream data = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(data);
                responseStr = reader.ReadToEnd();
            }
            response.Close();
            return responseStr;
        }

        /**
         * Get VIEWSTATE, EVENTVALIDATION, ETC
         */
        public static void getData()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UserInput.fullPath);

            request.Host = UserInput.host;
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
          
            __VIEWSTATE = "";
            __VIEWSTATEGENERATOR = "";
            __EVENTVALIDATION = "";

            using (StreamReader read = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                string resp = read.ReadToEnd();

                //If not a ASP form this will throw an error
                try
                {
                    __VIEWSTATE = getViewState(resp);
                    __EVENTVALIDATION = getEventVal(resp);
                    __VIEWSTATEGENERATOR = getViewStateGen(resp);
                }
                catch(IndexOutOfRangeException e)
                {

                }
            }
        }

        /**
         * Send Request to the server and returns a response
         * However event validation is active so i have to pull that from the initial response
         */
        public static string sendLoginRequest(string username, string password)
        {
           

            //If the form type is ASP then use this to grab required values
            if (UserInput.formType == 1)
            {
                //Gets the VIEWSTATE, EVENTVALIDATION ETC.
                getData();
            }

            if (UserInput.formType != 3)
            {
                //Setup a new web request to the URL
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UserInput.fullPath);
                request.Method = "POST";
                var postData = "";

                //Set the post data to include VIEWSTATE Properties if acquired
                if (__VIEWSTATE.Length > 0)
                {
                    postData = "__VIEWSTATE=" + __VIEWSTATE + "&";
                    postData += "__VIEWSTATEGENERATOR=" + __VIEWSTATEGENERATOR + "&";
                    postData += "__EVENTVALIDATION=" + __EVENTVALIDATION + "&";
                    postData += HttpUtility.UrlEncode(UserInput.usernameField, Encoding.ASCII) + "=" + username + "&";
                    postData += HttpUtility.UrlEncode(UserInput.passwordField, Encoding.ASCII) + "=" + password;
                }

                //If not just send the user name fields
                else
                {
                    postData = HttpUtility.UrlEncode(UserInput.usernameField, Encoding.ASCII) + "=" + username + "&";
                    postData += HttpUtility.UrlEncode(UserInput.passwordField, Encoding.ASCII) + "=" + password;

                    //If we are using the general form
                    if (UserInput.formType == 2)
                    {
                        postData += "&" + UserInput.customFormVariables;

                    }

                }

                //Set general HTTP headers
                request.ContentLength = postData.Length;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";
            
                request.ContentType = "application/x-www-form-urlencoded";
                request.Host = UserInput.host;
                request.Referer = UserInput.fullPath;

                //Write the user information into the stream
                StreamWriter requestWriter = new StreamWriter(request.GetRequestStream());
                requestWriter.Write(postData);
                requestWriter.Close();

                //Get, read and return the response
                var response = request.GetResponse();
                responseStr = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            else
            {
                UserInput.customFormVariables = UserInput.buildCustomOptions(UserInput.rawFormVariables);
    
                //Setup a new web request to the URL
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UserInput.fullPath + "?" + HttpUtility.UrlEncode(UserInput.usernameField, Encoding.ASCII) + "=" + username + "&" + HttpUtility.UrlEncode(UserInput.passwordField, Encoding.ASCII) + "=" + password + "&" + UserInput.customFormVariables);
                //Set general HTTP headers
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";

                if(UserInput.cookies.Length > 0)
                if(UserInput.cookies.Length > 0)
                {
                    request.Headers.Add(HttpRequestHeader.Cookie, UserInput.cookies);
                }

                request.ContentType = "application/x-www-form-urlencoded";
                request.Host = UserInput.host;
                request.Referer = UserInput.fullPath;

                WebResponse response = request.GetResponse();
                using (Stream data = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(data);
                    responseStr = reader.ReadToEnd();
                }
                response.Close();
            }
            return responseStr;

            
        }


        /**
         * Used to grab the __VIEWSTATE of the request
         */
        private static string getViewState(string response)
        {
            string valString = "value=\"";
            string headerString = "__VIEWSTATE";


            int viewStateNamePos = response.IndexOf(headerString);
            int viewStateValuePos = response.IndexOf(valString, viewStateNamePos);

            int viewStateStartPosition = viewStateValuePos +
                                          valString.Length;
            int viewStateEndPosition = response.IndexOf("\"", viewStateStartPosition);

            return HttpUtility.UrlEncode(response.Substring(viewStateStartPosition,viewStateEndPosition - viewStateStartPosition),Encoding.ASCII);
        }

        /**
         * Used to grab the __EVENTVALIDATION of the request
         */
        private static string getEventVal(string s)
        {
            string eventValidationNameDelimiter = "__EVENTVALIDATION";
            string valueDelimiter = "value=\"";

            int eventValidationNamePosition = s.IndexOf(eventValidationNameDelimiter);
            int eventValidationValuePosition = s.IndexOf(valueDelimiter, eventValidationNamePosition);

            int eventValidationStartPosition = eventValidationValuePosition +
                                         valueDelimiter.Length;
            int eventValidationEndPosition = s.IndexOf("\"", eventValidationStartPosition);

            return HttpUtility.UrlEncode(s.Substring(eventValidationStartPosition,eventValidationEndPosition - eventValidationStartPosition),Encoding.ASCII);
        }

        /**
         * Used to grab the __VIEWSTATEGENERATOR of the request
         */
        private static string getViewStateGen(string response)
        {
            string valString = "value=\"";
            string headerString = "__VIEWSTATEGENERATOR";


            int viewStateNamePos = response.IndexOf(headerString);
            int viewStateValuePos = response.IndexOf(valString, viewStateNamePos);

            int viewStateStartPosition = viewStateValuePos +
                                          valString.Length;
            int viewStateEndPosition = response.IndexOf("\"", viewStateStartPosition);

            return HttpUtility.UrlEncode(response.Substring(viewStateStartPosition, viewStateEndPosition - viewStateStartPosition), Encoding.ASCII);
        }

        /**
         * Check if the IP resolves to something and is up
         */
        private static bool checkValidURL()
        {
            if (new Ping().Send(UserInput.ip).Status == IPStatus.Success)
            {
                return true;
            }
            return false;
        }

    }
}
