using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Funkcje_GA
{
    //Interfejs łączący UI z prezenterem optymalizacji.
    public interface IViewOptimization : IUserNotifier
    {
        //Wywołano optymalizację.
        event Func<int, decimal, decimal, int, int, Task> OptimizationRequested;

        //Informacja, gdy wystąpił warning.
        void RaiseUserNotificationWarning(string message);

        //Uaktualniamy etykietę z raportem.
        void UdpateOptimizationProgress(string raport);
    }
}
