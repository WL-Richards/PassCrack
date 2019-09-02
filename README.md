# PassCrack
An intuitive windows based web form password brute forcer

## Usage Cases
 1. ASP / ASPX Cracking mode the program will pull correct values for VIEWSTATE, EVENTVALIDATION, etc.
 2. In POST / GET Web form modes the program will allow the user to grab dynamic values from the website and include them in a request
    * Example: username=^USERNAME^&password=^PASS^&token=getKey(user_token)
    * The getKey() method can simply be typed into the custom form options and give the name of the html object and it will dynamically         pull the value from that HTML object
