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
