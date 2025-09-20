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
        public int Id { get; }                 //Numer listBoxa.

        //Konstruktor.
        public ListBoxGrafik(int id)
        {
            this.Id = id;
        }

        //Zwraca numer osoby wskazanej przez index.
        public int GetNumber(int index)
        {
            //Sprawdzamy poprawność danych.
            CheckData(index);

            string str = Items[index].ToString();          //Wartość pobrana z listBoxa.

            //Jeśli jest literka s lub t to ją usuwamy. Konwertujemy do int.
            if (str[str.Length - 1] == 's')
                str = str.Remove(str.Length - 1);

            if (str[str.Length - 1] == 't')
                str = str.Remove(str.Length - 1);

            //Zwracamy numer osoby.
            return Convert.ToInt32(str);
        }

        //Zmienia funkcję wybranej osoby na salę.
        public void ToSala(int index)
        {
            //Jeśli nie wybrano elementu to kończymy.
            if (index == -1)
                return;

            //Sprawdzamy poprawność danych.
            CheckData(index);

            string str = Items[index].ToString();              //Wartość pobrana z listBoxa.

            //Jeśli jest literka s to nic nie robimy.
            if (str[str.Length - 1] == 's')
                Items[index] = str;

            //Jeśli jest literka t to podmieniamy na s.
            else if (str[str.Length - 1] == 't')
            {
                str = str.Remove(str.Length - 1);
                Items[index] = str + 's';
            }

            //Jeśli nie ma literki to dopisujemy s.
            else Items[index] = str + 's';
        }

        //Zmienia funkcję wybranej osoby na triaż.
        public void ToTriaz(int index)
        {
            //Jeśli nie wybrano elementu to kończymy.
            if (index == -1)
                return;

            //Sprawdzamy poprawność danych.
            CheckData(index);
 
            string str = Items[index].ToString();                  //Wartość pobrana z listBoxa.

            //Jeśli jest literka t to nic nie robimy.
            if (str[str.Length - 1] == 't')
                Items[index] = str;

            //Jeśli jest literka s to podmieniamy na t.
            else if (str[str.Length - 1] == 's')
            {
                str = str.Remove(str.Length - 1);
                Items[index] = str + 't';
            }

            //Jeśli nie ma literki to dopisujemy t.
            else Items[index] = str + 't';
        }

        //Sprawdzamy poprawność danych.
        private void CheckData(int index)
        {
            //Sprawdzamy, czy indeks jest we właściym zakresie.
            if(index < -1 || index >= Items.Count)
                throw new InvalidDataException($"Nie ma elementu o indeksie {index}.");

            //Pobieramy item jako string.
            string str = Items[index].ToString();

            //Sprawdzamy, czy item nie jest pusty.
            if (string.IsNullOrWhiteSpace(str))
                throw new InvalidDataException($"Element o indeksie {index} jest pusty.");

            //Jeżeli dane kończą się sufiksem, to do usuwamy.
            if (str[str.Length - 1] == 's' || str[str.Length - 1] == 't')
                str = str.Remove(str.Length - 1);

            //Sprawdzamy, czy item jest liczbą.
            if (!Int32.TryParse(str, out int number))
                throw new InvalidDataException($"Element o indeksie {index} nie jest liczbą całkowitą.");
        }
    }
}
