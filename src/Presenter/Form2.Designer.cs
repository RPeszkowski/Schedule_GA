namespace Funkcje_GA
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxImie = new System.Windows.Forms.TextBox();
            this.labelImie = new System.Windows.Forms.Label();
            this.textBoxNazwisko = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonDodaj = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDownZaleglosci = new System.Windows.Forms.NumericUpDown();
            this.checkBoxCzyTriazDzien = new System.Windows.Forms.CheckBox();
            this.checkBoxCzyTriazNoc = new System.Windows.Forms.CheckBox();
            this.listBoxNumerOsoby = new System.Windows.Forms.ListBox();
            this.buttonEdytujPracownika = new System.Windows.Forms.Button();
            this.buttonUsun = new System.Windows.Forms.Button();
            this.buttonSaveAndQuit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZaleglosci)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxImie
            // 
            this.textBoxImie.Font = new System.Drawing.Font("Times New Roman", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxImie.Location = new System.Drawing.Point(148, 236);
            this.textBoxImie.Name = "textBoxImie";
            this.textBoxImie.Size = new System.Drawing.Size(168, 32);
            this.textBoxImie.TabIndex = 2;
            // 
            // labelImie
            // 
            this.labelImie.AutoSize = true;
            this.labelImie.Font = new System.Drawing.Font("Times New Roman", 16.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelImie.Location = new System.Drawing.Point(12, 237);
            this.labelImie.Name = "labelImie";
            this.labelImie.Size = new System.Drawing.Size(50, 25);
            this.labelImie.TabIndex = 3;
            this.labelImie.Text = "Imię";
            // 
            // textBoxNazwisko
            // 
            this.textBoxNazwisko.Font = new System.Drawing.Font("Times New Roman", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxNazwisko.Location = new System.Drawing.Point(148, 284);
            this.textBoxNazwisko.Name = "textBoxNazwisko";
            this.textBoxNazwisko.Size = new System.Drawing.Size(168, 32);
            this.textBoxNazwisko.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 16.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(12, 284);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 25);
            this.label2.TabIndex = 3;
            this.label2.Text = "Nazwisko";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Times New Roman", 16.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(331, 285);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(146, 25);
            this.label3.TabIndex = 7;
            this.label3.Text = "Triaż dzien/noc";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Times New Roman", 16.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label5.Location = new System.Drawing.Point(331, 237);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(102, 25);
            this.label5.TabIndex = 9;
            this.label5.Text = "Zaległości";
            // 
            // buttonDodaj
            // 
            this.buttonDodaj.Location = new System.Drawing.Point(17, 39);
            this.buttonDodaj.Name = "buttonDodaj";
            this.buttonDodaj.Size = new System.Drawing.Size(130, 65);
            this.buttonDodaj.TabIndex = 10;
            this.buttonDodaj.Text = "Dodaj nowego pracownika";
            this.buttonDodaj.UseVisualStyleBackColor = true;
            this.buttonDodaj.Click += new System.EventHandler(this.buttonDodaj_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Times New Roman", 16.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label6.Location = new System.Drawing.Point(12, 182);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(131, 25);
            this.label6.TabIndex = 12;
            this.label6.Text = "Numer osoby";
            // 
            // numericUpDownZaleglosci
            // 
            this.numericUpDownZaleglosci.Font = new System.Drawing.Font("Times New Roman", 16.25F);
            this.numericUpDownZaleglosci.Location = new System.Drawing.Point(467, 235);
            this.numericUpDownZaleglosci.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownZaleglosci.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.numericUpDownZaleglosci.Name = "numericUpDownZaleglosci";
            this.numericUpDownZaleglosci.Size = new System.Drawing.Size(168, 32);
            this.numericUpDownZaleglosci.TabIndex = 13;
            // 
            // checkBoxCzyTriazDzien
            // 
            this.checkBoxCzyTriazDzien.AutoSize = true;
            this.checkBoxCzyTriazDzien.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.checkBoxCzyTriazDzien.Location = new System.Drawing.Point(505, 295);
            this.checkBoxCzyTriazDzien.Name = "checkBoxCzyTriazDzien";
            this.checkBoxCzyTriazDzien.Size = new System.Drawing.Size(15, 14);
            this.checkBoxCzyTriazDzien.TabIndex = 14;
            this.checkBoxCzyTriazDzien.UseVisualStyleBackColor = true;
            // 
            // checkBoxCzyTriazNoc
            // 
            this.checkBoxCzyTriazNoc.AutoSize = true;
            this.checkBoxCzyTriazNoc.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.checkBoxCzyTriazNoc.Location = new System.Drawing.Point(582, 295);
            this.checkBoxCzyTriazNoc.Name = "checkBoxCzyTriazNoc";
            this.checkBoxCzyTriazNoc.Size = new System.Drawing.Size(15, 14);
            this.checkBoxCzyTriazNoc.TabIndex = 15;
            this.checkBoxCzyTriazNoc.UseVisualStyleBackColor = true;
            // 
            // listBoxNumerOsoby
            // 
            this.listBoxNumerOsoby.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.listBoxNumerOsoby.FormattingEnabled = true;
            this.listBoxNumerOsoby.ItemHeight = 23;
            this.listBoxNumerOsoby.Location = new System.Drawing.Point(149, 182);
            this.listBoxNumerOsoby.Name = "listBoxNumerOsoby";
            this.listBoxNumerOsoby.Size = new System.Drawing.Size(167, 27);
            this.listBoxNumerOsoby.TabIndex = 16;
            this.listBoxNumerOsoby.SelectedIndexChanged += new System.EventHandler(this.listBoxNumerOsoby_SelectedIndexChanged);
            // 
            // buttonEdytujPracownika
            // 
            this.buttonEdytujPracownika.Location = new System.Drawing.Point(176, 39);
            this.buttonEdytujPracownika.Name = "buttonEdytujPracownika";
            this.buttonEdytujPracownika.Size = new System.Drawing.Size(130, 65);
            this.buttonEdytujPracownika.TabIndex = 17;
            this.buttonEdytujPracownika.Text = "Edytuj dane pracownika";
            this.buttonEdytujPracownika.UseVisualStyleBackColor = true;
            this.buttonEdytujPracownika.Click += new System.EventHandler(this.buttonEdytujPracownika_Click);
            // 
            // buttonUsun
            // 
            this.buttonUsun.Location = new System.Drawing.Point(345, 39);
            this.buttonUsun.Name = "buttonUsun";
            this.buttonUsun.Size = new System.Drawing.Size(130, 65);
            this.buttonUsun.TabIndex = 18;
            this.buttonUsun.Text = "Usuń dane pracownika";
            this.buttonUsun.UseVisualStyleBackColor = true;
            this.buttonUsun.Click += new System.EventHandler(this.buttonUsun_Click);
            // 
            // buttonSaveAndQuit
            // 
            this.buttonSaveAndQuit.Location = new System.Drawing.Point(505, 39);
            this.buttonSaveAndQuit.Name = "buttonSaveAndQuit";
            this.buttonSaveAndQuit.Size = new System.Drawing.Size(130, 65);
            this.buttonSaveAndQuit.TabIndex = 19;
            this.buttonSaveAndQuit.Text = "Zapisz i zamknij";
            this.buttonSaveAndQuit.UseVisualStyleBackColor = true;
            this.buttonSaveAndQuit.Click += new System.EventHandler(this.buttonSaveAndQuit_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 339);
            this.ControlBox = false;
            this.Controls.Add(this.buttonSaveAndQuit);
            this.Controls.Add(this.buttonUsun);
            this.Controls.Add(this.buttonEdytujPracownika);
            this.Controls.Add(this.listBoxNumerOsoby);
            this.Controls.Add(this.checkBoxCzyTriazNoc);
            this.Controls.Add(this.checkBoxCzyTriazDzien);
            this.Controls.Add(this.numericUpDownZaleglosci);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.buttonDodaj);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxNazwisko);
            this.Controls.Add(this.labelImie);
            this.Controls.Add(this.textBoxImie);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownZaleglosci)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBoxImie;
        private System.Windows.Forms.Label labelImie;
        private System.Windows.Forms.TextBox textBoxNazwisko;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonDodaj;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericUpDownZaleglosci;
        private System.Windows.Forms.CheckBox checkBoxCzyTriazDzien;
        private System.Windows.Forms.CheckBox checkBoxCzyTriazNoc;
        private System.Windows.Forms.ListBox listBoxNumerOsoby;
        private System.Windows.Forms.Button buttonEdytujPracownika;
        private System.Windows.Forms.Button buttonUsun;
        private System.Windows.Forms.Button buttonSaveAndQuit;
    }
}