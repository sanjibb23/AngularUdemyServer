using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiQuespond.Errors
{
    public class ApiException
    {

       public int _StatusCode;
       public string _message;
       public string? _details;

       public ApiException(int StatusCode, string message, string? details)
        {
            _StatusCode = StatusCode;
            _message = message;
            _details = details;

        }


    }
}
