using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    public class CustomExceptions
    {
        //Wyjątek zwracany przy próbie dodania pracownika jeśli w systemie nie ma miejsca.
        public class TooManyEmployeesException : Exception
        {
            public TooManyEmployeesException() { }

            public TooManyEmployeesException(string message) : base(message) { }

            public TooManyEmployeesException(string message, Exception inner) : base(message, inner) { }
        }
    }
}
