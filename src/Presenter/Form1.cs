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
using Serilog;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    internal partial class Form1 : Form, IViewSchedule, IViewEmployee, IViewOptimization, IViewFile
    {
        private readonly System.Windows.Forms.Label[] labelsDzien = new System.Windows.Forms.Label[LICZBA_DNI];             //Tworzenie etykiet wyświetlających numer dziennej zmiany.
        private readonly Dictionary<int, System.Windows.Forms.Label> labelsPracownicy = new Dictionary<int, System.Windows.Forms.Label>(MAX_LICZBA_OSOB);   //Tworzenie etykiet pracowników.
        private readonly ListBoxGrafik[] listboxesSchedule = new ListBoxGrafik[2 * LICZBA_DNI];                             //Tworzenie listboxów grafiku.
        
        private readonly IEmployeeForm _form2;                                              //Form2.
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
        public Form1(IEmployeeForm form2)
        {
            //Przypisujemy menadżery.
            this._form2 = form2;

            //Generuje większość kontrolek. Metoda stworzona przez Designera.
            InitializeComponent();

            //Tworzymy listboxy i ich etykiety.
            InitializeScheduleControls();

            //Tworzymy etykiety pracowników.
            InitializeEmployeeControls();
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
        public bool AskUserConfirmation(string message)
        {
            //Wyświetlamy MessageBox.
            var result = MessageBox.Show(message, "Potwierdzenie", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        //Zmieniamy na bez funkcji.
        private void buttonBezFunkcji_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na bez funkcji.
            var selected = GetAllSelectedEmployeeIds();
            SelectedShiftsAssigned?.Invoke(selected, FunctionTypes.Bez_Funkcji);
        }

        //Czyścimy grafik.
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Usuwamy grafik i wyświetlamy powiadomienie.
            ScheduleCleared?.Invoke();
        }

        //Wyświetlamy Form2.
        private void buttonDodajOsoby_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Wyświetlamy Form2.
            _form2.ShowForm();
        }

        //Zamieniamy wszystkie wybrane dyżury na sale.
        private void buttonSala_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na sale.
            var selected = GetAllSelectedEmployeeIds();
            SelectedShiftsAssigned?.Invoke(selected, FunctionTypes.Sala);
        }

        //Zamieniamy wszystkie wybrane dyżury na triaż.
        private void buttonTriaz_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na triaż.
            var selected = GetAllSelectedEmployeeIds();
            SelectedShiftsAssigned?.Invoke(selected, FunctionTypes.Triaz);
        }

        //Usuwamy wszystkie wybrane dyżury.
        private void buttonUsunDyzur_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Próbujemy usunąć zaznaczone dyżury.
            var selected = GetAllSelectedEmployeeIds();
            SelectedShiftsRemoved?.Invoke(selected);
        }

        //Wczytujemy grafik z pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonWczytajGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie i wczytujemy grafik.
            UsunPodswietlenie();
            ViewLoadSchedule?.Invoke();
        }

        //Zapisujemy grafik do pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonZapiszGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie i zapisujemy grafk.
            UsunPodswietlenie();
            ViewSaveSchedule?.Invoke();
        }

        //Załadowanie Form1.
        public void Form1_Load(object sender, EventArgs e) { }

        //Pobieramy numery zaznaczonych pracowników na wszytskich zmianach.
        protected virtual IEnumerable<(int ShiftId, int EmployeeId)> GetAllSelectedEmployeeIds()
        {
            var result = new List<(int ShiftId, int EmployeeId)>();     //Zwracamy pracy element pracownik.

            //Sprawdzamy po kolei którzy pracownicy i które zmiany są wybrane.
            for (int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
            {
                int selectedIndex = listboxesSchedule[shiftId].SelectedIndex;  //Sprawdzamy zaznaczony indeks w danej kontrolce.

                //Jeśli kontrolka ma zaznaczony element to pobieramy numer zaznaczonego pracownika.
                if (selectedIndex != -1)
                {
                    try
                    {
                        int employeeId = listboxesSchedule[shiftId].GetNumber(selectedIndex);
                        result.Add((shiftId, employeeId));
                    }

                    catch (Exception ex)
                    {
                        throw new FormatException($"Kontrolka: {shiftId} ma niepoprawne dane {ex.Message}.", ex);
                    };
                }
            }

            return result;
        }

        //Wyświetlanie kolorów.
        public virtual void HandleEmployeeMouseDown(IEnumerable<(int shiftId, FunctionTypes function)> highlights)
        {
            //Wyświetlamy kolory.
            foreach (var (shiftId, function) in highlights)
            {
                switch (function)
                {
                    case FunctionTypes.Bez_Funkcji:
                        listboxesSchedule[shiftId].BackColor = Color.Red;
                        break;

                    case FunctionTypes.Sala:
                        listboxesSchedule[shiftId].BackColor = Color.Green;
                        break;

                    case FunctionTypes.Triaz:
                        listboxesSchedule[shiftId].BackColor = Color.Blue;
                        break;
                }
            }
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
                    UsunPodswietlenie();

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
            //Tworzenie etykiet i dodawanie kontrolek grafiku.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                listboxesSchedule[nrZmiany] = new ListBoxGrafik(nrZmiany);
                listboxesSchedule[nrZmiany].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                listboxesSchedule[nrZmiany].Size = new System.Drawing.Size(40, 400);
                listboxesSchedule[nrZmiany].AllowDrop = true;

                //Przypisujemy delegaty do zdarzeń drag and drop.
                int _nrZmiany = nrZmiany;
                listboxesSchedule[nrZmiany].DragEnter += (sender, e) =>
                {
                    //Jeśli etykieta nie była pusta, to kopiujemy numer osoby.
                    if (e.Data.GetDataPresent(DataFormats.Text) && e.Data.GetData(DataFormats.Text).ToString().Length != 0)
                        e.Effect = DragDropEffects.Copy;
                    else
                        e.Effect = DragDropEffects.None;
                };

                listboxesSchedule[nrZmiany].DragDrop += (sender, e) =>
                {
                    //Pobieramy dane i dodajemy osobę do zmiany.
                    string pom = e.Data.GetData(DataFormats.Text).ToString();
                    EmployeeAddedToShift?.Invoke(_nrZmiany, Convert.ToInt32(pom));
                };

                //Tworzymy etykiety dla dyżurów dziennych.
                if (nrZmiany < LICZBA_DNI)
                {
                    //Tworzenie etykiet.
                    labelsDzien[nrZmiany] = new System.Windows.Forms.Label();
                    labelsDzien[nrZmiany].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                    labelsDzien[nrZmiany].Size = new System.Drawing.Size(340, 40);
                    labelsDzien[nrZmiany].Text = (nrZmiany + 1).ToString();

                    //Dodawanie kontrolek do formularza.
                    tableLayoutPanel2.Controls.Add(labelsDzien[nrZmiany], nrZmiany, 0);
                    tableLayoutPanel2.Controls.Add(listboxesSchedule[nrZmiany], nrZmiany, 1);
                }

                //Dodajemy listboxy dla dyżurów nocnych.
                else
                    tableLayoutPanel2.Controls.Add(listboxesSchedule[nrZmiany], nrZmiany - LICZBA_DNI, 2);
            }
        }

        //Zmieniamy atrybut listboxów w zależności od miesiąca.
        private void ListBoxesDropable(string currMonth, int currYear)
        {
            //Odblokowujemy allow drop we wszystkich listboxach.
            foreach (var ctrl in listboxesSchedule)
                ctrl.AllowDrop = true;

            //Sprawdzamy, czy miesiąc ma 31 dni.
            if (currMonth == "Styczeń" || currMonth == "Marzec" || currMonth == "Maj" || currMonth == "Lipiec"
             || currMonth == "Sierpień" || currMonth == "Październik" || currMonth == "Grudzień")
                return;

            //Sprawdzamy, czy miesiąc ma 30 dni
            if (currMonth == "Kwiecień" || currMonth == "Czerwiec" || currMonth == "Wrzesień" || currMonth == "Listopad")
            {
                listboxesSchedule[30].AllowDrop = false;
                listboxesSchedule[61].AllowDrop = false;
                return;
            }

            //Sprawdzamy, czy wybralismy luty.
            if(currMonth == "Luty")
            {
                listboxesSchedule[29].AllowDrop = false;
                listboxesSchedule[30].AllowDrop = false;
                listboxesSchedule[60].AllowDrop = false;
                listboxesSchedule[61].AllowDrop = false;

                //Sprawdzamy, czy rok jest przestępny
                int temp = Math.DivRem(currYear, 4, out int Rem);
                if(Rem == 0)
                {
                    listboxesSchedule[28].AllowDrop = false;
                    listboxesSchedule[59].AllowDrop = false;
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
                UsunPodswietlenie();

                //Dezaktywujemy wszystkie kontrolki z wyjątkiem etykiety labelRaport.
                SetContolsEnabled(false);

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
                SetContolsEnabled(true);
            };

            //Usunięcia zaznaczenia i podświetlenia po kliknięciu form1.
            this.Click += (sender, e) =>
            {
                for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
                {
                    listboxesSchedule[nrZmiany].ResetBackColor();
                    listboxesSchedule[nrZmiany].ClearSelected();
                }
            };

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

            //Wczytujemy pracowników i grafik z pliku tekstowego przy starcie programu.
            LoadAtStart?.Invoke();
        }

        //Wzywamy subskrybenta OnUserNotification.
        public virtual void RaiseUserNotification(string message)
        {
            MessageBox.Show(message, "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Aktywujemy/dezaktywujemy kontrolki (oprócz labelRaport).
        public virtual void SetContolsEnabled(bool enable)
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
        public virtual void UdpateOptimizationProgress(string raport)
        {
                //Odświeżamy UI bezpośrednio w bezpieczny sposób.
                if (labelRaport.InvokeRequired)
                    labelRaport.Invoke(new Action(() => { labelRaport.Text = raport; }));

                else
                    labelRaport.Text = raport;
        }

        //Odświeżanie listboxów.
        public virtual void UpdateShift(int shiftId, List<string> lista)
        {
            listboxesSchedule[shiftId].Items.Clear();
            foreach (var item in lista)
                listboxesSchedule[shiftId].Items.Add(item);
        }

        //Informacja, gdy wystąpił warning podczas optymalizacji.
        public virtual void RaiseUserNotificationWarning(string message)
        {
            Log.Error(message);
            MessageBox.Show(message, "Ostrzeżenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        //Usuwamy podświetlenie.
        protected virtual void UsunPodswietlenie()
        {
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                listboxesSchedule[nrZmiany].ResetBackColor();
            }
        }
    }
}
