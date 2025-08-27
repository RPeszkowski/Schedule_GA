using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Form1;

namespace Funkcje_GA
{

    public partial class Form1 : Form
    {
        #region Zmienne, klasy
        public partial class myListBox : ListBox
        {
            public int GetNumber(int index)
            {
                int number;
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 's')
                        str = str.Remove(str.Length - 1);

                    if (str[str.Length - 1] == 't')
                        str = str.Remove(str.Length - 1);

                    try
                    {
                        number = Convert.ToInt32(str);
                    }

                    catch { number = -1; }
                    ;
                }
                else number = -1;

                return number;
            }

            public void ToTriaz(int index)
            {
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 't')
                        this.Items[index] = str;

                    else if (str[str.Length - 1] == 's')
                    {
                        str = str.Remove(str.Length - 1);
                        this.Items[index] = str + 't';
                    }
                    else this.Items[index] = str + 't';
                }
            }

            public void ToSala(int index)
            {
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 's')
                        this.Items[index] = str;

                    else if (str[str.Length - 1] == 't')
                    {
                        str = str.Remove(str.Length - 1);
                        this.Items[index] = str + 's';
                    }
                    else this.Items[index] = str + 's';
                }
            }

            public void ToBezFunkcji(int index)
            {
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 's' || str[str.Length - 1] == 't')
                        str = str.Remove(str.Length - 1);

                    this.Items[index] = str;
                }
            }

            public int GetFunction(int index)
            {
                int nrFunkcji = -1;
                string str = this.Items[index].ToString();
                if (this.Items != null)
                {
                    if (str[str.Length - 1] == 's')
                        nrFunkcji = 1;

                    else if (str[str.Length - 1] == 't')
                        nrFunkcji = 2;

                    else
                    {
                        try
                        {
                            Convert.ToInt32(str);
                            nrFunkcji = 0;
                        }
                        catch { }
                    }
                }
                return nrFunkcji;
            }
        }
        public class Osoba
        {
            public int numer;
            public string imie;
            public string nazwisko;
            public double wymiarEtatu;
            public int zaleglosci;
            public bool czyTriazDzien;
            public bool czyTriazNoc;

            public Osoba(int osobaNumer, string osobaImie, string osobaNazwisko, double osobaWymiarEtatu, int osobaZaleglosci, bool osobaCzyTriazDzien, bool osobaCzyTriazNoc)
            {
                numer = osobaNumer;
                imie = osobaImie;
                nazwisko = osobaNazwisko;
                wymiarEtatu = osobaWymiarEtatu;
                zaleglosci = osobaZaleglosci;
                czyTriazDzien = osobaCzyTriazDzien;
                czyTriazNoc = osobaCzyTriazNoc;
            }
        }
        public class Osobnik
        {
            public bool[] genotyp = { };
            public decimal wartosc;
            public Osobnik(bool[] gen, decimal war)
            {
                genotyp = gen;
                wartosc = war;
            }
        }
        public class OsobnikComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return (new CaseInsensitiveComparer()).Compare(((Osobnik)x).wartosc, ((Osobnik)y).wartosc);
            }
        }

        public delegate decimal Callback(bool[] funkcje, int[] grafik, int[] nieTriazDzien, int[] nieTriazNoc, int[] liczbaDyzurow, double[] oczekiwanaLiczbaFunkcji);
        Callback handler = FunkcjaCelu;
        public const int MAX_LICZBA_OSOB = 45;
        public const int LICZBA_DNI = 31;
        public const int MAX_LICZBA_DYZUROW = 7;
        public static int liczbaOsob = 0;
        public static System.Windows.Forms.Label[] labels = new System.Windows.Forms.Label[MAX_LICZBA_OSOB];
        public static System.Windows.Forms.Label[] labelsDzien = new System.Windows.Forms.Label[LICZBA_DNI];
        public static System.Windows.Forms.Label[] labelsNoc = new System.Windows.Forms.Label[LICZBA_DNI];
        public static myListBox[] listBoxesDzien = new myListBox[LICZBA_DNI];
        public static myListBox[] listBoxesNoc = new myListBox[LICZBA_DNI];
        public static Osoba[] osoby = new Osoba[MAX_LICZBA_OSOB];
        #endregion

        #region Form1 konstruktor, load
        public Form1()
        {
            InitializeComponent();

            #region Tworzenie kontrolek (listboxy, etykiety)
            for (int j = 0; j < LICZBA_DNI; j++)
            {
                listBoxesDzien[j] = new myListBox();
                listBoxesDzien[j].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                listBoxesDzien[j].Size = Size = new System.Drawing.Size(40, 400);
                listBoxesDzien[j].AllowDrop = true;
                tableLayoutPanel2.Controls.Add(listBoxesDzien[j], j, 1);

                labelsDzien[j] = new System.Windows.Forms.Label();
                labelsDzien[j].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labelsDzien[j].Size = Size = new System.Drawing.Size(340, 40);
                labelsDzien[j].Text = (j + 1).ToString();
                tableLayoutPanel2.Controls.Add(labelsDzien[j], j, 0);

                listBoxesNoc[j] = new myListBox();
                listBoxesNoc[j].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                listBoxesNoc[j].Size = Size = new System.Drawing.Size(40, 400);
                listBoxesNoc[j].AllowDrop = true;
                tableLayoutPanel3.Controls.Add(listBoxesNoc[j], j, 1);

                labelsNoc[j] = new System.Windows.Forms.Label();
                labelsNoc[j].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labelsNoc[j].Size = Size = new System.Drawing.Size(340, 40);
                labelsNoc[j].Text = (j + 1).ToString();
                tableLayoutPanel3.Controls.Add(labelsNoc[j], j, 0);
            }

            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                labels[i] = new System.Windows.Forms.Label();
                labels[i].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                labels[i].Size = Size = new System.Drawing.Size(340, 40);
                labels[i].Text = "";
                tableLayoutPanel1.Controls.Add(labels[i], i / 10, i % 10);
            }
            #endregion

            #region Wczytywanie pracowników
            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                string readText;
                string[] subs;
                try
                {
                    readText = File.ReadAllLines("Pracownicy.txt").Skip(i).Take(1).First();
                    subs = readText.Split(' ');
                    labels[Convert.ToInt32(subs[0]) - 1].Text = subs[0] + ". " + subs[1] + " " + subs[2] + " 0 " + " " + subs[3].ToString();
                    Osoba newOsoba = new Osoba(Convert.ToInt32(subs[0]), subs[1], subs[2], 0.0, Convert.ToInt32(subs[3]), Convert.ToBoolean(subs[4]), Convert.ToBoolean(subs[5]));
                    osoby[Convert.ToInt32(subs[0]) - 1] = newOsoba;
                    if (!(newOsoba.czyTriazDzien && newOsoba.czyTriazNoc))
                        labels[Convert.ToInt32(subs[0]) - 1].ForeColor = Color.Orange;

                    else
                        labels[Convert.ToInt32(subs[0]) - 1].ForeColor = Color.Black;

                    liczbaOsob = liczbaOsob + 1;
                }
                catch
                { }
            }
            #endregion

            #region Drag and drop/Podświetlenie eventy

            for (int i = 0; i < MAX_LICZBA_OSOB; i ++)
            {
                int iter3 = i;
                labels[i].MouseDown += new MouseEventHandler((sender, e) => lbl_MouseDown(sender, e, iter3));
            }

            for (int j = 0; j < LICZBA_DNI; j++)
            {
                int iter3 = j;
                listBoxesDzien[j].DragEnter += new DragEventHandler(this.listBoxDzien_DragEnter);
                listBoxesDzien[j].DragDrop += new DragEventHandler((sender, e) => listBoxDzien_DragDrop(sender, e, iter3));
            }

            for (int j = 0; j < LICZBA_DNI; j++)
            {
                int iter3 = j;
                listBoxesNoc[j].DragEnter += new DragEventHandler(this.listBoxNoc_DragEnter);
                listBoxesNoc[j].DragDrop += new DragEventHandler((sender, e) => listBoxNoc_DragDrop(sender, e, iter3));
            }

            buttonOptymalizacja.Click += async (o, e) =>
            {
                int[] dyzuryGrafik = new int[2 * LICZBA_DNI * MAX_LICZBA_DYZUROW];
                int[] nieTriazDzien;
                int[] nieTriazNoc;
                int[] liczbaDyzurow = new int[2 * LICZBA_DNI];
                bool[] optymalneRozwiazanie;
                double[] oczekiwanaLiczbaFunkcji = new double[MAX_LICZBA_OSOB];
                decimal stopienZdegenerowania = 0.0m;

                for (int i = 0; i < LICZBA_DNI; i++)
                {
                    listBoxesDzien[i].ResetBackColor();
                    listBoxesNoc[i].ResetBackColor();
                }

                for (int i = 0; i < LICZBA_DNI; i++)
                {
                    if (listBoxesDzien[i].Items.Count > MAX_LICZBA_DYZUROW || listBoxesNoc[i].Items.Count > MAX_LICZBA_DYZUROW)
                    {
                        MessageBox.Show("Aby móc wykorzystać automatyczne rozdzielanie funkcji liczba dyżurów danego dnia nie może przekraczać " + MAX_LICZBA_DYZUROW.ToString() + ".");
                        return;
                    }
                }

                dyzuryGrafik = UtworzGrafik();
                nieTriazDzien = ListaNieTiazDzien();
                nieTriazNoc = ListaNieTiazNoc();
                liczbaDyzurow = LiczbaDyzurow(dyzuryGrafik);
                oczekiwanaLiczbaFunkcji = OczekiwanaLiczbaFunkcji(osoby);
                stopienZdegenerowania = StopienZdegenerowania(liczbaDyzurow, nieTriazDzien, nieTriazNoc, dyzuryGrafik);

                if (liczbaDyzurow.Contains(1) || liczbaDyzurow.Contains(2))
                {
                    MessageBox.Show("Niepoprawny grafik. Sprawdź, czy do każdego dnia przypisane są co najmniej 3 osoby.");
                    return;
                }

                optymalneRozwiazanie = await Task.Run(() => OptymalizacjaGA(4 * LICZBA_DNI * MAX_LICZBA_DYZUROW, handler, 100, 0.000004m, 0.00000000001m, 10000, 1000000, dyzuryGrafik, nieTriazDzien, nieTriazNoc, liczbaDyzurow, oczekiwanaLiczbaFunkcji, stopienZdegenerowania));
                DodajFunkcje(optymalneRozwiazanie);
                zapiszGrafik("GrafikGA.txt");
                labelRaport.Text = labelRaport.Text + " Ukończono.";                
            };

            #endregion

            #region Pytanie o wczytanie grafiku

            if (File.Exists("Grafik.txt"))
            {
                var result = MessageBox.Show("Wczytać ostatni grafik?", "Wczytywanie grafiku", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                    wczytajGrafik("Grafik.txt");
            }
            #endregion
        }

        public void Form1_Load(object sender, EventArgs e) {}

        #endregion

        #region DragAndDrop/Podświetlenie

        public void lbl_MouseDown(object sender, MouseEventArgs e, int nrOsoby)
        {
            for (int i = 0; i < LICZBA_DNI; i++)
            {
                listBoxesDzien[i].ResetBackColor();
                listBoxesNoc[i].ResetBackColor();
                for (int j = 0; j < listBoxesDzien[i].Items.Count; j++)
                {
                    if (osoby[nrOsoby] != null)
                    {
                        if (listBoxesDzien[i].GetNumber(j) == osoby[nrOsoby].numer)
                        {
                            switch (listBoxesDzien[i].GetFunction(j))
                            {
                                case 0:
                                    listBoxesDzien[i].BackColor = Color.Red;
                                    break;

                                case 1:
                                    listBoxesDzien[i].BackColor = Color.Green;
                                    break;

                                case 2:
                                    listBoxesDzien[i].BackColor = Color.Blue;
                                    break;
                            }
                        }
                    }
                }
                if (osoby[nrOsoby] != null)
                {
                    for (int j = 0; j < listBoxesNoc[i].Items.Count; j++)
                    {
                        if (listBoxesNoc[i].GetNumber(j) == osoby[nrOsoby].numer)
                        {
                            switch (listBoxesNoc[i].GetFunction(j))
                            {
                                case 0:
                                    listBoxesNoc[i].BackColor = Color.Red;
                                    break;

                                case 1:
                                    listBoxesNoc[i].BackColor = Color.Green;
                                    break;

                                case 2:
                                    listBoxesNoc[i].BackColor = Color.Blue;
                                    break;
                            }
                        }
                    }
                }
            }

            labels[nrOsoby].DoDragDrop(labels[nrOsoby].Text, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void listBoxDzien_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listBoxDzien_DragDrop(object sender, DragEventArgs e, int nrListBoxa)
        {
            string pom = e.Data.GetData(DataFormats.Text).ToString();
            string[] subs = pom.Split('.');
            bool flag1 = true;
            foreach (string item in listBoxesDzien[nrListBoxa].Items)
            {

                if (item == subs[0] || item == subs[0] + "s" || item == subs[0] + "t")
                    flag1 = false;
            }
            if (flag1)
                listBoxesDzien[nrListBoxa].Items.Add(subs[0]);

            foreach (Osoba iter in osoby)
            {

                if (iter != null)
                {
                    if (iter.numer.ToString() == subs[0] && flag1)
                    {
                        iter.wymiarEtatu = iter.wymiarEtatu + 1.0;
                        labels[iter.numer - 1].Text = iter.numer.ToString() + ". " + iter.imie + " " + iter.nazwisko + " " + iter.wymiarEtatu.ToString() + " " + iter.zaleglosci.ToString();
                    }
                }
            }
        }

        private void listBoxNoc_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listBoxNoc_DragDrop(object sender, DragEventArgs e, int nrListBoxa)
        {
            string pom = e.Data.GetData(DataFormats.Text).ToString();
            string[] subs = pom.Split('.');
            bool flag1 = true;
            foreach (string item in listBoxesNoc[nrListBoxa].Items)
            {

                if (item == subs[0] || item == subs[0] + "s" || item == subs[0] + "t")
                    flag1 = false;
            }
            if (flag1)
                listBoxesNoc[nrListBoxa].Items.Add(subs[0]);

            foreach (Osoba iter in osoby)
            {

                if (iter != null)
                {
                    if (iter.numer.ToString() == subs[0] && flag1)
                    {
                        iter.wymiarEtatu = iter.wymiarEtatu + 1.0;
                        labels[iter.numer - 1].Text = iter.numer.ToString() + ". " + iter.imie + " " + iter.nazwisko + " " + iter.wymiarEtatu.ToString() + " " + iter.zaleglosci.ToString();
                    }
                }
            }
        }


        #endregion DragAndDrop

        #region Funkcje - kontrolki
        private void buttonDodajOsoby_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < LICZBA_DNI; j++)
            {
                listBoxesDzien[j].ResetBackColor();
                listBoxesNoc[j].ResetBackColor();
            }

            Form2 dialog = new Form2();
            dialog.ShowDialog();
        }

        private void buttonUsunDyzur_Click(object sender, EventArgs e)
        {
            foreach (myListBox iter in listBoxesDzien)
            {
                if (iter.SelectedItem != null)
                {
                    foreach (Osoba iter2 in osoby)
                    {
                        if (iter2 != null)
                        {
                            if (iter2.numer == iter.GetNumber(iter.SelectedIndex))
                            {
                                iter2.wymiarEtatu = iter2.wymiarEtatu - 1;
                                labels[iter2.numer - 1].Text = iter2.numer.ToString() + ". " + iter2.imie + " " + iter2.nazwisko + " " + iter2.wymiarEtatu.ToString() + " " + iter2.zaleglosci.ToString();
                            }
                        }
                    }
                }
            }

            foreach (myListBox iter in listBoxesNoc)
            {
                if (iter.SelectedItem != null)
                {
                    foreach (Osoba iter2 in osoby)
                    {
                        if (iter2 != null)
                        {
                            if (iter2.numer == iter.GetNumber(iter.SelectedIndex))
                            {
                                iter2.wymiarEtatu = iter2.wymiarEtatu - 1;
                                labels[iter2.numer - 1].Text = iter2.numer.ToString() + ". " + iter2.imie + " " + iter2.nazwisko + " " + iter2.wymiarEtatu.ToString() + " " + iter2.zaleglosci.ToString();
                            }
                        }
                    }
                }
            }

            for (int j = 0; j < LICZBA_DNI; j++)
            {
                listBoxesDzien[j].ResetBackColor();
                listBoxesNoc[j].ResetBackColor();

                listBoxesDzien[j].Items.Remove(listBoxesDzien[j].SelectedItem);
                listBoxesNoc[j].Items.Remove(listBoxesNoc[j].SelectedItem);
            }
        }

        private void buttonSala_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < LICZBA_DNI; j++)
            {
                listBoxesDzien[j].ResetBackColor();
                listBoxesNoc[j].ResetBackColor();

                int idx;
                try
                {
                    idx = listBoxesDzien[j].Items.IndexOf(listBoxesDzien[j].SelectedItem);
                    listBoxesDzien[j].ToSala(idx);
                }
                catch { }

                try
                {
                    idx = listBoxesNoc[j].Items.IndexOf(listBoxesNoc[j].SelectedItem);
                    listBoxesNoc[j].ToSala(idx);
                }
                catch { }
            }
        }

        private void buttonBezFunkcji_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < LICZBA_DNI; j++)
            {
                listBoxesDzien[j].ResetBackColor();
                listBoxesNoc[j].ResetBackColor();

                int idx;
                try
                {
                    idx = listBoxesDzien[j].Items.IndexOf(listBoxesDzien[j].SelectedItem);
                    listBoxesDzien[j].ToBezFunkcji(idx);
                }
                catch { }

                try
                {
                    idx = listBoxesNoc[j].Items.IndexOf(listBoxesNoc[j].SelectedItem);
                    listBoxesNoc[j].ToBezFunkcji(idx);
                }
                catch { }
            }
        }

        private void buttonTriaz_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < LICZBA_DNI; j++)
            {
                listBoxesDzien[j].ResetBackColor();
                listBoxesNoc[j].ResetBackColor();

                int idx;
                try
                {
                    idx = listBoxesDzien[j].Items.IndexOf(listBoxesDzien[j].SelectedItem);
                    listBoxesDzien[j].ToTriaz(idx);
                }
                catch { }

                try
                {
                    idx = listBoxesNoc[j].Items.IndexOf(listBoxesNoc[j].SelectedItem);
                    listBoxesNoc[j].ToTriaz(idx);
                }
                catch { }
            }
        }

        private void buttonClearAll_Click(object sender, EventArgs e)
        {
            foreach (myListBox iter in listBoxesDzien)
            {
                iter.Items.Clear();
                iter.Items.Clear();
                iter.ResetBackColor();
            }

            foreach (myListBox iter in listBoxesNoc)
            {
                iter.Items.Clear();
                iter.Items.Clear();
                iter.ResetBackColor();
            }

            foreach (Osoba iter in osoby)
            {
                if (iter != null)
                {
                    iter.wymiarEtatu = 0.0;
                    labels[iter.numer - 1].Text = iter.numer.ToString() + ". " + iter.imie + " " + iter.nazwisko + " " + iter.wymiarEtatu.ToString() + " " + iter.zaleglosci.ToString();
                }
            }
        }

        private void formClick(object sender, EventArgs e)
        {
            for (int j = 0; j < LICZBA_DNI; j++)
            {
                listBoxesDzien[j].ClearSelected();
                listBoxesNoc[j].ClearSelected();

                listBoxesDzien[j].ResetBackColor();
                listBoxesNoc[j].ResetBackColor();
            }
        }

        private void buttonZapiszGrafik_Click(object sender, EventArgs e)
        {
            zapiszGrafik("Grafik.txt");
            MessageBox.Show("Grafik zapisany.");
        }

        private void buttonWczytajGrafik_Click(object sender, EventArgs e)
        {
            wczytajGrafik("Grafik.txt");
            MessageBox.Show("Grafik wczytany.");
        }

        public void buttonOptymalizacja_Click(object sender, EventArgs e)
        {
            int[] dyzuryGrafik = new int[2*LICZBA_DNI*MAX_LICZBA_DYZUROW];
            int[] nieTriazDzien;
            int[] nieTriazNoc;
            int[] liczbaDyzurow = new int[2 * LICZBA_DNI];
            bool[] optymalneRozwiazanie;
            double[] oczekiwanaLiczbaFunkcji = new double[MAX_LICZBA_OSOB];
            decimal stopienZdegenerowania = 0.0m;

            for (int i = 0; i < LICZBA_DNI; i++)
            {
                listBoxesDzien[i].ResetBackColor();
                listBoxesNoc[i].ResetBackColor();
            }

            for (int i = 0; i < LICZBA_DNI; i++)
            {
                if (listBoxesDzien[i].Items.Count > MAX_LICZBA_DYZUROW || listBoxesNoc[i].Items.Count > MAX_LICZBA_DYZUROW)
                {
                    MessageBox.Show("Aby móc wykorzystać automatyczne rozdzielanie funkcji liczba dyżurów danego dnia nie może przekraczać " + MAX_LICZBA_DYZUROW.ToString() + ".");
                    return;
                }
            }

            dyzuryGrafik = UtworzGrafik();
            nieTriazDzien = ListaNieTiazDzien();
            nieTriazNoc = ListaNieTiazNoc();
            liczbaDyzurow = LiczbaDyzurow(dyzuryGrafik);
            oczekiwanaLiczbaFunkcji = OczekiwanaLiczbaFunkcji(osoby);
            stopienZdegenerowania = StopienZdegenerowania(liczbaDyzurow, nieTriazDzien, nieTriazNoc, dyzuryGrafik);

            if (liczbaDyzurow.Contains(1) || liczbaDyzurow.Contains(2))
            {
                MessageBox.Show("Niepoprawny grafik. Sprawdź, czy do każdego dnia przypisane są co najmniej 3 osoby.");
                return;
            }

            optymalneRozwiazanie = OptymalizacjaGA(4*LICZBA_DNI*MAX_LICZBA_DYZUROW, handler, 100, 0.000004m, 0.00000000001m, 20000, 1000000, dyzuryGrafik, nieTriazDzien, nieTriazNoc, liczbaDyzurow, oczekiwanaLiczbaFunkcji, stopienZdegenerowania);
            DodajFunkcje(optymalneRozwiazanie);
            zapiszGrafik("GrafikGA.txt");
            labelRaport.Text = labelRaport.Text + " Ukończono.";
        }
        #endregion

        #region Funkcje - inne

        private void zapiszGrafik(string plik)
        {
            File.WriteAllText(plik, "");
            foreach (myListBox iter in listBoxesDzien)
            {
                string str = "";
                for (int i = 0; i < iter.Items.Count; i++)
                {
                    str = str + iter.Items[i].ToString() + " ";
                }
                str = str + "\n";
                File.AppendAllText(plik, str);
            }

            foreach (myListBox iter in listBoxesNoc)
            {
                string str = "";
                for (int i = 0; i < iter.Items.Count; i++)
                {
                    str = str + iter.Items[i].ToString() + " ";
                }
                str = str + "\n";
                File.AppendAllText(plik, str);
            }
        }

        private void wczytajGrafik(string plik)
        {
            for (int i = 0; i < LICZBA_DNI; i++)
            {
                listBoxesDzien[i].ResetBackColor();
                listBoxesNoc[i].ResetBackColor();
            }

            foreach (Osoba iter in osoby)
            {

                if (iter != null)
                {
                    iter.wymiarEtatu = 0.0;
                    labels[iter.numer - 1].Text = iter.numer.ToString() + ". " + iter.imie + " " + iter.nazwisko + " " + iter.wymiarEtatu.ToString() + " " + iter.zaleglosci.ToString();
                }
            }

            string readText;
            string[] subs;
            string str;
            for (int i = 0; i < LICZBA_DNI; i++)
            {
                listBoxesDzien[i].Items.Clear();
                listBoxesNoc[i].Items.Clear();

                readText = File.ReadAllLines(plik).Skip(i).Take(1).First();
                subs = readText.Split(' ');

                for (int j = 0; j < subs.Length - 1; j++)
                {
                    listBoxesDzien[i].Items.Add(subs[j]);
                    str = subs[j];

                    if (str[str.Length - 1] == 's' || str[str.Length - 1] == 't')
                        str = str.Remove(str.Length - 1);

                    foreach (Osoba iter in osoby)
                    {

                        if (iter != null)
                        {
                            if (iter.numer == Convert.ToInt32(str))
                            {
                                iter.wymiarEtatu = iter.wymiarEtatu + 1.0;
                                labels[iter.numer - 1].Text = iter.numer.ToString() + ". " + iter.imie + " " + iter.nazwisko + " " + iter.wymiarEtatu.ToString() + " " + iter.zaleglosci.ToString();
                            }
                        }
                    }

                }

                readText = File.ReadAllLines(plik).Skip(i + LICZBA_DNI).Take(1).First();
                subs = readText.Split(' ');
                for (int j = 0; j < subs.Length - 1; j++)
                {
                    listBoxesNoc[i].Items.Add(subs[j]);
                    str = subs[j];

                    if (str[str.Length - 1] == 's' || str[str.Length - 1] == 't')
                        str = str.Remove(str.Length - 1);

                    foreach (Osoba iter in osoby)
                    {

                        if (iter != null)
                        {
                            if (iter.numer == Convert.ToInt32(str))
                            {
                                iter.wymiarEtatu = iter.wymiarEtatu + 1.0;
                                labels[iter.numer - 1].Text = iter.numer.ToString() + ". " + iter.imie + " " + iter.nazwisko + " " + iter.wymiarEtatu.ToString() + " " + iter.zaleglosci.ToString();
                            }
                        }
                    }
                }
            }
        }

        public static decimal FunkcjaCelu(bool[] funkcje, int[] grafik, int[] nieTriazDzien, int[] nieTriazNoc, int[] liczbaDyzurow, double[] oczekiwanaLiczbaFunkcji)
        {
            decimal W = 0.0m;
            decimal a = 0.0m;
            int[] liczbaSalData = new int[2 * LICZBA_DNI];
            int[] liczbaTriazyData = new int[2 * LICZBA_DNI];
            int[] liczbaFunkcyjnychStazystow = new int[2 * LICZBA_DNI];
            int[] liczbaStazystowNaTriazu = new int[2 * LICZBA_DNI];
            int[] liczbaFunkcjiDzien = new int[MAX_LICZBA_OSOB];
            int[] liczbaFunkcjiNoc = new int[MAX_LICZBA_OSOB];
            int[] liczbaSalOsobaDzien = new int[MAX_LICZBA_OSOB];
            int[] liczbaSalOsobaNoc = new int[MAX_LICZBA_OSOB];
            int[] liczbaTriazyOsobaDzien = new int[MAX_LICZBA_OSOB];
            int[] liczbaTriazyOsobaNoc = new int[MAX_LICZBA_OSOB];
            int siz = funkcje.Length;
            int nrDnia;
            bool flagStazDzien = false;
            bool flagStazNoc = false;

            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                liczbaTriazyData[i] = 0;
                liczbaSalData[i] = 0;
                liczbaStazystowNaTriazu[i] = 0;
            }

            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                liczbaFunkcjiDzien[i] = 0;
                liczbaFunkcjiNoc[i] = 0;
                liczbaSalOsobaDzien[i] = 0;
                liczbaSalOsobaNoc[i] = 0;
                liczbaTriazyOsobaDzien[i] = 0;
                liczbaTriazyOsobaNoc[i] = 0;
            }

            for (int i = 0; i < siz / 2; i++)
            {
                nrDnia = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / MAX_LICZBA_DYZUROW));
                if (funkcje[i] && !funkcje[i + siz / 2] && grafik[i] != 0)
                {
                    liczbaTriazyData[nrDnia]++;
                    if (i < siz / 4)
                    {
                        liczbaFunkcjiDzien[grafik[i] - 1]++;
                        liczbaTriazyOsobaDzien[grafik[i] - 1]++;
                    }

                    else
                    {
                        liczbaFunkcjiNoc[grafik[i] - 1]++;
                        liczbaTriazyOsobaNoc[grafik[i] - 1]++;
                    }

                    for (int j = 0; j < nieTriazNoc.Length; j++)
                    {
                        if (grafik[i] == nieTriazNoc[j])
                        {
                            liczbaStazystowNaTriazu[nrDnia]++;
                            break;
                        }
                    }
                }

                if (!funkcje[i] && funkcje[i + siz / 2] && grafik[i] != 0)
                {
                    liczbaSalData[nrDnia]++;
                    if (i < siz / 4)
                    {
                        liczbaFunkcjiDzien[grafik[i] - 1]++;
                        liczbaSalOsobaDzien[grafik[i] - 1]++;
                    }

                    else
                    {
                        liczbaFunkcjiNoc[grafik[i] - 1]++;
                        liczbaSalOsobaNoc[grafik[i] - 1]++;
                    }
                }

                if ((funkcje[i] || funkcje[i + siz / 2]) && grafik[i] == 0)
                    a = a + 1000000.0m;

                if ((funkcje[i] && funkcje[i + siz / 2]))
                    a = a + 1000000.0m;

                for (int j = 0; j < nieTriazDzien.Length; j++)
                {
                    if (funkcje[i] && !funkcje[i + siz / 2] && (i < siz / 4) && grafik[i] == nieTriazDzien[j])
                    {
                        a = a + 100.0m;
                        break;
                    }
                }

                for (int j = 0; j < nieTriazNoc.Length; j++)
                {
                    if (funkcje[i] && !funkcje[i + siz / 2] && (i >= siz / 4) && grafik[i] == nieTriazNoc[j])
                    {
                        a = a + 100.0m;
                        break;
                    }
                }

            }

            for (int i = 0; i < 2 * LICZBA_DNI; i++)
            {
                if (liczbaDyzurow[i] >= 3)
                {
                    a = a + 10000.0m * Math.Abs(liczbaTriazyData[i] - 2);
                    a = a + 10000.0m * Math.Abs(liczbaSalData[i] - 1);
                }

                if (liczbaStazystowNaTriazu[i] >= 2)
                    a = a + 10000.0m;
            }

            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                if (Math.Abs(liczbaFunkcjiDzien[i] + liczbaFunkcjiNoc[i] - oczekiwanaLiczbaFunkcji[i]) >= 1.99)
                    W = W + 0.01m * Convert.ToDecimal(Math.Floor(Math.Abs(liczbaFunkcjiDzien[i] + liczbaFunkcjiNoc[i] - oczekiwanaLiczbaFunkcji[i])));

                else if (Math.Abs(liczbaFunkcjiDzien[i] + liczbaFunkcjiNoc[i] - oczekiwanaLiczbaFunkcji[i]) >= 0.99)
                    W = W + 0.000001m * Convert.ToDecimal(Math.Floor(Math.Abs(liczbaFunkcjiDzien[i] + liczbaFunkcjiNoc[i] - oczekiwanaLiczbaFunkcji[i])));

                flagStazDzien = false;
                for (int j = 0; j < nieTriazDzien.Length; j++)
                {
                    if (i == nieTriazDzien[j] - 1)
                    {
                        flagStazDzien = true;
                        break;
                    }
                }

                flagStazNoc = false;
                for (int j = 0; j < nieTriazNoc.Length; j++)
                {
                    if (i == nieTriazNoc[j] - 1)
                    {
                        flagStazNoc = true;
                        break;
                    }
                }

                if (Math.Abs(2 * (liczbaSalOsobaDzien[i] + liczbaSalOsobaNoc[i]) - (liczbaTriazyOsobaDzien[i] + liczbaTriazyOsobaNoc[i])) > 2 && !flagStazDzien && !flagStazNoc)
                    W = W + 0.00000001m * Convert.ToDecimal(Math.Abs(2 * (liczbaSalOsobaDzien[i] + liczbaSalOsobaNoc[i]) - (liczbaTriazyOsobaDzien[i] + liczbaTriazyOsobaNoc[i])));

                else if (Math.Abs(2 * (liczbaSalOsobaDzien[i] + liczbaSalOsobaNoc[i]) - (liczbaTriazyOsobaDzien[i] + liczbaTriazyOsobaNoc[i])) == 2 && !flagStazDzien && !flagStazNoc)
                    W = W + 0.0000000002m;

                if (Math.Abs(liczbaSalOsobaDzien[i] - liczbaTriazyOsobaDzien[i]) > 2 && flagStazNoc)
                        W = W + 0.00000001m * Convert.ToDecimal(Math.Abs(liczbaSalOsobaDzien[i] - liczbaTriazyOsobaDzien[i]));

                else if (Math.Abs(liczbaSalOsobaDzien[i] - liczbaTriazyOsobaDzien[i]) == 2 && flagStazNoc)
                    W = W + 0.0000000002m;

                if (Math.Abs(liczbaFunkcjiDzien[i] - liczbaFunkcjiNoc[i]) > 2)
                    W = W + 1.0m * Convert.ToDecimal(Math.Abs(liczbaFunkcjiDzien[i] - liczbaFunkcjiNoc[i]));

                else if (Math.Abs(liczbaFunkcjiDzien[i] - liczbaFunkcjiNoc[i]) == 2)
                    W = W + 0.0002m;
            }

            W = W + a;
            return W;
        }

        public bool[] OptymalizacjaGA(int siz, Callback fCelu, int liczbaOsobnikow, decimal tol, decimal tolX, int maxKonsekwentnychIteracji, int maxIteracji, int[] grafik, int[] nieTriazDzien, int[] nieTriazNoc, int[] liczbaDyzurow, double[] oczekiwanaLiczbaFunkcji, decimal stopienZdegenerowania)
        {
            if (liczbaOsobnikow < 10)
                liczbaOsobnikow = 10;

            Random rnd = new Random();
            OsobnikComparer OsComp = new OsobnikComparer();
            Osobnik[] osobniki = new Osobnik[liczbaOsobnikow];
            Osobnik[] osobnikiTemp = new Osobnik[liczbaOsobnikow];

            double SZANSA_MUTACJA = 0.006;
            int MAX_LICZBA_PROB = 3;
            double FRACTION_OF_ELITES = 0.01;
            double FRACTION_OF_REPRODUCING = 0.25;

            int nrKonsekwentnejIteracji = 1;
            int nrIteracji = 1;
            int liczbaWywolanFunkcjiCelu = 0;
            int liczbaProbKrzyzowania = 0;
            double temp, temp2;
            int temp3 = 0;
            int[] temp4 = new int[MAX_LICZBA_PROB];
            decimal prevCel = 0;
            decimal cel = 0;
            double[] attraction = new double[MAX_LICZBA_PROB];
            double liczbaReprodukujacych = Math.Floor(FRACTION_OF_REPRODUCING * Convert.ToDouble(liczbaOsobnikow));
            int NumberOfElites = Convert.ToInt32(Math.Max(Convert.ToInt32(Math.Floor(FRACTION_OF_ELITES * Convert.ToDouble(liczbaOsobnikow))), 1));
            double sumaSzans = 0.0;
            double[] szansa = new double[Convert.ToInt32(liczbaReprodukujacych)];
            bool[] gen = new bool[siz];

            for (int i = 0; i < liczbaReprodukujacych; i++)
                sumaSzans = sumaSzans + (i + 1);

            szansa[0] = liczbaReprodukujacych / sumaSzans;
            for (int i = 1; i < liczbaReprodukujacych; i++)
                szansa[i] = szansa[i - 1] + (liczbaReprodukujacych - i) / sumaSzans;

            for (int j = 0; j < liczbaOsobnikow; j++)
            {
                bool[] bools = new bool[siz];
                for (int i = 0; i < siz; i++)
                {
                    temp = rnd.NextDouble();
                    if (temp < 0.5)
                        bools[i] = false;

                    else
                        bools[i] = true;
                }
                Osobnik newOsobnik = new Osobnik(bools, 0.0m);
                osobniki[j] = newOsobnik;
                osobnikiTemp[j] = newOsobnik;
            }

            for (int i = 0; i < liczbaOsobnikow; i++)
            {
                osobniki[i].wartosc = fCelu(osobniki[i].genotyp, grafik, nieTriazDzien, nieTriazNoc, liczbaDyzurow, oczekiwanaLiczbaFunkcji);
                liczbaWywolanFunkcjiCelu++;
            }

            Array.Sort(osobniki, OsComp);
            cel = osobniki[0].wartosc;

            while ((nrIteracji <= maxIteracji && nrKonsekwentnejIteracji <= maxKonsekwentnychIteracji) || (cel > stopienZdegenerowania + tol))
            {
                for (int i = 0; i < NumberOfElites; i++)
                    osobnikiTemp[i] = osobniki[i];

                for (int i = NumberOfElites; i < liczbaOsobnikow; i++)
                {
                    temp = rnd.NextDouble();
                    for (int j = Convert.ToInt32(liczbaReprodukujacych) - 1; j > 0; j--)
                    {
                        if (szansa[j] <= temp)
                        {
                            temp3 = j;
                            break;
                        }
                    }

                    liczbaProbKrzyzowania = 0;
                    for (int iter1 = 0; iter1 < MAX_LICZBA_PROB; iter1++)
                    {
                        attraction[iter1] = 0.0;
                        temp4[iter1] = 0;
                    }

                    while (liczbaProbKrzyzowania < MAX_LICZBA_PROB)
                    {
                        temp = rnd.NextDouble();
                        for (int j = Convert.ToInt32(liczbaReprodukujacych) - 1; j > 0; j--)
                        {
                            if (szansa[j] <= temp)
                            {
                                temp4[liczbaProbKrzyzowania] = j;
                                break;
                            }
                        }

                        attraction[liczbaProbKrzyzowania] = 0.0;
                        for (int k = 0; k < siz; k++)
                        {
                            if (osobnikiTemp[temp3].genotyp[k] != osobnikiTemp[temp4[liczbaProbKrzyzowania]].genotyp[k])
                                attraction[liczbaProbKrzyzowania] = attraction[liczbaProbKrzyzowania] + 1.0;
                        }

                        attraction[liczbaProbKrzyzowania] = attraction[liczbaProbKrzyzowania] / Convert.ToDouble(siz);
                        temp = rnd.NextDouble();
                        if (temp <= attraction[liczbaProbKrzyzowania])
                            break;

                        else
                            liczbaProbKrzyzowania++;
                    }

                    if (liczbaProbKrzyzowania < MAX_LICZBA_PROB)
                    {
                        temp = rnd.NextDouble();
                        for (int k = 0; k < siz / 2; k++)
                        {
                            if (temp < 0.5)
                            {
                                osobnikiTemp[i].genotyp[k] = osobniki[temp3].genotyp[k];
                                osobnikiTemp[i].genotyp[k + siz / 2] = osobniki[temp3].genotyp[k + siz / 2];
                            }

                            else
                            {
                                osobnikiTemp[i].genotyp[k] = osobniki[temp4[liczbaProbKrzyzowania]].genotyp[k];
                                osobnikiTemp[i].genotyp[k + siz / 2] = osobniki[temp4[liczbaProbKrzyzowania]].genotyp[k + siz / 2];
                            }
                        }
                    }

                    else
                    {
                        temp = rnd.NextDouble();
                        for (int k = 0; k < siz / 2; k++)
                        {
                            if (temp < 0.5)
                            {
                                osobnikiTemp[i].genotyp[k] = osobniki[temp3].genotyp[k];
                                osobnikiTemp[i].genotyp[k + siz / 2] = osobniki[temp3].genotyp[k + siz / 2];
                            }

                            else
                            {
                                osobnikiTemp[i].genotyp[k] = osobniki[Array.IndexOf(attraction, attraction.Max())].genotyp[k];
                                osobnikiTemp[i].genotyp[k + siz / 2] = osobniki[Array.IndexOf(attraction, attraction.Max())].genotyp[k + siz / 2];
                            }
                        }
                    }
                }

                for (int i = NumberOfElites; i < liczbaOsobnikow; i++)
                {
                    for (int j = 0; j < siz / 2; j++)
                    {
                        temp = rnd.NextDouble();
                        temp2 = rnd.NextDouble();
                        if (temp <= SZANSA_MUTACJA && temp2 < 0.3333)
                        {
                            osobnikiTemp[i].genotyp[j] = false;
                            osobnikiTemp[i].genotyp[j + siz / 2] = false;
                        }

                        else if (temp <= SZANSA_MUTACJA && temp2 >= 0.3333 && temp2 < 0.6666)
                        {
                            osobnikiTemp[i].genotyp[j] = true;
                            osobnikiTemp[i].genotyp[j + siz / 2] = false;
                        }

                        else if (temp <= SZANSA_MUTACJA && temp2 >= 0.6666)
                        {
                            osobnikiTemp[i].genotyp[j] = false;
                            osobnikiTemp[i].genotyp[j + siz / 2] = true;
                        }
                    }
                }

                osobniki = osobnikiTemp;

                if (nrIteracji % 1000 == 0)
                {
                    for (int j = NumberOfElites; j < liczbaOsobnikow; j++)
                    {
                        for (int i = 0; i < siz; i++)
                        {
                            temp = rnd.NextDouble();
                            if (temp < 0.5)
                                osobniki[j].genotyp[i] = false;

                            else
                                osobniki[j].genotyp[i] = true;
                        }
                    }
                }

                

                for (int i = NumberOfElites; i < liczbaOsobnikow; i++)
                {
                    osobniki[i].wartosc = fCelu(osobniki[i].genotyp, grafik, nieTriazDzien, nieTriazNoc, liczbaDyzurow, oczekiwanaLiczbaFunkcji);
                    liczbaWywolanFunkcjiCelu++;
                }

                nrIteracji++;
                prevCel = cel;
                Array.Sort(osobniki, OsComp);
                cel = osobniki[0].wartosc;

                if (Math.Abs(prevCel - cel) < tolX)
                    nrKonsekwentnejIteracji++;

                else
                    nrKonsekwentnejIteracji = 1;

                if (nrIteracji % 100 == 0 || nrKonsekwentnejIteracji == maxKonsekwentnychIteracji)
                {
                    labelRaport.Text = "Wartość f. celu: " + cel.ToString() + " Nr. Iteracji: " + nrIteracji.ToString() + " Liczba konsekwentnych iteracji: " + nrKonsekwentnejIteracji.ToString() + " Liczba wywołań: " + liczbaWywolanFunkcjiCelu.ToString() + " Cel: " + (stopienZdegenerowania + tol).ToString() + ".";
                    labelRaport.Refresh();
                }
            }
            return osobniki[0].genotyp;
        }

        public int[] UtworzGrafik()
        {
            int nrListBoxa;
            int[] dyzuryGrafik = new int[2 * LICZBA_DNI * MAX_LICZBA_DYZUROW];
            for (int i = 0; i < LICZBA_DNI * MAX_LICZBA_DYZUROW; i++)
            {
                nrListBoxa = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / MAX_LICZBA_DYZUROW));
                if (listBoxesDzien[nrListBoxa].Items.Count > i % MAX_LICZBA_DYZUROW)
                {
                    listBoxesDzien[nrListBoxa].ToBezFunkcji(i % MAX_LICZBA_DYZUROW);
                    dyzuryGrafik[i] = Convert.ToInt32(listBoxesDzien[nrListBoxa].GetNumber(i % MAX_LICZBA_DYZUROW));
                }

                else
                    dyzuryGrafik[i] = 0;

                nrListBoxa = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / MAX_LICZBA_DYZUROW));
                if (listBoxesNoc[nrListBoxa].Items.Count > i % MAX_LICZBA_DYZUROW)
                {
                    listBoxesNoc[nrListBoxa].ToBezFunkcji(i % MAX_LICZBA_DYZUROW);
                    dyzuryGrafik[i + LICZBA_DNI * MAX_LICZBA_DYZUROW] = Convert.ToInt32(listBoxesNoc[nrListBoxa].GetNumber(i % MAX_LICZBA_DYZUROW));
                }

                else
                    dyzuryGrafik[i + LICZBA_DNI * MAX_LICZBA_DYZUROW] = 0;
            }

            return dyzuryGrafik;
        }

        public int[] ListaNieTiazDzien()
        {
            int nieStazDzienNumber = 0;
            foreach (Osoba iter in osoby)
            {
                if (iter != null)
                {
                    if (!iter.czyTriazDzien)
                        nieStazDzienNumber++;
                }
            }

            int[] nieStazDzien = new int[nieStazDzienNumber];
            int iter1 = 0;
            foreach (Osoba iter in osoby)
            {
                if (iter != null)
                {
                    if (!iter.czyTriazDzien)
                    {
                        nieStazDzien[iter1] = iter.numer;
                        iter1++;
                    }
                }
            }

            return nieStazDzien;
        }

        public int[] ListaNieTiazNoc()
        {
            int nieStazNocNumber = 0;
            foreach (Osoba iter in osoby)
            {
                if (iter != null)
                {
                    if (!iter.czyTriazNoc)
                        nieStazNocNumber++;
                }
            }

            int[] nieStazNoc = new int[nieStazNocNumber];
            int iter1 = 0;
            foreach (Osoba iter in osoby)
            {
                if (iter != null)
                {
                    if (!iter.czyTriazNoc)
                    {
                        nieStazNoc[iter1] = iter.numer;
                        iter1++;
                    }
                }
            }

            return nieStazNoc;
        }

        public int[] LiczbaDyzurow(int[] dyzuryGrafik)
        {
            int[] liczbaDyzurow = new int[2 * LICZBA_DNI];
            for (int i = 0; i < LICZBA_DNI; i++)
            {
                liczbaDyzurow[i] = 0;
                for (int j = 0; j < MAX_LICZBA_DYZUROW; j++)
                {
                    if (dyzuryGrafik[i * MAX_LICZBA_DYZUROW + j] != 0)
                        liczbaDyzurow[i]++;

                    if (dyzuryGrafik[i * MAX_LICZBA_DYZUROW + j + LICZBA_DNI * MAX_LICZBA_DYZUROW] != 0)
                        liczbaDyzurow[i + LICZBA_DNI]++;
                }
            }

            return liczbaDyzurow;
        }

        public void DodajFunkcje(bool[] optymalneRozwiazanie)
        {
            for (int i = 0; i < MAX_LICZBA_DYZUROW * LICZBA_DNI; i++)
            {
                int nrListBoxa = Convert.ToInt32(Math.Floor(Convert.ToDouble(i) / MAX_LICZBA_DYZUROW));
                if (listBoxesDzien[nrListBoxa].Items.Count > i % MAX_LICZBA_DYZUROW)
                {
                    listBoxesDzien[nrListBoxa].ToBezFunkcji(i % MAX_LICZBA_DYZUROW);

                    if (optymalneRozwiazanie[i] && !optymalneRozwiazanie[i + 2 * MAX_LICZBA_DYZUROW * LICZBA_DNI])
                        listBoxesDzien[nrListBoxa].ToTriaz(i % MAX_LICZBA_DYZUROW);

                    else if (!optymalneRozwiazanie[i] && optymalneRozwiazanie[i + 2 * MAX_LICZBA_DYZUROW * LICZBA_DNI])
                        listBoxesDzien[nrListBoxa].ToSala(i % MAX_LICZBA_DYZUROW);
                }

                if (listBoxesNoc[nrListBoxa].Items.Count > i % MAX_LICZBA_DYZUROW)
                {
                    listBoxesNoc[nrListBoxa].ToBezFunkcji(i % MAX_LICZBA_DYZUROW);

                    if (optymalneRozwiazanie[i + MAX_LICZBA_DYZUROW * LICZBA_DNI] && !optymalneRozwiazanie[i + 3 * MAX_LICZBA_DYZUROW * LICZBA_DNI])
                        listBoxesNoc[nrListBoxa].ToTriaz(i % MAX_LICZBA_DYZUROW);

                    else if (!optymalneRozwiazanie[i + MAX_LICZBA_DYZUROW * LICZBA_DNI] && optymalneRozwiazanie[i + 3 * MAX_LICZBA_DYZUROW * LICZBA_DNI])
                        listBoxesNoc[nrListBoxa].ToSala(i % MAX_LICZBA_DYZUROW);
                }
            }
        }

        public double[] OczekiwanaLiczbaFunkcji(Osoba[] osoby)
        {
            const double MAX_LICZBA_FUNKCJI = 8.0;
            const double MIN_LICZBA_FUNKCJI = 2.0;
            double[] oczekiwanaLiczbaFunkcji = new double[MAX_LICZBA_OSOB];
            int liczbaDniRoboczych = 0;
            double sumaEtatow = 0.0;

            for (int i = 0; i < LICZBA_DNI; i++)
            {
                if (listBoxesDzien[i].Items.Count > 0)
                    liczbaDniRoboczych++;

                if (listBoxesNoc[i].Items.Count > 0)
                    liczbaDniRoboczych++;
            }

            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                if (osoby[i] != null)
                    sumaEtatow = sumaEtatow + Convert.ToInt32(osoby[i].wymiarEtatu);
            }

            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                if (osoby[i] != null)
                {
                    oczekiwanaLiczbaFunkcji[i] = Math.Min(Math.Max(((3 * liczbaDniRoboczych * osoby[i].wymiarEtatu / sumaEtatow) - 0.8*osoby[i].zaleglosci), 0), MAX_LICZBA_FUNKCJI);
                    if (osoby[i].wymiarEtatu > 0.1)
                    {
                        oczekiwanaLiczbaFunkcji[i] = Math.Max(oczekiwanaLiczbaFunkcji[i], MIN_LICZBA_FUNKCJI);
                    }
                }
            }
            return oczekiwanaLiczbaFunkcji;
        }

        public decimal StopienZdegenerowania(int[] liczbaDyzurow, int[] nieTriazDzien, int[] nieTriazNoc, int[] dyzuryGrafik)
        {
            decimal stopienZdegenerowania = 0.0m;
            int liczbaStazystowDzien;
            int liczbaStazystowNoc;
            for (int i = 0; i < 2*LICZBA_DNI; i++)
            {
                liczbaStazystowDzien = 0;
                liczbaStazystowNoc = 0;
                for (int j = 0; j < MAX_LICZBA_DYZUROW; j++)
                {
                    if (nieTriazDzien.Contains(dyzuryGrafik[j + i * MAX_LICZBA_DYZUROW]))
                        liczbaStazystowDzien++;

                    if (nieTriazNoc.Contains(dyzuryGrafik[j + i * MAX_LICZBA_DYZUROW]))
                        liczbaStazystowNoc++;
                }

                if (liczbaDyzurow[i] == liczbaStazystowNoc)
                    stopienZdegenerowania = stopienZdegenerowania + 10000.0m;

                if ((liczbaDyzurow[i] == liczbaStazystowDzien) && i < LICZBA_DNI)
                    stopienZdegenerowania = stopienZdegenerowania + 200.0m;

                else if ((liczbaDyzurow[i] - liczbaStazystowDzien == 1) && i < LICZBA_DNI)
                    stopienZdegenerowania = stopienZdegenerowania + 100.0m;

                if ((liczbaDyzurow[i] == liczbaStazystowNoc) && i >= LICZBA_DNI)
                    stopienZdegenerowania = stopienZdegenerowania + 200.0m;

                else if ((liczbaDyzurow[i] - liczbaStazystowNoc == 1) && i >= LICZBA_DNI)
                    stopienZdegenerowania = stopienZdegenerowania + 100.0m;
            }
            return stopienZdegenerowania;
        }
        #endregion
    }
}
