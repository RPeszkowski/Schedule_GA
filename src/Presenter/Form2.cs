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
using static Funkcje_GA.Constants;
using static Funkcje_GA.CustomExceptions;

namespace Funkcje_GA
{
    //Form2 do dodawania/edytowania/usuwania osób.
    internal partial class Form2 : Form, IViewForm2, IEmployeeForm
    {
        //Konstruktor. Aktualizacja listboxa z numerami aktywnych pracowników.
        public Form2()
        {
            //Generuje kontrolki. Metoda stworzona przez Designera.
            InitializeComponent();
        }

        //Zdarzenie zgłaszające, że użytkownik chce dodać pracownika do repozytorium.
        public event Action<string, string, int, bool, bool> EmployeeAddedFromUI;

        //Zdarzenie zgłaszające, że użytkownik chce edytować dane pracownika.
        public event Action<int, string, string, int, bool, bool> EmployeeEditedFromUI;

        //Zdarzenie zgłaszające, że użytkownik chce usunąć dane pracownika.
        public event Action<int> EmployeeDeletedFromUI;

        //Zdarzenie zgłaszające, że załadowano Form2.
        public event Action Form2Loaded;

        //Zdarzenie zgłaszające, że użytkownik chce zapisać pracowników
        public event Action SaveEmployees;

        //Zdarzenie zgłaszające, że zmieniono wybraną osobę i trzeba wyświetlić nowe dane.
        public event Action<int> SelectedEmployeeChanged;

        //Dodawanie osoby do listy i do pliku Pracownicy.txt.
        private void buttonDodaj_Click(object sender, EventArgs e)
        {
            //Wywołujemy akcję z danymi z boxów.
            EmployeeAddedFromUI?.Invoke(textBoxImie.Text, textBoxNazwisko.Text, Convert.ToInt32(numericUpDownZaleglosci.Value), checkBoxCzyTriazDzien.Checked, checkBoxCzyTriazNoc.Checked);
        }

        //Edycja danych pracownika.
        private void buttonEdytujPracownika_Click(object sender, EventArgs e)
        {
            //Sprawdzamy, czy wybrano osobę.
            if (listBoxNumerOsoby.SelectedIndex != -1)
            {
                //Zczytujemy z listBoxa numer osoby i próbujemy edytować dane. Jeśli się udało, wyświetlamy informację.
                int nrOsoby = Convert.ToInt32(listBoxNumerOsoby.SelectedItem);      //Numer wybranej osoby.
                    
                //Wywołujemy akcję.
                EmployeeEditedFromUI?.Invoke(nrOsoby, textBoxImie.Text, textBoxNazwisko.Text, Convert.ToInt32(numericUpDownZaleglosci.Value), checkBoxCzyTriazDzien.Checked, checkBoxCzyTriazNoc.Checked);
            }

            else
                RaiseUserNotification("Wybierz osobę, której dane chcesz zmienić.");
        }

        //Zapisujemy dane pracowników do pliku "Pracownicy.txt" i zamykamy Form2.
        private void buttonSaveAndQuit_Click(object sender, EventArgs e)
        {
            //Zapisujemy plik, zamykamy okno.
            SaveEmployees?.Invoke();
            this.Close();
        }

        //Usuwamy wybraną osobę.
        private void buttonUsun_Click(object sender, EventArgs e)
        {
            //Usuwamy osobę.
            if (listBoxNumerOsoby.SelectedIndex != -1)
            {
                //Próbujemy pobrać numer osoby. Wyjątek oznacza krytyczną niespójność danych.
                if (!Int32.TryParse(listBoxNumerOsoby.SelectedItem.ToString(), out int nrOsoby))
                    throw new InvalidDataException("Kontrolka z numerem osoby posiada nieprawidłowe dane.");

                //Zgłaszamy zdarzenie - usunięcie pracownika.
                EmployeeDeletedFromUI?.Invoke(nrOsoby);
            }

            //Jeśli się nie udało, wyświetlamy komunikat.
            else
                RaiseUserNotification("Wybierz osobę, której dane chcesz usunąć.");
        }

        //Załadowanie Form2.
        private void Form2_Load(object sender, EventArgs e) 
        {
            //Zdarzenie - załadowanie Form2.
            Form2Loaded?.Invoke();
        }

        //Wyświetlamy dane wybranej osoby.
        private void listBoxNumerOsoby_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Próbujemy pobrać numer osoby. Wyjątek oznacza krytyczną niespójność danych.
            if (!Int32.TryParse(listBoxNumerOsoby.SelectedItem.ToString(), out int nrOsoby))
                throw new InvalidDataException("Kontrolka z numerem osoby posiada nieprawidłowe dane.");

            //Zgłaszamy prośbę o dane pracownika.
            SelectedEmployeeChanged?.Invoke(nrOsoby);
        }

        //Powiadomienie użytkownika.
        public virtual void RaiseUserNotification(string message)
        {
            MessageBox.Show(message, "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Wyświetlamy dane wybranego pracownika.
        public virtual void ShowEmployeeData(string imie, string nazwisko, int zaleglosci, bool triazDzien, bool triazNoc)
        {
            //Wyświetlamy dane.
            textBoxImie.Text = imie;
            textBoxNazwisko.Text = nazwisko;
            numericUpDownZaleglosci.Value = zaleglosci;
            checkBoxCzyTriazDzien.Checked = triazDzien;
            checkBoxCzyTriazNoc.Checked = triazNoc;
        }

        //Wyświetlamy okno.
        public void ShowForm()
        {
            this.ShowDialog();
        }

        //Wyświetlamy numery istniejących w systemie osób.
        public virtual void UpdateControlNumerOsoby(List<int> listaNumerów)
        {
            //Usuń wszytskie numery. Jeśli osoba jest w systemie to dodaj jej numer.
            listBoxNumerOsoby.Items.Clear();
            foreach(int numer in listaNumerów)
                listBoxNumerOsoby.Items.Add(numer);
        }
    }
}
