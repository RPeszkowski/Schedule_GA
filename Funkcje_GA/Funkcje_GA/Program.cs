using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static Funkcje_GA.Form1;

namespace Funkcje_GA
{

    internal static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        
        [STAThread]


        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
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

    public class FileOperations
    {
        public void zapiszGrafik(string plik)
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

        public void wczytajGrafik(string plik)
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

        public void WczytajPracownikow(string plik)
        {
            for (int i = 0; i < MAX_LICZBA_OSOB; i++)
            {
                string readText;
                string[] subs;
                try
                {
                    readText = File.ReadAllLines(plik).Skip(i).Take(1).First();
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
        }

        public void ZapiszPracownikow(string plik)
        {
            File.WriteAllText(plik, "");
            for (int i = 0; i < Form1.MAX_LICZBA_OSOB; i++)
            {
                if (Form1.osoby[i] != null)
                {
                    string danePracownika = Form1.osoby[i].numer.ToString() + " " + Form1.osoby[i].imie + " " + Form1.osoby[i].nazwisko + " " + Form1.osoby[i].zaleglosci.ToString() + " " + Form1.osoby[i].czyTriazDzien.ToString() + " " + Form1.osoby[i].czyTriazNoc.ToString() + "\n";
                    File.AppendAllText(plik, danePracownika);
                }
            }
        }
    }

    }
