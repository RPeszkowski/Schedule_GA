using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    public class CustomExceptions
    {
        //Wyjątek rzucany, gdy próbujemy dodać pracownika o numerze, który jest już zajęty.
        public class EmployeeAlreadyExistException : Exception
        {
            public EmployeeAlreadyExistException() { }

            public EmployeeAlreadyExistException(string message) : base(message) { }

            public EmployeeAlreadyExistException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek rzucany, gdy imię lub nazwisko pracownika zawiera spację.
        public class EmployeeNameSurnameException : Exception
        {
            public EmployeeNameSurnameException() { }

            public EmployeeNameSurnameException(string message) : base(message) { }

            public EmployeeNameSurnameException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek rzucany, gdy numer pracownika jest poza zakresem.
        public class EmployeeNumberOutOfRangeException : Exception
        {
            public EmployeeNumberOutOfRangeException() { }

            public EmployeeNumberOutOfRangeException(string message) : base(message) { }

            public EmployeeNumberOutOfRangeException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek rzucany, gdy plik pracownikó ma zły format.
        public class FileServiceInvalidEmployeesFormat : Exception
        {
            public FileServiceInvalidEmployeesFormat() { }

            public FileServiceInvalidEmployeesFormat(string message) : base(message) { }

            public FileServiceInvalidEmployeesFormat(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek rzucany, gdy plik grafiku ma zły format.
        public class FileServiceInvalidScheduleFormat : Exception
        {
            public FileServiceInvalidScheduleFormat() { }

            public FileServiceInvalidScheduleFormat(string message) : base(message) { }

            public FileServiceInvalidScheduleFormat(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek rzucany, gdy grafik jest nie przystosowany do optymalizacji.
        public class OptimizationInvalidScheduleException : Exception
        {
            public OptimizationInvalidScheduleException() { }

            public OptimizationInvalidScheduleException(string message) : base(message) { }

            public OptimizationInvalidScheduleException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjatek rzucany, gdy dane do optymalizacji nie są poprawne, i uniemożliwia to przeprowadzenie optymalizacji.
        public class OptimizationInvalidDataException : Exception
        {
            public OptimizationInvalidDataException() { }

            public OptimizationInvalidDataException(string message) : base(message) { }

            public OptimizationInvalidDataException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek rzucamy, gdy employeeManager zwróci błąd.
        public class ScheduleEmployeeManagerException : Exception
        {
            public ScheduleEmployeeManagerException() { }

            public ScheduleEmployeeManagerException(string message) : base(message) { }

            public ScheduleEmployeeManagerException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek rzucamy, gdy dodaj funkcje zgłosi błąd.
        public class ScheduleFunctionEncodingException : Exception
        {
            public ScheduleFunctionEncodingException() { }

            public ScheduleFunctionEncodingException(string message) : base(message) { }

            public ScheduleFunctionEncodingException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek rzucamy, gdy employeeManager zwróci błąd.
        public class ScheduleInvalidEmployeeIdException : Exception
        {
            public ScheduleInvalidEmployeeIdException() { }

            public ScheduleInvalidEmployeeIdException(string message) : base(message) { }

            public ScheduleInvalidEmployeeIdException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek rzucamy, gdy employeeManager zwróci błąd.
        public class ScheduleInvalidScheduleIdException : Exception
        {
            public ScheduleInvalidScheduleIdException() { }

            public ScheduleInvalidScheduleIdException(string message) : base(message) { }

            public ScheduleInvalidScheduleIdException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjątek zwracany przy próbie dodania pracownika jeśli w systemie nie ma miejsca.
        public class TooManyEmployeesException : Exception
        {
            public TooManyEmployeesException() { }

            public TooManyEmployeesException(string message) : base(message) { }

            public TooManyEmployeesException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjatek zwracany, gdy wybrana kontrolka grafiku nie istnieje.
        public class UIInvalidEmployeeControlIdException : Exception
        {
            public UIInvalidEmployeeControlIdException() { }

            public UIInvalidEmployeeControlIdException(string message) : base(message) { }

            public UIInvalidEmployeeControlIdException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjatek zwracany, gdy wybrana kontrolka pracownika nie istnieje.
        public class UIInvalidScheduleControlIdException : Exception
        {
            public UIInvalidScheduleControlIdException() { }

            public UIInvalidScheduleControlIdException(string message) : base(message) { }

            public UIInvalidScheduleControlIdException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjatek zwracany, gdy plik grafiku jest uszkodzony.
        public class UIInvalidScheduleFileException : Exception
        {
            public UIInvalidScheduleFileException() { }

            public UIInvalidScheduleFileException(string message) : base(message) { }

            public UIInvalidScheduleFileException(string message, Exception inner) : base(message, inner) { }
        }

        //Wyjatek zwracany, gdy plik pracowników jest uszkodzony.
        public class UIInvalidEmployeeFileException : Exception
        {
            public UIInvalidEmployeeFileException() { }

            public UIInvalidEmployeeFileException(string message) : base(message) { }

            public UIInvalidEmployeeFileException(string message, Exception inner) : base(message, inner) { }
        }
    }
}
