using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using static Funkcje_GA.FileServiceTxt;
using static Funkcje_GA.Form1;

namespace Funkcje_GA
{

    internal static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>

        [STAThread]

        //Main
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Tworzymy logger.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("log-.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

            IEmployeeManagement _employeeManager = new EmployeeManagement();                             //Instancja do zarządzania pracownikami. 
            IScheduleManagement _scheduleManager = new ScheduleManagement(_employeeManager);            //Instancja do zarządzania grafikiem.
            IScheduleFileService _fileManagerGrafik = new FileManagementGrafik(_employeeManager, _scheduleManager);//Instancja do zarządzania plikiem grafiku.
            IEmployeesFileService _fileManagerPracownicy = new FileManagementPracownicy(_employeeManager);     //Instancja do zarządzania plikiem pracowników.
            IOptimization _optimization = new Optimization(_employeeManager, _scheduleManager); //Instancja do optymalizacji.
            
            IViewFile _viewFile = new ViewFile(_fileManagerPracownicy, _fileManagerGrafik);                 //Instancja do zarządzania plikami grafiku i pracowników w warstwie prezentera.
            IViewEmployee _viewEmployee = new ViewEmployee(_employeeManager, _scheduleManager);         //Instancja do zarządzania etykietami pracowników w warstwie prezentera.
            IViewOptimization _viewOptimization = new ViewOptimization(_optimization, _scheduleManager, _viewFile); //Instanjca do zarządzania optymalizacją w warstwie prezentera.

            _employeeManager.EmployeeChanged += (emp) => _viewEmployee.UpdateEmployeeLabel(emp);           //Zdarzenie: dodanie lub zmiana danych pracownika.
            _employeeManager.EmployeeDeleted += (id) => _viewEmployee.ClearEmployeeLabel(id);               //Zdarzenie: usunięcie pracownika.

            var _viewForm1 = new Form1(_employeeManager, _viewFile, _viewEmployee, _viewOptimization); //Tworzymy Form1.

            IPresenterSchedule _viewSchedule = new PresenterSchedule(_scheduleManager, _viewForm1);  //Instancja do zarządzania kontrolkami wyświetlającymi grafik.
            
            _scheduleManager.ShiftChanged += (shift) => _viewSchedule.UpdateScheduleControl(shift);       //Zdarzenie: modyfikacja zmiany.
            _viewForm1.LoadEmployeeAndSchedule();                  //Wczytanie pracowników, grafiku.

            Application.Run(_viewForm1);
        }
    }
}
