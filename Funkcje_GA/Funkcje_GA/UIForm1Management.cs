using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Funkcje_GA.Constans;
using static Funkcje_GA.Form1;

namespace Funkcje_GA
{
    public class UIForm1Management : IUIManagement
    {
        private readonly Dictionary<int, System.Windows.Forms.Label> uiEmployeesControls;
        private readonly List<ListBoxGrafik> uiScheduleControls;

        public UIForm1Management()
        {
            uiEmployeesControls = new Dictionary<int, System.Windows.Forms.Label>(MAX_LICZBA_OSOB);
            uiScheduleControls = new List<ListBoxGrafik>(2 * LICZBA_DNI);

            for (int nrZmiany = 0; nrZmiany < 2 * LICZBA_DNI; nrZmiany++)
            {
                ListBoxGrafik listBoxGrafik = new ListBoxGrafik(nrZmiany);
                uiScheduleControls.Add(listBoxGrafik);
                uiScheduleControls[nrZmiany].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                uiScheduleControls[nrZmiany].Size = new System.Drawing.Size(40, 400);
                uiScheduleControls[nrZmiany].AllowDrop = true;
            }

            for (int nrOsoby = 0; nrOsoby < MAX_LICZBA_OSOB; nrOsoby++)
            {
                System.Windows.Forms.Label labelEmployee = new System.Windows.Forms.Label();
                uiEmployeesControls[nrOsoby] = labelEmployee;
                uiEmployeesControls[nrOsoby] = new System.Windows.Forms.Label();
                uiEmployeesControls[nrOsoby].Font = new System.Drawing.Font("Times New Roman", 12.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
                uiEmployeesControls[nrOsoby].Size = new System.Drawing.Size(340, 40);
                uiEmployeesControls[nrOsoby].Text = "";
            }
        }

        public void ChangeColor(int id, int color)
        {
            if (color != 0 && color != 1 && color != 2 && color != 99)
                throw new InvalidOperationException("Zmienna 'color' przyjęłą niedozwololną wartość " + color.ToString() + " .");

            switch(color)
            {
                case (int)FunctionTypes.Bez_Funkcji:
                    uiScheduleControls[id].BackColor = Color.Red;
                    break;

                case (int)FunctionTypes.Sala:
                    uiScheduleControls[id].BackColor = Color.Green;
                    break;

                case (int)FunctionTypes.Triaz:
                    uiScheduleControls[id].BackColor = Color.Blue;
                    break;

                case (int)FunctionTypes.Default:
                    uiScheduleControls[id].ResetBackColor();
                    break;
            }
        }

        public void ClearEmployeeData(int id) => uiEmployeesControls[id].Text = "";

        public void ClearIndex(int id) => uiScheduleControls[id].ClearSelected();

        public void DisplayScheduleControl(Shift shift)
        {
            uiScheduleControls[shift.Id].Items.Clear();
            if (shift.Present_employees.Count > 0)
            {
                for (int nrOsoby = 0; nrOsoby < shift.Present_employees.Count; nrOsoby++)
                    uiScheduleControls[shift.Id].Items.Add(shift.Present_employees[nrOsoby].Numer.ToString());
            }

            if (shift.Sala_employees.Count > 0)
            {     
                for (int liczbaSal = 0; liczbaSal < shift.Sala_employees.Count; liczbaSal++)
                    uiScheduleControls[shift.Id].ToSala(GetElementItemsByIdAsList(shift.Id).IndexOf(shift.Sala_employees[liczbaSal].Numer.ToString()));
            }

            if (shift.Triaz_employees.Count > 0)
            {
                for (int liczbaTriazy = 0; liczbaTriazy < shift.Triaz_employees.Count; liczbaTriazy++)
                    uiScheduleControls[shift.Id].ToTriaz(GetElementItemsByIdAsList(shift.Id).IndexOf(shift.Triaz_employees[liczbaTriazy].Numer.ToString()));
            }
        }

        public IEnumerable<(int ShiftId, int EmployeeId)> GetAllSelectedEmployeeIds()
        {
            var result = new List<(int ShiftId, int EmployeeId)>();

            for (int shiftId = 0; shiftId < 2 * LICZBA_DNI; shiftId++)
            {
                int selectedIndex = GetSelectedIndex(shiftId);
                if (selectedIndex != -1)
                {
                    int employeeId = GetSelectedEmployeeNumber(shiftId, selectedIndex);
                    result.Add((shiftId, employeeId));
                }
            }

            return result;
        }

        public List<string> GetElementItemsByIdAsList(int id) => uiScheduleControls[id].Items.Cast<string>().ToList();

        public System.Windows.Forms.Label GetEmployeeControlById(int id) => uiEmployeesControls[id];

        public string GetEmployeeData(int id) => uiEmployeesControls[id].Text;

        public ListBoxGrafik GetScheduleControlById(int id) => uiScheduleControls[id];

        public int GetSelectedEmployeeFunction(int id, int index) => uiScheduleControls[id].GetFunction(index);

        public int GetSelectedEmployeeNumber(int id, int index) => uiScheduleControls[id].GetNumber(index);

        public int GetSelectedIndex(int id) => uiScheduleControls[id].SelectedIndex;

        //Wyświetlanie informacji o pracowniku na etykiecie.
        public void UpdateEmployeeLabel(Employee employee)
        {
            if (employee != null)
            {
                //Aktualizujemy pojedynczą etykietę.
                string employeeData = employee.Numer.ToString() + ". "
                                    + employee.Imie + " " + employee.Nazwisko + " "
                                    + employee.WymiarEtatu.ToString() + " "
                                    + employee.Zaleglosci.ToString();
                uiEmployeesControls[employee.Numer - 1].Text = employeeData;

                //Jeśli osoba jest stażystą to podświetlamy.
                if (!(employee.CzyTriazDzien && employee.CzyTriazNoc))
                    uiEmployeesControls[employee.Numer - 1].ForeColor = Color.Orange;

                else
                    uiEmployeesControls[employee.Numer - 1].ForeColor = Color.Black;
            }
        }
    }
}
