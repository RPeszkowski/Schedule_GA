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

namespace Funkcje_GA
{
    public partial class Form2 : Form
    {
        private FileOperations fileOperator = new FileOperations();
        public Form2()
        {
            InitializeComponent();

            
            listBoxNumerOsoby.Items.Clear();
            for (int i = 0; i < Form1.MAX_LICZBA_OSOB; i++)
            {
                if (Form1.osoby[i] != null)
                {
                    listBoxNumerOsoby.Items.Add(Form1.osoby[i].numer.ToString());
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e) {}

        private void listBoxNumerOsoby_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                textBoxImie.Text = Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].imie;
                textBoxNazwisko.Text = Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].nazwisko;
                numericUpDownZaleglosci.Value = Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].zaleglosci;
                checkBoxCzyTriazDzien.Checked = Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].czyTriazDzien;
                checkBoxCzyTriazNoc.Checked = Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].czyTriazNoc;

            }
            catch { }
        }

        private void buttonDodaj_Click(object sender, EventArgs e)
        {
            if (textBoxImie.Text.Contains(' ') || textBoxNazwisko.Text.Contains(' '))
                MessageBox.Show("Imię i nazwisko nie mogą zawierać spacji.");

            else
            {
                if (Form1.liczbaOsob < Form1.MAX_LICZBA_OSOB)
                {
                    int wolnyNumer = Form1.MAX_LICZBA_OSOB - 1;

                    for (int i = Form1.MAX_LICZBA_OSOB - 1; i >= 0; i--)
                    {
                        if (Form1.osoby[i] == null)
                            wolnyNumer = i;
                    }

                    Osoba newOsoba = new Osoba(wolnyNumer + 1, textBoxImie.Text, textBoxNazwisko.Text, 0.0, Convert.ToInt32(numericUpDownZaleglosci.Value), checkBoxCzyTriazDzien.Checked, checkBoxCzyTriazNoc.Checked);
                    Form1.osoby[wolnyNumer] = newOsoba;
                    listBoxNumerOsoby.Items.Clear();
                    for (int i = 0; i < Form1.MAX_LICZBA_OSOB; i++)
                    {
                        if (Form1.osoby[i] != null)
                        {
                            listBoxNumerOsoby.Items.Add(Form1.osoby[i].numer.ToString());
                        }
                    }
                    Form1.liczbaOsob = Form1.liczbaOsob + 1;
                    string str = Form1.osoby[wolnyNumer].numer.ToString() + ". " + Form1.osoby[wolnyNumer].imie + " " + Form1.osoby[wolnyNumer].nazwisko + " " + Form1.osoby[wolnyNumer].wymiarEtatu.ToString() + " " + Form1.osoby[wolnyNumer].zaleglosci.ToString();
                    Form1.labels[wolnyNumer].Text = str;
                    if (!(Form1.osoby[wolnyNumer].czyTriazDzien && Form1.osoby[wolnyNumer].czyTriazNoc))
                        Form1.labels[wolnyNumer].ForeColor = Color.Orange;

                    else
                        Form1.labels[wolnyNumer].ForeColor = Color.Black;
                }
                else
                {
                    MessageBox.Show("Maksymalna liczba pracowników to " + Form1.MAX_LICZBA_OSOB.ToString() + ".");
                }
            }
        }

        private void buttonEdytujPracownika_Click(object sender, EventArgs e)
        {
            if (textBoxImie.Text.Contains(' ') || textBoxNazwisko.Text.Contains(' '))
                MessageBox.Show("Imię i nazwisko nie mogą zawierać spacji.");

            else
            {
                try
                {
                    Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].imie = textBoxImie.Text;
                    Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].nazwisko = textBoxNazwisko.Text;
                    Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].zaleglosci = Convert.ToInt32(numericUpDownZaleglosci.Value);
                    Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].czyTriazDzien = checkBoxCzyTriazDzien.Checked;
                    Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].czyTriazNoc = checkBoxCzyTriazNoc.Checked;

                    string str = Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].numer.ToString() + ". " + Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].imie + " " + Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].nazwisko + " " + Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].wymiarEtatu.ToString() + " " + Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].zaleglosci.ToString();
                    Form1.labels[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].Text = str;
                    if (!(Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].czyTriazDzien && Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].czyTriazNoc))
                        Form1.labels[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].ForeColor = Color.Orange;

                    else
                        Form1.labels[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].ForeColor = Color.Black;

                    MessageBox.Show("Zmieniono dane pracownika: " + Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].numer.ToString() + " " + Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].imie + " " + Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].nazwisko);
                }
                catch
                {
                    MessageBox.Show("Wybierz osobę, której dane chcesz zmienić.");
                }
            }
        }

        private void buttonSaveAndQuit_Click(object sender, EventArgs e)
        {
            fileOperator.ZapiszPracownikow("Pracownicy.txt");
            this.Close();
        }

        private void buttonUsun_Click(object sender, EventArgs e)
        {
            try
            {
                int nrUsunietejosoby = Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].numer;
                Form1.osoby[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1] = null;
                Form1.labels[Convert.ToInt32(listBoxNumerOsoby.SelectedItem) - 1].Text = "";
                listBoxNumerOsoby.Items.Remove(listBoxNumerOsoby.SelectedItem); 
                Form1.liczbaOsob--;
                for (int i = 0; i < Form1.LICZBA_DNI; i++)
                {
                    for(int j = 0; j < Form1.listBoxesDzien[i].Items.Count; j++)
                    {
                        
                        if (Form1.listBoxesDzien[i].GetNumber(j) == nrUsunietejosoby)
                        {
                            Form1.listBoxesDzien[i].Items.RemoveAt(j);
                            Form1.listBoxesDzien[i].Refresh();
                        }
                    }

                    for (int j = 0; j < Form1.listBoxesNoc[i].Items.Count; j++)
                    {
                        if (Form1.listBoxesNoc[i].GetNumber(j) == nrUsunietejosoby)
                        {
                            Form1.listBoxesNoc[i].Items.RemoveAt(j);
                            Form1.listBoxesNoc[i].Refresh();
                        }
                    }
                }
                MessageBox.Show("Usunięto dane pracownika.");
            }
            catch { }
        }
    }
}
