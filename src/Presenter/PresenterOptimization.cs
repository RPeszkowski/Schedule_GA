using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Ta klasa odpowiada za połączenie optymalizacji z warstwy modelu z view.
    internal class PresenterOptimization
    {
        private readonly IViewOptimization _viewOptimization;
        private readonly IOptimization _optimization;           //Instancja do zarządzania optymzalizacją.
        private readonly IScheduleManagement _scheduleManager;  //Instancja do zarządzania grafikiem.

        //Konstruktor
        public PresenterOptimization(IOptimization optimization, IScheduleManagement scheduleManager, IViewOptimization viewOptimization)
        {
            this._optimization = optimization;
            this._scheduleManager = scheduleManager;
            this._viewOptimization = viewOptimization;

            //Subskrybujemy postęp optymalizacji.
            _optimization.ProgressUpdated += (raport) => _viewOptimization.UdpateOptimizationProgress(raport);

            //Subskrybujemy pojawienie się warningu.
            _optimization.WarningRaised += (message) => _viewOptimization.RaiseUserNotificationWarning(message);

            //Subskrybujemy zdarzenie - zarządano przeprowadzenia optymalizacji.
            _viewOptimization.OptimizationRequested += async (liczbaOsobnikow, tol, tolX, maxIterations, maxConsIterations) => await RunOptimizationAsync(liczbaOsobnikow, tol, tolX, maxIterations, maxConsIterations);
        }

        //Przeprowadzamy optymalizację.
        private async Task RunOptimizationAsync(int liczbaOsobnikow, decimal tol, decimal tolX, int maxIterations, int maxConsIterations)
        {
            try
            {
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
                _viewOptimization.RaiseUserNotification($"Przydzielanie funkcji ukończone w: {czasOptymalizacja}.");
            }

            //Jeśli grafik był zły, to powiadamiamy.
            catch (OptimizationInvalidScheduleException ex)
            {
                Log.Error(ex.Message);
                _viewOptimization.RaiseUserNotification($"Aby przeprowadzić przydzielanie funkcji na każdej zmianie musi być od 3 do {MAX_LICZBA_DYZUROW}.");
            }

            //Jeśli liczba zmiennych była zła to powiadamiamy.
            catch (OptimizationInvalidDataException ex)
            {
                Log.Error(ex.Message);
                _viewOptimization.RaiseUserNotification("Liczba zmiennych musi być większa niż 0.");
            }

            catch (ScheduleFunctionEncodingException ex)
            {
                Log.Error(ex.Message);
                _viewOptimization.RaiseUserNotification(ex.Message);
            }
        }
    }
}
