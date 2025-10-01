using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;
using Funkcje_GA.Presenter;
using Serilog;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    internal partial class Form1 : Form, IViewSchedule, IViewEmployee, IViewOptimization, IViewFile
    {
        private readonly Dictionary<int, System.Windows.Forms.Label> labelsPracownicy = new Dictionary<int, System.Windows.Forms.Label>(MAX_LICZBA_OSOB);   //Tworzenie etykiet pracowników.
        
        private readonly IEmployeeForm _form2;                                              //Form2.
        private readonly IScheduleRendererWinforms _scheduleRenderer;                               //Renderer do grafiku.
        protected readonly Dictionary<int, string> months = new Dictionary<int, string>(12)   //Miesiące.
        {
            {1, "Styczeń" }, {2, "Luty" }, {3, "Marzec" },
            {4, "Kwiecień" }, {5, "Maj" }, {6, "Czerwiec" },
            {7, "Lipiec" }, {8, "Sierpień" }, {9, "Wrzesień" },
            {10, "Październik" }, {11, "Listopad" }, {12, "Grudzień" },
        };

        private string currentMonth;                                 //Obecny miesiąc.
        private int currentYear;                                 //Obecny rok.

        //Konstruktor.
        public Form1(IEmployeeForm form2, IScheduleRendererWinforms scheduleRenderer)
        {
            //Przypisujemy menadżery.
            this._form2 = form2;
            this._scheduleRenderer = scheduleRenderer;

            //Generuje większość kontrolek. Metoda stworzona przez Designera.
            InitializeComponent();

            //Tworzymy listboxy i ich etykiety.
            InitializeScheduleControls();

            //Tworzymy etykiety pracowników.
            InitializeEmployeeControls();

            //Tworzymy kontrolki daty.
            InitializeDateControls();
        }

        //Zdarzenie - zmiana daty.
        public event Action<string, string> DateChanged;

        //Zdarzenie zgłaszające, że użytkownik chce dodać pracownika do zmiany.
        public event Action<int, int> EmployeeAddedToShift;

        //Zdarzenie zgłaszające, że uzytkownik kliknął na etykietę pracownika.
        public event Action<int> EmployeeLabelMouseDown;

        //Zdarzenie - załaduj grafik i pracowników przy starcie programu.
        public event Action LoadAtStart;

        //Wywołano optymalizację.
        public event Func<int, decimal, decimal, int, int, Task> OptimizationRequested;

        //Zdarzenie zgłaszające, że użytkownik chce wyczyścić grafik.
        public event Action ScheduleCleared;

        //Zdarzenie zgłaszające, że użytkownik chce przypisać funkcję
        public event Action<IEnumerable<(int ShiftId, int EmployeeId)>, FunctionTypes> SelectedShiftsAssigned;

        //Zdarzenie zgłaszające, że użytkownik chce usunąć pracownika ze zmiany.
        public event Action<IEnumerable<(int ShiftId, int EmployeeId)>> SelectedShiftsRemoved;

        //Zdarzenie - zapisanie grafiku po optymalizacji.
        public event Action SaveOptimalSchedule;

        //Zdarzenie - zapisanie grafiku.
        public event Action ViewLoadSchedule;

        //Zdarzenie - zapisanie grafiku.
        public event Action ViewSaveSchedule;

        //Pytamy użytkownika.
        public virtual bool AskUserConfirmation(string message)
        {
            //Wyświetlamy MessageBox.
            var result = MessageBox.Show(message, "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        //Zmieniamy na bez funkcji.
        private void buttonBezFunkcji_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            _scheduleRenderer.UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na bez funkcji.
            var selected = _scheduleRenderer.GetAllSelectedEmployeeIds();
            SelectedShiftsAssigned?.Invoke(selected, FunctionTypes.Bez_Funkcji);
        }

        //Czyścimy grafik.
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            _scheduleRenderer.UsunPodswietlenie();

            //Usuwamy grafik i wyświetlamy powiadomienie.
            ScheduleCleared?.Invoke();
        }

        //Wyświetlamy Form2.
        private void buttonDodajOsoby_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            _scheduleRenderer.UsunPodswietlenie();

            //Wyświetlamy Form2.
            _form2.ShowForm();
        }

        //Zamieniamy wszystkie wybrane dyżury na sale.
        private void buttonSala_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            _scheduleRenderer.UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na sale.
            var selected = _scheduleRenderer.GetAllSelectedEmployeeIds();
            SelectedShiftsAssigned?.Invoke(selected, FunctionTypes.Sala);
        }

        //Zamieniamy wszystkie wybrane dyżury na triaż.
        private void buttonTriaz_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            _scheduleRenderer.UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na triaż.
            var selected = _scheduleRenderer.GetAllSelectedEmployeeIds();
            SelectedShiftsAssigned?.Invoke(selected, FunctionTypes.Triaz);
        }

        //Usuwamy wszystkie wybrane dyżury.
        private void buttonUsunDyzur_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            _scheduleRenderer.UsunPodswietlenie();

            //Próbujemy usunąć zaznaczone dyżury.
            var selected = _scheduleRenderer.GetAllSelectedEmployeeIds();
            SelectedShiftsRemoved?.Invoke(selected);
        }

        //Wczytujemy grafik z pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonWczytajGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie i wczytujemy grafik.
            _scheduleRenderer.UsunPodswietlenie();
            ViewLoadSchedule?.Invoke();
        }

        //Zapisujemy grafik do pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonZapiszGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie i zapisujemy grafk.
            _scheduleRenderer.UsunPodswietlenie();
            ViewSaveSchedule?.Invoke();
        }

        //Załadowanie Form1.
        public void Form1_Load(object sender, EventArgs e) { }

        //Wyświetlanie kolorów.
        public virtual void HandleEmployeeMouseDown(IEnumerable<(int shiftId, Color color)> highlights)
        {
            //Wyświetlamy kolory.
            foreach (var (shiftId, color) in highlights)
                _scheduleRenderer.GetControlById(shiftId).BackColor = color;
        }

        //Inicjalizacja kontrolek związanych z wyborem daty.
        protected virtual void InitializeDateControls()
        {
            //Wybrany aktualnie miesiąc i rok.
            currentMonth = months[DateTime.Now.Month];
            currentYear = DateTime.Now.Year;

            //ComboBox dla miesięcy.
            comboBoxMonth.Items.Clear();
            for (int m = 1; m <= 12; m++)
                comboBoxMonth.Items.Add(months[m]);

            comboBoxMonth.SelectedItem = months[DateTime.Now.Month];

            //ComboBox dla lat.
            comboBoxYear.Items.Clear();
            for (int y = 2001; y <= 2099; y++)
                comboBoxYear.Items.Add(y);

            comboBoxYear.SelectedItem = DateTime.Now.Year;

            //Podajemy do wiadomości obecną datę, uaktulaniamy listboxy. Subskrybujemy zdarzenia wyboru innej daty.
            ListBoxesDropable(currentMonth, currentYear);
            DateChanged?.Invoke(comboBoxMonth.SelectedItem.ToString(), comboBoxYear.SelectedItem.ToString());
            comboBoxMonth.SelectedIndexChanged += (s, e) =>
            {
                //Usuwamy grafik.
                ScheduleCleared?.Invoke();
                currentMonth = comboBoxMonth.SelectedItem.ToString();
                ListBoxesDropable(currentMonth, currentYear);
                DateChanged?.Invoke(comboBoxMonth.SelectedItem.ToString(), comboBoxYear.SelectedItem.ToString());
            };
            comboBoxYear.SelectedIndexChanged += (s, e) =>
            {
                //Usuwamy grafik.
                ScheduleCleared?.Invoke();
                currentYear = Convert.ToInt32(comboBoxYear.SelectedItem);
                ListBoxesDropable(currentMonth, currentYear);
                DateChanged?.Invoke(comboBoxMonth.SelectedItem.ToString(), comboBoxYear.SelectedItem.ToString());
            };
        }
        
        //Tworzymy etykiety pracowników.
        protected virtual void InitializeEmployeeControls()
        {
            //Dodajemy etykiety wyświetlające dane pracowników do furmularza.
            for (int nrOsoby = 1; nrOsoby <= MAX_LICZBA_OSOB; nrOsoby++)
            {
                //Dodawanie etykiet.
                labelsPracownicy[nrOsoby] = new System.Windows.Forms.Label();
                labelsPracownicy[nrOsoby].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labelsPracownicy[nrOsoby].Size = new System.Drawing.Size(340, 40);
                labelsPracownicy[nrOsoby].Text = "";

                //Przypisujemy lambdy do zdarzeń drag and drop.
                int _nrOsoby = nrOsoby;
                labelsPracownicy[nrOsoby].MouseDown += (sender, e) =>
                {
                    //Usuwamy podświetlenie.
                    _scheduleRenderer.UsunPodswietlenie();

                    //Sprawdzamy, czy etykieta nie jest pusta.
                    if (labelsPracownicy[_nrOsoby].Tag == null)
                        return;

                    //Wywołujemy event w presenterze.
                    EmployeeLabelMouseDown?.Invoke(_nrOsoby);

                    //Rozpoczynamy drag & drop.
                    labelsPracownicy[_nrOsoby].DoDragDrop(labelsPracownicy[_nrOsoby].Tag.ToString(), DragDropEffects.Copy | DragDropEffects.Move);
                };

                tableLayoutPanel1.Controls.Add(labelsPracownicy[nrOsoby], (nrOsoby - 1) / 10, (nrOsoby - 1) % 10);
            }
        }

        //Tworzymy listboxy i etykiety grafiku.
        protected virtual void InitializeScheduleControls()
        {
            //Inicjalizacja kontrolek.
            _scheduleRenderer.Initialize();

            //Tworzenie etykiet i dodawanie kontrolek grafiku.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                //Tworzymy etykiety dla dyżurów dziennych.
                if (nrZmiany < LICZBA_DNI)
                {
                    //Dodawanie kontrolek do formularza.
                    tableLayoutPanel2.Controls.Add(_scheduleRenderer.GetLabel(nrZmiany), nrZmiany, 0);
                    tableLayoutPanel2.Controls.Add(_scheduleRenderer.GetControlById(nrZmiany), nrZmiany, 1);
                }

                //Dodajemy listboxy dla dyżurów nocnych.
                else
                    tableLayoutPanel2.Controls.Add(_scheduleRenderer.GetControlById(nrZmiany), nrZmiany - LICZBA_DNI, 2);
            }
        }

        //Zmieniamy atrybut listboxów w zależności od miesiąca.
        private void ListBoxesDropable(string currMonth, int currYear)
        {
            //Odblokowujemy allow drop we wszystkich listboxach.
            foreach (var ctrl in _scheduleRenderer.GetAll())
                ctrl.AllowDrop = true;

            //Sprawdzamy, czy miesiąc ma 31 dni.
            if (currMonth == "Styczeń" || currMonth == "Marzec" || currMonth == "Maj" || currMonth == "Lipiec"
             || currMonth == "Sierpień" || currMonth == "Październik" || currMonth == "Grudzień")
                return;

            //Sprawdzamy, czy miesiąc ma 30 dni
            if (currMonth == "Kwiecień" || currMonth == "Czerwiec" || currMonth == "Wrzesień" || currMonth == "Listopad")
            {
                _scheduleRenderer.GetControlById(30).AllowDrop = false;
                _scheduleRenderer.GetControlById(61).AllowDrop = false;
                return;
            }

            //Sprawdzamy, czy wybralismy luty.
            if(currMonth == "Luty")
            {
                _scheduleRenderer.GetControlById(29).AllowDrop = false;
                _scheduleRenderer.GetControlById(30).AllowDrop = false;
                _scheduleRenderer.GetControlById(60).AllowDrop = false;
                _scheduleRenderer.GetControlById(61).AllowDrop = false;

                //Sprawdzamy, czy rok jest przestępny
                int temp = Math.DivRem(currYear, 4, out int Rem);
                if(Rem == 0)
                {
                    _scheduleRenderer.GetControlById(28).AllowDrop = false;
                    _scheduleRenderer.GetControlById(59).AllowDrop = false;
                }
            }

        }

        //Wczytanie pracowników i grafiku. Subskrypcja zdarzeń.
        public virtual void LoadAndSubscribe()
        {
            //Zdarzenie asynchroniczne po kliknięciu przycisku "Opt".
            buttonOptymalizacja.Click += async (sender, e) =>
            {
                //Usuwamy podświetlenie.
                _scheduleRenderer.UsunPodswietlenie();

                //Dezaktywujemy wszystkie kontrolki z wyjątkiem etykiety labelRaport.
                SetControlsEnabled(false);

                int liczbaOsobnikow = 100;          //Liczba osobników.
                decimal tol = 0.0000003m;           //Jeśli wartość funkcji celu jest mniejsza lub równa tol, to przerywamy optymalizację.
                decimal tolX = 0.00000000001m;      //Minimalna zmiana funkcji celu powodująca zresetowanie liczby iteracji bez poprawy.
                int maxIterations = 200000;         //Maksymalna liczba iteracji.
                int maxConsIterations = 40000;      //maksymalna liczba iteracji bez poprawy.

                //Zdarzenie - prośba o przeprowadzenie optymalizacji.
                if (OptimizationRequested != null)
                    await OptimizationRequested.Invoke(liczbaOsobnikow, tol, tolX, maxIterations, maxConsIterations);

                //Po skończonej optymalizacji zapisujemy grafik.
                SaveOptimalSchedule?.Invoke();

                //Po skończonej optymalizacji aktywujemy kontrolki.
                SetControlsEnabled(true);
            };

            //Usunięcia zaznaczenia i podświetlenia po kliknięciu form1.
            this.Click += (sender, e) =>
            {
                _scheduleRenderer.ClearSelected();
                _scheduleRenderer.UsunPodswietlenie();
            };

            //Zdarzenie - dodano pracownika do zmiany.
            _scheduleRenderer.Drop += (int shiftId, int employeeId) => EmployeeAddedToShift?.Invoke(shiftId, employeeId);

            //Wczytujemy pracowników i grafik z pliku tekstowego przy starcie programu.
            LoadAtStart?.Invoke();
        }

        //Wzywamy subskrybenta OnUserNotification.
        public virtual void RaiseUserNotification(string message)
        {
            MessageBox.Show(message, "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Aktywujemy/dezaktywujemy kontrolki (oprócz labelRaport).
        public virtual void SetControlsEnabled(bool enable)
        {
            foreach (Control control in this.Controls)
            {
                if (control != labelRaport)
                    control.Enabled = enable;
            }
        }

        //Odświeżanie etykiet.
        public virtual void UpdateEmployeeLabel(int employeeId, (string data, EnumLabelStatus status) info, bool tag)
        {
            //Id - numer kontrolki, Item1 - tekst, Item2 - kolor.
            labelsPracownicy[employeeId].Text = info.Item1;
            labelsPracownicy[employeeId].ForeColor = info.Item2 == EnumLabelStatus.Normal ? Color.Black : Color.Orange;
            
            //Uaktulaniamy tag.
            if(tag)
                labelsPracownicy[employeeId].Tag = employeeId;

            else
                labelsPracownicy[employeeId].Tag = null;
        }

        //Uaktualniamy etykietę z raportem.
        public virtual void UpdateOptimizationProgress(string raport)
        {
                //Odświeżamy UI bezpośrednio w bezpieczny sposób.
                if (labelRaport.InvokeRequired)
                    labelRaport.Invoke(new Action(() => { labelRaport.Text = raport; }));

                else
                    labelRaport.Text = raport;
        }

        //Odświeżanie kontrolek.
        public virtual void UpdateShift(int shiftId, List<string> lista)
        {
            _scheduleRenderer.Clear(shiftId);
            _scheduleRenderer.Add(shiftId, lista);
        }

        //Informacja, gdy wystąpił warning podczas optymalizacji.
        public virtual void RaiseUserNotificationWarning(string message)
        {
            Log.Error(message);
            MessageBox.Show(message, "Ostrzeżenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
