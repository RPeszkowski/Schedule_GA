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
using static Funkcje_GA.FileService;
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

            var _uiManager = new UIForm1Management();                                              //Instancja do zarządzania kontrolkami wyświetlającymi grafik.
            var _employeeManager = new EmployeeManagement();                             //Instancja do zarządzania pracownikami. 
            var _scheduleManager = new ScheduleManagement(_employeeManager);            //Instancja do zarządzania grafikiem.
            var _fileManagerGrafik = new FileManagementGrafik(_employeeManager, _scheduleManager);//Instancja do zarządzania plikiem grafiku.
            var _fileManagerPracownicy = new FileManagementPracownicy(_employeeManager);     //Instancja do zarządzania plikiem pracowników.
            var _optimization = new Optimization(_employeeManager, _scheduleManager);

            _employeeManager.EmployeeChanged += (emp) => _uiManager.UpdateEmployeeLabel(emp);           //Zdarzenie: dodanie lub zmiana danych pracownika.
            _employeeManager.EmployeeDeleted += (id) => _uiManager.ClearEmployeeData(id);               //Zdarzenie: usunięcie pracownika.
            _scheduleManager.ShiftChanged += (shift) => _uiManager.DisplayScheduleControl(shift);       //Zdarzenie: modyfikacja zmiany.

            //Tworzymy logger.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("log-.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Application.Run(new Form1(_uiManager, _employeeManager, _scheduleManager, _fileManagerGrafik, _fileManagerPracownicy, _optimization));
        }
    }
}
