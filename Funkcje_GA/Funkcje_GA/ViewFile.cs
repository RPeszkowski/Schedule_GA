using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klas odpowiada za połączenie View z menadżerami plików.
    public class ViewFile : IViewFile
    {
        private readonly IScheduleFileService _fileManagerGrafik;               //Instancja do zarządzania plikiem grafiku.
        private readonly IEmployeesFileService _fileManagerPracownicy;          //Instancja do zarządzania plikiem pracowników

        //Konstruktor
        public ViewFile(IEmployeesFileService fileManagerPracownicy, IScheduleFileService fileManagerGrafik)
        {
            this._fileManagerPracownicy = fileManagerPracownicy;
            this._fileManagerGrafik = fileManagerGrafik;
        }

        //Powiadomienie użytkownika.
        public event Action<string> UserNotificationRaise;

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
                UserNotificationRaise?.Invoke("Grafik wczytany.");
            }

            catch (Exception ex)
            {
                throw new UIInvalidScheduleFileException($"Plik {filePath} jest uszkodzony. Napraw go lub usuń. {ex.Message}", ex);
            }
        }

        //Zapisz pracowników.
        public void SaveEmployees(string filePath)
        {
            //Próbujemy zapisać grafik
            try
            {
                _fileManagerPracownicy.ZapiszPracownikow(filePath);
            }

            catch (Exception ex)
            {
                throw new UIInvalidEmployeeFileException($"Plik {filePath} jest uszkodzony. Napraw go lub usuń. {ex.Message}", ex);
            }
        }

        //Zapisz grafik.
        public void SaveSchedule(string filePath)
        {
            //Próbujemy zapisać grafik
            try
            {
                _fileManagerGrafik.ZapiszGrafik(filePath);
                UserNotificationRaise?.Invoke("Grafik zapisany.");
            }

            catch (Exception ex)
            {
                throw new UIInvalidScheduleFileException($"Plik {filePath} jest uszkodzony. Napraw go lub usuń. {ex.Message}", ex);
            }
        }
    }
}
