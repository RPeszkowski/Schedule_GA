using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Form1;
using static Funkcje_GA.Constans;

namespace Funkcje_GA
{
    //Form2 do dodawania/edytowania/usuwania osób.
    public partial class Form2 : Form
    {            

        //Konstruktor. Aktualizacja listboxa z numerami aktywnych pracowników.
        public Form2()
        {
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
                //Tworzymy osobę z pierwszym wolnym numerem i danymi takimi, jakie zostały wprowadzone do boxów. Dodajemy nową osobę do listy osób. Wyświetlamy numery istniejących w systemie osób.
                employeeManager.EmployeeAdd(textBoxImie.Text, textBoxNazwisko.Text, 0.0, Convert.ToInt32(numericUpDownZaleglosci.Value), checkBoxCzyTriazDzien.Checked, checkBoxCzyTriazNoc.Checked);
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
            //Zczytujemy z listBoxa numer osoby i próbujemy edytować dane. Jeśli się udało, wyświetlamy informację.
            try
            {
                int nrOsoby = Convert.ToInt32(listBoxNumerOsoby.SelectedItem);      //Numer wybranej osoby.
                employeeManager.EmployeeEdit(employeeManager.GetEmployeeById(nrOsoby), 
                                             textBoxImie.Text, 
                                             textBoxNazwisko.Text, 
                                             employeeManager.GetEmployeeById(nrOsoby).WymiarEtatu, 
                                             Convert.ToInt32(numericUpDownZaleglosci.Value), 
                                             checkBoxCzyTriazDzien.Checked, 
                                             checkBoxCzyTriazNoc.Checked);
                MessageBox.Show("Zmieniono dane pracownika: " + employeeManager.GetEmployeeById(nrOsoby).Numer.ToString() + " " + employeeManager.GetEmployeeById(nrOsoby).Imie + " " + employeeManager.GetEmployeeById(nrOsoby).Nazwisko);
            }

            //Obsługa wyjątku: osoba nie istnieje.
            catch (NullReferenceException)
            {
                MessageBox.Show("Dana osoba nie istnieje");
            }

            //Obsługa wyjątku: niepoprawne dane.
            catch (InvalidDataException)
            {
                MessageBox.Show("Imię i nazwisko nie mogą mogą być puste ani zawierać spacji.");
            }

            //Jeśli się nie udało, to wyświetlamy informację.
            catch
            {
                MessageBox.Show("Wybierz osobę, której dane chcesz zmienić.");
            }
        }

        //Zapisujemy dane pracowników do pliku "Pracownicy.txt" i zamykamy Form2.
        private void buttonSaveAndQuit_Click(object sender, EventArgs e)
        {
            FileManagementPracownicy.ZapiszPracownikow("Pracownicy.txt");
            this.Close();
        }

        //Usuwamy wybraną osobę.
        private void buttonUsun_Click(object sender, EventArgs e)
        {
            int nrOsoby = Convert.ToInt32(listBoxNumerOsoby.SelectedItem);      //Numer wybranej osoby.

            //Usuwamy osobę. Wyświetlamy numery istniejących w systemie osób.
            try
            {
                employeeManager.EmployeeDelete(employeeManager.GetEmployeeById(nrOsoby));
                UpdateListBoxNumerOsoby();
            }

            //Jeśli się nie udało, wyświetlamy komunikat.
            catch 
            {
                MessageBox.Show("Wybierz osobę, której dane chcesz usunąć.");
            };
        }

        //Załadowanie Form2.
        private void Form2_Load(object sender, EventArgs e) { }

        //Wyświetlamy dane wybranej osoby.
        private void listBoxNumerOsoby_SelectedIndexChanged(object sender, EventArgs e)
        {
            int nrOsoby = Convert.ToInt32(listBoxNumerOsoby.SelectedItem);      //Numer wybranej osoby.

            //Jeśli osoba istnieje w systemie to wyświetlamy jej numer.
            if (employeeManager.GetEmployeeById(nrOsoby) != null)
            {
                textBoxImie.Text = employeeManager.GetEmployeeById(nrOsoby).Imie;
                textBoxNazwisko.Text = employeeManager.GetEmployeeById(nrOsoby).Nazwisko;
                numericUpDownZaleglosci.Value = employeeManager.GetEmployeeById(nrOsoby).Zaleglosci;
                checkBoxCzyTriazDzien.Checked = employeeManager.GetEmployeeById(nrOsoby).CzyTriazDzien;
                checkBoxCzyTriazNoc.Checked = employeeManager.GetEmployeeById(nrOsoby).CzyTriazNoc;

            }
        }

        //Wyświetlamy numery istniejących w systemie osób.
        private void UpdateListBoxNumerOsoby()
        {
            //Usuń wszytskie numery. Jeśli osoba jest w systemie to dodaj jej numer.
            listBoxNumerOsoby.Items.Clear();
            for (int nrOsoby = 1; nrOsoby <= MAX_LICZBA_OSOB; nrOsoby++)
            {
                if (employeeManager.GetEmployeeById(nrOsoby) != null)
                    listBoxNumerOsoby.Items.Add(employeeManager.GetEmployeeById(nrOsoby).Numer.ToString());

            }
        }
    }
}
