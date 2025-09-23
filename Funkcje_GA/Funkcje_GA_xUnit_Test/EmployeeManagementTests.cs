using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Funkcje_GA;
using static Funkcje_GA.Constans;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA_xUnit_Test
{
    public class EmployeeManagementTests
    {
        //Testujemy dodawanie pracowników.
        [Theory]
        [InlineData(1, "Jan", "Kowalski", 10.0, 0, true, true)]
        [InlineData(2, "Anna", "Nowak", 9.0, 2, false, true)]
        [InlineData(3, "Piotr", "Zielinski", 5.0, -2, true, false)]
        public void EmployeeAdd_ValidData_ShouldAddEmployee(int numer, string imie, string nazwisko, double etat, int zaleglosci, bool dzien, bool noc)
        {
            // Arrange
            var manager = new EmployeeManagement();

            // Act
            manager.EmployeeAdd(numer, imie, nazwisko, etat, zaleglosci, dzien, noc);
            var addedEmployee = manager.GetEmployeeById(numer);

            // Assert
            Assert.NotNull(addedEmployee);
            Assert.Equal(imie, addedEmployee.Imie);
            Assert.Equal(nazwisko, addedEmployee.Nazwisko);
            Assert.Equal(etat, addedEmployee.WymiarEtatu);
            Assert.Equal(zaleglosci, addedEmployee.Zaleglosci);
            Assert.Equal(dzien, addedEmployee.CzyTriazDzien);
            Assert.Equal(noc, addedEmployee.CzyTriazNoc);
        }

        //Testujemy wyjątki przy dodawaniu pracownika.
        [Theory]
        // Numer istniejący
        [InlineData(1, "Jan", "Kowalski", 1.0, 0, true, true, typeof(EmployeeAlreadyExistException))]
        // Lista pełna
        [InlineData(MAX_LICZBA_OSOB + 1, "Adam", "Nowak", 1.0, 0, true, true, typeof(TooManyEmployeesException))]
        // Numer spoza zakresu (0)
        [InlineData(0, "Jan", "Kowalski", 1.0, 0, true, true, typeof(EmployeeNumberOutOfRangeException))]
        // Numer spoza zakresu (MAX+1)
        [InlineData(MAX_LICZBA_OSOB + 1, "Adam", "Nowak", 1.0, 0, true, true, typeof(EmployeeNumberOutOfRangeException))]
        // Wymiar etatu < 0
        [InlineData(1, "Jan", "Kowalski", -1.0, 0, true, true, typeof(InvalidDataException))]
        // Zaległości < -MAX
        [InlineData(1, "Jan", "Kowalski", 1.0, -MAX_MIN_ZALEGLOSCI - 1, true, true, typeof(InvalidDataException))]
        // Zaległości > MAX
        [InlineData(1, "Adam", "Nowak", 1.0, MAX_MIN_ZALEGLOSCI + 1, true, true, typeof(InvalidDataException))]
        public void EmployeeAdd_InvalidData_ShouldThrow(int numer, string imie, string nazwisko, double wymiarEtatu, int zaleglosci, bool triazDzien, bool triazNoc, Type expectedException)
        {
            // Arrange
            var manager = new EmployeeManagement();

            // Dla testu numer istniejący i lista pełna musimy wstępnie dodać dane
            if (numer == 1)
            {
                manager.EmployeeAdd(1, "Existing", "Employee", 1.0, 0, true, true);
            }
            if (expectedException == typeof(TooManyEmployeesException))
            {
                for (int i = 1; i <= MAX_LICZBA_OSOB; i++)
                {
                    manager.EmployeeAdd(i, $"Imie{i}", $"Nazwisko{i}", 1.0, 0, true, true);
                }
            }

            // Act & Assert
            Assert.Throws(expectedException, () =>
                manager.EmployeeAdd(numer, imie, nazwisko, wymiarEtatu, zaleglosci, triazDzien, triazNoc));
        }

        //Testujemy wyjątek przy niepoprawnym imieniu/nazwisku pracownika.
        [Theory]
        [InlineData("", "Kowalski")]      // puste imię
        [InlineData("Jan", "")]           // puste nazwisko
        [InlineData("Jan ", "Kowalski")]  // spacja w imieniu
        [InlineData("Jan", "Ko wal ski")] // spacja w nazwisku
        public void EmployeeAdd_InvalidName_ShouldThrow(string imie, string nazwisko)
        {
            // Arrange
            var manager = new EmployeeManagement();

            // Act & Assert
            Assert.Throws<EmployeeNameSurnameException>(() =>
                manager.EmployeeAdd(1, imie, nazwisko, 1.0, 0, true, true));
        }

        //Testujemy zmianę samego wymiaru etatu.
        [Fact]
        public void EmployeeEdit_WymiarEtatu_ShouldUpdate()
        {
            var manager = new EmployeeManagement();
            manager.EmployeeAdd(1, "Jan", "Kowalski", 1.0, 0, true, true);

            var employee = manager.GetEmployeeById(1);
            manager.EmployeeEdit(employee, 0.5);

            Assert.Equal(0.5, employee.WymiarEtatu);
        }

        //Testujemy zmianę pełnych danych.
        [Fact]
        public void EmployeeEdit_FullData_ShouldUpdateAllFields()
        {
            // Arrange
            var manager = new EmployeeManagement();
            manager.EmployeeAdd(1, "Jan", "Kowalski", 1.0, 0, true, false);
            var employee = manager.GetEmployeeById(1);

            // Act
            manager.EmployeeEdit(employee, "Adam", "Nowak", 0.8, 2, false, true);

            // Assert
            Assert.Equal("Adam", employee.Imie);
            Assert.Equal("Nowak", employee.Nazwisko);
            Assert.Equal(0.8, employee.WymiarEtatu);
            Assert.Equal(2, employee.Zaleglosci);
            Assert.False(employee.CzyTriazDzien);
            Assert.True(employee.CzyTriazNoc);
        }

        //Testujemy wyjątki przy zmianie wszystkich danych.
        [Theory]
        // Edycja wymiaru etatu < 0
        [InlineData(1, "Jan", "Kowalski", -0.5, 0, true, true, typeof(InvalidDataException))]
        // Edycja zaległości < -10
        [InlineData(1, "Jan", "Kowalski", 1.0, -MAX_MIN_ZALEGLOSCI - 1, true, true, typeof(InvalidDataException))]
        // Edycja zaległości > 10
        [InlineData(1, "Jan", "Kowalski", 1.0, MAX_MIN_ZALEGLOSCI + 1, true, true, typeof(InvalidDataException))]
        // Edycja imienia z pustym znakiem
        [InlineData(1, "", "Kowalski", 1.0, 0, true, true, typeof(EmployeeNameSurnameException))]
        // Edycja nazwiska z pustym znakiem
        [InlineData(1, "Jan", "", 1.0, 0, true, true, typeof(EmployeeNameSurnameException))]
        // Edycja imienia zawierającego spację
        [InlineData(1, "Ja n", "Kowalski", 1.0, 0, true, true, typeof(EmployeeNameSurnameException))]
        // Edycja nazwiska zawierającego spację
        [InlineData(1, "Jan", "Kowal ski", 1.0, 0, true, true, typeof(EmployeeNameSurnameException))]
        public void EmployeeEdit_InvalidData_ShouldThrow(int numer, string imie, string nazwisko, double wymiarEtatu, int zaleglosci,
                                                         bool triazDzien, bool triazNoc, Type expectedException)
        {
            // Arrange
            var manager = new EmployeeManagement();
            manager.EmployeeAdd(numer, "Jan", "Kowalski", 1.0, 0, true, true);
            var employee = manager.GetEmployeeById(numer);

            // Act & Assert
            Assert.Throws(expectedException, () =>
                manager.EmployeeEdit(employee, imie, nazwisko, wymiarEtatu, zaleglosci, triazDzien, triazNoc));
        }

        //Testujemy usuwanie pracownika.
        [Fact]
        public void EmployeeDelete_ShouldRemoveEmployee()
        {
            // Arrange
            var manager = new EmployeeManagement();
            manager.EmployeeAdd(1, "Jan", "Kowalski", 1.0, 0, true, false);

            // Act
            manager.EmployeeDelete(manager.GetEmployeeById(1));

            // Assert
            Assert.Null(manager.GetEmployeeById(1));
            Assert.Empty(manager.GetAllActive());
        }

        //Usuwanie pracownika, którego nie ma.
        [Fact]
        public void EmployeeDelete_NonExistentEmployee_ShouldThrow()
        {
            // Arrange
            var manager = new EmployeeManagement();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => manager.EmployeeDelete(null));
        }

        //Pobieranie poprawnego pracownika.
        [Fact]
        public void GetEmployeeById_ExistingEmployee_ShouldReturnEmployee()
        {
            // Arrange
            var manager = new EmployeeManagement();
            manager.EmployeeAdd(1, "Jan", "Kowalski", 1.0, 0, true, true);

            // Act
            var employee = manager.GetEmployeeById(1);

            // Assert
            Assert.NotNull(employee);
            Assert.Equal(1, employee.Numer);
            Assert.Equal("Jan", employee.Imie);
            Assert.Equal("Kowalski", employee.Nazwisko);
        }

        //Pobieranie błędnego pracownika.
        [Theory]
        [InlineData(2)]     // nieistniejący pracownik (null)
        [InlineData(0)]     // numer spoza zakresu (za mały)
        [InlineData(MAX_LICZBA_OSOB + 1)]   // numer spoza zakresu (za duży)
        public void GetEmployeeById_NonExistingOrOutOfRange_ShouldHandleProperly(int numer)
        {
            // Arrange
            var manager = new EmployeeManagement();
            manager.EmployeeAdd(1, "Jan", "Kowalski", 1.0, 0, true, true);

            // Act & Assert
            if (numer < 1 || numer > MAX_LICZBA_OSOB)
            {
                // numer spoza zakresu → wyjątek
                Assert.Throws<EmployeeNumberOutOfRangeException>(() => manager.GetEmployeeById(numer));
            }
            else
            {
                // numer w zakresie, ale pracownik nie istnieje → null
                var employee = manager.GetEmployeeById(numer);
                Assert.Null(employee);
            }
        }

        //Pobieramy wszystkich i sprawdzamy, czy zwraca w kolejności.
        [Fact]
        public void GetAllActive_ShouldReturnOnlyNonNullEmployees_InOrder()
        {
            // Arrange
            var manager = new EmployeeManagement();

            // Dodajemy kilka pracowników w nieciągłej kolejności
            manager.EmployeeAdd(2, "Anna", "Nowak", 1.0, 0, true, true);
            manager.EmployeeAdd(4, "Jan", "Kowalski", 1.0, 0, true, false);
            manager.EmployeeAdd(1, "Maria", "Wiśniewska", 1.0, 0, false, true);

            // Act
            var activeEmployees = manager.GetAllActive().ToList();

            // Assert
            Assert.All(activeEmployees, e => Assert.NotNull(e)); // tylko nie-null
            Assert.Equal(3, activeEmployees.Count);               // dokładnie 3 pracowników
            // Sprawdzenie kolejności odpowiadającej numerom w liście
            Assert.Equal(1, activeEmployees[0].Numer);
            Assert.Equal(2, activeEmployees[1].Numer);
            Assert.Equal(4, activeEmployees[2].Numer);
        }

        //testowanie eventów zmiany i usunięcia pracownika.
        [Theory]
        [InlineData("Add", true, false)]   // Add -> EmployeeChanged
        [InlineData("Edit", true, false)]  // Edit -> EmployeeChanged
        [InlineData("Delete", false, true)]// Delete -> EmployeeDeleted
        public void EmployeeManagement_ShouldRaiseEvents(
            string operation,
            bool expectChanged,
            bool expectDeleted)
        {
            // Arrange
            var manager = new EmployeeManagement();
            bool changedRaised = false;
            bool deletedRaised = false;

            manager.EmployeeChanged += (_) => changedRaised = true;
            manager.EmployeeDeleted += (_) => deletedRaised = true;

            // Act
            switch (operation)
            {
                case "Add":
                    manager.EmployeeAdd(1, "Jan", "Kowalski", 1.0, 0, true, false);
                    break;
                case "Edit":
                    manager.EmployeeAdd(1, "Jan", "Kowalski", 1.0, 0, true, false);
                    var emp = manager.GetEmployeeById(1);
                    manager.EmployeeEdit(emp, "Adam", "Nowak", 1.0, 0, true, false);
                    break;
                case "Delete":
                    manager.EmployeeAdd(1, "Jan", "Kowalski", 1.0, 0, true, false);
                    changedRaised = false;
                    manager.EmployeeDelete(manager.GetEmployeeById(1));
                    break;
            }

            // Assert
            Assert.Equal(expectChanged, changedRaised);
            Assert.Equal(expectDeleted, deletedRaised);
        }
    }
}

