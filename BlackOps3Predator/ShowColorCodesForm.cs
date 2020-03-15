using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackOps3Predator
{
    public partial class ShowColorCodesForm : Form
    {
        public ShowColorCodesForm()
        {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            //Found this on the internet (Created by: David Browne)
            //It grabs the only instance of the BaseForm object running in memory
            //and creates it as a variable that I can use.
            var GrabMyFormsInstance = (Application.OpenForms.OfType<BaseForm>().Single());
            GrabMyFormsInstance.WindowState = (FormWindowState.Normal);
            Close();
        }
    }
}
