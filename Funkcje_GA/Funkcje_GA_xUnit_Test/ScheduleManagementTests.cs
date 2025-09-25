using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Funkcje_GA;
using Moq;
using Xunit;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA_xUnit_Test
{
    //Testy jednostkowe klasy ScheduleManagement.
    public class ScheduleManagementTests
    {
        //Tworzenie mocka z wieloma pracownikami.
        public static Mock<IEmployeeManagement> CreateWithEmployees(int count)
        {
            var mock = new Mock<IEmployeeManagement>();
            var employees = new List<Employee>();

            for (int i = 1; i <= count; i++)
            {
                var emp = new Employee(i, $"Imie{i}", $"Nazwisko{i}", 0.0, 0, true, false);
                employees.Add(emp);

                // GetEmployeeById
                mock.Setup(m => m.GetEmployeeById(i)).Returns(emp);
            }

            // Zwracamy wszystkich aktywnych
            mock.Setup(m => m.GetAllActive()).Returns(employees);

            // Symulacja edycji pracownika
            mock.Setup(m => m.EmployeeEdit(It.IsAny<Employee>(), It.IsAny<double>())).Callback<Employee, double>((emp, newWymiar) =>
            {
                emp.WymiarEtatu = newWymiar;
            }).Verifiable(); 

            return mock;
        }

        //Helper: zakodowanie numeru pracownika na bool[]
        private bool[] EncodeEmployeeNumber(int number, int bits)
        {
            bool[] result = new bool[bits];
            for (int i = bits - 1; i >= 0; i--)
            {
                result[i] = (number & 1) == 1;
                number >>= 1;
            }
            return result;
        }

        //Dodawanie pracownika do zmiany.
        [Fact]
        public void AddToShift_ValidData_ShouldAddEmployee()
        {
            // Arrange
            var mockEmployeeManager = CreateWithEmployees(1);
            var schedule = new ScheduleManagement(mockEmployeeManager.Object);

            // Act
            schedule.AddToShift(0, 1);

            // Assert
            var shift = schedule.GetShiftById(0);

            // Sprawdzenie, że pracownik został dodany do zmiany
            Assert.Single(shift.PresentEmployees);
            Assert.Equal(1, shift.PresentEmployees[0].Numer);

            // Sprawdzenie, że EmployeeEdit został wywołany dokładnie raz z poprawnym wymiarem etatu
            mockEmployeeManager.Verify(m => m.EmployeeEdit(It.Is<Employee>(e => e.Numer == 1), 1.0), Times.Once);
        }

        //Dodawanie pracownika, który już jest na zmianie.
        [Fact]
        public void AddToShift_ShouldNotAddEmployee_IfAlreadyInShift()
        {
            // Arrange
            var mockEmployeeManager = CreateWithEmployees(1);
            var schedule = new ScheduleManagement(mockEmployeeManager.Object);

            // Dodajemy pracownika przed testem
            schedule.AddToShift(0, 1);

            // Act
            schedule.AddToShift(0, 1); // próbujemy dodać ponownie

            // Assert
            var shift = schedule.GetShiftById(0);

            // Sprawdzenie, że lista pracowników nie zwiększyła się
            Assert.Single(shift.PresentEmployees);

            // EmployeeEdit nie został wywołany ponownie
            mockEmployeeManager.Verify(m => m.EmployeeEdit(It.IsAny<Employee>(), It.IsAny<double>()), Times.Once);
        }

        //Wyjątek rzucany przez EmployeeEdit.
        [Fact]
        public void AddToShift_ShouldThrow_WhenEmployeeEditThrows()
        {
            // Arrange
            var mockEmployeeManager = CreateWithEmployees(1);

            //EmployeeEdit rzuca wyjątek.
            mockEmployeeManager.Setup(m => m.EmployeeEdit(It.IsAny<Employee>(), It.IsAny<double>())).Throws(new System.Exception("Błąd"));

            var schedule = new ScheduleManagement(mockEmployeeManager.Object);

            // Act + Assert
            var ex = Assert.Throws<ScheduleEmployeeManagerException>(() => schedule.AddToShift(0, 1));
            Assert.Contains("Nie udało się dodać pracownika", ex.Message);
        }

        //Dodawanie do zmiany - niepoprawne dane Id.
        [Theory]
        [InlineData(-1, 1)] // niepoprawny shiftId
        [InlineData(0, 0)]  // niepoprawny employeeId
        [InlineData(1000, 1)] // shiftId poza zakresem
        [InlineData(0, 1000)] // employeeId poza zakresem
        public void AddToShift_InvalidId_ShouldThrow(int shiftId, int employeeId)
        {
            // Arrange
            var mockEmployeeManager = CreateWithEmployees(1);
            var schedule = new ScheduleManagement(mockEmployeeManager.Object);

            // Act + Assert
            if (shiftId < 0 || shiftId >= 2 * LICZBA_DNI)
            {
                Assert.Throws<ScheduleInvalidScheduleIdException>(() => schedule.AddToShift(shiftId, employeeId));
            }
            else
            {
                Assert.Throws<ScheduleInvalidEmployeeIdException>(() => schedule.AddToShift(shiftId, employeeId));
            }
        }

        //Przypisanie funkcji - poprawne dane.
        [Fact]
        public void ApplyOptimizationToSchedule_AssignsSalaAndTriazCorrectly()
        {
            // Arrange
            var employeeManagerMock = CreateWithEmployees(3);
            var scheduleManager = new ScheduleManagement(employeeManagerMock.Object);
            int TestShiftId = 0;        //Testowy numer zmiany.

            // Dodajemy pracowników do zmiany
            scheduleManager.AddToShift(TestShiftId, 1);
            scheduleManager.AddToShift(TestShiftId, 2);
            scheduleManager.AddToShift(TestShiftId, 3);

            var shift = scheduleManager.GetShiftById(TestShiftId);

            // Tworzymy optymalneRozwiazanie dla jednej zmiany
            bool[] optymalneRozwiazanie = new bool[2 * LICZBA_DNI * 3 * MAX_LICZBA_BITOW];

            // Zakoduj numer pracowników: Sala=0, Triaz1=1, Triaz2=2
            Array.Copy(EncodeEmployeeNumber(0, MAX_LICZBA_BITOW), 0, optymalneRozwiazanie, 0, MAX_LICZBA_BITOW);
            Array.Copy(EncodeEmployeeNumber(1, MAX_LICZBA_BITOW), 0, optymalneRozwiazanie, MAX_LICZBA_BITOW, MAX_LICZBA_BITOW);
            Array.Copy(EncodeEmployeeNumber(2, MAX_LICZBA_BITOW), 0, optymalneRozwiazanie, 2 * MAX_LICZBA_BITOW, MAX_LICZBA_BITOW);

            // Act
            scheduleManager.ApplyOptimizationToSchedule(optymalneRozwiazanie);

            // Assert
            Assert.Contains(shift.SalaEmployees, e => e.Numer == 1);
            Assert.Contains(shift.TriazEmployees, e => e.Numer == 2);
            Assert.Contains(shift.TriazEmployees, e => e.Numer == 3);

            // Sprawdzenie, że żadna osoba nie ma przypisanych dwóch funkcji
            Assert.DoesNotContain(shift.SalaEmployees, e => shift.TriazEmployees.Contains(e));
        }

        //Przypisanie funkcji - jeden pracownik ma dwie funkcje.
        [Fact]
        public void ApplyOptimizationToSchedule_AssignsTwoFunctionsToSamePerson_ShouldThrow()
        {
            // Arrange
            var employeeManagerMock = CreateWithEmployees(3);
            var scheduleManager = new ScheduleManagement(employeeManagerMock.Object);
            int TestShiftId = 0;        //Testowy numer zmiany.

            // Dodajemy pracowników do zmiany
            scheduleManager.AddToShift(TestShiftId, 1);
            scheduleManager.AddToShift(TestShiftId, 2);
            scheduleManager.AddToShift(TestShiftId, 3);

            var shift = scheduleManager.GetShiftById(TestShiftId);

            // Tworzymy optymalneRozwiazanie dla jednej zmiany
            bool[] optymalneRozwiazanie = new bool[2 * LICZBA_DNI * 3 * MAX_LICZBA_BITOW];

            // Zakoduj numer pracowników: Sala=0, Triaz1=1, Triaz2=2
            Array.Copy(EncodeEmployeeNumber(0, MAX_LICZBA_BITOW), 0, optymalneRozwiazanie, 0, MAX_LICZBA_BITOW);
            Array.Copy(EncodeEmployeeNumber(0, MAX_LICZBA_BITOW), 0, optymalneRozwiazanie, MAX_LICZBA_BITOW, MAX_LICZBA_BITOW);
            Array.Copy(EncodeEmployeeNumber(2, MAX_LICZBA_BITOW), 0, optymalneRozwiazanie, 2 * MAX_LICZBA_BITOW, MAX_LICZBA_BITOW);

            //Act & Assert.
            var ex = Assert.Throws<ScheduleFunctionEncodingException>(() => scheduleManager.ApplyOptimizationToSchedule(optymalneRozwiazanie)
            );
            Assert.Contains("ma przypisane dwie funkcje", ex.Message);
        }

        //Przypisanie funkcji - przypisanie do niestniejącego pracownika.
        [Fact]
        public void ApplyOptimization_AssignsFunctionToNonExistingPerson_ShouldThrow()
        {
            // Arrange
            var mock = CreateWithEmployees(2);
            var scheduleManager = new ScheduleManagement(mock.Object);
            scheduleManager.AddToShift(0, 1);
            scheduleManager.AddToShift(0, 2);

            // Triaz2 = 2 (nie istnieje, bo PresentEmployees.Count = 2 -> indeks max 1)
            bool[] optymalneRozwiazanie = new bool[2 * LICZBA_DNI * 3 * MAX_LICZBA_BITOW];

            // Zakoduj numer pracowników: Sala=0, Triaz1=1.
            Array.Copy(EncodeEmployeeNumber(0, MAX_LICZBA_BITOW), 0, optymalneRozwiazanie, 0, MAX_LICZBA_BITOW);
            Array.Copy(EncodeEmployeeNumber(1, MAX_LICZBA_BITOW), 0, optymalneRozwiazanie, MAX_LICZBA_BITOW, MAX_LICZBA_BITOW);
            Array.Copy(EncodeEmployeeNumber(2, MAX_LICZBA_BITOW), 0, optymalneRozwiazanie, MAX_LICZBA_BITOW, MAX_LICZBA_BITOW);

            // Act & Assert
            var ex = Assert.Throws<ScheduleFunctionEncodingException>(() => scheduleManager.ApplyOptimizationToSchedule(optymalneRozwiazanie)
            );
            Assert.Contains("jest większy bądź równy liczbie pracowników", ex.Message);
        }

        //Przypisanie funkcji - poprawne dane.
        [Theory]
        [InlineData(FunctionTypes.Sala)]
        [InlineData(FunctionTypes.Triaz)]
        [InlineData(FunctionTypes.Bez_Funkcji)]
        public void AssignFunctionToEmployee_ShouldDo(FunctionTypes function)
        {
            // Arrange
            var mock = CreateWithEmployees(1);
            var scheduleManager = new ScheduleManagement(mock.Object);
            scheduleManager.AddToShift(0, 1);

            bool shiftChangedRaised = false;
            scheduleManager.ShiftChanged += (shift) => shiftChangedRaised = true;

            // Act
            scheduleManager.AssignFunctionToEmployee(0, 1, function);

            var shift = scheduleManager.GetShiftById(0);

            // Assert
            switch (function)
            {
                case FunctionTypes.Sala:
                    Assert.Contains(shift.SalaEmployees, e => e.Numer == 1);
                    Assert.DoesNotContain(shift.TriazEmployees, e => e.Numer == 1);
                    break;
                case FunctionTypes.Triaz:
                    Assert.Contains(shift.TriazEmployees, e => e.Numer == 1);
                    Assert.DoesNotContain(shift.SalaEmployees, e => e.Numer == 1);
                    break;
                case FunctionTypes.Bez_Funkcji:
                    Assert.DoesNotContain(shift.SalaEmployees, e => e.Numer == 1);
                    Assert.DoesNotContain(shift.TriazEmployees, e => e.Numer == 1);
                    break;
            }

            // Sprawdzenie, czy event został wywołany, jeśli była zmiana
            if (function != FunctionTypes.Bez_Funkcji)
            {
                Assert.True(shiftChangedRaised);
            }
        }

        //Przypisanie funkcji, gdy na zmianie nie ma danego pracownika.
        [Fact]
        public void AssignFunctionToEmployee_EmployeeNotOnShift_DoesNothing()
        {
            // Arrange
            var mock = CreateWithEmployees(1);
            var schedManager = new ScheduleManagement(mock.Object);

            bool shiftChangedRaised = false;
            schedManager.ShiftChanged += (shift) => shiftChangedRaised = true;

            // Act
            // Pracownik 1 nie został dodany do zmiany 0
            schedManager.AssignFunctionToEmployee(0, 1, FunctionTypes.Sala);

            var shift = schedManager.GetShiftById(0);

            // Assert
            Assert.Empty(shift.SalaEmployees);
            Assert.Empty(shift.TriazEmployees);
            Assert.False(shiftChangedRaised);
        }

        //Pobranie zmiany - poprawny numer.
        [Fact]
        public void GetShiftById_ShouldDo()
        {
            // Arrange
            var mock = CreateWithEmployees(1);
            var schedManager = new ScheduleManagement(mock.Object);
            int validId = 0;

            // Act
            var shift = schedManager.GetShiftById(validId);

            // Assert
            Assert.NotNull(shift);
            Assert.Equal(validId, shift.Id);
        }

        //Pobranie zmiany - błędny numer.
        [Fact]
        public void GetShiftById_InvalidId_ShouldThrow()
        {
            // Arrange
            var mock = CreateWithEmployees(1);
            var schedManager = new ScheduleManagement(mock.Object);
            int invalidId = 2 * LICZBA_DNI + 1; // Poza zakresem

            // Act & Assert
            Assert.Throws<ScheduleInvalidScheduleIdException>(() => schedManager.GetShiftById(invalidId));
        }

        //Sprawdzamy pobieranie zmian danego pracownika dla poprawnych danych.
        [Fact]
        public void GetShiftsForEmployee_ShouldDo()
        {
            // Arrange
            var mockEmployeeManager = CreateWithEmployees(3); // 3 pracowników
            var scheduleManagement = new ScheduleManagement(mockEmployeeManager.Object);

            // Dodajemy pracownika 1 do 3 zmian z różnymi funkcjami
            scheduleManagement.AddToShift(0, 1);
            scheduleManagement.AssignFunctionToEmployee(0, 1, FunctionTypes.Bez_Funkcji);

            scheduleManagement.AddToShift(1, 1);
            scheduleManagement.AssignFunctionToEmployee(1, 1, FunctionTypes.Sala);

            scheduleManagement.AddToShift(2, 1);
            scheduleManagement.AssignFunctionToEmployee(2, 1, FunctionTypes.Triaz);

            // Act
            var result = scheduleManagement.GetShiftsForEmployee(1).ToList();

            // Assert
            Assert.Equal(3, result.Count); // powinno być 3 zmiany
            Assert.Contains(result, r => r.shiftId == 0 && r.function == (int)FunctionTypes.Bez_Funkcji);
            Assert.Contains(result, r => r.shiftId == 1 && r.function == (int)FunctionTypes.Sala);
            Assert.Contains(result, r => r.shiftId == 2 && r.function == (int)FunctionTypes.Triaz);
        }

        //Pobieranie zmian danego pracownika - id poza zakresem.
        [Fact]
        public void GetShiftsForEmployee_WhenEmployeeIdIsInvalid_ShouldThrow()
        {
            // Arrange
            var mockEmployeeManager = CreateWithEmployees(3);
            var scheduleManagement = new ScheduleManagement(mockEmployeeManager.Object);

            int invalidEmployeeId = MAX_LICZBA_OSOB + 1; // Id, które nie istnieje

            // Act & Assert
            var exception = Assert.Throws<ScheduleInvalidEmployeeIdException>(
                () => scheduleManagement.GetShiftsForEmployee(invalidEmployeeId)
            );

            Assert.Contains("niepoprawny", exception.Message);
        }

        //Pobieranie zmian danego pracownika - pracownik nie istnieje.
        [Fact]
        public void GetShiftsForEmployee_WhenEmployeeDoesNotExistInManager_ShouldThrow()
        {
            // Arrange
            var mockEmployeeManager = new Mock<IEmployeeManagement>();
            #pragma warning disable 8600, 8625
            mockEmployeeManager.Setup(m => m.GetEmployeeById(It.IsAny<int>())).Returns((Employee)null); // Zwraca null
            #pragma warning restore 8600, 8625
            var scheduleManagement = new ScheduleManagement(mockEmployeeManager.Object);

            int employeeId = 1;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => scheduleManagement.GetShiftsForEmployee(employeeId)
            );

            Assert.Contains("nie istnieje", exception.Message);
        }

        //Czyszczenie grafiku.
        [Fact]
        public void RemoveAll_ShouldDo()
        {
            // Arrange
            var mockEmployeeManager = CreateWithEmployees(3);
            var scheduleManager = new ScheduleManagement(mockEmployeeManager.Object);

            // Dodajemy pracowników do zmian, aby mieć dane do czyszczenia
            scheduleManager.AddToShift(0, 1);
            scheduleManager.AddToShift(0, 2);
            scheduleManager.AddToShift(1, 3);

            var shiftChangedCalls = new List<Shift>();
            scheduleManager.ShiftChanged += shift => shiftChangedCalls.Add(shift);

            // Act
            scheduleManager.RemoveAll();

            // Assert
            // 1. Wszystkie listy pracowników w zmianach są puste
            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                var shift = scheduleManager.GetShiftById(i);
                Assert.Empty(shift.PresentEmployees);
                Assert.Empty(shift.SalaEmployees);
                Assert.Empty(shift.TriazEmployees);
            }

            // 2. ShiftChanged wywołany dla każdej zmiany
            Assert.Equal(2 * LICZBA_DNI, shiftChangedCalls.Count);

            // 3. EmployeeEdit został wywołany dla każdego pracownika z 0.0
            mockEmployeeManager.Verify(m => m.EmployeeEdit(It.IsAny<Employee>(), 0.0), Times.Exactly(3));
        }

        //Usuwanie pracownika.
        [Fact]
        public void RemoveFromShift_ShouldDo()
        {
            // Arrange
            var mockEmployeeManager = CreateWithEmployees(2);
            var scheduleManager = new ScheduleManagement(mockEmployeeManager.Object);

            var employee = mockEmployeeManager.Object.GetEmployeeById(1);

            int shiftId = 0;
            int employeeIdSala = 1;
            int employeeIdTriaz = 2;

            // Dodaj obu pracowników do zmiany
            scheduleManager.AddToShift(shiftId, employeeIdSala);
            scheduleManager.AddToShift(shiftId, employeeIdTriaz);

            // Przypisz funkcje
            scheduleManager.AssignFunctionToEmployee(shiftId, employeeIdSala, FunctionTypes.Sala);
            scheduleManager.AssignFunctionToEmployee(shiftId, employeeIdTriaz, FunctionTypes.Triaz);

            var shiftChangedCalled = false;
            scheduleManager.ShiftChanged += shift => shiftChangedCalled = true;

            // Act
            scheduleManager.RemoveFromShift(shiftId, employeeIdSala);

            // Assert
            var shift = scheduleManager.GetShiftById(shiftId);

            //Sprawdzenie, że usunięty pracownik nie ma już żadnej funkcji
            Assert.DoesNotContain(shift.PresentEmployees, e => e.Numer == employeeIdSala);
            Assert.DoesNotContain(shift.SalaEmployees, e => e.Numer == employeeIdSala);
            Assert.DoesNotContain(shift.TriazEmployees, e => e.Numer == employeeIdSala);

            //Sprawdzenie, że drugi pracownik nadal jest w zmianie i zachował funkcję
            Assert.Contains(shift.PresentEmployees, e => e.Numer == employeeIdTriaz);
            Assert.Contains(shift.TriazEmployees, e => e.Numer == employeeIdTriaz);
            Assert.Empty(shift.SalaEmployees);

            //Event i EmployeeEdit
            Assert.True(shiftChangedCalled);
            Assert.Equal(0.0, employee.WymiarEtatu);
            mockEmployeeManager.Verify(m => m.EmployeeEdit(It.Is<Employee>(e => e.Numer == employeeIdSala), 0.0), Times.Once);
        }

        //Usuwanie pracownika - błąd employeeManager.
        [Fact]
        public void RemoveFromShift_WhenEmployeeEditFails_ShouldThrow()
        {
            // Arrange
            var mockEmployeeManager = CreateWithEmployees(1);
            var employee = mockEmployeeManager.Object.GetEmployeeById(1);

            var scheduleManager = new ScheduleManagement(mockEmployeeManager.Object);

            // Dodajemy pracownika do zmiany
            scheduleManager.AddToShift(0, 1);

            // Nadpisujemy zachowanie EmployeeEdit, aby rzucało wyjątek
            mockEmployeeManager.Setup(m => m.EmployeeEdit(It.IsAny<Employee>(), It.IsAny<double>()))
                               .Throws(new InvalidOperationException("Błąd zarządzania pracownikiem"));

            // Act & Assert
            var exception = Assert.Throws<ScheduleEmployeeManagerException>(() => scheduleManager.RemoveFromShift(0, 1));

            // Weryfikacja treści wyjątku i typu InnerException
            Assert.Contains("Nie udało się dodać pracownika do zmiany", exception.Message);
            Assert.IsType<InvalidOperationException>(exception.InnerException);
        }
    }
}
