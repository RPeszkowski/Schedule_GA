using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Funkcje_GA.Model;
using Xunit.Sdk;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    public class ScheduleManagement : IScheduleManagement
    {
        private IEmployeeManagement _employeeManager;                   //Instancja menadżera pracowników.
        private readonly List<IShift> schedule;                                     //Lista przechowuje grafik.

        //Konstruktor
        public ScheduleManagement(IEmployeeManagement EmpManager)
        {
            //Tworzymy pusty grafik i instancję employeeManager.
            schedule = new List<IShift>(2 * LICZBA_DNI);
            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                IShift newShift = new Shift(i);
                schedule.Add(newShift);
            }

            this._employeeManager = EmpManager;

            //Subskrybujemy event - usunięcie pracownika.
            _employeeManager.EmployeeDeleted += (employeeId) =>
            {
                var shifts = GetShiftsForEmployee(employeeId);
                foreach (var shift in shifts)
                    RemoveFromShift(shift.shiftId, employeeId);
            };
        }

        //Event wywoływany przy zmianie grafiku.
        public event Action<IEnumerable<IShift>> ShiftChanged;

        //Dodawanie pracownika do grafiku.
        public void AddToShift(int shiftId, int employeeId)
        {
            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId);       //Pracownik.
            var shift = schedule[shiftId];                                 //Zmiana.
            var result = new List<IShift>();            //Zmiany, do których dodano pracownika.

            //Jeżeli pracownika nie ma na zmianie to go dodajemy, inkrementujemy wymiar etatu i odświeżamy listboxa.
            if (shift.AddEmployeeToShift(employee))
            {
                try
                {
                    //Zmieniamy wymiar etatu.
                    _employeeManager.EmployeeEdit(employee, employee.WymiarEtatu + 1.0);
                    result.Add(shift);
                }

                //Jeśli się nie uda to rzucamy wyjątek.
                catch(Exception ex)
                {
                    throw new ScheduleEmployeeManagerException($"Nie udało się dodać pracownika do zmiany {ex.Message}.", ex);
                }

                //Wołamy subskrybenta.
                ShiftChanged?.Invoke(result);
            }
        }

        //Dodajemy funkcje do grafiku. Funkcja wyrzuca wyjątek przy próbie prypisania funkcji do nieistniejącej osoby
        //lub próbie przypisania dwóch funkcji jednej osobie.
        public void ApplyOptimizationToSchedule(bool[] optymalneRozwiazanie)
        {
            int nrSala;                                     //Numer pracownika, który ma salę.
            int nrTriaz1;                                   //Numer pierwszego pracownika, który ma triaż.
            int nrTriaz2;                                   //Numer drugiego pracownika, który ma triaż.
            bool[] numerOsoby = new bool[MAX_LICZBA_BITOW]; //Numer osoby binarnie.
            IShift shift;                                    //Zmiana.

            //Dla każdej zmiany zdekoduj i dodaj funkcje.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                shift = schedule[nrZmiany];             //Zmiana.

                //Sprawdzamy, czy w danym miesiącu istnieje ta zmiana.
                if (shift.GetEmployees().Count() == 0)
                    continue;

                //Pobieramy zapisany binarnie numer osoby, która ma salę i dekodujemy.
                Array.Copy(optymalneRozwiazanie, 3 * MAX_LICZBA_BITOW * nrZmiany, numerOsoby, 0, MAX_LICZBA_BITOW);
                nrSala = DecodeEmployeeNumber(numerOsoby);

                //Pobieramy zapisany binarnie numer pierwszej osoby, która ma triaż i dekodujemy.
                Array.Copy(optymalneRozwiazanie, 3 * MAX_LICZBA_BITOW * nrZmiany + MAX_LICZBA_BITOW, numerOsoby, 0, MAX_LICZBA_BITOW);

                //Dekodujemy numer.
                nrTriaz1 = DecodeEmployeeNumber(numerOsoby);

                //Pobieramy zapisany binarnie numer drugiej osoby, która ma triaż i dekodujemy.
                Array.Copy(optymalneRozwiazanie, 3 * MAX_LICZBA_BITOW * nrZmiany + 2 * MAX_LICZBA_BITOW, numerOsoby, 0, MAX_LICZBA_BITOW);
                nrTriaz2 = DecodeEmployeeNumber(numerOsoby);

                //Pobieramy numer zmiany i sprawdzamy, czy dane z optymalizacji są poprawne.            
                if (nrSala >= shift.GetEmployees().Count()
                    || nrTriaz1 >= shift.GetEmployees().Count()
                    || nrTriaz2 >= shift.GetEmployees().Count())
                {
                    throw new ScheduleFunctionEncodingException(
                        $"Jeden z numerów: {nrSala}, {nrTriaz1} lub {nrTriaz2} jest większy bądź równy liczbie pracowników");
                }

                //Sprawdzamy, czy dana osoba nie ma dwóch funkcji.
                if (nrSala == nrTriaz1)
                    throw new ScheduleFunctionEncodingException(
                        $"Numer: {nrSala}, na zmianie {nrZmiany} ma przypisane dwie funkcje");

                //Sprawdzamy, czy dana osoba nie ma dwóch funkcji.
                if (nrSala == nrTriaz2)
                    throw new ScheduleFunctionEncodingException(
                        $"Numer: {nrSala}, na zmianie {nrZmiany} ma przypisane dwie funkcje");

                //Sprawdzamy, czy dana osoba nie ma dwóch funkcji.
                if (nrTriaz2 == nrTriaz1)
                    throw new ScheduleFunctionEncodingException(
                        $"Numer: {nrTriaz1}, na zmianie {nrZmiany} ma przypisane dwie funkcje");

                //Przypisanie funkcji.
                AssignFunctionToEmployee(nrZmiany, shift.GetEmployees().ToList()[nrSala].Numer, FunctionTypes.Sala);
                AssignFunctionToEmployee(nrZmiany, shift.GetEmployees().ToList()[nrTriaz1].Numer, FunctionTypes.Triaz);
                AssignFunctionToEmployee(nrZmiany, shift.GetEmployees().ToList()[nrTriaz2].Numer, FunctionTypes.Triaz);
            }
        }

        //Zmieniamy funkcje pracownika.
        public void AssignFunctionToEmployee(int shiftId, int employeeId, FunctionTypes function)
        {
            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId);       //Pracownik.
            var shift = schedule[shiftId];        //Zmiana.
            var result = new List<IShift>();            //Zmiany, do których dodano pracownika.

            //Przypisujemy funkcje i w razie czego wzywamy presentera.
            if (shift.AssignFunction(employee, function))
                result.Add(shift);            //Zmiany, do których przypisano funkcje.

            //Event.
            ShiftChanged?.Invoke(result);
        }

        //Dekodujemy numer pracownika na podstawie danych z rozwiązania problemu optymalizacji.
        private int DecodeEmployeeNumber(bool[] bits)
        {
            //Sprawdzamy, czy tablica nie jest pusta.
            if (bits == null || bits.Length == 0)
                throw new ArgumentException("Tablica bitów nie może być pusta.");

            //Dekodujemy numer pracownika.
            int number = 0;
            foreach (bool bit in bits)
            {
                number = (number * 2) + (bit ? 1 : 0);
            }
            return number;
        }

        //Pobierz zmianę o określonym Id.
        public IShift GetShiftById(int id)
        {
            //Sprawdzamy, czy Id jest poprawne.
            if (id < 0 || id >= schedule.Count)
                throw new ScheduleInvalidScheduleIdException($"Numer zmiany {id} jest niepoprawny");

            return schedule[id];
        }

        //Walidacja Id.
        private void ShiftValidate(int shiftId, int employeeId)
        {
            //Sprawdzamy, czy Id jest poprawne.
            if (shiftId < 0 || shiftId >= 2 * LICZBA_DNI)
                throw new ScheduleInvalidScheduleIdException($"Numer zmiany {shiftId} jest niepoprawny");

            //Sprawdzamy, czy Id jest poprawne.
            if (employeeId < 1 || employeeId > MAX_LICZBA_OSOB)
                throw new ScheduleInvalidEmployeeIdException($"Numer pracownika {employeeId} jest niepoprawny");

            if(_employeeManager.GetEmployeeById(employeeId) == null) throw new ArgumentNullException("Osoba nie istnieje.");     //Pracownik.
            if(schedule[shiftId] == null) throw new ArgumentNullException("Zmiana nie została utworzona.");        //Zmiana.
        }

        //Zwraca numery zmian i pełnione na nich funkcje danego pracownika.
        public IEnumerable<(int shiftId, FunctionTypes function)> GetShiftsForEmployee(int employeeId)
        {
            var result = new List<(int, FunctionTypes function)> ();                        //Lista zmian i pełnionych funkcji.

            //Sprawdzamy, czy Id jest poprawne.
            if (employeeId < 1 || employeeId > MAX_LICZBA_OSOB)
                throw new ScheduleInvalidEmployeeIdException($"Numer pracownika {employeeId} jest niepoprawny");

            if (_employeeManager.GetEmployeeById(employeeId) == null) throw new ArgumentNullException("Osoba nie istnieje.");     //Pracownik - sprawdzamy null.

            //Sprawdzamy po kolei każdą zmianę.
            for (int nrZmiany = 0; nrZmiany < 2*LICZBA_DNI; nrZmiany++)
            {
                var shift = schedule[nrZmiany];                            //Pobieramy zmianę.

                //Sprawdzamy, czy pracownik występuje.
                if (shift.GetEmployees().Any(e => e.Numer == employeeId))
                {
                    FunctionTypes function = FunctionTypes.Bez_Funkcji;          //Funkcja pracownika.

                    //Sprawdzamy, czy pracownik ma salę.
                    if (shift.GetEmployeesByFunction(FunctionTypes.Sala).Any(e => e.Numer == employeeId))
                        function = FunctionTypes.Sala;

                    //Sprawdzamy, czy pracownik ma triaż.
                    if (shift.GetEmployeesByFunction(FunctionTypes.Triaz).Any(e => e.Numer == employeeId))
                        function = FunctionTypes.Triaz;

                    result.Add((nrZmiany, function));
                }
            }

            return result;
        }

        //Usuwanie całego grafiku.
        public void RemoveAll()
        {
            var result = new List<IShift>();            //Zmiany wyczyszczone.

            //Czyścimy wszystkie zmiany i odświeżamy kontrolki.
            foreach (var shift in schedule)
            {
                if(shift.ClearShift())
                    result.Add(shift);
            }

            //Event.
            ShiftChanged?.Invoke(result);

            //Usuwamy dane o wymiarze etatu.
            foreach (Employee employee in _employeeManager.GetAllActive())
                _employeeManager.EmployeeEdit(employee, 0.0);
        }

        //Usuwamy osobę ze zmiany.
        public void RemoveFromShift(int shiftId, int employeeId)
        {
            //Sprawdzamy, czy Id jest poprawne.
            ShiftValidate(shiftId, employeeId);

            Employee employee = _employeeManager.GetEmployeeById(employeeId);       //Pracownik.
            var shift = schedule[shiftId];        //Zmiana.
            var result = new List<IShift>();            //Zmiany, z których usunięto pracownika.

            //Jeśli pracownik był na zmianie to dekrementujemy wymiar etatu.
            if (shift.RemoveEmployeeFromShift(employee))
            {
                //Próbujemy usunąć pracownika.
                try
                {
                    //Edytujemy wymiar etatu.
                    _employeeManager.EmployeeEdit(employee, employee.WymiarEtatu - 1.0);
                    result.Add(shift);
                }

                //Jeśli się nie udało rzucamy wyjątek.
                catch (Exception ex)
                {
                    throw new ScheduleEmployeeManagerException($"Nie udało się dodać pracownika do zmiany {ex.Message}.", ex);
                }

                //Event.
                ShiftChanged?.Invoke(result);
            }
        }
    }
}
