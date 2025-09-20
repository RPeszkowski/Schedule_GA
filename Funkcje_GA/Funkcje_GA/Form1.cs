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
using static Funkcje_GA.Constans;
using static Funkcje_GA.FileService;

namespace Funkcje_GA
{
    public partial class Form1 : Form
    {
        private readonly System.Windows.Forms.Label[] labelsDzien = new System.Windows.Forms.Label[LICZBA_DNI];     //Tworzenie etykiet wyświetlających numer dziennej zmiany.
        private readonly System.Windows.Forms.Label[] labelsNoc = new System.Windows.Forms.Label[LICZBA_DNI];       //Tworzenie etykiet wyświetlających numer nocnej zmiany.

        private TimeSpan czasOptymalizacja;                                                                         //Pomiar czasu działania algorytmu optymalizacji.
        private DateTime startOptymalizacja;                                                                        //Pomiar czasu działania algorytmu optymalizacji.
        private readonly EmployeeManagement _employeeManager;                                                       //Instancja do zarządzania pracownikami. 
        private readonly FileManagementGrafik _fileManagerGrafik;                                                   //Instancja do zarządzania plikiem grafiku.
        private readonly FileManagementPracownicy _fileManagerPracownicy;                                           //Instancja do zarządzania plikiem pracowników
        private readonly Optimization _optimization;                                                                //Instancja do optymzalicaji.
        private readonly ScheduleManagement _scheduleManager;                                                       //Instancja do zarządzania grafikiem.
        private readonly UIForm1Management _uiManager;                                                              //Instancja do zarządzania kontrolkami wyświetlającymi grafik.

        //Konstruktor.
        public Form1(UIForm1Management uiManager, 
                     EmployeeManagement employeeManager, 
                     ScheduleManagement scheduleManager,
                     FileManagementGrafik fileManagerGrafik,
                     FileManagementPracownicy fileManagerPracownicy,
                     Optimization optimization)
        {
            //Przypisujemy menadżery.
            this._uiManager = uiManager;
            this._employeeManager = employeeManager;
            this._scheduleManager = scheduleManager;
            this._fileManagerGrafik = fileManagerGrafik;
            this._fileManagerPracownicy = fileManagerPracownicy;
            this._optimization = optimization;

            //Generuje większość kontrolek. Metoda stworzona przez Designera.
            InitializeComponent();

            //Generuje listboxy i etykiety grafiku i listy pracowników. Zdarzenie asynchroniczne przycisku optymalizacji.
            //Wyświetlanie informacji o przebiegu optymalizacji.
            InitializeComponent2();

            //Wczytujemy pracowników z pliku tekstowego przy starcie programu.
            try 
            {
                _fileManagerPracownicy.WczytajPracownikow("Pracownicy.txt");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            //Jeśli plik z grafikiem istnieje, to wyświetlane jest zapytanie, czy go wczytać.
            if (File.Exists("Grafik.txt"))
            {
                var result = MessageBox.Show("Wczytać ostatni grafik?", "Wczytywanie grafiku", MessageBoxButtons.YesNo);

                //Jeśli wybrano opcje "Tak" to wczytywany jest grafik.
                if (result == DialogResult.Yes)
                {
                    //Próbujemy wczytać grafik
                    try
                    {
                        _fileManagerGrafik.WczytajGrafik("Grafik.txt");
                        MessageBox.Show("Grafik wczytany.");
                    }

                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                        MessageBox.Show("Grafik nie wczytany.");
                    }
                }
            }
        }

        //Zamieniamy wszystkie wybrane dyżury na bezfunkcyjne.
        private void buttonBezFunkcji_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na bez funkcji.
            foreach (var (shiftId, employeeId) in _uiManager.GetAllSelectedEmployeeIds())
                _scheduleManager.ToBezFunkcji(shiftId, employeeId);
        }

        //Usuwamy grafik.
        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana. Usuwamy grafik. Wyświetlamy informację.
            UsunPodswietlenie();
            _scheduleManager.RemoveAll();
            MessageBox.Show("Grafik usunięty");
        }

        //Wyświetlamy Form2.
        private void buttonDodajOsoby_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Wyświetlamy Form2.
            Form2 dialog = new Form2(_employeeManager, _fileManagerPracownicy);
            dialog.ShowDialog();
        }

        //Zamieniamy wszystkie wybrane dyżury na sale.
        private void buttonSala_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na sale.
            foreach (var (shiftId, employeeId) in _uiManager.GetAllSelectedEmployeeIds())
                _scheduleManager.ToSala(shiftId, employeeId);
        }

        //Zamieniamy wszystkie wybrane dyżury na triaż.
        private void buttonTriaz_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Zamieniamy zaznaczone dyżury na triaż.
            foreach (var (shiftId, employeeId) in _uiManager.GetAllSelectedEmployeeIds())
                _scheduleManager.ToTriaz(shiftId, employeeId);
        }

        //Usuwamy wszystkie wybrane dyżury.
        private void buttonUsunDyzur_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Próbujemy usunąć zaznaczone. Jeśli się nie uda, wyświetlamy powiadomienie.
            foreach (var (shiftId, employeeId) in _uiManager.GetAllSelectedEmployeeIds())
            {
                _scheduleManager.RemoveFromShift(shiftId, employeeId);
            }
        }

        //Wczytujemy grafik z pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonWczytajGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Próbujemy wczytać grafik
            try
            {
                _fileManagerGrafik.WczytajGrafik("Grafik.txt");
                MessageBox.Show("Grafik wczytany.");
            }

            catch
            {
                MessageBox.Show("Grafik nie został wczytany.");
            }
        }

        //Zapisujemy grafik do pliku "Grafik.txt" i jeśli się uda, wyświetlamy informację.
        private void buttonZapiszGrafik_Click(object sender, EventArgs e)
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            UsunPodswietlenie();

            //Próbujemy zapisać grafik
            try
            {
                _fileManagerGrafik.ZapiszGrafik("Grafik.txt");
                MessageBox.Show("Grafik zapisany.");
            }
            catch { MessageBox.Show("Grafik nie został zapisany."); }
        }

        //Załadowanie Form1.
        public void Form1_Load(object sender, EventArgs e) { }

        //Generuje kontrolki grafiku i etykiety grafiku i listy pracowników. Zdarzenie asynchroniczne przycisku optymalizacji.
        private void InitializeComponent2()
        {
            //Akcja globalna wywołana po naciśnięciu formularza.
            this.Click += (sender, e) => EventGlobal.RaiseForm1Click();

            for (int nrZmiany = 0; nrZmiany <  2 *LICZBA_DNI; nrZmiany++)
            {
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
                    tableLayoutPanel2.Controls.Add(_uiManager.GetScheduleControlById(nrZmiany), nrZmiany, 1);
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
                    tableLayoutPanel3.Controls.Add(_uiManager.GetScheduleControlById(nrZmiany), nrZmiany - LICZBA_DNI, 1);
                }

                //Przypisujemy delegaty do zdarzeń drag and drop.
                int _nrZmiany = nrZmiany;
                _uiManager.GetScheduleControlById(nrZmiany).DragEnter += new DragEventHandler(this.scheduleControl_DragEnter);
                _uiManager.GetScheduleControlById(nrZmiany).DragDrop += new DragEventHandler((sender, e) => this.scheduleControl_DragDrop(sender, e, _nrZmiany));
            }

            //Tworzymy etykiety wyświetlające dane pracowników i delegaty do zdarzeń drag and drop.
            for (int nrOsoby = 0; nrOsoby < MAX_LICZBA_OSOB; nrOsoby++)
            {
                //Przypisujemy delegaty do zdarzeń drag and drop.
                int _nrOsoby = nrOsoby;
                _uiManager.GetEmployeeControlById(nrOsoby).MouseDown += new MouseEventHandler((sender, e) => labelsPracownicy_MouseDown(sender, e, _nrOsoby));
                
                //Dodajemy etykiety wyświetlające dane pracowników do furmularza.
                tableLayoutPanel1.Controls.Add(_uiManager.GetEmployeeControlById(nrOsoby), nrOsoby / 10, nrOsoby % 10);
            }

            //Zdarzenie asynchroniczne po kliknięciu przycisku "Opt".
            buttonOptymalizacja.Click += async (sender, e) =>
            {
                //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
                UsunPodswietlenie();

                //Próbujemy przeprowadzić optymalizację.
                try
                {
                    bool[] optymalneRozwiazanie;           //Rozwiąznie (genom) uzyskany w wyniku optymalizacji.
                    decimal tol = 0.0000003m;              //Wartość f. celu jaką należy osiągnąć, aby zakończyć optymalizację.
                    decimal tolX = 0.00000000001m;         //Minimalna zmiana f. celu skutkująca zresetowaniem licznika konsekwentnych iteracji.
                    int maxIterations = 200000;            //Maksymalna liczba iteracji. Po jej osiągnięciu algorytm kończy działać, nawet jeśli nie osiągnął wartości tol.
                    int maxConsIterations = 40000;         //Maksymalna liczba iteracji od ostatniej poprawy f. celu przynajmniej o tolX. Po jej osiągnięciu algorytm kończy działać, nawet jeśli nie osiągnął wartości tol.
                    int liczbaOsobnikow = 100;             //Liczebność populacji.

                    //Dezaktywujemy wszystkie kontrolki z wyjątkiem etykiety labelRaport.
                    foreach (Control control in this.Controls)
                    {
                        if (control != labelRaport)
                            control.Enabled = false;
                    }

                    //Przygotowujemy dane do optymalizacji.
                    _optimization.Prepare();

                    //Jeśli wszystko jest w porządku uruchamia się optymalizacja i mierzony jest czas.
                    startOptymalizacja = DateTime.Now;
                    {
                        optymalneRozwiazanie = await Task.Run(() =>_optimization.OptymalizacjaGA(LICZBA_ZMIENNYCH, liczbaOsobnikow, tol, tolX, maxConsIterations, maxIterations));
                    }
                    czasOptymalizacja = DateTime.Now - startOptymalizacja;

                    //Wyświetl grafik, zapisz grafik i wyświetl czas.
                    _optimization.DodajFunkcje(optymalneRozwiazanie);
                    _fileManagerGrafik.ZapiszGrafik("GrafikGA.txt");
                    MessageBox.Show("Przydzielanie funkcji ukończone w: " + czasOptymalizacja.ToString() + ".");
                }

                //Wyświetla informację, jeśli nie udało się przeprowadzić optymalizacji ze względu na złą liczbę pracowników.
                catch (InvalidDataException)
                {
                    MessageBox.Show("Aby przeprowadzić przydzielanie funkcji na każdej zmianie musi być od 3 do " + MAX_LICZBA_DYZUROW.ToString() + " .");
                }

                //Wyświetla informację, jeśli nie udało się przeprowadzić optymalizacji.
                catch
                {
                    MessageBox.Show("Przydzielanie funkcji nie powiodło się.");
                }

                //Odblokowuje UI po zakończeniu optymalizacji.
                finally
                {
                    foreach (Control control in this.Controls)
                    {
                        if (control != labelRaport)
                            control.Enabled = true;
                    }
                }
            };

            //Subskrybujemy zdarzenie nowych informacji o optymalizacji.
            _optimization.ProgressUpdated += raport =>
            {
                //Odświeżamy UI bezpośrednio w bezpieczny sposób.
                if (labelRaport.InvokeRequired)
                    labelRaport.Invoke(new Action(() => { labelRaport.Text = raport; }));

                else
                    labelRaport.Text = raport;
            };
        }

        //Drag and drop, etykieta Pracownicy.
        private void labelsPracownicy_MouseDown(object sender, MouseEventArgs e, int nrOsoby)
        {
            //Usuwamy podświetlenie, jeśli ktoś był zaznaczony.
            UsunPodswietlenie();

            //Sprawdzamy dyżury gdzie osoba występuje i podświetlamy: czerwony - bez funkcji, zielony - sala, niebieski - triaż.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                Employee employee = _employeeManager.GetEmployeeById(nrOsoby + 1);              //Pracownik.
                if (employee == null) return;

                //Zmiany pracownika.
                var shifts = _scheduleManager.GetShiftsForEmployee(employee.Numer);

                //Sprawdzamy funkcje i wyświetlamy kolor.
                foreach (var (shiftId, function) in shifts)
                {
                    try
                    {
                        _uiManager.ChangeColor(shiftId, function);
                    }
                    catch { };
                }
            }

            //Efekty drag nad drop.
            _uiManager.GetEmployeeControlById(nrOsoby).DoDragDrop((nrOsoby + 1).ToString(), DragDropEffects.Copy | DragDropEffects.Move);
        }

        //Drag and drop. Efekt wizualny i kopiowanie tekstu.
        private void scheduleControl_DragEnter(object sender, DragEventArgs e)
        {
            //Jeśli etykieta nie była pusta, to kopiujemy numer osoby.
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        //Drag and drop. Dodajemy dyżur.
        private void scheduleControl_DragDrop(object sender, DragEventArgs e, int nrZmiany)
        {
            //Pobieramy dane i dodajemy osobę do zmiany.
            string pom = e.Data.GetData(DataFormats.Text).ToString();
            _scheduleManager.AddToShift(nrZmiany, Convert.ToInt32(pom));
        }

        //Usuwamy podświetlenie, jeśli jakaś osoba jest zaznaczona.
        public void UsunPodswietlenie()
        {
            //Usuwamy podświetlenie, jeśli jakaś osoba jest wybrana.
            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
                _uiManager.ChangeColor(nrZmiany, (int)FunctionTypes.Default);
        }
    }
}
