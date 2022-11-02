using MetroSet_UI.Forms;
using Propert_Utility.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Telerik.Charting;

namespace FlexMold.Forms
{
    public partial class FrmHome : MetroSet_UI.Forms.MetroSetForm
    {
        private string ParentFolder = ConfigurationManager.AppSettings["ParentFolderPath"].ToString();
        private string ProjectsFolder = ConfigurationManager.AppSettings["ProjectsFolderPath"].ToString();

        public FrmHome()
        {
            InitializeComponent();
            LoadParentFolder();
            //TbContrl.UseAnimation = false;
            
        }

        private void FrmHome_Load(object sender, EventArgs e)
        {
            fillChart();
            this.uI_SettingsMachine1.Show();
            this.uI_SettingsMotor1.Hide();
            this.uI_SettingsPower1.Hide();
            this.uI_SettingsVaccuum1.Hide();

            this.uI_AccessoriesLasor1.Hide();
            this.uI_AccessoriesHeater1.Show();

            metroSetTextBox2.Visible = false;
            metroSetTextBox2.Hide();

        }
        private void fillChart()
        {
            //AddXY value in chart1 in series named as Salary  
            chart1.Series["Motor"].Points.AddXY("Motor Speed", "100");
            chart1.Series["Error"].Points.AddXY("Error Count", "80");
            chart1.Series["Rotation"].Points.AddXY("Rotation/Sec", "7000");
            chart1.Series["Laser"].Points.AddXY("Laser", "10000");
           
            //chart title  
            chart1.Titles.Add("System Status");



        //ChartSeries obj = new ChartSeries();
        //obj.Name = "Motor";
        //obj.DataLabelsColumn = "h";
        //obj.DataXColumn = "d";
        //radChart1.Series.Add(obj);// .Series["Motor"].Points.AddXY("Motor Speed", "100");
            //radChart1.Series["Error"].Points.AddXY("Error Count", "80");
            //radChart1.Series["Rotation"].Points.AddXY("Rotation/Sec", "7000");
            //radChart1.Series["Laser"].Points.AddXY("Laser", "10000");

            //chart title  
           
        }
    

    #region Project Tab Working

    public string[] GetFolderList(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

        }



        public void LoadParentFolder()
        {

            string[] folderslist = GetFolderList(ParentFolder);
            List<ComboBoxItems> lstcmxitm = new List<ComboBoxItems>();
            for (int i = 0; i < folderslist.Length; i++)
            {
                string foldername = folderslist[i].Substring(folderslist[i].LastIndexOf(@"\"));
                string text = foldername.Remove(0, 1);
                string value = folderslist[i];
                ComboBoxItems objitems = new ComboBoxItems();
                objitems.Text = text;
                objitems.Value = value;
                lstcmxitm.Add(objitems);
            }

            cmbparentfolder.DataSource = lstcmxitm;
            cmbparentfolder.DisplayMember = "Text";
            cmbparentfolder.ValueMember = "Value";
        }


        public void LoadChildFolder(ComboBoxItems folderpath)
        {

            string[] folderslist = GetFolderList(folderpath.Value);
            List<ComboBoxItems> lstcmxitm = new List<ComboBoxItems>();
            for (int i = 0; i < folderslist.Length; i++)
            {
                string foldername = folderslist[i].Substring(folderslist[i].LastIndexOf(@"\"));
                string text = foldername.Remove(0, 1);
                string value = folderslist[i];
                ComboBoxItems objitems = new ComboBoxItems();
                objitems.Text = text;
                objitems.Value = value;
                lstcmxitm.Add(objitems);
            }

            cmbpanel.DataSource = lstcmxitm;
            cmbpanel.DisplayMember = "Text";
            cmbpanel.ValueMember = "Value";
        }

        #endregion

        #region TreeView By MASB
        public void LoadDirectory_treeview(string Dir)
        {
            DirectoryInfo di = new DirectoryInfo(Dir);
            
            //Setting ProgressBar Maximum Value
            //progressBar1.Maximum = Directory.GetFiles(Dir, "*.*", SearchOption.AllDirectories).Length + Directory.GetDirectories(Dir, "**", SearchOption.AllDirectories).Length;
            TreeNode tds = treeView1.Nodes.Add(di.Name);
            tds.Tag = di.FullName;
            tds.StateImageIndex = 0;

            LoadFiles_treeview(Dir, tds,"*.las");
            LoadSubDirectories_treeview(Dir, tds);



            string[] CSVFILES = Directory.GetFiles(Dir, "*.csv", SearchOption.AllDirectories);
            //listLaserFiles.Items.Clear();
            List<ListBoxItems> csvfilegrd = new List<ListBoxItems>();

            for (int i = 0; i < CSVFILES.Length; i++)
            {
                string FileName = CSVFILES[i].Substring(CSVFILES[i].LastIndexOf(@"\"));
                string text = FileName.Remove(0, 1);
                string value = CSVFILES[i];
                ListBoxItems csvfilelist = new ListBoxItems();
                csvfilelist.Text = text;
                csvfilelist.Value = value;
                csvfilegrd.Add(csvfilelist);
            }

            dgvcsvfiles.DataSource = csvfilegrd;

        }

        private void LoadSubDirectories_treeview(string dir, TreeNode td)
        {
            // Get all subdirectories
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            // Loop through them to see if they have any other subdirectories
            foreach (string subdirectory in subdirectoryEntries)
            {

                DirectoryInfo di = new DirectoryInfo(subdirectory);
                TreeNode tds = td.Nodes.Add(di.Name);
                tds.StateImageIndex = 0;
                tds.Tag = di.FullName;
                LoadFiles_treeview(subdirectory, tds,"*.las");
                LoadSubDirectories_treeview(subdirectory, tds);

            }
        }

        private void LoadFiles_treeview(string dir, TreeNode td,string FileExtension)
        {
            //string[] Files = Directory.GetFiles(dir, "*.*");
            string[] Files = Directory.GetFiles(dir, FileExtension);

            // Loop through them to see files
            foreach (string file in Files)
            {
                FileInfo fi = new FileInfo(file);
                TreeNode tds = td.Nodes.Add(fi.Name);
                tds.Tag = fi.FullName;
                tds.StateImageIndex = 1;

            }
        }

        #endregion

        private void cmbparentfolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cm = cmbparentfolder;
            ComboBoxItems selectedvalue = (ComboBoxItems)cm.SelectedItem;

            LoadChildFolder(selectedvalue);
        }

        public void LoadFiles(string filespath)
        {

            treeView1.Nodes.Clear();

            if (filespath != "" && Directory.Exists(filespath))
                LoadDirectory_treeview(filespath);
            else
                MessageBox.Show("Select Directory!!");

            metroSetListBox1.Items.Clear();
            ArrayList alPanels = new ArrayList();
            String[] panelsArr;

            foreach (string st in Directory.GetDirectories(filespath))
            {
                alPanels.Add(st.Substring(st.LastIndexOf('\\')+1));
            }

            if(alPanels.Count>0)
                metroSetListBox1.AddItems((string[])alPanels.ToArray(typeof(string)));

            string[] LaserFiles = Directory.GetFiles(filespath, "*.las",SearchOption.AllDirectories);
            //listLaserFiles.Items.Clear();
            //List<ListBoxItems> listboxitm = new List<ListBoxItems>();
            //ListBoxItems objlstboxitem = new ListBoxItems()
            //{
            //    Text = "Double Click On File",
            //    Value = ""
            //};
            //listboxitm.Add(objlstboxitem);
            //for (int i = 0; i < LaserFiles.Length; i++)
            //{
            //    string FileName = LaserFiles[i].Substring(LaserFiles[i].LastIndexOf(@"\"));
            //    string text = FileName.Remove(0, 1);
            //    string value = LaserFiles[i];
            //    objlstboxitem = new ListBoxItems();
            //    objlstboxitem.Text = text;
            //    objlstboxitem.Value = value;
            //    listboxitm.Add(objlstboxitem);
              
            //}
            //listLaserFiles.ScrollAlwaysVisible = true;

            //listLaserFiles.DataSource = listboxitm;// new BindingSource(listboxitm, null);
            //listLaserFiles.ValueMember = "value";
            //listLaserFiles.DisplayMember = "Text";


            //CSVFILELOADING


          //  string[] CSVFILES = Directory.GetFiles(filespath.Value, "*.csv",SearchOption.AllDirectories);
          //  //listLaserFiles.Items.Clear();
          //  List<ListBoxItems> csvfilegrd = new List<ListBoxItems>();

          //  for (int i = 0; i < CSVFILES.Length; i++)
          //  {
          //      string FileName = CSVFILES[i].Substring(CSVFILES[i].LastIndexOf(@"\"));
          //      string text = FileName.Remove(0, 1);
          //      string value = CSVFILES[i];
          //      ListBoxItems csvfilelist = new ListBoxItems();
          //      csvfilelist.Text = text;
          //      csvfilelist.Value = value;
          //      csvfilegrd.Add(csvfilelist);
          //  }

          //dgvcsvfiles.DataSource = csvfilegrd;



        }

        private void cmbpanel_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cm = cmbpanel;
            ComboBoxItems selectedvalue = (ComboBoxItems)cm.SelectedItem;

            LoadFiles(selectedvalue.Value);
        }

        private void metroTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (metroSetTabControl1.SelectedIndex == 0) //Display Home Visual Chart Status
            {
                chart1.Show();
                metroSetTextBox2.Visible = false;
                metroSetTextBox2.Hide();

            }
            else if (metroSetTabControl1.SelectedIndex == 1) //Display Vaccuum Status
            {
                chart1.Hide();
                metroSetTextBox2.Visible = true;
                metroSetTextBox2.Text = "System Vaccuum Details will be displayed here!";
                metroSetTextBox2.Show();

            }
            else if (metroSetTabControl1.SelectedIndex == 2) //Display Machine Status
            {
                chart1.Hide();
                metroSetTextBox2.Visible = true;
                metroSetTextBox2.Text = "System Machine Status Details will be displayed here!";
                metroSetTextBox2.Show();

            }
            else if (metroSetTabControl1.SelectedIndex == 3) //Display Motor Status
            {
                chart1.Hide();
                metroSetTextBox2.Visible = true;
                metroSetTextBox2.Text = "System Motor Details will be displayed here!";
                metroSetTextBox2.Show();

            }
            else if (metroSetTabControl1.SelectedIndex == 4) //Display Heater Status
            {
                chart1.Hide();
                metroSetTextBox2.Visible = true;
                metroSetTextBox2.Text = "System Heater Details will be displayed here!";
                metroSetTextBox2.Show();

            }
            else if (metroSetTabControl1.SelectedIndex == 5) //Display Power Status
            {
                chart1.Hide();
                metroSetTextBox2.Visible = true;
                metroSetTextBox2.Text = "System Power Details will be displayed here!";
                metroSetTextBox2.Show();

            }
            else if (metroSetTabControl1.SelectedIndex == 6) //Display Emergency Status
            {
                chart1.Hide();
                metroSetTextBox2.Visible = true;
                metroSetTextBox2.Text = "System Emergency Details will be displayed here!";
                metroSetTextBox2.Show();

            }
            else if (metroSetTabControl1.SelectedIndex == 7) //Display Laser Status
            {
                chart1.Hide();
                metroSetTextBox2.Visible = true;
                metroSetTextBox2.Text = "System Laser Details will be displayed here!";
                metroSetTextBox2.Show();

            }


        }

        private void metroSetTabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.metroSetTabControl2.SelectedIndex == 0)
            {                
                this.uI_SettingsMotor1.Hide();
                this.uI_SettingsVaccuum1.Hide();
                this.uI_SettingsPower1.Hide();
                this.uI_SettingsMachine1.Show();
            }
            else if (this.metroSetTabControl2.SelectedIndex == 1)
            {
                this.uI_SettingsMotor1.Hide();
                this.uI_SettingsVaccuum1.Hide();
                this.uI_SettingsMachine1.Hide();
                this.uI_SettingsPower1.Show();
            }
            else if (this.metroSetTabControl2.SelectedIndex == 2)
            {
                this.uI_SettingsVaccuum1.Hide();
                this.uI_SettingsPower1.Hide();
                this.uI_SettingsMachine1.Hide();
                this.uI_SettingsMotor1.Show();
            }
            else if (this.metroSetTabControl2.SelectedIndex == 3)
            {
                this.uI_SettingsMotor1.Hide();                
                this.uI_SettingsPower1.Hide();
                this.uI_SettingsMachine1.Hide();
                this.uI_SettingsVaccuum1.Show();
            }

        }

        private void metroSetTabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.metroSetTabControl3.SelectedIndex == 0)
            {
                this.uI_AccessoriesLasor1.Hide();
                this.uI_AccessoriesHeater1.Show();
            }
            else if (this.metroSetTabControl3.SelectedIndex == 1)
            {
                this.uI_AccessoriesHeater1.Hide();
                this.uI_AccessoriesLasor1.Show();
            }

        }

        private void TbContrl_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listLaserFiles_MouseDoubleClick(object sender, EventArgs e)
        {
            //string st = treeView1.SelectedNode.ImageKey;
            string FilePath = "";
            if (treeView1.SelectedNode.Level>1)
                FilePath = ProjectsFolder+"\\"+treeView1.SelectedNode.FullPath;
            else
                FilePath = ((Propert_Utility.Models.ComboBoxItems)cmbpanel.SelectedItem).Value + "\\" + treeView1.SelectedNode.FullPath;

            if (File.Exists(FilePath))
            {
                MetroSetMessageBox.Show(this, treeView1.SelectedNode.FullPath + " Selected For Execution", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // radChart1.ChartTitle.TextBlock = 
            }
            else MetroSetMessageBox.Show(this,"File not found at location "+ FilePath, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private void metroSetListBox1_DoubleClick(object sender, EventArgs e)
        {

            treeView1.Nodes.Clear();
            string selectedPanel = cmbpanel.SelectedValue + "\\"+ metroSetListBox1.SelectedValue.ToString();

            if (selectedPanel != "" && Directory.Exists(selectedPanel))
                LoadDirectory_treeview(selectedPanel);
            else
                MessageBox.Show("Selected Panel Has Incorrect Values!");


        }
    }
}
