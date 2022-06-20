using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using PS3Lib;

namespace BlackOps3Predator
{
    public partial class BaseForm : Form
    {
        //Variables
        public static PS3API PS3 = (new PS3API());
        bool SelectedAPI = (false);
        bool RPC_Enabled = (false);
        bool Connected = (false);
        bool Attached = (false);
        uint G_Client = (0x18C6220);
        uint G_Client_Size = (0x6200);
        uint selected_client = (0);

        //Save clients mod values in memory 
        bool[] setUFOon = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        bool[] setGodOn = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        bool[] setInfiAmmoOn = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        int[] setSpeed = new int[12] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        bool[] setThirdPersonOn = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };
        bool[] setJetPackOn = new bool[12] { false, false, false, false, false, false, false, false, false, false, false, false };

        //AllClients
        bool AllClients = (false);
        bool setUFOonAC = (false);
        bool setGodOnAC = (false);
        bool setInfiAmmoOnAC = (false);
        bool setThirdPersonOnAC = (false);
        int setSpeedAC = (1);
        bool setJetPackOnAC = (false);

        //Background worker loop check
        bool forceHostLoopOn = (false);

        //The constructor
        public BaseForm()
        {
            InitializeComponent();
            grabIPsTimer.Start();
        }

        ///////////////////////
        ///Connect and Attach///
        /////////////////////////
        ///
        public void updatePS3StatusUI()
        {
            if (Connected) { connectedBoolLabel.Text = ("True"); connectedBoolLabel.ForeColor = (Color.FromName("Green")); }
            else { connectedBoolLabel.Text = ("False"); connectedBoolLabel.ForeColor = (Color.FromName("DarkRed")); }

            if (Connected && Attached) { attachedBoolLabel.Text = ("True"); attachedBoolLabel.ForeColor = (Color.FromName("Green")); }
            else { attachedBoolLabel.Text = ("False"); attachedBoolLabel.ForeColor = (Color.FromName("DarkRed")); }
        }

        private void ccapiCheckBox_CheckedChanged(object sender)
        {
            if (ccapiCheckBox.Checked)
            {
                tmapiCheckBox.Checked = (false);
                PS3.ChangeAPI(SelectAPI.ControlConsole);
                SelectedAPI = (true);
            }

            else
            {
                SelectedAPI = (false);
            }
        }

        private void tmapiCheckBox_CheckedChanged(object sender)
        {
            if (tmapiCheckBox.Checked)
            {
                ccapiCheckBox.Checked = (false);
                PS3.ChangeAPI(SelectAPI.TargetManager);
                SelectedAPI = (true);
            }

            else
            {
                SelectedAPI = (false);
            }
        }

        private void antiBanCheckBox_CheckedChanged(object sender)
        {
            if (Connected && Attached)
            {
                antiBanCheckBox.Enabled = (false);

                //New Anti-Ban
                PS3.SetMemory(0x7C4148, new byte[] { 0x60, 0, 0, 0 });
                PS3.SetMemory(0x7C4050, new byte[] { 0x60, 0, 0, 0 });
                PS3.SetMemory(0x55705C, new byte[] { 0x60, 0, 0, 0 });
                PS3.SetMemory(0x5570FC, new byte[] { 0x60, 0, 0, 0 });
                PS3.SetMemory(0x4AC9C0, new byte[] { 0x60, 0, 0, 0 });
                PS3.SetMemory(0x4AAD20, new byte[] { 0x60, 0, 0, 0 });

                //Old Anti-Ban (Freezes systems)
                /*
                PS3.SetMemory(0x7c4148, new byte[] { 0x60, 0x00, 0x00, 0x00 });
                PS3.SetMemory(0x7c4050, new byte[] { 0x60, 0x00, 0x00, 0x00 });
                PS3.SetMemory(0x7c4758, new byte[] { 0x60, 0x00, 0x00, 0x00 });
                PS3.SetMemory(0x7c4660, new byte[] { 0x60, 0x00, 0x00, 0x00 });
                */

                MessageBox.Show("Anti-Ban: Enabled.", "Black Ops 3 Anti-Ban.", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                PS3.CCAPI.Notify(CCAPI.NotifyIcon.TROPHY4, "Anti-Ban: Enabled");
            }

            else
            {
                MessageBox.Show("You must be connected and attached in order to be able to use anti-ban.", "Error");
            }
        }

        private void connect_Button_Click(object sender, EventArgs e)
        {
            //Is there a target selected?, Can we connect with the selected target?
            Connected = (false);
            Attached = (false);
            antiBanCheckBox.Enabled = (true);
            PS3.DisconnectTarget();
            updatePS3StatusUI();

            if (PS3.GetCurrentAPIName() == "Control Console" && SelectedAPI && PS3.ConnectTarget(0))
            {
                PS3.CCAPI.Notify(CCAPI.NotifyIcon.TROPHY4, "Successfully Connected To PS3!");
                Connected = (true);
            }

            else if (PS3.GetCurrentAPIName() == "Target Manager" && SelectedAPI && PS3.ConnectTarget(0))
            {
                Connected = (true);
            }

            else
            {
                MessageBox.Show("You must select a PS3 API to use before you can connect.", "Error");
            }

            //We connected successufully, now can we attach?
            if (Connected && SelectedAPI && PS3.AttachProcess())
            {
                PS3.CCAPI.Notify(CCAPI.NotifyIcon.TROPHY4, "Successfully Attached To PS3 with Black Ops III Predator RTM tool v1.00!");
                PS3.CCAPI.RingBuzzer(CCAPI.BuzzerMode.Double);
                Attached = (true);
            }

            else if (!Connected)
            {
                MessageBox.Show("An error occurred while trying to connect to your playstation.", "Error");
            }

            else
            {
                MessageBox.Show("Failed to attach to Black Ops 3.", "Error");
            }

            updatePS3StatusUI();
        }

        private void disconnect_Button_Click(object sender, EventArgs e)
        {
            PS3.CCAPI.RingBuzzer(CCAPI.BuzzerMode.Single);
            Connected = (false);
            Attached = (false);
            PS3.DisconnectTarget();
            updatePS3StatusUI();
        }
        //End connect and attach tab

        ///////////////
        ///Name & Clan//
        /////////////////
        ///
        //Methods
        public void ChangeName(string InputText)
        {
            byte[] bytes = (Encoding.ASCII.GetBytes(InputText + "\0"));
            PS3.SetMemory(0x1f3cff4, bytes);
            byte[] buffer = (Encoding.ASCII.GetBytes(InputText + "\0"));
            PS3.SetMemory(0x20dfe78, buffer);
        }

        public void ChangeClantag(string InputText)
        {
            byte[] bytes = (Encoding.ASCII.GetBytes(InputText + "\0"));
            PS3.SetMemory(0x37EF8FD9, bytes);
        }

        public void ChangePlayercard(string InputText)
        {
            byte[] bytes = (Encoding.ASCII.GetBytes(InputText + "\0"));
            PS3.SetMemory(0x1F2CE45, bytes);
        }

        //Timers
        private void flashNameTimer_Tick(object sender, EventArgs e)
        {
            int flashColor = (new Random().Next(0, 7));
            ChangeName("^" + flashColor + changeNameTxT.Text);
        }

        private void flashClantagTimer_Tick(object sender, EventArgs e)
        {
            int flashColor = (new Random().Next(0, 7));
            ChangeClantag("^" + flashColor + "^");
        }

        //Check Boxes
        private void flashNameCheckBox_CheckedChanged(object sender)
        {
            if (flashNameCheckBox.Checked)
            {
                flashNameTimer.Start();
            }

            else
            {
                flashNameTimer.Stop();
            }
        }

        private void flashClanCheckBox_CheckedChanged(object sender)
        {
            if (flashClanCheckBox.Checked)
            {
                flashClantagTimer.Start();
            }

            else
            {
                flashClantagTimer.Stop();
            }
        }

        //Buttons
        private void changeNameButton_Click(object sender, EventArgs e)
        {
            ChangeName(changeNameTxT.Text);
        }

        private void changePlayercardButton_Click(object sender, EventArgs e)
        {
            ChangePlayercard(changeNameTxT.Text);
        }

        private void changeClanButton_Click(object sender, EventArgs e)
        {
            ChangeClantag(changeClantagTxT.Text);
        }

        //Preset names
        private void name_LordVirus_Button_Click(object sender, EventArgs e)
        {
            ChangeName("^5Lord^6Virus");
        }

        private void name_god_Button_Click(object sender, EventArgs e)
        {
            ChangeName("^B^^2God^B^");
        }

        private void name_Developer_Button_Click(object sender, EventArgs e)
        {
            ChangeName("^6Developer");
        }

        private void name_Sex_Button_Click(object sender, EventArgs e)
        {
            ChangeName("^5Se[{+gostand}]");
        }

        //Preset clantags
        private void clan_KEY_UNBOUND_Button_Click(object sender, EventArgs e)
        {
            int flashColor = (new Random().Next(0, 7));
            ChangeClantag("{^" + flashColor + "}");
        }

        private void clan_RandomColor_Button_Click(object sender, EventArgs e)
        {
            int flashColor = (new Random().Next(0, 7));
            ChangeClantag("^" + flashColor + "^");
        }

        private void clan_Checker_Big_Button_Click(object sender, EventArgs e)
        {
            ChangeClantag("^Hac");
        }

        private void clan_Checker_Small_Button_Click(object sender, EventArgs e)
        {
            ChangeClantag("^B^");
        }
        //End names and clantags tab

        ////////////////////
        ///Account options//
        /////////////////////
        ///
        //Unlock all method
        public void UnlockAll()
        {
            MessageBox.Show("Please be patient this will take up to 2 minutes.", "Please Wait...", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            byte[] buffer = new byte[] { 0xff };
            PS3.SetMemory(0x37eec9f8, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37eec1c7 + 0x3ca1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cc6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cea, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cf0, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cf5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cfb, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d01, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d07, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e91, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3eb6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3eda, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3edf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3ee6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3eeb, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3ef1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3ef8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f0d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f32, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f56, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f5b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f62, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f67, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f6d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f74, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d99, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3dbe, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3de2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3de7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3ded, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3df3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3df9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3dff, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d1d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d41, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d66, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d6c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d72, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d78, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d7e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d84, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e15, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e39, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e5d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e63, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e69, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e6f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e75, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e7b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4179, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x419e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41c2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41c7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41d3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41d9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41df, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4369, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x438e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43b1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43b7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43bd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43c3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43c9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43cf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4271, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42ba, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42c0, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42c6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42cc, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42d2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42d8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4295, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41f5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x421a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4243, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x423d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4249, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x424f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4255, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x425b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42ed, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4312, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4335, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x433b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4341, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4347, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x434d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4353, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4461, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4486, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44a9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44af, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44b5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44bb, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44c1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44c7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43e5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x440a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x442d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4433, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4439, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x443f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4445, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x444b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50f9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5147, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5142, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x514e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5153, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5159, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x515f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5165, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5175, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5199, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51c3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51ca, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51cf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51d5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51db, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51bd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5001, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5026, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x504a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5050, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5056, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x505c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5062, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5068, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x507d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50c6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50cc, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50d2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50d8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50de, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50e4, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50ea, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4749, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x476d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4792, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4798, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x479e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47a4, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47aa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47b0, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4651, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4675, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4699, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46a0, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46a6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46ac, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46b2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46b8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47c5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47e9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x480d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4814, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x481a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4820, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4826, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x482c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46f1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4715, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x471c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4722, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4728, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x472e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4734, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4ba5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4bed, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4bf4, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4bfa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c00, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c06, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c0c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c12, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b29, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b71, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b77, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b7e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b83, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b8a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b8f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b96, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c9d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4ce5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4cec, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4cf2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4cf8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4cfe, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4d04, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4d0a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c21, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c69, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c70, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c75, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c7c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c82, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c88, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c8e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3845, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3869, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x388d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3894, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3899, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x389f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x38a6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x38ab, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x38c1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x38e6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3909, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3910, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3916, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x391c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3921, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3928, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x393d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3961, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3986, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x398b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3992, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3998, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x399e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x39a4, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x53e2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x542a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5430, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5436, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x543c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5442, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5448, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x544e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x545e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54a5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54ab, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54b1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54b7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54be, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54c3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54ca, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x67bd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x67c3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x6806, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x680c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x6811, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x6817, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x681d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x6824, buffer);
            PS3.SetMemory(0x37ef299b, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef0357, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef03d3, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef7ff5, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef04cb, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef0547, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef05c3, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef063f, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37eefe7f, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37eefefb, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37eeff77, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37eefff3, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef006f, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef00eb, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef082f, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef08ab, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef0927, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef09a3, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef0d07, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef0d83, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef91df, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef0dff, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef11df, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef125b, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef12d7, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef1353, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37eefa23, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37eefa9f, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37eefb1b, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef15bf, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37ef163b, new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            PS3.SetMemory(0x37eec1c7 + 0xc539, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc53e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc544, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc54a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc550, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc556, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc55c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc562, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc568, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc56e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc58c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc5e1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc74f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc76e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc791, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc797, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc7c6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc80e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc815, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc856, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc862, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc868, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc8aa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc8e1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc8e6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc8fe, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc90a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc916, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc91d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc958, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc965, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc96a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc971, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc977, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc97d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc995, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc9a6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc9c5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca18, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca1f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca2a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca31, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca36, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca6d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca8a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca91, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca96, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca9d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaa3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaa8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaa9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaaf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcab5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcacd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaf1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb08, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb15, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb38, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb3e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb45, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb5d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb62, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb7b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb98, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb9e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcba5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbaa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbb1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbc2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbd5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbda, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbe1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbed, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbf8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcc95, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xccc3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcd01, buffer);
            byte[] buffer2 = new byte[] { 11 };
            PS3.SetMemory(0x37eec1c7 + 0xcd20, buffer2);
            PS3.SetMemory(0x37eec1c7 + 0xcd5a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcdae, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcdcd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcdd8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcdf1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xce93, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcfe9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xd0cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xd13f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc011, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc03a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc041, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc047, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc04d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc053, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc088, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc0d1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc0e2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc0e8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc0ee, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc101, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc107, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc10d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc113, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc118, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc137, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc13d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc143, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc14e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc155, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc167, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc178, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc17e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc185, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc18a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1c1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1c7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1d2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1d8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1e5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc20e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc227, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc232, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc251, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc257, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2a5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2aa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2b1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2b7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2bd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2c8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2ce, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2d5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2db, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2e1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2e7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2ed, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2f2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2f8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2fe, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc305, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc30b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc317, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc31d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc322, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc328, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc34d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc352, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc358, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc35e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc365, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc371, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc382, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc38e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3a7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3b2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3b8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3bf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3c5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3ca, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3d1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3d7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3dd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3e3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3e8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3fa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc401, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc407, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc40d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc412, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc418, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc41e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc425, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc42a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc431, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc436, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc44e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc455, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc461, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc467, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc472, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc478, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc47e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc49d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4a2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4a8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4b5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4e5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4ea, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc50f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc515, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc51a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc521, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc527, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc52d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc532, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc538, buffer);
            PS3.SetMemory(0x37EF8E65, new byte[] { 0, 1, 0, 0 });
            MessageBox.Show("Level 55 / Unlock All / Weapon Unlocks", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        public void Derank()
        {
            MessageBox.Show("Please be patient this will take up to 2 minutes.", "Please Wait...", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            byte[] buffer = new byte[] { 0x00 };
            PS3.SetMemory(0x37eec9f8, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37eec1c7 + 0x3ca1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cc6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cea, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cf0, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cf5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3cfb, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d01, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d07, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e91, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3eb6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3eda, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3edf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3ee6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3eeb, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3ef1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3ef8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f0d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f32, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f56, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f5b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f62, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f67, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f6d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3f74, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d99, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3dbe, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3de2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3de7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3ded, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3df3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3df9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3dff, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d1d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d41, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d66, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d6c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d72, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d78, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d7e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3d84, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e15, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e39, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e5d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e63, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e69, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e6f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e75, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3e7b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4179, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x419e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41c2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41c7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41d3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41d9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41df, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4369, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x438e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43b1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43b7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43bd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43c3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43c9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43cf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4271, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42ba, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42c0, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42c6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42cc, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42d2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42d8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4295, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x41f5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x421a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4243, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x423d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4249, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x424f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4255, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x425b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x42ed, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4312, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4335, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x433b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4341, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4347, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x434d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4353, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4461, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4486, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44a9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44af, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44b5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44bb, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44c1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x44c7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x43e5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x440a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x442d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4433, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4439, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x443f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4445, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x444b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50f9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5147, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5142, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x514e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5153, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5159, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x515f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5165, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5175, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5199, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51c3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51ca, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51cf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51d5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51db, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x51bd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5001, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5026, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x504a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5050, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5056, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x505c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5062, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5068, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x507d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50c6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50cc, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50d2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50d8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50de, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50e4, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x50ea, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4749, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x476d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4792, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4798, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x479e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47a4, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47aa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47b0, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4651, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4675, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4699, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46a0, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46a6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46ac, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46b2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46b8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47c5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x47e9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x480d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4814, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x481a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4820, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4826, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x482c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x46f1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4715, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x471c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4722, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4728, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x472e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4734, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4ba5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4bed, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4bf4, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4bfa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c00, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c06, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c0c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c12, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b29, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b71, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b77, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b7e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b83, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b8a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b8f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4b96, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c9d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4ce5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4cec, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4cf2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4cf8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4cfe, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4d04, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4d0a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c21, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c69, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c70, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c75, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c7c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c82, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c88, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x4c8e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3845, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3869, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x388d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3894, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3899, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x389f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x38a6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x38ab, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x38c1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x38e6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3909, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3910, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3916, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x391c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3921, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3928, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x393d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3961, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3986, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x398b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3992, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x3998, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x399e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x39a4, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x53e2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x542a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5430, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5436, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x543c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5442, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x5448, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x544e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x545e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54a5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54ab, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54b1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54b7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54be, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54c3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x54ca, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x67bd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x67c3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x6806, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x680c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x6811, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x6817, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x681d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0x6824, buffer);
            PS3.SetMemory(0x37ef299b, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef0357, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef03d3, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef7ff5, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef04cb, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef0547, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef05c3, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef063f, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37eefe7f, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37eefefb, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37eeff77, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37eefff3, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef006f, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef00eb, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef082f, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef08ab, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef0927, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef09a3, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef0d07, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef0d83, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef91df, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef0dff, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef11df, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef125b, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef12d7, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef1353, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37eefa23, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37eefa9f, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37eefb1b, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef15bf, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37ef163b, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            PS3.SetMemory(0x37eec1c7 + 0xc539, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc53e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc544, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc54a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc550, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc556, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc55c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc562, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc568, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc56e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc58c, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc5e1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc74f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc76e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc791, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc797, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc7c6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc80e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc815, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc856, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc862, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc868, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc8aa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc8e1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc8e6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc8fe, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc90a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc916, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc91d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc958, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc965, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc96a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc971, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc977, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc97d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc995, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc9a6, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc9c5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca18, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca1f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca2a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca31, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca36, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca6d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca8a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca91, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca96, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xca9d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaa3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaa8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaa9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaaf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcab5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcacd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcaf1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb08, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb15, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb38, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb3e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb45, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb5d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb62, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb7b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb98, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcb9e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcba5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbaa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbb1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbc2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbd5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbda, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbe1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbed, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcbf8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcc95, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xccc3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcd01, buffer);
            byte[] buffer2 = new byte[] { 11 };
            PS3.SetMemory(0x37eec1c7 + 0xcd20, buffer2);
            PS3.SetMemory(0x37eec1c7 + 0xcd5a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcdae, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcdcd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcdd8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcdf1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xce93, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xcfe9, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xd0cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xd13f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc011, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc03a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc041, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc047, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc04d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc053, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc088, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc0d1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc0e2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc0e8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc0ee, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc101, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc107, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc10d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc113, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc118, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc137, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc13d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc143, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc14e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc155, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc167, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc178, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc17e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc185, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc18a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1c1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1c7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1d2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1d8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc1e5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc20e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc227, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc232, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc251, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc257, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2a5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2aa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2b1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2b7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2bd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2c8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2ce, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2d5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2db, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2e1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2e7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2ed, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2f2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2f8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc2fe, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc305, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc30b, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc317, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc31d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc322, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc328, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc34d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc352, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc358, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc35e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc365, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc371, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc382, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc38e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3a7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3b2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3b8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3bf, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3c5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3ca, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3d1, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3d7, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3dd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3e3, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3e8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc3fa, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc401, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc407, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc40d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc412, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc418, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc41e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc425, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc42a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc431, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc436, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc44e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc455, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc461, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc467, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc472, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc478, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc47e, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc49d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4a2, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4a8, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4b5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4cd, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4e5, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc4ea, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc50f, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc515, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc51a, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc521, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc527, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc52d, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc532, buffer);
            PS3.SetMemory(0x37eec1c7 + 0xc538, buffer);

            byte[] Level = (new byte[] { 255, 255, 255, 255 });
            PS3.SetMemory(0x37EF8E65, Level);

            byte[] XPBytes = BitConverter.GetBytes(0);
            PS3.SetMemory(0x37EF8E63, XPBytes);

            byte[] points = BitConverter.GetBytes(0);
            PS3.SetMemory(0x37EF8E87, points);

            byte[] kills = BitConverter.GetBytes(0);
            PS3.SetMemory(0x37EF86D1, kills);

            byte[] deaths = BitConverter.GetBytes(0);
            PS3.SetMemory(0x37EF828D, deaths);

            byte[] wins = BitConverter.GetBytes(0);
            PS3.SetMemory(0x37EF8FB3, wins);

            byte[] losses = BitConverter.GetBytes(0);
            PS3.SetMemory(0x37EF8FB0, losses);

            byte[] Tokens = new byte[] { 0 };
            PS3.SetMemory(0x37EF80F7, Tokens);

            MessageBox.Show("You have deranked yourself.", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void unlockAllButton_Click(object sender, EventArgs e)
        {
            UnlockAll();
        }

        private void derankButton_Click(object sender, EventArgs e)
        {
            Derank();
        }

        private void maxTokensButton_Click(object sender, EventArgs e)
        {
            //Tokens offset 0x37EF80F7
            byte[] buffer = new byte[] { 255 };
            PS3.SetMemory(0x37EF80F7, buffer);
        }

        //Numaric add / subtract, Prestige
        private void prestigeMakeShiftNumaricAdd_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt16(prestigeMakeShiftNumaricValue.Text) < 100)
            {
                int Increment = (Convert.ToInt16(prestigeMakeShiftNumaricValue.Text));
                Increment++;
                prestigeMakeShiftNumaricValue.Text = (Increment.ToString());
            }
        }

        private void prestigeMakeShiftNumaricSubtract_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt16(prestigeMakeShiftNumaricValue.Text) > 0)
            {
                int Decrement = (Convert.ToInt16(prestigeMakeShiftNumaricValue.Text));
                Decrement--;
                prestigeMakeShiftNumaricValue.Text = (Decrement.ToString());
            }
        }

        private void flashPrestigeTimer_Tick(object sender, EventArgs e)
        {
            int flashPrestige = (new Random().Next(0, 12));
            byte[] bytes = (BitConverter.GetBytes(flashPrestige));
            PS3.SetMemory(0x37EF8E45, bytes);
        }

        private void flashPrestigeCheckBox_CheckedChanged(object sender)
        {
            if (flashPrestigeCheckBox.Checked)
            {
                flashPrestigeTimer.Start();
            }

            else
            {
                flashPrestigeTimer.Stop();
            }
        }

        private void setPrestigeButton_Click(object sender, EventArgs e)
        {
            //Prestige offset 0x37EF8E45
            byte[] bytes = (BitConverter.GetBytes(Convert.ToInt32(prestigeMakeShiftNumaricValue.Text)));
            PS3.SetMemory(0x37EF8E45, bytes);
        }

        private void setLevel55Button_Click(object sender, EventArgs e)
        {
            //Level offset 0x37EF8E65
            byte[] buffer = (new byte[] { 0, 1, 0, 0 });
            PS3.SetMemory(0x37EF8E65, buffer);
        }

        private void setLevel1Button_Click(object sender, EventArgs e)
        {
            byte[] buffer = (new byte[] { 255, 255, 255, 255 });
            PS3.SetMemory(0x37EF8E65, buffer);
        }

        //Set stats
        private void setPointsButton_Click(object sender, EventArgs e)
        {
            //Points offset 0x37EF8E87
            byte[] bytes = BitConverter.GetBytes(Convert.ToInt32(pointsTxT.Text));
            PS3.SetMemory(0x37EF8E87, bytes);
        }

        private void setKillsButton_Click(object sender, EventArgs e)
        {
            //Kills offset 0x37EF86D1
            byte[] bytes = BitConverter.GetBytes(Convert.ToInt32(killsTxT.Text));
            PS3.SetMemory(0x37EF86D1, bytes);
        }

        private void setDeathsButton_Click(object sender, EventArgs e)
        {
            //Deaths offset 0x37EF828D
            byte[] bytes = BitConverter.GetBytes(Convert.ToInt32(deathsTxT.Text));
            PS3.SetMemory(0x37EF828D, bytes);
        }

        private void setWinsButton_Click(object sender, EventArgs e)
        {
            //Wins offset 0x37EF8FB3
            byte[] bytes = BitConverter.GetBytes(Convert.ToInt32(winsTxT.Text));
            PS3.SetMemory(0x37EF8FB3, bytes);
        }

        private void setLossesButton_Click(object sender, EventArgs e)
        {
            //Losses offsets 0x37EF8FB0
            byte[] bytes = BitConverter.GetBytes(Convert.ToInt32(lossesTxT.Text));
            PS3.SetMemory(0x37EF8FB0, bytes);
        }

        //Class name method
        public void ClassName(int MyClassNum, string ClassName)
        {
            uint MyOffset = (0);

            //Set offset
            if (MyClassNum == 1) { MyOffset = (0x37F107B1); }
            if (MyClassNum == 2) { MyOffset = (0x37F107C1); }
            if (MyClassNum == 3) { MyOffset = (0x37F107D1); }
            if (MyClassNum == 4) { MyOffset = (0x37F107E1); }
            if (MyClassNum == 5) { MyOffset = (0x37F107F1); }
            if (MyClassNum == 6) { MyOffset = (0x37F10801); }
            if (MyClassNum == 7) { MyOffset = (0x37F10811); }
            if (MyClassNum == 8) { MyOffset = (0x37F10821); }
            if (MyClassNum == 9) { MyOffset = (0x37F10831); }
            if (MyClassNum == 10) { MyOffset = (0x37F10841); }

            //If the method was used correctly
            if (MyClassNum > 0 && MyClassNum < 11)
            {
                byte[] bytes = (Encoding.ASCII.GetBytes(ClassName + "\0"));
                PS3.SetMemory(MyOffset, bytes);
            }

            else
            {
                MessageBox.Show("There are only 10 classes, so you can not change class: \"" + MyClassNum + "\"", "Error");
            }
        }

        private void setClassesButton_Click(object sender, EventArgs e)
        {
            if (class1Checkbox.Checked) { ClassName(1, class1TxT.Text); }
            if (class2Checkbox.Checked) { ClassName(2, class2TxT.Text); }
            if (class3Checkbox.Checked) { ClassName(3, class3TxT.Text); }
            if (class4Checkbox.Checked) { ClassName(4, class4TxT.Text); }
            if (class5Checkbox.Checked) { ClassName(5, class5TxT.Text); }
            if (class6Checkbox.Checked) { ClassName(6, class6TxT.Text); }
            if (class7Checkbox.Checked) { ClassName(7, class7TxT.Text); }
            if (class8Checkbox.Checked) { ClassName(8, class8TxT.Text); }
            if (class9Checkbox.Checked) { ClassName(9, class9TxT.Text); }
            if (class10Checkbox.Checked) { ClassName(10, class10TxT.Text); }
        }

        private void openColorCodesForm_Click(object sender, EventArgs e)
        {
            this.WindowState = (FormWindowState.Minimized);
            ShowColorCodesForm ColorsPopup = (new ShowColorCodesForm());
            ColorsPopup.Show();
        }
        //End account tab

        /////////////////////////
        ///Non-Host / Host mods///
        ///////////////////////////
        ///
        //Non-Host mods
        private void endGameButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                RPC.CBuf_Addtext("cmd mr " + PS3.Extension.ReadInt32(0x1198860) + " 3 endround");
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        private void crashServerButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                RPC.CBuf_Addtext("cmd sl 9999999 3 END_HOST");
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        private void fpsCheckBox_CheckedChanged(object sender)
        {
            if (fpsCheckBox.Checked)
            {
                PS3.Extension.WriteBytes(0xCC5A8, new byte[] { 0x2C, 0x14, 0x00, 0x01 });
            }

            else
            {
                PS3.Extension.WriteBytes(0xCC5A8, new byte[] { 0x2C, 0x14, 0x00, 0x00 });
            }
        }

        private void uavCheckBox_CheckedChanged(object sender)
        {
            if (uavCheckBox.Checked)
            {
                PS3.Extension.WriteBytes(0xC8998, new byte[] { 0x60, 0x00, 0x00, 0x00 });
            }

            else
            {
                PS3.Extension.WriteBytes(0xC8998, new byte[] { 0x41, 0x82, 0x00, 0xAC });
            }
        }

        private void redBoxesCheckBox_CheckedChanged(object sender)
        {
            if (redBoxesCheckBox.Checked)
            {
                PS3.Extension.WriteBytes(0x58DF48, new byte[] { 0x60, 0x00, 0x00, 0x00 });
                PS3.Extension.WriteBytes(0x58DF10, new byte[] { 0x60, 0x00, 0x00, 0x00 });
            }

            else
            {
                PS3.Extension.WriteBytes(0x58DF48, new byte[] { 0x41, 0x82, 0x01, 0xD4 });
                PS3.Extension.WriteBytes(0x58DF10, new byte[] { 0x41, 0x82, 0x02, 0x0C });
            }
        }

        private void noSpreadCheckBox_CheckedChanged(object sender)
        {
            if (noSpreadCheckBox.Checked)
            {
                PS3.Extension.WriteBytes(0x76A174, new byte[] { 0x2C, 0x04, 0x00, 0x00 });
            }

            else
            {
                PS3.Extension.WriteBytes(0x76A174, new byte[] { 0x2C, 0x04, 0x00, 0x02 });
            }
        }

        private void noRecoilCheckBox_CheckedChanged(object sender)
        {
            if (noRecoilCheckBox.Checked)
            {
                PS3.Extension.WriteBytes(0x1908E8, new byte[] { 0x60, 0x00, 0x00, 0x00 });
            }

            else
            {
                PS3.Extension.WriteBytes(0x1908E8, new byte[] { 0x48, 0x5F, 0x75, 0x31 });
            }
        }

        private void forceHostBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RPC.CBuf_Addtext("set lobbyTimerStartInterval 10;");
            RPC.CBuf_Addtext("set LobbySearchSkip 1;");
            RPC.CBuf_Addtext("set lobbyTimerStatusVotingInterval 10;");
            RPC.CBuf_Addtext("set party_minPlayers 1;");

            while (forceHostLoopOn)
            {
                //Thread.Sleep(5000);
                RPC.CBuf_Addtext("set lobbyTimerStartInterval 10;");
                RPC.CBuf_Addtext("set LobbySearchSkip 1;");
                RPC.CBuf_Addtext("set lobbyTimerStatusVotingInterval 10;");
                RPC.CBuf_Addtext("set party_minPlayers 1;");
            }

            RPC.CBuf_Addtext("reset lobbyTimerStartInterval;");
            RPC.CBuf_Addtext("reset LobbySearchSkip;");
            RPC.CBuf_Addtext("reset lobbyTimerStatusVotingInterval;");
            RPC.CBuf_Addtext("reset party_minPlayers;");
        }

        private void forceHostCheckBox_CheckedChanged(object sender)
        {
            if (RPC_Enabled)
            {
                if (forceHostCheckBox.Checked)
                {
                    forceHostLoopOn = (true);

                    if (!forceHostBackgroundWorker.IsBusy)
                    {
                        forceHostBackgroundWorker.RunWorkerAsync();
                    }
                }

                else
                {
                    forceHostLoopOn = (false);
                }
            }

            else
            {
                forceHostCheckBox.Checked = (false);
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        /////////////
        //Host mods///
        ///////////////
        ///
        //Coded by: AssumingAgate & LordVirus (Most of the offsets and functions found by AssumingAgate)

        //Show clients
        private void fillListBoxWithNamesButton_Click(object sender, EventArgs e)
        {
            showClientNamesListBox.Items.Clear();
            for (uint i = 0; i < 12; i++)
            {
                string ClientName = (PS3.Extension.ReadString(G_Client + (G_Client_Size * i) + 0x5DB0));
                showClientNamesListBox.Items.Add(ClientName);
            }
        }

        //Select client
        private void showClientNamesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            clientMakeShiftNumaricValue.Text = (showClientNamesListBox.SelectedIndex.ToString());
        }

        //All clients
        private void allClientsCheckBox_CheckedChanged(object sender)
        {
            if (allClientsCheckBox.Checked)
            {
                AllClients = (true);
                clientMakeShiftNumaricAdd.Enabled = (false);
                clientMakeShiftNumaricSubtract.Enabled = (false);
                clientMakeShiftNumaricValue.Enabled = (false);
                MessageBox.Show("Sending large ammounts of data to the game may cause your system to freeze!", "Notice");
            }

            else
            {
                AllClients = (false);
                clientMakeShiftNumaricAdd.Enabled = (true);
                clientMakeShiftNumaricSubtract.Enabled = (true);
                clientMakeShiftNumaricValue.Enabled = (true);
            }
        }

        //Client add
        private void clientMakeShiftNumaricAdd_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt16(clientMakeShiftNumaricValue.Text) < 11)
            {
                int Increment = (Convert.ToInt16(clientMakeShiftNumaricValue.Text));
                Increment++;
                clientMakeShiftNumaricValue.Text = (Increment.ToString());
            }
        }

        //Client subtract
        private void clientMakeShiftNumaricSubtract_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt16(clientMakeShiftNumaricValue.Text) > 0)
            {
                int Decrement = (Convert.ToInt16(clientMakeShiftNumaricValue.Text));
                Decrement--;
                clientMakeShiftNumaricValue.Text = (Decrement.ToString());
            }
        }

        //RPC
        private void enableRPCButton_Click(object sender, EventArgs e)
        {
            RPC.Init();
            RPC_Enabled = (true);
        }

        //Ghetto UFO Mode
        private void ufoModeButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (AllClients)
                {
                    if (!setUFOonAC)
                    {
                        setUFOonAC = (true);

                        for (int x = 0; x < 12; x++)
                        {
                            setUFOon[x] = (true);
                        }

                        for (uint i = 0; i < 12; i++)
                        {
                            //Created by: ѕι∂яα∂ι from iHax.fr
                            PS3.SetMemory(G_Client + 0x5D03 + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x01 });
                            PS3.SetMemory(G_Client + 0x5DFE + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x00, 0xff });
                            Thread.Sleep(0x1388);
                            PS3.SetMemory(G_Client + 0x5D03 + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x02 });
                            RPC.iPrintlnBold((int)i, "^5Press ^2L1 ^5or ^2R1 ^5to move up, ^2Disable UFO mode to fall.");
                            RPC.iPrintln((int)i, "^7UFO mode: ^2Enabled");
                        }
                    }

                    else
                    {
                        setUFOonAC = (false);

                        for (int x = 0; x < 12; x++)
                        {
                            setUFOon[x] = (false);
                        }

                        for (uint i = 0; i < 12; i++)
                        {
                            PS3.SetMemory(G_Client + 0x5D03 + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x00 });
                            PS3.SetMemory(G_Client + 0x5DFE + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x80, 0x00 });
                            RPC.iPrintlnBold((int)i, "^6I hope that you found the right spot!");
                            RPC.iPrintln((int)i, "^7UFO mode: ^1Disabled");
                        }
                    }
                }

                else
                {
                    if (!setUFOon[(int)selected_client])
                    {
                        setUFOon[(int)selected_client] = (true);
                        //Created by: ѕι∂яα∂ι from iHax.fr
                        PS3.SetMemory(G_Client + 0x5D03 + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x01 });
                        PS3.SetMemory(G_Client + 0x5DFE + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x00, 0xff });
                        Thread.Sleep(0x1388);
                        PS3.SetMemory(G_Client + 0x5D03 + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x02 });
                        RPC.iPrintlnBold((int)selected_client, "^5Press ^2L1 ^5or ^2R1 ^5to move up, ^2Disable UFO mode to fall.");
                        RPC.iPrintln((int)selected_client, "^7UFO mode: ^2Enabled");
                    }

                    else
                    {
                        setUFOon[(int)selected_client] = (false);
                        PS3.SetMemory(G_Client + 0x5D03 + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x00 });
                        PS3.SetMemory(G_Client + 0x5DFE + (selected_client * G_Client_Size) * 0x61E0, new byte[] { 0x80, 0x00 });
                        RPC.iPrintlnBold((int)selected_client, "^6I hope that you found the right spot!");
                        RPC.iPrintln((int)selected_client, "^7UFO mode: ^1Disabled");
                    }
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        private void godModeAndInfAmmoBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (0 == 0)
            {
                if (setGodOnAC)
                {
                    for (uint i = 0; 0 < 12; i++)
                    {
                        //Send data to all clients
                        Thread.Sleep(500);
                        PS3.SetMemory(G_Client + 0x23 + (i * G_Client_Size), new byte[] { 0x05 });
                    }
                }

                else
                {
                    for (uint i = 0; i < 12; i++)
                    {
                        if (setGodOn[(int)i])
                        {
                            //Only send data to clients that have god mode enabled
                            Thread.Sleep(500);
                            PS3.SetMemory(G_Client + 0x23 + (i * G_Client_Size), new byte[] { 0x05 });
                        }
                    }
                }

                if (setInfiAmmoOnAC)
                {
                    for (uint i = 0; 0 < 12; i++)
                    {
                        //Send data to all clients
                        Thread.Sleep(500);
                        PS3.SetMemory(G_Client + 0x581 + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                        PS3.SetMemory(G_Client + 0x585 + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                        PS3.SetMemory(G_Client + 0x589 + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                        PS3.SetMemory(G_Client + 0x58D + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                        PS3.SetMemory(G_Client + 0x591 + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                    }
                }

                else
                {
                    for (uint i = 0; i < 12; i++)
                    {
                        if (setInfiAmmoOn[(int)i])
                        {
                            //Only send data to clients that have Infinite ammo enabled
                            Thread.Sleep(500);
                            PS3.SetMemory(G_Client + 0x581 + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                            PS3.SetMemory(G_Client + 0x585 + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                            PS3.SetMemory(G_Client + 0x589 + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                            PS3.SetMemory(G_Client + 0x58D + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                            PS3.SetMemory(G_Client + 0x591 + (i * G_Client_Size), new byte[] { 0xFF, 0xFF });
                        }
                    }
                }
            }
        }

        //God mode
        private void godModeButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (AllClients)
                {
                    if (!setGodOnAC)
                    {
                        setGodOnAC = (true);

                        for (int x = 0; x < 12; x++)
                        {
                            setGodOn[x] = (true);
                        }

                        if (!godModeAndInfAmmoBackgroundWorker.IsBusy)
                        {
                            godModeAndInfAmmoBackgroundWorker.RunWorkerAsync();
                        }

                        RPC.iPrintln(-1, "^7God mode: ^2Enabled");
                    }

                    else
                    {
                        setGodOnAC = (false);

                        for (int x = 0; x < 12; x++)
                        {
                            setGodOn[x] = (false);
                        }

                        for (uint i = 0; i < 12; i++)
                        {
                            PS3.SetMemory(G_Client + 0x23 + (i * G_Client_Size), new byte[] { 0x04 });
                        }

                        RPC.iPrintln(-1, "^7God mode: ^1Disabled");
                    }
                }

                else
                {
                    if (!setGodOn[(int)selected_client])
                    {
                        setGodOn[(int)selected_client] = (true);

                        if (!godModeAndInfAmmoBackgroundWorker.IsBusy)
                        {
                            godModeAndInfAmmoBackgroundWorker.RunWorkerAsync();
                        }

                        RPC.iPrintln((int)selected_client, "^7God mode: ^2Enabled");
                    }

                    else
                    {
                        setGodOn[(int)selected_client] = (false);
                        PS3.SetMemory(G_Client + 0x23 + (selected_client * G_Client_Size), new byte[] { 0x04 });
                        RPC.iPrintln((int)selected_client, "^7God mode: ^1Disabled");
                    }
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Infinite ammo
        private void infiniteAmmoButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (AllClients)
                {
                    if (!setInfiAmmoOnAC)
                    {
                        setInfiAmmoOnAC = (true);

                        for (int x = 0; x < 12; x++)
                        {
                            setInfiAmmoOn[x] = (true);
                        }

                        if (!godModeAndInfAmmoBackgroundWorker.IsBusy)
                        {
                            godModeAndInfAmmoBackgroundWorker.RunWorkerAsync();
                        }

                        RPC.iPrintln(-1, "^7Infinite Ammo: ^2Enabled");
                    }

                    else
                    {
                        setInfiAmmoOnAC = (false);

                        for (int x = 0; x < 12; x++)
                        {
                            setInfiAmmoOn[x] = (false);
                        }

                        for (uint i = 0; i < 12; i++)
                        {
                            PS3.SetMemory(G_Client + 0x581 + (i * G_Client_Size), new byte[] { 0x00, 0x00 });
                            PS3.SetMemory(G_Client + 0x585 + (i * G_Client_Size), new byte[] { 0x00, 0x00, 0x1E });
                            PS3.SetMemory(G_Client + 0x589 + (i * G_Client_Size), new byte[] { 0x00, 0x00, 0x01 });
                            PS3.SetMemory(G_Client + 0x58D + (i * G_Client_Size), new byte[] { 0x00, 0x00 });
                            PS3.SetMemory(G_Client + 0x591 + (i * G_Client_Size), new byte[] { 0x00, 0x00 });
                        }

                        RPC.iPrintln(-1, "^7Infinite Ammo: ^1Disabled");
                    }
                }

                else
                {
                    if (!setInfiAmmoOn[(int)selected_client])
                    {
                        setInfiAmmoOn[(int)selected_client] = (true);

                        if (!godModeAndInfAmmoBackgroundWorker.IsBusy)
                        {
                            godModeAndInfAmmoBackgroundWorker.RunWorkerAsync();
                        }

                        RPC.iPrintln((int)selected_client, "^7Infinite Ammo: ^2Enabled");
                    }

                    else
                    {
                        setInfiAmmoOn[(int)selected_client] = (false);
                        PS3.SetMemory(G_Client + 0x581 + (selected_client * G_Client_Size), new byte[] { 0x00, 0x00 });
                        PS3.SetMemory(G_Client + 0x585 + (selected_client * G_Client_Size), new byte[] { 0x00, 0x00, 0x1E });
                        PS3.SetMemory(G_Client + 0x589 + (selected_client * G_Client_Size), new byte[] { 0x00, 0x00, 0x01 });
                        PS3.SetMemory(G_Client + 0x58D + (selected_client * G_Client_Size), new byte[] { 0x00, 0x00 });
                        PS3.SetMemory(G_Client + 0x591 + (selected_client * G_Client_Size), new byte[] { 0x00, 0x00 });
                        RPC.iPrintln((int)selected_client, "^7Infinite Ammo: ^1Disabled");
                    }
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Speed scale
        private void superSpeedBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (0 == 0)
            {
                if (setSpeedAC != 1)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        SetMoveSpeedScale(i, setSpeedAC);
                    }
                }

                else
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (setSpeed[i] != 1)
                        {
                            SetMoveSpeedScale(i, setSpeed[i]);
                        }
                    }
                }
            }
        }

        void SetMoveSpeedScale(int Client, float Speed)
        {
            int Ent = 0x017FF620 + (Client * 0x350);
            uint Ents = (uint)Ent + 0x1D4;
            int PlayerState = PS3.Extension.ReadInt32(Ents);
            PS3.Extension.WriteFloat((uint)PlayerState + 0x5DF8, Speed);
        }

        private void speedButton_Click(object sender, EventArgs e)
        {
            //PS3.SetMemory(G_Client + 0x5DF8 + selected_client, new byte[] { 0x40 });
            //PS3.SetMemory(G_Client + 0x5DF8 + selected_client, new byte[] { 0x3F });
            if (RPC_Enabled)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (AllClients)
                {
                    if (setSpeedAC < 6 && setSpeedAC > 0)
                    {
                        setSpeedAC = (++setSpeedAC);

                        for (int x = 0; x < 12; x++)
                        {
                            setSpeed[x] = (setSpeedAC);
                        }

                        RPC.iPrintln(-1, "^7Super Speed: ^2x" + setSpeedAC);
                    }

                    else
                    {
                        if (setSpeedAC == 6)
                        {
                            setSpeedAC = (0);

                            for (int x = 0; x < 12; x++)
                            {
                                setSpeed[x] = (setSpeedAC);
                            }

                            RPC.iPrintln(-1, "^7Super Speed: ^5Frozen");
                        }

                        else
                        {
                            setSpeedAC = (1);

                            for (int x = 0; x < 12; x++)
                            {
                                setSpeed[x] = (setSpeedAC);
                            }

                            for (int i = 0; i < 12; i++)
                            {
                                SetMoveSpeedScale(i, setSpeedAC);
                            }

                            RPC.iPrintln(-1, "^7Super Speed: ^1Disabled");
                        }
                    }

                    if (!superSpeedBackgroundWorker.IsBusy)
                    {
                        superSpeedBackgroundWorker.RunWorkerAsync();
                    }
                }

                else
                {
                    if (setSpeed[(int)selected_client] < 6 && setSpeed[(int)selected_client] > 0)
                    {
                        setSpeed[(int)selected_client] = (++setSpeed[(int)selected_client]);
                        RPC.iPrintln((int)selected_client, "^7Super Speed: ^2x" + setSpeed[(int)selected_client]);
                    }

                    else
                    {
                        if (setSpeed[(int)selected_client] == 6)
                        {
                            setSpeed[(int)selected_client] = (0);
                            RPC.iPrintln((int)selected_client, "^7Super Speed: ^5Frozen");
                        }

                        else
                        {
                            setSpeed[(int)selected_client] = (1);
                            SetMoveSpeedScale((int)selected_client, setSpeed[(int)selected_client]);
                            RPC.iPrintln((int)selected_client, "^7Super Speed: ^1Disabled");
                        }
                    }

                    if (!superSpeedBackgroundWorker.IsBusy)
                    {
                        superSpeedBackgroundWorker.RunWorkerAsync();
                    }
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Third person
        private void thirdPersonButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (AllClients)
                {
                    if (!setThirdPersonOnAC)
                    {
                        setThirdPersonOnAC = (true);

                        for (int x = 0; x < 12; x++)
                        {
                            setThirdPersonOn[x] = (true);
                        }

                        for (uint i = 0; i < 12; i++)
                        {
                            PS3.SetMemory(G_Client + 0x12D + i, new byte[] { 0x01 });
                        }

                        RPC.iPrintln(-1, "^7Third Person: ^2Enabled");
                    }

                    else
                    {
                        setThirdPersonOnAC = (false);

                        for (int x = 0; x < 12; x++)
                        {
                            setThirdPersonOn[x] = (false);
                        }

                        for (uint i = 0; i < 12; i++)
                        {
                            PS3.SetMemory(G_Client + 0x12D + i, new byte[] { 0x00 });
                        }

                        RPC.iPrintln(-1, "^7Third Person: ^1Disabled");
                    }
                }

                else
                {
                    if (!setThirdPersonOn[(int)selected_client])
                    {
                        setThirdPersonOn[(int)selected_client] = (true);
                        PS3.SetMemory(G_Client + 0x12D + (selected_client * G_Client_Size), new byte[] { 0x01 });
                        RPC.iPrintln((int)selected_client, "^7Third Person: ^2Enabled");
                    }

                    else
                    {
                        setThirdPersonOn[(int)selected_client] = (false);
                        PS3.SetMemory(G_Client + 0x12D + (selected_client * G_Client_Size), new byte[] { 0x00 });
                        RPC.iPrintln((int)selected_client, "^7Third Person: ^1Disabled");
                    }
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Jet pack
        private void jetPackBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (0 == 0)
            {
                if (setJetPackOnAC)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        int Ent = 0x017FF620 + (i * 0x350);
                        uint Ents = (uint)Ent + 0x1D4;
                        int PlayerState = PS3.Extension.ReadInt32(Ents);
                        PS3.Extension.WriteFloat((uint)PlayerState + 0x730, 5);
                    }
                }

                else
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (setJetPackOn[i])
                        {
                            int Ent = 0x017FF620 + (i * 0x350);
                            uint Ents = (uint)Ent + 0x1D4;
                            int PlayerState = PS3.Extension.ReadInt32(Ents);
                            PS3.Extension.WriteFloat((uint)PlayerState + 0x730, 5);
                        }
                    }
                }
            }
        }

        private void jetPackButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (AllClients)
                {
                    if (!setJetPackOnAC)
                    {
                        setInfiAmmoOnAC = (true);

                        for (int x = 0; x < 12; x++)
                        {
                            setJetPackOn[x] = (true);
                        }

                        if (!jetPackBackgroundWorker.IsBusy)
                        {
                            jetPackBackgroundWorker.RunWorkerAsync();
                        }

                        RPC.iPrintln(-1, "^7Jetpack: ^2Enabled");
                    }

                    else
                    {
                        setInfiAmmoOnAC = (false);

                        for (int x = 0; x < 12; x++)
                        {
                            setJetPackOn[x] = (false);
                        }

                        RPC.iPrintln(-1, "^7Jetpack: ^1Disabled");
                    }
                }

                else
                {
                    if (!setJetPackOn[(int)selected_client])
                    {
                        setJetPackOn[(int)selected_client] = (true);

                        if (!jetPackBackgroundWorker.IsBusy)
                        {
                            jetPackBackgroundWorker.RunWorkerAsync();
                        }

                        RPC.iPrintln((int)selected_client, "^7Jetpack: ^2Enabled");
                    }

                    else
                    {
                        setJetPackOn[(int)selected_client] = (false);
                        RPC.iPrintln((int)selected_client, "^7Jetpack: ^1Disabled");
                    }
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Remove barrieors
        private void removeBarrieorsButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                MessageBox.Show("This may take a minute... Be patient.", "Notice.");
                int Ent = 0x017FF620;
                for (int i = 0; i < 1024; i++)
                {
                    int eType = Ent + (0x350 * i) + 0x8;
                    int EntType = PS3.Extension.ReadInt32((uint)eType);
                    if (EntType == 0x21)
                    {
                        int EntOrigin = Ent + (0x350 * i) + 0x20;
                        PS3.Extension.WriteFloat((uint)EntOrigin, 9999999);
                        EntOrigin = Ent + (0x350 * i) + 0x1B8;
                        PS3.Extension.WriteFloat((uint)EntOrigin, 9999999);
                    }
                }
                RPC.iPrintln(-1, "^7Death barriors: ^1Disabled");
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Unlock MP trophies
        private void unlockAchivementsButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                string[] Achievements = new string[]
                {
                  "# MP_PRESTIGE10", "# MP_ALLCAREER", "# MP_ALLGAMEMODE",
                  "# MP_ALLBOOTCAMP", "# MP_ALLSPECIALISTS", "# MP_PRESTIGE1",
                  "# MP_COMMANDER", "# MP_10MULTIKILL", "# MP_10MEDALS", "# MP_LEVEL10"
                };

                if (AllClients)
                {
                    RPC.iPrintlnBold(-1, "^6Unlocking Multiplayer achivements.");

                    foreach (var SendAchivevment in Achievements)
                    {
                        RPC.SV_GameSendServerCommand(-1, SendAchivevment);
                        Thread.Sleep(25);
                        RPC.SV_GameSendServerCommand(-1, SendAchivevment);
                    }
                }

                else
                {
                    RPC.iPrintlnBold((int)selected_client, "^6Unlocking Multiplayer achivements.");

                    foreach (var SendAchivevment in Achievements)
                    {
                        RPC.SV_GameSendServerCommand((int)selected_client, SendAchivevment);
                        Thread.Sleep(25);
                        RPC.SV_GameSendServerCommand((int)selected_client, SendAchivevment);
                    }
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //AddBot
        private void addBotButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                showClientNamesListBox.Items.Clear();
                int NumOfClients = (0);
                for (uint i = 0; i < 12; i++)
                {
                    string ClientName = (PS3.Extension.ReadString(G_Client + (G_Client_Size * i) + 0x5DB0));
                    showClientNamesListBox.Items.Add(ClientName);
                    if (ClientName != "")
                    {
                        NumOfClients++;
                    }
                }

                if (NumOfClients < 12)
                {
                    Thread.Sleep(1000);
                    RPC.Call(0x005EBEB0);
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Kick client
        private void kickClientButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (AllClients)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        RPC.CBuf_Addtext("clientkick " + i);
                    }
                }

                else
                {
                    RPC.CBuf_Addtext("clientkick " + selected_client);
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Kill client
        private void killClientButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (AllClients)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        Thread.Sleep(1000);
                        PS3.SetMemory(0x00303748, new byte[] { 0x38, 0x60, 0x00, 0x14, 0x1C, 0x63, 0x03, 0x50, });
                        PS3.SetMemory(0x0030374B, new byte[] { Convert.ToByte(i) });
                        RPC.Call(0x0030372C, 1);
                    }
                }

                else
                {
                    Thread.Sleep(1000);
                    PS3.SetMemory(0x00303748, new byte[] { 0x38, 0x60, 0x00, 0x14, 0x1C, 0x63, 0x03, 0x50, });
                    PS3.SetMemory(0x0030374B, new byte[] { Convert.ToByte(selected_client) });
                    RPC.Call(0x0030372C, 1);
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Level 55 / Prestige master
        private void rankLobbyButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                MessageBox.Show("Exploit codded by: AssumingAgate", "Black Ops 3: Remote Code Execution");
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));

                if (AllClients)
                {
                    /*
                    var name1 = 0x01F3CFF4 + 0 - 0x36C85AC4;
                    var names1 = Convert.ToDecimal(name1);
                    RPC.SV_GameSendServerCommand(-1, "i " + names1 / 4 + " " + Convert.ToDecimal(0x5E32424F));

                    var name2 = 0x01F3CFF4 + 4 - 0x36C85AC4;
                    var names2 = Convert.ToDecimal(name2);
                    RPC.SV_GameSendServerCommand(-1, "i " + names2 / 4 + " " + Convert.ToDecimal(0x33205072));

                    var name3 = 0x01F3CFF4 + 8 - 0x36C85AC4;
                    var names3 = Convert.ToDecimal(name3);
                    RPC.SV_GameSendServerCommand(-1, "i " + names3 / 4 + " " + Convert.ToDecimal(0x65646174));

                    var name4 = 0x01F3CFF4 + 12 - 0x36C85AC4;
                    var names4 = Convert.ToDecimal(name4);
                    RPC.SV_GameSendServerCommand(-1, "i " + names4 / 4 + " " + Convert.ToDecimal(0x6F722120));

                    var name5 = 0x01F3CFF4 + 16 - 0x36C85AC4;
                    var names5 = Convert.ToDecimal(name5);
                    RPC.SV_GameSendServerCommand(-1, "i " + names5 / 4 + " " + Convert.ToDecimal(0x00000000));
                    */
                    var clantag1 = 0x37EF8FD8 + 0 - 0x36C85AC4;
                    var clantags1 = Convert.ToDecimal(clantag1);
                    RPC.SV_GameSendServerCommand(-1, "i " + clantags1 / 4 + " " + Convert.ToDecimal(0x005E425E));

                    var clantag2 = 0x37EF8FD8 + 4 - 0x36C85AC4;
                    var clantags2 = Convert.ToDecimal(clantag2);
                    RPC.SV_GameSendServerCommand(-1, "i " + clantags2 / 4 + " " + Convert.ToDecimal(0x00000000));
                    //
                    var rank1 = 0x37EF8E60 - 0x36C85AC4;
                    var ranks1 = Convert.ToDecimal(rank1);
                    RPC.SV_GameSendServerCommand(-1, "i " + ranks1 / 4 + " " + Convert.ToDecimal(0x39393939));

                    var rank2 = 0x37EF8E64 - 0x36C85AC4;
                    var ranks2 = Convert.ToDecimal(rank2);
                    RPC.SV_GameSendServerCommand(-1, "i " + ranks2 / 4 + " " + Convert.ToDecimal(0x39393939));
                    //
                    var prestige1 = 0x37EF8E44 - 0x36C85AC4;
                    var prestiges1 = Convert.ToDecimal(prestige1);
                    RPC.SV_GameSendServerCommand(-1, "i " + prestiges1 / 4 + " " + Convert.ToDecimal(0x000B0000));
                    //
                    RPC.iPrintlnBold(-1, "^5You have been given: ^2Level 55 & Prestige Master");
                }

                else
                {
                    /*
                    var name1 = 0x01F3CFF4 + 0 - 0x36C85AC4;
                    var names1 = Convert.ToDecimal(name1);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names1 / 4 + " " + Convert.ToDecimal(0x5E32424F));

                    var name2 = 0x01F3CFF4 + 4 - 0x36C85AC4;
                    var names2 = Convert.ToDecimal(name2);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names2 / 4 + " " + Convert.ToDecimal(0x33205072));

                    var name3 = 0x01F3CFF4 + 8 - 0x36C85AC4;
                    var names3 = Convert.ToDecimal(name3);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names3 / 4 + " " + Convert.ToDecimal(0x65646174));

                    var name4 = 0x01F3CFF4 + 12 - 0x36C85AC4;
                    var names4 = Convert.ToDecimal(name4);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names4 / 4 + " " + Convert.ToDecimal(0x6F722120));

                    var name5 = 0x01F3CFF4 + 16 - 0x36C85AC4;
                    var names5 = Convert.ToDecimal(name5);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names5 / 4 + " " + Convert.ToDecimal(0x00000000));
                    */
                    var clantag1 = 0x37EF8FD8 + 0 - 0x36C85AC4;
                    var clantags1 = Convert.ToDecimal(clantag1);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + clantags1 / 4 + " " + Convert.ToDecimal(0x005E425E));

                    var clantag2 = 0x37EF8FD8 + 4 - 0x36C85AC4;
                    var clantags2 = Convert.ToDecimal(clantag2);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + clantags2 / 4 + " " + Convert.ToDecimal(0x00000000));
                    //
                    var rank1 = 0x37EF8E60 - 0x36C85AC4;
                    var ranks1 = Convert.ToDecimal(rank1);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + ranks1 / 4 + " " + Convert.ToDecimal(0x39393939));

                    var rank2 = 0x37EF8E64 - 0x36C85AC4;
                    var ranks2 = Convert.ToDecimal(rank2);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + ranks2 / 4 + " " + Convert.ToDecimal(0x39393939));
                    //
                    var prestige1 = 0x37EF8E44 - 0x36C85AC4;
                    var prestiges1 = Convert.ToDecimal(prestige1);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + prestiges1 / 4 + " " + Convert.ToDecimal(0x000B0000));
                    //
                    RPC.iPrintlnBold((int)selected_client, "^5You have been given: ^2Level 55 & Prestige Master");
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //Derank
        private void derankLobbyButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                // Remote Memory Editing stats RME.
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));

                if (AllClients)
                {
                    /* If I recall correctly the name stats only changed if the user was running CFW.
                    var name1 = 0x01F3CFF4 + 0 - 0x36C85AC4;
                    var names1 = Convert.ToDecimal(name1);
                    RPC.SV_GameSendServerCommand(-1, "i " + names1 / 4 + " " + Convert.ToDecimal(0x5E314920));

                    var name2 = 0x01F3CFF4 + 4 - 0x36C85AC4;
                    var names2 = Convert.ToDecimal(name2);
                    RPC.SV_GameSendServerCommand(-1, "i " + names2 / 4 + " " + Convert.ToDecimal(0x57617320));

                    var name3 = 0x01F3CFF4 + 8 - 0x36C85AC4;
                    var names3 = Convert.ToDecimal(name3);
                    RPC.SV_GameSendServerCommand(-1, "i " + names3 / 4 + " " + Convert.ToDecimal(0x44657261));

                    var name4 = 0x01F3CFF4 + 12 - 0x36C85AC4;
                    var names4 = Convert.ToDecimal(name4);
                    RPC.SV_GameSendServerCommand(-1, "i " + names4 / 4 + " " + Convert.ToDecimal(0x6E6B6564));

                    var name5 = 0x01F3CFF4 + 16 - 0x36C85AC4;
                    var names5 = Convert.ToDecimal(name5);
                    RPC.SV_GameSendServerCommand(-1, "i " + names5 / 4 + " " + Convert.ToDecimal(0x21000000));
                    */

                    var clantag1 = 0x37EF8FD8 + 0 - 0x36C85AC4;
                    var clantags1 = Convert.ToDecimal(clantag1);
                    RPC.SV_GameSendServerCommand(-1, "i " + clantags1 / 4 + " " + Convert.ToDecimal(0x007B5E31));

                    var clantag2 = 0x37EF8FD8 + 4 - 0x36C85AC4;
                    var clantags2 = Convert.ToDecimal(clantag2);
                    RPC.SV_GameSendServerCommand(-1, "i " + clantags2 / 4 + " " + Convert.ToDecimal(0x7D000000));
                    //
                    var rank1 = 0x37EF8E60 - 0x36C85AC4;
                    var ranks1 = Convert.ToDecimal(rank1);
                    RPC.SV_GameSendServerCommand(-1, "i " + ranks1 / 4 + " " + Convert.ToDecimal(0x00000000));

                    var rank2 = 0x37EF8E64 - 0x36C85AC4;
                    var ranks2 = Convert.ToDecimal(rank2);
                    RPC.SV_GameSendServerCommand(-1, "i " + ranks2 / 4 + " " + Convert.ToDecimal(0x00000000));
                    //
                    var prestige1 = 0x37EF8E44 - 0x36C85AC4;
                    var prestiges1 = Convert.ToDecimal(prestige1);
                    RPC.SV_GameSendServerCommand(-1, "i " + prestiges1 / 4 + " " + Convert.ToDecimal(0x00000000));
                    //
                    RPC.iPrintlnBold(-1, "^5You have been: ^1Deranked!");
                }

                else
                {
                    /*
                    var name1 = 0x01F3CFF4 + 0 - 0x36C85AC4;
                    var names1 = Convert.ToDecimal(name1);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names1 / 4 + " " + Convert.ToDecimal(0x5E314920));

                    var name2 = 0x01F3CFF4 + 4 - 0x36C85AC4;
                    var names2 = Convert.ToDecimal(name2);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names2 / 4 + " " + Convert.ToDecimal(0x57617320));

                    var name3 = 0x01F3CFF4 + 8 - 0x36C85AC4;
                    var names3 = Convert.ToDecimal(name3);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names3 / 4 + " " + Convert.ToDecimal(0x44657261));

                    var name4 = 0x01F3CFF4 + 12 - 0x36C85AC4;
                    var names4 = Convert.ToDecimal(name4);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names4 / 4 + " " + Convert.ToDecimal(0x6E6B6564));

                    var name5 = 0x01F3CFF4 + 16 - 0x36C85AC4;
                    var names5 = Convert.ToDecimal(name5);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + names5 / 4 + " " + Convert.ToDecimal(0x21000000));
                    */
                    var clantag1 = 0x37EF8FD8 + 0 - 0x36C85AC4;
                    var clantags1 = Convert.ToDecimal(clantag1);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + clantags1 / 4 + " " + Convert.ToDecimal(0x007B5E31));

                    var clantag2 = 0x37EF8FD8 + 4 - 0x36C85AC4;
                    var clantags2 = Convert.ToDecimal(clantag2);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + clantags2 / 4 + " " + Convert.ToDecimal(0x7D000000));
                    //
                    var rank1 = 0x37EF8E60 - 0x36C85AC4;
                    var ranks1 = Convert.ToDecimal(rank1);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + ranks1 / 4 + " " + Convert.ToDecimal(0x00000000));

                    var rank2 = 0x37EF8E64 - 0x36C85AC4;
                    var ranks2 = Convert.ToDecimal(rank2);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + ranks2 / 4 + " " + Convert.ToDecimal(0x00000000));
                    //
                    var prestige1 = 0x37EF8E44 - 0x36C85AC4;
                    var prestiges1 = Convert.ToDecimal(prestige1);
                    RPC.SV_GameSendServerCommand((int)selected_client, "i " + prestiges1 / 4 + " " + Convert.ToDecimal(0x00000000));
                    //
                    RPC.iPrintlnBold((int)selected_client, "^5You have been: ^1Deranked!");
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        //iPrintlnBold
        private void iPrintlnBoldButton_Click(object sender, EventArgs e)
        {
            selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
            if (RPC_Enabled)
            {
                if (AllClients)
                {
                    RPC.iPrintlnBold(-1, iPrintlnBoldTxT.Text);
                }

                else
                {
                    RPC.iPrintlnBold((int)selected_client, iPrintlnBoldTxT.Text);
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        private void iPrintlnBoldTxT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (RPC_Enabled)
                {
                    if (AllClients)
                    {
                        RPC.iPrintlnBold(-1, iPrintlnBoldTxT.Text);
                    }

                    else
                    {
                        RPC.iPrintlnBold((int)selected_client, iPrintlnBoldTxT.Text);
                    }
                }

                else
                {
                    MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
                }
            }
        }

        //iPrintln
        private void iPrintlnButton_Click(object sender, EventArgs e)
        {
            selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
            if (RPC_Enabled)
            {
                if (AllClients)
                {
                    RPC.iPrintln(-1, iPrintlnTxT.Text);
                }

                else
                {
                    RPC.iPrintln((int)selected_client, iPrintlnTxT.Text);
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        private void iPrintlnTxT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (RPC_Enabled)
                {
                    if (AllClients)
                    {
                        RPC.iPrintln(-1, iPrintlnTxT.Text);
                    }

                    else
                    {
                        RPC.iPrintln((int)selected_client, iPrintlnTxT.Text);
                    }
                }

                else
                {
                    MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
                }
            }
        }

        //Cbuf_AddText
        private void CBuf_AddtextButton_Click(object sender, EventArgs e)
        {
            if (RPC_Enabled)
            {
                RPC.CBuf_Addtext(CBuf_AddtextTxT.Text);
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }


        private void CBuf_AddtextTxT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (RPC_Enabled)
                {
                    RPC.CBuf_Addtext(CBuf_AddtextTxT.Text);
                }

                else
                {
                    MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
                }
            }
        }

        //Send Game Server Command
        private void SV_GameSendServerCommandButton_Click(object sender, EventArgs e)
        {
            selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
            if (RPC_Enabled)
            {
                if (AllClients)
                {
                    RPC.SV_GameSendServerCommand(-1, SV_GameSendServerCommandTxT.Text);
                }

                else
                {
                    RPC.SV_GameSendServerCommand((int)selected_client, SV_GameSendServerCommandTxT.Text);
                }
            }

            else
            {
                MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
            }
        }

        private void SV_GameSendServerCommandTxT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                selected_client = (Convert.ToUInt16(clientMakeShiftNumaricValue.Text));
                if (RPC_Enabled)
                {
                    if (AllClients)
                    {
                        RPC.SV_GameSendServerCommand(-1, SV_GameSendServerCommandTxT.Text);
                    }

                    else
                    {
                        RPC.SV_GameSendServerCommand((int)selected_client, SV_GameSendServerCommandTxT.Text);
                    }
                }

                else
                {
                    MessageBox.Show("You must enable RPC in order to use this command!", "Black Ops 3: Remote Procedure Call");
                }
            }
        }
        //End mods tab


        //////////////////////////////////
        ///Ip Grabber by: Mayhem Modding///
        ////////////////////////////////////
        ///
        bool canGrab = false;
        bool loadInfo = false;
        List<string> addName = new List<string>();
        List<string> addPublicIp = new List<string>();
        List<string> addPort = new List<string>();
        List<string> addLocalIp = new List<string>();
        uint startOfs = 0x020bb927;

        private static string convertIp(byte[] ip)
        {
            return string.Format("{0}.{1}.{2}.{3}", ip[0], ip[1], ip[2], ip[3]);
        }

        private static string convertPort(byte[] ip)
        {
            return string.Format("{0}{1}{2}", ip[0], ip[1], ip[2]);
        }

        public static List<int> IndexOfSequence(byte[] buffer, byte[] pattern, int startIndex)
        {
            List<int> positions = new List<int>();
            int i = Array.IndexOf<byte>(buffer, pattern[0], startIndex);
            while (i >= 0 && i <= buffer.Length - pattern.Length)
            {
                byte[] segment = new byte[pattern.Length];
                Buffer.BlockCopy(buffer, i, segment, 0, pattern.Length);
                if (segment.SequenceEqual<byte>(pattern))
                    positions.Add(i);
                i = Array.IndexOf<byte>(buffer, pattern[0], i + pattern.Length);
            }
            return positions;
        }

        private void grabInfo()
        {
            if (canGrab)
            {
                canGrab = false;
                int[] nameResults = IndexOfSequence(PS3.Extension.ReadBytes(startOfs, 20000), new byte[] { 0x00, 0x24 }, 0).ToArray();
                addName.Clear();
                addPublicIp.Clear();
                addPort.Clear();
                addLocalIp.Clear();
                for (int i = 0; i < nameResults.Length; i++)
                {
                    addName.Add(PS3.Extension.ReadString(startOfs + 2 + (uint)nameResults[i]));
                    addPublicIp.Add(convertIp(PS3.Extension.ReadBytes(startOfs + (uint)nameResults[i] - 0x1E, 4)));
                    addPort.Add(convertPort(PS3.Extension.ReadBytes(startOfs + (uint)nameResults[i] - 0x1A, 3)));
                    addLocalIp.Add(convertIp(PS3.Extension.ReadBytes(startOfs + (uint)nameResults[i] - 0x3C, 4)));
                }
                loadInfo = true;
            }
        }

        private void ipGrabberBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            PS3.ConnectTarget(0);
            while (0 == 0)
            {
                grabInfo();
            }
        }

        private void grabIP_Button_Click(object sender, EventArgs e)
        {
            if (!ipGrabberBackgroundWorker.IsBusy)
                ipGrabberBackgroundWorker.RunWorkerAsync();
            canGrab = true;
        }

        private void copyInfoButton_Click(object sender, EventArgs e)
        {
            List<string> addInfo = new List<string>();
            for (int i = 0; i < ipGrabberDataGridView.RowCount; i++)
                addInfo.Add("Name: " + ipGrabberDataGridView[0, i].Value + " | Public IP: " + ipGrabberDataGridView[1, i].Value + " | Port: " + ipGrabberDataGridView[2, i].Value + " | Local IP: " + ipGrabberDataGridView[3, i].Value);
            if (addInfo.Count != 0)
                Clipboard.SetText(String.Join("\n", addInfo.ToArray()));
        }

        private void grabIPsTimer_Tick(object sender, EventArgs e)
        {
            if (loadInfo)
            {
                loadInfo = false;
                if (addName.Count == addPublicIp.Count && addName.Count == addPort.Count && addName.Count == addLocalIp.Count)
                    try
                    {
                        ipGrabberDataGridView.Rows.Clear();
                        for (int i = 0; i < addName.Count; i++)
                            ipGrabberDataGridView.Rows.Add(addName[i], addPublicIp[i], addPort[i], addLocalIp[i]);
                    }

                    catch
                    {

                    }
            }
        }
        //End IP Grabber Tab
    }
}
