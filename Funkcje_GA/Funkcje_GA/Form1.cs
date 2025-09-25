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
    public partial class Form1 : Form
    {
        private readonly System.Windows.Forms.Label[] labelsDzien = new System.Windows.Forms.Label[LICZBA_DNI];             //Tworzenie etykiet wyświetlających numer dziennej zmiany.
        private readonly System.Windows.Forms.Label[] labelsNoc = new System.Windows.Forms.Label[LICZBA_DNI];               //Tworzenie etykiet wyświetlających numer nocnej zmiany.
        private readonly Dictionary<int, System.Windows.Forms.Label> labelsPracownicy = new Dictionary<int, System.Windows.Forms.Label>(MAX_LICZBA_OSOB);   //Tworzenie etykiet pracowników.
        private readonly ListBoxGrafik[] listboxesSchedule = new ListBoxGrafik[2 * LICZBA_DNI];                             //Tworzenie listboxów grafiku.
     
        private readonly IEmployeeManagement _employeeManager;                      //Instancja do zarządzania pracownikami.
        private readonly IViewSchedule _viewSchedule;                               //Instancja do zarządzania kontrolkami wyświetlającymi grafik.
        private readonly IViewEmployee _viewEmployee;                               //Instancja prezentera zajmująca się obsługą plików.
        private readonly IViewFile _viewFile;                                       //Instancja prezentera zajmująca się wyświetlaniem etykiet pracowników.
        private readonly IViewOptimization _viewOptimization;                       //Instancja prezentera zajmująca się wyświetlaniem optymalizacji.

        //Konstruktor.
        public Form1(IViewSchedule viewSchedule, 
                     IEmployeeManagement employeeManager,
                     IViewFile viewFile,
                     IViewEmployee viewEmployee,
                     IViewOptimization viewOptimization)
        {
            //Przypisujemy menadżery.
            this._viewSchedule = viewSchedule;
            this._employeeManager = employeeManager;
            this._viewFile = viewFile;
            this._viewEmployee = viewEmployee;
            this._viewOptimization = viewOptimization;

            //Generuje większość kontrolek. Metoda stworzona przez Designera.
            InitializeComponent();

            //Tworzymy listboxy i ich etykiety.
            InitializeListboxes();

            //Tworzymy etykiety pracowników.
            InitializeLabels();

            //Subskrybujemy eventy kliknięcia w form1, przycisku optymalizacji, powiadomień itd.
            SubscribeToEvents();

            //Wczytujemy pracowników z pliku tekstowego przy starcie programu.
            try
            {
                _viewFile.LoadEmployees("Pracownicy.txt");
            }

            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                RaiseUserNotification("Plik 'Pracownicy.txt' jest uszkodzony.");
            }

            //Jeśli plik z grafikiem istnieje, to wyświetlane jest zapytanie, czy go wczytać.
            if (File.Exists("Grafik.txt"))
            {
                var result = MessageBox.Show("Wczytać ostatni grafik?", "Wczytywanie grafiku", MessageBoxButtons.YesNo);

                //Jeśli wybrano opcje "Tak" to wczytywany jest grafik.
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _viewFile.LoadSchedule("Grafik.txt");
                    }

                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                        RaiseUserNotification("Plik 'Grafik.txt' jest uszkodzony.");
                    }
                }
            }
        }

        //Akcja powiadomienia użytkownika.
        public Action<string> UserNotificationRaise;

        //Zmieniamy na bez funkcji.
        private void buttonBezFunkcji_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na bez funkcji.
            var selected = GetAllSelectedEmployeeIds();
            _viewSchedule.SetSelectedShifts(selected, FunctionTypes.Bez_Funkcji);
        }

        //Czyścimy grafik.
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            //Usuwamy grafik i wyświetlamy powiadomienie.
            _viewSchedule.ClearSchedule();
        }

        //Wyświetlamy Form2.
        private void buttonDodajOsoby_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Wyświetlamy Form2.
            Form2 dialog = new Form2(_employeeManager, _viewFile);
            dialog.ShowDialog();
        }

        //Zamieniamy wszystkie wybrane dyżury na sale.
        private void buttonSala_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na sale.
            var selected = GetAllSelectedEmployeeIds();
            _viewSchedule.SetSelectedShifts(selected, FunctionTypes.Sala);
        }

        //Zamieniamy wszystkie wybrane dyżury na triaż.
        private void buttonTriaz_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na triaż.
            var selected = GetAllSelectedEmployeeIds();
            _viewSchedule.SetSelectedShifts(selected, FunctionTypes.Triaz);
        }

        //Usuwamy wszystkie wybrane dyżury.
        private void buttonUsunDyzur_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie.
            UsunPodswietlenie();

            //Próbujemy usunąć zaznaczone dyżury.
            var selected = GetAllSelectedEmployeeIds();
            _viewSchedule.RemoveSelectedShifts(selected);
        }

        //Wczytujemy grafik z pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonWczytajGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie i wczytujemy grafik.
            UsunPodswietlenie();
            try
            {
                _viewFile.LoadSchedule("Grafik.txt");
            }

            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                RaiseUserNotification("Plik 'Grafik.txt' jest uszkodzony.");
            }
        }

        //Zapisujemy grafik do pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonZapiszGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie i zapisujemy grafk.
            UsunPodswietlenie();

            //Próbujemy zapisać grafik
            try
            {
                _viewFile.SaveSchedule("Grafik.txt");
            }

            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                RaiseUserNotification("Plik 'Grafik.txt' jest uszkodzony.");
            }
        }

        //Załadowanie Form1.
        public void Form1_Load(object sender, EventArgs e) { }

        //Pobieramy numery zaznaczonych pracowników na wszytskich zmianach.
        private IEnumerable<(int ShiftId, int EmployeeId)> GetAllSelectedEmployeeIds()
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

        //Tworzymy etykiety pracowników.
        private void InitializeLabels()
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

                    //Wywołujemy event w presenterze.
                    var highlights = _viewEmployee.HandleEmployeeMouseDown(_nrOsoby);
                    foreach (var (shiftId, color) in highlights)
                        listboxesSchedule[shiftId].BackColor = color;

                    //Rozpoczynamy drag & drop.
                    if (e.Button == MouseButtons.Left)
                        labelsPracownicy[_nrOsoby].DoDragDrop((_nrOsoby).ToString(), DragDropEffects.Copy | DragDropEffects.Move);
                };

                tableLayoutPanel1.Controls.Add(labelsPracownicy[nrOsoby], (nrOsoby - 1) / 10, (nrOsoby - 1) % 10);
            }
        }

        //Tworzymy listboxy i etykiety grafiku.
        private void InitializeListboxes()
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
                    if (e.Data.GetDataPresent(DataFormats.Text))
                        e.Effect = DragDropEffects.Copy;
                    else
                        e.Effect = DragDropEffects.None;
                };

                listboxesSchedule[nrZmiany].DragDrop += (sender, e) =>
                {
                    //Pobieramy dane i dodajemy osobę do zmiany.
                    string pom = e.Data.GetData(DataFormats.Text).ToString();
                    _viewSchedule.AddEmployeeToShift(_nrZmiany, Convert.ToInt32(pom));
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

                //Tworzymy etykiety dla dyżurów nocnych.
                else
                {
                    //Tworzenie etykiet.
                    labelsNoc[nrZmiany - LICZBA_DNI] = new System.Windows.Forms.Label();
                    labelsNoc[nrZmiany - LICZBA_DNI].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                    labelsNoc[nrZmiany - LICZBA_DNI].Size = new System.Drawing.Size(340, 40);
                    labelsNoc[nrZmiany - LICZBA_DNI].Text = (nrZmiany + 1 - LICZBA_DNI).ToString();

                    //Dodawanie kontrolek do formularza.
                    tableLayoutPanel3.Controls.Add(labelsNoc[nrZmiany - LICZBA_DNI], nrZmiany - LICZBA_DNI, 0);
                    tableLayoutPanel3.Controls.Add(listboxesSchedule[nrZmiany], nrZmiany - LICZBA_DNI, 1);
                }
            }
        }

        //Wzywamy subskrybenta OnUserNotification.
        protected virtual void RaiseUserNotification(string message)
        {
            UserNotificationRaise?.Invoke(message);
        }

        //Subskrybujemy eventy form1.
        private void SubscribeToEvents()
        {
            //Zdarzenie asynchroniczne po kliknięciu przycisku "Opt".
            buttonOptymalizacja.Click += async (sender, e) =>
            {
                //Usuwamy podświetlenie.
                UsunPodswietlenie();

                //Dezaktywujemy wszystkie kontrolki z wyjątkiem etykiety labelRaport.
                foreach (Control control in this.Controls)
                {
                    if (control != labelRaport)
                        control.Enabled = false;
                }

                //Uruchamiamy optymalizację.
                await _viewOptimization.RunOptimizationAsync();

                //Po skończonej optymalizacji aktywujemy kontrolki.
                foreach (Control control in this.Controls)
                    if (control != labelRaport)
                        control.Enabled = true;
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

            //Wyświetlenie wiadomości dla użytkownika.
            UserNotificationRaise += message => MessageBox.Show(message, "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _viewFile.UserNotificationRaise += message => MessageBox.Show(message, "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _viewOptimization.UserNotificationRaise += message => MessageBox.Show(message, "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _viewSchedule.UserNotificationRaise += message => MessageBox.Show(message, "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //Subskrybujemy event odświeżono etykietę pracownika.
            _viewEmployee.EmployeeLabelChanged += (id, info) =>
            {
                //Id - numer kontrolki, Item1 - tekst, Item2 - kolor.
                labelsPracownicy[id].Text = info.Item1;
                labelsPracownicy[id].ForeColor = info.Item2 == 0 ? Color.Black : Color.Orange;
            };

            //Subskrybujemy zdarzenie nowych informacji o optymalizacji.
            _viewOptimization.ProgressUpdated += raport =>
            {
                //Odświeżamy UI bezpośrednio w bezpieczny sposób.
                if (labelRaport.InvokeRequired)
                    labelRaport.Invoke(new Action(() => { labelRaport.Text = raport; }));

                else
                    labelRaport.Text = raport;
            };

            //Informacja, gdy wystąpił warning podczas optymalizacji.
            _viewOptimization.UserNotificationRaiseWarning += message =>
            {
                Log.Error(message);
                MessageBox.Show(message, "Ostrzeżenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            //Subskrybujemy event odświezono kontrolkę grafiku.
            _viewSchedule.ScheduleControlChanged += (id, lista) =>
            {
                listboxesSchedule[id].Items.Clear();
                foreach (var item in lista)
                    listboxesSchedule[id].Items.Add(item);
            };
        }

        //Usuwamy podświetlenie.
        private void UsunPodswietlenie()
        {
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                listboxesSchedule[nrZmiany].ResetBackColor();
            }
        }
    }
}
