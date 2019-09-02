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
     * Runner class
     */
    class Program
    {
        static void Main(string[] args)
        {

            //Ignore invalid certs
            Requests.setCertIgnore();

            //Set up configurations
            UserInput.userSetup();
           
            
        }
    }


}
