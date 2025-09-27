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

            IEmployeeManagement _employeeManager = new EmployeeManagement();                                        //Instancja do zarządzania pracownikami. 
            IScheduleManagement _scheduleManager = new ScheduleManagement(_employeeManager);                        //Instancja do zarządzania grafikiem.
            IScheduleFileService _fileManagerGrafik = new FileManagementGrafik(_employeeManager, _scheduleManager); //Instancja do zarządzania plikiem grafiku.
            IEmployeesFileService _fileManagerPracownicy = new FileManagementPracownicy(_employeeManager);          //Instancja do zarządzania plikiem pracowników.
            IOptimization _optimization = new Optimization(_employeeManager, _scheduleManager);                     //Instancja do optymalizacji.

            var _viewForm2 = new Form2();                       //Tworzymy Form2.
            var _viewForm1 = new Form1(_viewForm2);             //Tworzymy Form1.

            var _presenterFile = new PresenterFile(_fileManagerPracownicy, _fileManagerGrafik, _viewForm1, _viewForm2);      //Instancja do zarządzania plikami grafiku i pracowników w warstwie prezentera.
            var _presenterOptimization = new PresenterOptimization(_optimization, _scheduleManager, _viewForm1);      //Instancja do zarządzania optymalizacją w warstwie prezentera.
            var _presenterEmployee = new PresenterEmployee(_employeeManager, _scheduleManager, _viewForm1, _viewForm2);  //Instancja do zarządzania etykietami pracowników w warstwie prezentera.
            var _presenterSchedule = new PresenterSchedule(_scheduleManager, _viewForm1);                                //Instancja do zarządzania kontrolkami wyświetlającymi grafik.

            _viewForm1.LoadAndSubscribe();      //Wczytanie pracowników, grafiku.
            Application.Run(_viewForm1);        //Startujemy aplikację.
            Log.CloseAndFlush();                //Zamykamy plik log.
        }
    }
}
