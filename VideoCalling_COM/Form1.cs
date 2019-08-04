using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Contrinex.RFID;

namespace VideoCalling_COM
{
    public partial class Form1 : Form
    {
        ConIDHF BusRfidHF;
        IniFile myConfig = new IniFile();
        public Form1()
        {
            InitializeComponent();
            comboBox_Name.SelectedIndex = 0;
            comboBox_Baud.SelectedIndex = 1;
            comboBox_DataSize.SelectedIndex = 1;
            comboBox_Parity.SelectedIndex = 0;
            comboBox_Handshake.SelectedIndex = 0;
            comboBox_Mode.SelectedIndex = 0;
            btn_Status.Enabled = false;
            btn_Read.Enabled = false;
            btn_Write.Enabled = false;
        }

        private void tsBtn_Main_Click(object sender, EventArgs e)
        {
            panel_Binding.Dock = DockStyle.None;
            panel_Binding.Visible = false;
            panel_Main.Visible = true;
            panel_Main.Dock = DockStyle.Fill;
        }

        private void tsBtn_Binding_Click(object sender, EventArgs e)
        {
            panel_Main.Dock = DockStyle.None;
            panel_Main.Visible = false;
            panel_Binding.Visible = true;
            panel_Binding.Dock = DockStyle.Fill;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tsBtn_Main_Click(sender, e);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BusRfidHF != null)//
            {
                BusRfidHF.Dispose();
            }
        }

        private void Btn_Init_Click(object sender, EventArgs e)
        {
            tBx_Status.Clear();
            tBx_DataRead.Clear();
            tBx_Data.Clear();
            btn_Init.BackColor = Color.Transparent;
            btn_Read.BackColor = Color.Transparent;
            btn_Write.BackColor = Color.Transparent;
            btn_Status.BackColor = Color.Transparent;
            if (BusRfidHF != null)// Est-ce que l'objet existe en m閙oire
            {
                BusRfidHF.Dispose();//Destruction de l'objet
            }
            BusRfidHF = new ConIDHF(); // cr閍tion de l'Objet BusRfidHF de type ConIDHF
            bool InitOK = BusRfidHF.Init(250, 115200, (int)(nUD_ComPort.Value));
            if (InitOK == true)
            {
                ConIDHF.Info InfoRWM;//
                InfoRWM = BusRfidHF.RWMInfo((byte)(nUD_RWMNb.Value));//
                lbl_RwmType.ForeColor = Color.Black;
                if (InfoRWM.AckCode == ConIDHF.Acknldg.OperationSuccessful)//
                {
                    btn_Status.Enabled = true;
                    btn_Read.Enabled = true;
                    btn_Write.Enabled = true;
                    btn_Init.BackColor = Color.GreenYellow;
                    if (InfoRWM.Type == (byte)(RWM.Type.RWM_M30))//
                    {
                        lbl_RwmType.Text = "RWM M30";
                    }
                    else
                    {
                        lbl_RwmType.Text = "RWM M18";
                    }
                }
                else
                {
                    btn_Init.BackColor = Color.Red;
                    lbl_RwmType.ForeColor = Color.Red;
                    lbl_RwmType.Text = "Wrong RWM number";
                    btn_Status.Enabled = false;
                    btn_Read.Enabled = false;
                    btn_Write.Enabled = false;
                }
            }
            else
            {
                btn_Init.BackColor = Color.Red;
                btn_Status.Enabled = false;
                btn_Read.Enabled = false;
                btn_Write.Enabled = false;
            }
        }

        private void Btn_Status_Click(object sender, EventArgs e)
        {
            tBx_Status.Clear();
            tBx_DataRead.Clear();
            tBx_Data.Clear();
            btn_Read.BackColor = Color.Transparent;
            btn_Write.BackColor = Color.Transparent;
            btn_Init.BackColor = Color.Transparent;
            TAG[] StatusData = BusRfidHF.Status((byte)(nUD_RWMNb.Value));//
            if (StatusData != null)
            {
                if (StatusData.Length != 0)
                {
                    btn_Status.BackColor = Color.GreenYellow;
                    foreach (TAG tag in StatusData)//
                    {
                        tBx_Status.AppendText(tag.UID.ToString("X08"));
                        tBx_Status.AppendText(Environment.NewLine);
                    }
                }
                else
                {
                    tBx_Status.Text = "No transponder";
                    btn_Status.BackColor = Color.Red;
                }
            }
            else
            {
                tBx_Status.Text = "No RWM";
                btn_Status.BackColor = Color.Red;
            }
        }

        private void Btn_Read_Click(object sender, EventArgs e)
        {
            btn_Init.BackColor = Color.Transparent;
            btn_Write.BackColor = Color.Transparent;
            btn_Status.BackColor = Color.Transparent;
            ConIDHF.Data ResRead;//
            ulong UID;
            TAG[] StatusData = BusRfidHF.Status((byte)(nUD_RWMNb.Value));//
            if (StatusData != null)
            {
                if (StatusData.Length > 1)
                {
                    if (chkBx_Addr.Checked == true)
                    {
                        UID = System.Convert.ToUInt64(tBx_UID.Text, 16);
                        ResRead = BusRfidHF.Read((byte)(nUD_RWMNb.Value), 0x20, (byte)(nUD_StartAddr.Value), (byte)(nUD_NbBlocks.Value), UID);//
                        if (ResRead.AckCode == ConIDHF.Acknldg.OperationSuccessful)//
                        {
                            btn_Read.BackColor = Color.GreenYellow;
                            DispData(ResRead);
                        }
                        else
                        {
                            btn_Read.BackColor = Color.Red;
                        }
                    }
                    else
                    {
                        tBx_DataRead.Text = "Impossible to read more than one transponder";
                        btn_Read.BackColor = Color.Red;
                    }
                }
                else if (StatusData.Length == 1)
                {
                    ResRead = BusRfidHF.Read((byte)(nUD_RWMNb.Value), 0, (byte)(nUD_StartAddr.Value), (byte)(nUD_NbBlocks.Value));//
                    if (ResRead.AckCode == ConIDHF.Acknldg.OperationSuccessful)//
                    {
                        btn_Read.BackColor = Color.GreenYellow;
                        DispData(ResRead);
                    }
                    else
                    {
                        btn_Read.BackColor = Color.Red;
                    }
                }
                else
                {
                    tBx_DataRead.Text = "No transponder";
                    btn_Read.BackColor = Color.Red;
                }
            }
            else
            {
                tBx_DataRead.Text = "No RWM";
            }
        }

        // DISPLAYING READING RESULTS
        private void DispData(ConIDHF.Data ResRead)
        {
            tBx_DataRead.Clear();
            int nbLines = Convert.ToInt32(nUD_NbBlocks.Value);
            String[] TablRead = new string[nbLines];
            int Addr = Convert.ToInt32(nUD_StartAddr.Value);
            int i;
            int j;
            for (j = 0; j < nUD_NbBlocks.Value; j++)
            {
                string StLine = "";
                for (i = 0; i < 4; i++)
                {
                    string StData = ResRead.TabData[j * 4 + i].ToString("X02");
                    StLine = StLine + StData + " ";
                }
                TablRead[j] = Convert.ToString(Addr + j) + ": " + StLine;
                tBx_DataRead.AppendText(TablRead[j]);
                tBx_DataRead.AppendText(Environment.NewLine);
            }
        }

        private void ChkBx_Addr_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBx_Addr.Checked == true)
            {
                tBx_UID.Enabled = true;
            }
            else
            {
                tBx_UID.Enabled = false;
            }
        }

        private void Btn_Write_Click(object sender, EventArgs e)
        {
            btn_Write.BackColor = Color.Transparent;
            btn_Read.BackColor = Color.Transparent;
            btn_Status.BackColor = Color.Transparent;
            btn_Init.BackColor = Color.Transparent;
            ConIDHF.Acknldg WriteAcknldg = ConIDHF.Acknldg.OperationSuccessful;//
            int nbLinesW = Convert.ToInt32(nUD_NbBlocksW.Value);
            string[] TablWrite = new string[nbLinesW];
            byte[] DataToWrite = new byte[nbLinesW * 4];
            int StrLength = tBx_Data.Text.Length;
            string Data0 = tBx_Data.Text;
            if (StrLength != (nbLinesW * 8 + (nbLinesW - 1) * 2))
            {
                btn_Write.BackColor = Color.Red;
                tBx_Data.AppendText(Environment.NewLine);
                tBx_Data.AppendText("Invalid data!");
            }
            else
            {
                for (int j = 0; j < nbLinesW; j++)
                {
                    TablWrite[j] = tBx_Data.Text.Substring(j * 10, 8);
                    for (int i = 0; i < 4; i++)
                    {
                        DataToWrite[4 * j + i] = Convert.ToByte((TablWrite[j].Substring(2 * i, 2)), 16);
                        ulong UID;
                        if (chkBx_Addr.Checked == true)
                        {
                            UID = System.Convert.ToUInt64(tBx_UID.Text, 16);
                            WriteAcknldg = BusRfidHF.Write((byte)(nUD_RWMNb.Value), 0x20, (byte)(nUD_StartAddrW.Value), (byte)(nUD_NbBlocksW.Value), DataToWrite, UID);//
                        }
                        else
                        {
                            WriteAcknldg = BusRfidHF.Write((byte)(nUD_RWMNb.Value), 0x0, (byte)(nUD_StartAddrW.Value), (byte)(nUD_NbBlocksW.Value), DataToWrite);//
                        }
                    }
                }
                if (WriteAcknldg == ConIDHF.Acknldg.OperationSuccessful)//
                {
                    btn_Write.BackColor = Color.GreenYellow;
                }
                else
                {
                    btn_Write.BackColor = Color.Red;
                }
            }
        }

        private void TBx_Data_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((e.KeyChar >= '0') && (e.KeyChar <= '9')) || ((e.KeyChar >= 'A') && (e.KeyChar <= 'F')) || ((e.KeyChar >= 'a') && (e.KeyChar <= 'f')) || (e.KeyChar == '\n') || (e.KeyChar == '\r'))
            {

            }
            else
            {
                e.Handled = true;
            }
        }

        private void Txt_Path_DoubleClick(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txt_Path.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void Btn_Add_Click(object sender, EventArgs e)
        {
            if (myConfig.KeyExists(txt_Label.Text))
            {

            }
        }
    }
}
