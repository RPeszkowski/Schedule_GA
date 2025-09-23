using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using static Funkcje_GA.Constans;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Form2 do dodawania/edytowania/usuwania osób.
    public partial class Form2 : Form
    {
        private readonly IEmployeeManagement _employeeManager;               //Instancja do zarządzania pracownikami.
        private readonly IViewFile _viewFile;                           //Instancja do zarządzania plikami.

        //Konstruktor. Aktualizacja listboxa z numerami aktywnych pracowników.
        public Form2(IEmployeeManagement empManager, IViewFile viewFile)
        {
            //Przypisujemy menagera pracowników.
            this._employeeManager = empManager;
            this._viewFile = viewFile;

            //Generuje kontrolki. Metoda stworzona przez Designera.
            InitializeComponent();

            //Wyświetlamy numery istniejących w systemie osób.
            UpdateListBoxNumerOsoby();
        }

        //Dodawanie osoby do listy i do pliku Pracownicy.txt.
        private void buttonDodaj_Click(object sender, EventArgs e)
        {
            //Jeśli wszystkie warunki są spełnione to dodajemy nową osobę.
            try
            {
                //Szukamy wolnego numeru.
                int wolnyNumer = Enumerable.Range(1, MAX_LICZBA_OSOB).FirstOrDefault(i => _employeeManager.GetEmployeeById(i) == null);

                if (wolnyNumer == 0)
                    throw new TooManyEmployeesException($"Maksymalna liczba osób to: {MAX_LICZBA_OSOB}.");

                //Tworzymy osobę z pierwszym wolnym numerem i danymi takimi, jakie zostały wprowadzone do boxów.
                //Dodajemy nową osobę do listy osób. Wyświetlamy numery istniejących w systemie osób.
                _employeeManager.EmployeeAdd(wolnyNumer,
                                            textBoxImie.Text, 
                                            textBoxNazwisko.Text, 
                                            0.0, 
                                            Convert.ToInt32(numericUpDownZaleglosci.Value), 
                                            checkBoxCzyTriazDzien.Checked, 
                                            checkBoxCzyTriazNoc.Checked);
                UpdateListBoxNumerOsoby();
            }

            //Sprawdzamy, czy nie została osiągnięta maksymalna liczba osób w systemie.
            catch (TooManyEmployeesException)
            {
                MessageBox.Show("Maksymalna liczba pracowników to " + MAX_LICZBA_OSOB.ToString() + ".");
            }

            //Obsługa wyjątku: niepoprawne dane.
            catch (InvalidDataException)
            {
                MessageBox.Show("Imię i nazwisko nie mogą mogą być puste ani zawierać spacji.");
            }
        }

        //Edycja danych pracownika.
        private void buttonEdytujPracownika_Click(object sender, EventArgs e)
        {
            //Sprawdzamy, czy wybrano osobę.
            if (listBoxNumerOsoby.SelectedIndex != -1)
            {
                //Zczytujemy z listBoxa numer osoby i próbujemy edytować dane. Jeśli się udało, wyświetlamy informację.
                try
                {
                    int nrOsoby = Convert.ToInt32(listBoxNumerOsoby.SelectedItem);      //Numer wybranej osoby.
                    Employee employee = _employeeManager.GetEmployeeById(nrOsoby);       //Pracownik.                                            
                    _employeeManager.EmployeeEdit(_employeeManager.GetEmployeeById(nrOsoby),
                                             textBoxImie.Text,
                                             textBoxNazwisko.Text,
                                             employee.WymiarEtatu,
                                             Convert.ToInt32(numericUpDownZaleglosci.Value),
                                             checkBoxCzyTriazDzien.Checked,
                                             checkBoxCzyTriazNoc.Checked);
                    MessageBox.Show("Zmieniono dane pracownika: " + employee.Numer.ToString() + " " + employee.Imie + " " + employee.Nazwisko);
                }

                //Obsługa wyjątku: niepoprawne dane.
                catch (EmployeeNameSurnameException ex)
                {
                    Log.Error(ex, "Imię i nazwisko nie mogą mogą być puste ani zawierać spacji.");
                    MessageBox.Show("Imię i nazwisko nie mogą mogą być puste ani zawierać spacji.");
                }

                //Inne poważne wyjątki.
                catch (Exception ex)
                {
                    Log.Error(ex, "Błędne dane pracownika");
                }
            }

            else
                MessageBox.Show("Wybierz osobę, której dane chcesz zmienić.");
        }

        //Zapisujemy dane pracowników do pliku "Pracownicy.txt" i zamykamy Form2.
        private void buttonSaveAndQuit_Click(object sender, EventArgs e)
        {
            _viewFile.SaveEmployees("Pracownicy.txt");
            this.Close();
        }

        //Usuwamy wybraną osobę.
        private void buttonUsun_Click(object sender, EventArgs e)
        {
            int nrOsoby = Convert.ToInt32(listBoxNumerOsoby.SelectedItem);      //Numer wybranej osoby.

            //Usuwamy osobę. Wyświetlamy numery istniejących w systemie osób.
            if (listBoxNumerOsoby.SelectedIndex != -1)
            {
                try
                {
                    _employeeManager.EmployeeDelete(_employeeManager.GetEmployeeById(nrOsoby));
                    UpdateListBoxNumerOsoby();
                    MessageBox.Show("Usunięto dane pracownika.");
                }

                catch(Exception ex)
                {
                    Log.Error(ex, "Usuwanie pracownika nie powiodło się.");
                }
            }

            //Jeśli się nie udało, wyświetlamy komunikat.
            else
                MessageBox.Show("Wybierz osobę, której dane chcesz usunąć.");
        }

        //Załadowanie Form2.
        private void Form2_Load(object sender, EventArgs e) { }

        //Wyświetlamy dane wybranej osoby.
        private void listBoxNumerOsoby_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nrOsoby = Convert.ToInt32(listBoxNumerOsoby.SelectedItem);      //Numer wybranej osoby.

            //Jeśli osoba istnieje w systemie to wyświetlamy jej numer.
            if (_employeeManager.GetEmployeeById(nrOsoby) != null)
            {
                textBoxImie.Text = _employeeManager.GetEmployeeById(nrOsoby).Imie;
                textBoxNazwisko.Text = _employeeManager.GetEmployeeById(nrOsoby).Nazwisko;
                numericUpDownZaleglosci.Value = _employeeManager.GetEmployeeById(nrOsoby).Zaleglosci;
                checkBoxCzyTriazDzien.Checked = _employeeManager.GetEmployeeById(nrOsoby).CzyTriazDzien;
                checkBoxCzyTriazNoc.Checked = _employeeManager.GetEmployeeById(nrOsoby).CzyTriazNoc;
            }
        }

        //Wyświetlamy numery istniejących w systemie osób.
        private void UpdateListBoxNumerOsoby()
        {
            //Usuń wszytskie numery. Jeśli osoba jest w systemie to dodaj jej numer.
            listBoxNumerOsoby.Items.Clear();
            for (int nrOsoby = 1; nrOsoby <= MAX_LICZBA_OSOB; nrOsoby++)
            {
                if (_employeeManager.GetEmployeeById(nrOsoby) != null)
                    listBoxNumerOsoby.Items.Add(_employeeManager.GetEmployeeById(nrOsoby).Numer.ToString());
            }
        }
    }
}
