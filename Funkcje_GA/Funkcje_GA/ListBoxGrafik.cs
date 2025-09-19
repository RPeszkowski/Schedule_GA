using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Funkcje_GA
{
    //Ta klasa zawiera ListBoxy przedstawiające grafik.
    public class ListBoxGrafik : ListBox
    {
        public int Id { get; set; }                 //Numer listBoxa.

        //Konstruktor.
        public ListBoxGrafik(int id)
        {
            this.Id = id;
        }

        //Zwraca funkcję danej osoby. 0 - bez funkcji, 1 - sala, 2 - triaż.
        public int GetFunction(int index)
        {
            //Sprawdzamy poprawność danych.
            try
            {
                CheckData(index);
            }

            //Jeśli dane są błędne to wyświetlamy komunikat.
            catch
            {
                MessageBox.Show("Zły format danych grafiku.");
            }

            int nrFunkcji;                                      //Numer funkcji. 0 - bez funkcji, 1 - sala, 2 - triaż.
            string str = this.Items[index].ToString();          //Numer osoby, ewentualnie z dodatkową literą.

            //Jeśli funkcja to sala zwracamy 1.
            if (str[str.Length - 1] == 's')
                nrFunkcji = (int)FunctionTypes.Sala;

            //Jeśli funkcja to triaż zwracamy 2.
            else if (str[str.Length - 1] == 't')
                nrFunkcji = (int)FunctionTypes.Triaz;

            //Jeśli nie ma funkcj izwracamy 0.
            else
                nrFunkcji = (int)FunctionTypes.Bez_Funkcji;

            //Zwracamy wartość.
            return nrFunkcji;
        }

        //Zwraca numer osoby wskazanej przez index.
        public int GetNumber(int index)
        {
            //Sprawdzamy poprawność danych.
            try
            {
                CheckData(index);
            }

            //Jeśli dane są błędne to wyświetlamy komunikat.
            catch
            {
                MessageBox.Show("Zły format danych grafiku.");
            }

            int number;                                         //Numer pracownika.
            string str = this.Items[index].ToString();          //Wartość pobrana z listBoxa przerobiona na string.

            //Jeśli jest literka s lub t to ją usuwamy. Konwertujemy do int.
            if (str[str.Length - 1] == 's')
                str = str.Remove(str.Length - 1);

            if (str[str.Length - 1] == 't')
                str = str.Remove(str.Length - 1);

            number = Convert.ToInt32(str);

            //Zwracamy numer osoby.
            return number;
        }

        //Zmienia funkcję wybranej osoby na bez funkcji.
        public void ToBezFunkcji(int index)
        {
            //Sprawdzamy poprawność danych.
            try
            {
                CheckData(index);
            }

            //Jeśli dane są błędne to wyświetlamy komunikat.
            catch
            {
                MessageBox.Show("Zły format danych grafiku.");
            }

            string str = this.Items[index].ToString();                          //Wartość pobrana z listBoxa przerobiona na string.

            //Jeśli jest literka s lub t to ją usuwamy i podmieniamy item w listBoxie.
            if (str[str.Length - 1] == 's' || str[str.Length - 1] == 't')
                str = str.Remove(str.Length - 1);

            this.Items[index] = str;
        }

        //Zmienia funkcję wybranej osoby na salę.
        public void ToSala(int index)
        {
            //Sprawdzamy poprawność danych.
            try
            {
                CheckData(index);
            }

            //Jeśli dane są błędne to wyświetlamy komunikat.
            catch
            {
                MessageBox.Show("Zły format danych grafiku.");
            }

            string str = this.Items[index].ToString();              //Wartość pobrana z listBoxa przerobiona na string.

            //Jeśli jest literka s to nic nie robimy.
            if (str[str.Length - 1] == 's')
                this.Items[index] = str;

            //Jeśli jest literka t to podmieniamy na s.
            else if (str[str.Length - 1] == 't')
            {
                str = str.Remove(str.Length - 1);
                this.Items[index] = str + 's';
            }

            //Jeśli nie ma literki to dopisujemy s.
            else this.Items[index] = str + 's';
        }

        //Zmienia funkcję wybranej osoby na triaż.
        public void ToTriaz(int index)
        {
            //Sprawdzamy poprawność danych.
            try
            {
                CheckData(index);
            }

            //Jeśli dane są błędne to wyświetlamy komunikat.
            catch
            {
                MessageBox.Show("Zły format danych grafiku.");
            }

            string str = this.Items[index].ToString();                  //Wartość pobrana z listBoxa przerobiona na string.

            //Jeśli jest literka t to nic nie robimy.
            if (str[str.Length - 1] == 't')
                this.Items[index] = str;

            //Jeśli jest literka s to podmieniamy na t.
            else if (str[str.Length - 1] == 's')
            {
                str = str.Remove(str.Length - 1);
                this.Items[index] = str + 't';
            }

            //Jeśli nie ma literki to dopisujemy t.
            else this.Items[index] = str + 't';
        }

        //Sprawdzamy poprawność danych.
        private void CheckData(int index)
        {
            //Sprawdzamy, czy wybrany item istnieje.
            if (this.Items == null)
                throw new InvalidDataException("ListBox " + this.Name + " nie zawiera elementów.");

            //Pobieramy item jako string.
            string str = this.Items[index].ToString();

            //Sprawdzamy, czy item nie jest pusty.
            if (str == "")
                throw new InvalidDataException("Element " + index.ToString() + " ListBoxa " + this.Name + " jest pusty.");

            //Sprawdzamy, czy item jest liczbą z ewentualną literą s lub t.
            if (!Int32.TryParse(str, out int number))
            {
                if (str[str.Length - 1] != 's' && str[str.Length - 1] != 't')
                    throw new InvalidDataException("Niepoprawne dane " + str + " w ListBoxie" + this.Name + " .");
            }
        }
    }
}
