using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Serilog;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klas odpowiada za połączenie View z menadżerami plików.
    internal class PresenterFile
    {
        private readonly IScheduleFileService _fileManagerGrafik;               //Instancja do zarządzania plikiem grafiku.
        private readonly IEmployeesFileService _fileManagerPracownicy;          //Instancja do zarządzania plikiem pracowników
        private readonly IViewForm2 _viewForm2;                                 //Interfejs do Form2.
        private readonly IViewFile _viewFile;                                   //Interfejs do Form1.

        private string empPath;                         //Scieżka do pliku z pracownikami;
        private string schedPath;                       //Scieżka do pliku z grafikiem.

        //Konstruktor
        public PresenterFile(IEmployeesFileService fileManagerPracownicy, IScheduleFileService fileManagerGrafik, IViewFile viewFile, IViewForm2 viewForm2)
        {
            this._fileManagerPracownicy = fileManagerPracownicy;
            this._fileManagerGrafik = fileManagerGrafik;
            this._viewForm2 = viewForm2;
            this._viewFile = viewFile;

            //Subskrybujemy zdarzenie - wybrano inną datę.
            _viewFile.DateChanged += (string month, string year) =>
            {
                //dekodujemy ścieżki.
                empPath = $"Employees/Emp_{month}_{year}.txt";
                schedPath = $"Schedules/Sched_{month}_{year}.txt";

                //Jeśli pliki istnieją to je wczytujemy.
                if (File.Exists(empPath) || File.Exists(schedPath))
                {
                    LoadEmployees(empPath);
                    LoadSchedule(schedPath);
                }
            };

            //Subskrybujemy zdarzenie - wczytywanie pracowników przy starcie programu.
            _viewFile.LoadAtStart += () =>
            {
                //Sprawdzamy, czy plik istnieje i wczytujemy pracowników.
                if (File.Exists(empPath))
                    LoadEmployees(empPath);

                //Sprawdzamy, czy plik istnieje i wczytujemy grafik.
                if (File.Exists(schedPath))
                    LoadSchedule(schedPath);
            };

            //Subskrybujemy zdarzenie - załaduj grafik.
            _viewFile.ViewLoadSchedule += () =>
            {
                LoadEmployees(empPath);
                LoadSchedule(schedPath);
                _viewFile.RaiseUserNotification("Grafik wczytany.");
            };

            //Subskrybujemy zdarzenie - zapisz grafik po optymalizacji.
            _viewFile.SaveOptimalSchedule += () => SaveSchedule("Grafik_GA.txt");

            //Subskrybujemy zdarzenie - zapisz grafik.
            _viewFile.ViewSaveSchedule += () =>
            {
                //Jeśli pliki istnieją, to pytamy uzytkownika, czy je nadpisać.
                if (File.Exists(empPath) || File.Exists(schedPath))
                {
                    bool confirmed = _viewFile.AskUserConfirmation("Pliki dla tego miesiąca już istnieją. Czy chcesz je nadpisać?");
                    
                    //Jeśli nie, to kończymy.
                    if (!confirmed)
                    {
                        _viewFile.RaiseUserNotification("Zapis anulowany.");
                        return;
                    }
                }

                //zapisujemy pliki.
                SaveEmployees(empPath);
                SaveSchedule(schedPath);
                _viewFile.RaiseUserNotification("Grafik zapisany.");
            };

            //Subskrybujemy zdarzenie - zapis do pliku z pracownikami z Form2.
            _viewForm2.SaveEmployees += () => SaveEmployees(empPath);
        }

        //Załaduj pracowników.
        private void LoadEmployees(string filePath)
        {
            //Sprawdzamy, czy plik istnieje.
            if (!File.Exists(filePath))
            {
                _viewFile.RaiseUserNotification($"Wybrany plik z pracownikami nie istnieje.");
                return;
            }

            //Próbujemy wczytać pracowników.
            try
            {
                _fileManagerPracownicy.WczytajPracownikow(filePath);
            }

            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                _viewFile.RaiseUserNotification($"Plik '{filePath}' jest uszkodzony.");
            }
        }

        //Załaduj schedule.
        private void LoadSchedule(string filePath)
        {
            //Sprawdzamy, czy plik istnieje.
            if (!File.Exists(filePath))
            {
                _viewFile.RaiseUserNotification($"Wybrany plik z grafikiem nie istnieje.");
                return;
            }

            //Próbujemy wczytać grafik
            try
            {
                _fileManagerGrafik.WczytajGrafik(filePath);
            }

            catch (Exception ex)
            {
                Log.Error(ex.Message);
                _viewFile.RaiseUserNotification($"Plik {filePath} jest uszkodzony. Napraw go lub usuń.");
            }
        }

        //Zapisz pracowników.
        private void SaveEmployees(string filePath)
        {
            //Próbujemy zapisać grafik
            try
            {
                _fileManagerPracownicy.ZapiszPracownikow(filePath);
            }

            catch (Exception ex)
            {
                Log.Error(ex.Message);
                _viewFile.RaiseUserNotification($"Plik {filePath} jest uszkodzony. Napraw go lub usuń.");
            }
        }

        //Zapisz grafik.
        private void SaveSchedule(string filePath)
        {
            //Próbujemy zapisać grafik
            try
            {
                _fileManagerGrafik.ZapiszGrafik(filePath);
            }

            catch (Exception ex)
            {
                Log.Error(ex.Message);
                _viewFile.RaiseUserNotification($"Plik {filePath} jest uszkodzony. Napraw go lub usuń.");
            }
        }
    }
}
