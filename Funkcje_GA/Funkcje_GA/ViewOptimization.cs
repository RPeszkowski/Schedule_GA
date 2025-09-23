using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using static Funkcje_GA.Constans;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klasa odpowiada za połączenie optymalizacji z warstwy modelu z view.
    public class ViewOptimization : IViewOptimization
    {
        private readonly IViewFile _viewFile;  //Instancja do zarządzania grafikiem.
        private readonly IOptimization _optimization;           //Instancja do zarządzania optymzalizacją.
        private readonly IScheduleManagement _scheduleManager;  //Instancja do zarządzania grafikiem.

        //Konstruktor
        public ViewOptimization(IOptimization optimization, IScheduleManagement scheduleManager, IViewFile viewFile)
        {
            this._optimization = optimization;
            this._scheduleManager = scheduleManager;
            this._viewFile = viewFile;

            //Subskrybujemy postęp optymalizacji.
            _optimization.ProgressUpdated += (raport) => ProgressUpdated?.Invoke(raport);

            //Subskrybujemy pojawienie się warningu.
            _optimization.WarningRaised += (message) => UserNotificationRaiseWarning?.Invoke(message);
        }

        //Powiadomienie użytkownika.
        public event Action<string> ProgressUpdated;

        //Powiadomienie użytkownika.
        public event Action<string> UserNotificationRaise;

        //Powiadomienie użytkownika - warning.
        public event Action<string> UserNotificationRaiseWarning;

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

                //Dodajemy funkcje, zapisujemy grafik i powiadamiamy użytkownika.
                _scheduleManager.ApplyOptimizationToSchedule(optymalneRozwiazanie);
                _viewFile.SaveSchedule("GrafikGA.txt");
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
    }
}
