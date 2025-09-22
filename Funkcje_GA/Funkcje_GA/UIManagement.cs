using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using static Funkcje_GA.Constans;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klasa odpowiada za zarządzanie wyświetlaniem grafiku i informacji o pracownikach na UI.
    public class UIManagement : IUIManagement
    {
        private readonly Dictionary<int, (string, int)> uiEmployeesControls;           //Tu przechowywane są kontrolki z danymi pracowników.
        private readonly Dictionary<int, List<string>> uiScheduleControls;                                    //Tu przechowywane są kontrolki z danymi grafiku.

        private readonly IEmployeeManagement _employeeManager;
        private readonly IScheduleManagement _scheduleManager;
        private readonly IEmployeesFileService _fileManagerPracownicy;
        private readonly IScheduleFileService _fileManagerGrafik;
        private readonly IOptimization _optimization;

        //Konstruktor.
        public UIManagement(IEmployeeManagement employeeManager, 
                                 IScheduleManagement scheduleManager, 
                                 IEmployeesFileService fileManagerPracownicy,
                                 IScheduleFileService filemanagerGrafik,
                                 IOptimization optimization)
        {
            this._employeeManager = employeeManager;
            this._scheduleManager = scheduleManager;
            this._fileManagerPracownicy = fileManagerPracownicy;
            this._fileManagerGrafik = filemanagerGrafik;
            this._optimization = optimization;

            uiEmployeesControls = new Dictionary<int, (string, int)>(MAX_LICZBA_OSOB);     //Inicjalizujemy zestaw kontrolek pracowników.
            uiScheduleControls = new Dictionary<int, List<string>> (2 * LICZBA_DNI);                               //Inicjalizujemy zestaw kontrolek grafiku.

            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                List<string> list = new List<string>();
                uiScheduleControls[i] = list;
            }

            //Subskrybujemy akcję zakończenia optymalizacji.
            EventGlobal.OptimizationFinishedSuccesfuly += (optymalneRozwiazanie) => _scheduleManager.DodajFunkcje(optymalneRozwiazanie);
        }

        //Akcja zmiana danych pracownika.
        public event Action<int, (string, int)> EmployeeLabelChanged;

        //Akcja zmiana kontrolki grafiku.
        public event Action<int, List<string>> ScheduleControlChanged;

        //Podświetlanie etykiet podczas drag and drop.
        public event Action<int, Color> ScheduleHighlightRaise;

        //Powiadomienie użytkownika.
        public event Action<string> UserNotificationRaise;

        //Dodawanie pracownika do zmiany.
        public void AddEmployeeToShift(int shiftId, int employeeId) => _scheduleManager.AddToShift(shiftId, employeeId);

        //Czyścimy kontrolkę z danymi pracownika.
        public void ClearEmployeeData(int id)
        {
            //Sprawdzamy, czy id kontrolki jest poprawne. Jeśli tak, to resetujemy tekst.
            if (id < 0 || id >= MAX_LICZBA_OSOB) throw new UIInvalidEmployeeControlIdException("Wybrano niepoprawny numer kontrolki");

            else  uiEmployeesControls[id] = ("", 0);

            //Wywołujemy zdarzenie uaktualniono opis pracownika.
            EmployeeLabelChanged?.Invoke(id, uiEmployeesControls[id]);

        }

        //Czyszczenie grafiku.
        public void ClearSchedule()
        {
            //Usuwamy grafik.
            _scheduleManager.RemoveAll();

            //Odświeżamy wszystkie kontrolki grafiku.
            for (int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
            {
                ScheduleControlChanged?.Invoke(shiftId, new List<string>());
            }

            // Powiadamiamy View
            UserNotificationRaise?.Invoke("Grafik usunięty");
        }

        //Funkcja realizowana po naciśnięciu etykiety pracownika.
        public void HandleEmployeeMouseDown(int employeeId)
        {
            //Pobieramy pracownika.
            var employee = _employeeManager.GetEmployeeById(employeeId + 1);
            if (employee == null) return;

            //Sprawdzamy funkcje i wyświetlamy kolor. Bez funkcji - czerwony, sala - zielony, triaż - niebieski.
            var shifts = _scheduleManager.GetShiftsForEmployee(employee.Numer);
            foreach (var (shiftId, function) in shifts)
            {
                Color color;

                switch (function)
                {
                    case (int)FunctionTypes.Bez_Funkcji:
                        color = Color.Red;
                        break;

                    case (int)FunctionTypes.Sala:
                        color = Color.Green;
                        break;

                    case (int)FunctionTypes.Triaz:
                        color = Color.Blue;
                        break;

                    default:
                        color = Color.White;
                        break;
                }

                //Event podśiwetlenie.
                ScheduleHighlightRaise?.Invoke(shiftId, color);
            }
        }

        //Załaduj pracowników.
        public void LoadEmployees(string filePath)
        {
            try
            {
                _fileManagerPracownicy.WczytajPracownikow(filePath);
            }
            catch (Exception ex)
            {
                throw new UIInvalidEmployeeFileException($"Plik {filePath} jest uszkodzony. Napraw go lub usuń. {ex.Message}", ex);
            }
        }

        //Załaduj schedule.
        public void LoadSchedule(string filePath)
        {
            //Próbujemy wczytać grafik
            try
            {
                _fileManagerGrafik.WczytajGrafik(filePath);
                UserNotificationRaise("Grafik wczytany.");
            }

            catch (Exception ex)
            {
                throw new UIInvalidScheduleFileException($"Plik {filePath} jest uszkodzony. Napraw go lub usuń. {ex.Message}", ex);
            }
        }

        //Przeprowadzamy optymalizację.
        public async Task RunOptimizationAsync()
        {
            try
            {
                int liczbaOsobnikow = 100;          //Liczba osobników.
                decimal tol = 0.0000003m;           //Jeśli wartość funkcji celu jest mniejsza lub równa tol, to przerywamy optymalizację.
                decimal tolX = 0.00000000001m;      //Minimalna zmiana funkcji celu powodująca zresetowanie liczby iteracji bez poprawy.
                int maxIterations = 200000;         //Maksymalna liczba iteracji.
                int maxConsIterations = 40000;      //maksymalna liczba iteracji bez poprawy.

                //Przygotowujemy optymalizację.
                _optimization.Prepare();

                //Startujemy optymalizację i mierzymy czas.
                var startTime = DateTime.Now;
                bool[] optymalneRozwiazanie = await Task.Run(() =>
                    _optimization.OptymalizacjaGA(LICZBA_ZMIENNYCH, liczbaOsobnikow, tol, tolX, maxConsIterations, maxIterations)
                );
                var czasOptymalizacja = DateTime.Now - startTime;

                //Jeśli optymalizacja ukończona pomyslnie to wczytujemy funkcje, zapisujemy grafik i powiadamiamy użytkownika.
                EventGlobal.RaiseOptimizationFinishedSuccesfuly(optymalneRozwiazanie);
                _fileManagerGrafik.ZapiszGrafik("GrafikGA.txt");
                UserNotificationRaise?.Invoke($"Przydzielanie funkcji ukończone w: {czasOptymalizacja}.");
            }

            //Jeśli grafik był zły, to powiadamiamy.
            catch (OptimizationInvalidScheduleException ex)
            {
                Log.Error(ex.Message);
                UserNotificationRaise?.Invoke($"Aby przeprowadzić przydzielanie funkcji na każdej zmianie musi być od 3 do {MAX_LICZBA_DYZUROW}.");
            }

            //Jeśli liczba zmiennych była zła to powiadamiamy.
            catch (OptimizationInvalidDataException ex)
            {
                Log.Error(ex.Message);
                UserNotificationRaise?.Invoke("Liczba zmiennych musi być większa niż 0.");
            }

            //Jeśli był inny błąd to powiadamiamy.
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                UserNotificationRaise?.Invoke(ex.Message);
            }
        }

        //Usuwamy zaznaczone dyżury.
        public void RemoveSelectedShifts(IEnumerable<(int ShiftId, int EmployeeId)> selected)
        {
            foreach (var (shiftId, employeeId) in selected)
            {
                _scheduleManager.RemoveFromShift(shiftId, employeeId);

                //Odśwież UI dla tej zmiany.
                Shift shift = _scheduleManager.GetShiftById(shiftId);
            }
        }

        //Zapisz grafik.
        public void SaveSchedule(string filePath)
        {
            //Próbujemy zapisać grafik
            try
            {
                _fileManagerGrafik.ZapiszGrafik(filePath);
                UserNotificationRaise("Grafik zapisany.");
            }

            catch (Exception ex)
            {
                throw new UIInvalidScheduleFileException($"Plik {filePath} jest uszkodzony. Napraw go lub usuń. {ex.Message}", ex);
            }
        }

        //Przypisujemy wybranym pracownikom brak funkcji.
        public void SetSelectedShiftsToBezFunkcji(IEnumerable<(int ShiftId, int EmployeeId)> selected)
        {
            foreach (var (shiftId, employeeId) in selected)
                _scheduleManager.ToBezFunkcji(shiftId, employeeId);
        }

        //Przypisujemy wybranym pracownikom sale.
        public void SetSelectedShiftsToSala(IEnumerable<(int ShiftId, int EmployeeId)> selected)
        {
            foreach (var (shiftId, employeeId) in selected)
                _scheduleManager.ToSala(shiftId, employeeId);
        }

        //Przypisujemy wybranym pracownikom triaz.
        public void SetSelectedShiftsToTriaz(IEnumerable<(int ShiftId, int EmployeeId)> selected)
        {
            foreach (var (shiftId, employeeId) in selected)
                _scheduleManager.ToSala(shiftId, employeeId);
        }


        //Wyświetlamy dane wybranej zmiany.
        public void UpdateScheduleControl(Shift shift)
        {
            //Czyścimy kontrolkę.
            if (uiScheduleControls.TryGetValue(shift.Id, out List<string> list))
            {
                list.Clear();
            }

            //Wyświetlamy pracowników
            if (shift.PresentEmployees.Count > 0)
            {
                //Próbujemy dodać pracowników do kontrolki.
                try
                {
                    for (int nrOsoby = 0; nrOsoby < shift.PresentEmployees.Count; nrOsoby++)
                    {
                        //Dopisujemy pracownika, jeśli ma salę.
                        if(shift.SalaEmployees.Any(emp => (shift.PresentEmployees[nrOsoby].Numer == emp.Numer)))
                            uiScheduleControls[shift.Id].Add(shift.PresentEmployees[nrOsoby].Numer.ToString() + "s");

                        //Dopisujemy pracownika, jeśli ma triaż.
                        else if (shift.TriazEmployees.Any(emp => (shift.PresentEmployees[nrOsoby].Numer == emp.Numer)))
                            uiScheduleControls[shift.Id].Add(shift.PresentEmployees[nrOsoby].Numer.ToString() + "t");

                        //Dopisujemy pracownika, jeśli nie ma funkcji.
                        else
                            uiScheduleControls[shift.Id].Add(shift.PresentEmployees[nrOsoby].Numer.ToString());

                    }
                }

                //Jeśli się nie udało to rzucamy wyjatek.
                catch(Exception ex)
                {
                    throw new FormatException($"Kontrolka: {shift.Id} ma niepoprawne dane {ex.Message}.", ex);
                };
            }
            //Podnosimy zdarzenie, zmiana kontrolki grafiku.
            ScheduleControlChanged?.Invoke(shift.Id, uiScheduleControls[shift.Id]);
        }

        //Wyświetlanie informacji o pracowniku na etykiecie.
        public void UpdateEmployeeLabel(Employee employee)
        {
            if (employee != null)
            {
                //Sprawdzamy, czy id kontrolki jest poprawne. Jeśli nie, to rzucamy wyjątek.
                if (employee.Numer - 1 < 0 || employee.Numer - 1 >= MAX_LICZBA_OSOB) throw new UIInvalidEmployeeControlIdException("Wybrano niepoprawny numer kontrolki");

                //Aktualizujemy pojedynczą etykietę.
                string employeeData = employee.Numer.ToString() + ". "
                                    + employee.Imie + " " + employee.Nazwisko + " "
                                    + employee.WymiarEtatu.ToString() + " "
                                    + employee.Zaleglosci.ToString();

                //Jeśli osoba jest stażystą i nie może być na triażu podczas zmiany to podświetlamy na pomarańczowo.
                if (employee.CzyTriazDzien &&  employee.CzyTriazNoc)
                    uiEmployeesControls[employee.Numer - 1] = (employeeData, 0);

                else
                    uiEmployeesControls[employee.Numer - 1] = (employeeData, 1);
            }

            //Wywołujemy zdarzenie auktualniono opis pracownika.
            EmployeeLabelChanged?.Invoke(employee.Numer - 1, uiEmployeesControls[employee.Numer - 1]);
        }
    }
}
