namespace FlexMold.Forms
{
    partial class UI_SettingsMotor

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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.UI_SettingsPowerGroupBox1 = new System.Windows.Forms.GroupBox();
            this.metroComboBox2 = new MetroFramework.Controls.MetroComboBox();
            this.metroComboBox1 = new MetroFramework.Controls.MetroComboBox();
            this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            this.UI_SettingsPowerGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // UI_SettingsPowerGroupBox1
            // 
            this.UI_SettingsPowerGroupBox1.Controls.Add(this.metroComboBox2);
            this.UI_SettingsPowerGroupBox1.Controls.Add(this.metroComboBox1);
            this.UI_SettingsPowerGroupBox1.Controls.Add(this.metroLabel2);
            this.UI_SettingsPowerGroupBox1.Controls.Add(this.metroLabel1);
            this.UI_SettingsPowerGroupBox1.Location = new System.Drawing.Point(3, 3);
            this.UI_SettingsPowerGroupBox1.Name = "UI_SettingsPowerGroupBox1";
            this.UI_SettingsPowerGroupBox1.Size = new System.Drawing.Size(321, 218);
            this.UI_SettingsPowerGroupBox1.TabIndex = 0;
            this.UI_SettingsPowerGroupBox1.TabStop = false;
            // 
            // metroComboBox2
            // 
            this.metroComboBox2.FormattingEnabled = true;
            this.metroComboBox2.ItemHeight = 23;
            this.metroComboBox2.Items.AddRange(new object[] {
            "0.1",
            "0.2",
            "0.3",
            "0.4",
            "0.5",
            "0.6",
            "0.7",
            "0.8",
            "0.9",
            "1"});
            this.metroComboBox2.Location = new System.Drawing.Point(177, 49);
            this.metroComboBox2.Name = "metroComboBox2";
            this.metroComboBox2.Size = new System.Drawing.Size(121, 29);
            this.metroComboBox2.TabIndex = 11;
            // 
            // metroComboBox1
            // 
            this.metroComboBox1.FormattingEnabled = true;
            this.metroComboBox1.ItemHeight = 23;
            this.metroComboBox1.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.metroComboBox1.Location = new System.Drawing.Point(177, 13);
            this.metroComboBox1.Name = "metroComboBox1";
            this.metroComboBox1.Size = new System.Drawing.Size(121, 29);
            this.metroComboBox1.TabIndex = 10;
            // 
            // metroLabel2
            // 
            this.metroLabel2.AutoSize = true;
            this.metroLabel2.Location = new System.Drawing.Point(6, 59);
            this.metroLabel2.Name = "metroLabel2";
            this.metroLabel2.Size = new System.Drawing.Size(85, 19);
            this.metroLabel2.TabIndex = 9;
            this.metroLabel2.Text = "Motor Errors";
            // 
            // metroLabel1
            // 
            this.metroLabel1.AutoSize = true;
            this.metroLabel1.Location = new System.Drawing.Point(6, 16);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(85, 19);
            this.metroLabel1.TabIndex = 8;
            this.metroLabel1.Text = "Motor Count";
            // 
            // UI_SettingsMotor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.UI_SettingsPowerGroupBox1);
            this.Name = "UI_SettingsMotor";
            this.Size = new System.Drawing.Size(327, 224);
            this.UI_SettingsPowerGroupBox1.ResumeLayout(false);
            this.UI_SettingsPowerGroupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox UI_SettingsPowerGroupBox1;
        private MetroFramework.Controls.MetroComboBox metroComboBox2;
        private MetroFramework.Controls.MetroComboBox metroComboBox1;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroLabel metroLabel1;
    }
}
