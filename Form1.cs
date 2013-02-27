using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Crawl.Sockets;
using System.Configuration;
using Crawl.PlayoutCommands;
using Crawl.DataAccess;
using Crawl.Output;

namespace Crawl
{
   
    public partial class Form1 : Form
    {
        private string msCrawl = "";
        private Functions _functions;
        private Talker _talker;
        private delegate void Invoker();
        private Timer _setRoundCrawlTimer;

        public Form1()
        {
            InitializeComponent();

            btnAnimateCrawlIn.Enabled = false;

            _functions = new Functions();

            _setRoundCrawlTimer = new Timer();
            _setRoundCrawlTimer.Interval = 10000;
            _setRoundCrawlTimer.Tick += new EventHandler(setRoundCrawlTimer_Tick);
            _setRoundCrawlTimer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadSponsors();

            initializeTalker();
            
            chkShowRd1.Checked = Convert.ToBoolean(Convert.ToInt16(Properties.Settings.Default.ShowRd1));
            chkShowRd2.Checked = Convert.ToBoolean(Convert.ToInt16(Properties.Settings.Default.ShowRd2));
            chkShowRd3.Checked = Convert.ToBoolean(Convert.ToInt16(Properties.Settings.Default.ShowRd3));
            chkShowRd4.Checked = Convert.ToBoolean(Convert.ToInt16(Properties.Settings.Default.ShowRd4));
            chkShowRd5.Checked = Convert.ToBoolean(Convert.ToInt16(Properties.Settings.Default.ShowRd5));
            chkShowRd6.Checked = Convert.ToBoolean(Convert.ToInt16(Properties.Settings.Default.ShowRd6));
            chkShowRd7.Checked = Convert.ToBoolean(Convert.ToInt16(Properties.Settings.Default.ShowRd7));
            chkShowMessage.Checked = Convert.ToBoolean(Convert.ToInt16(Properties.Settings.Default.ShowMessage));

            txtMessage.Text = Properties.Settings.Default.Message;

            for (int i = 1; i <= 7; i++)
            {
                _functions.gtRoundInfo[i].iShowRound = Convert.ToInt16(Properties.Settings.Default["ShowRd" + i.ToString()]);
                _functions.gtRoundInfo[i].iShowTeams = 1;
            }

            _functions.InitializeCrawl();
        }

        private void initializeTalker()
        {
            _talker = new Talker("3");

            _talker.DataArrival += new Talker.DataArrivalHandler(talkerDataArrival);
            _talker.Connected += new Talker.ConnectionHandler(connectedTo);
            _talker.ConnectionClosed += new Talker.ConnectionClosedHandler(connectionClosed);

            _talker.Connect(ConfigurationManager.AppSettings["PlayoutIP"].ToString(), ConfigurationManager.AppSettings["PlayoutPort"].ToString());           
        }

        private void connectedTo()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Invoker(connectedTo));
            }
            else
            { 
                btnAnimateCrawlIn.Enabled = true;
                lblPlayoutStatus.Text = "Connected To Playout";
                lblPlayoutStatus.ForeColor = Color.Green;
            }
        }

        private void talkerDataArrival(PlayerCommand CommandToProcess, string ID)
        {
            int iPickStart;

            if (CommandToProcess.Command.ToString() == "RequestData")
            {
                if (_functions.gbAddToCrawl == true)
                {
                    if (msCrawl == "")
                    {
                        iPickStart = _functions.FindNextPickStart();
                        msCrawl = _functions.AcquireString(iPickStart, 0);
                    }

                    //do the page engine stuff
                    PlayerCommand commandToSend = new PlayerCommand();

                    commandToSend.Command = (Crawl.PlayoutCommands.CommandType)Enum.Parse(typeof(Crawl.PlayoutCommands.CommandType), "ShowPage");
                    commandToSend.Parameters = new List<CommandParameter>();
                    commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Crawl"));
                    //commandToSend.Parameters.Add(new CommandParameter("QueueCommand", "TRUE"));
                    commandToSend.Parameters.Add(new CommandParameter("MergeDataWithoutTransitions", "true"));
                    
                    XmlDataRow xmlRow = new XmlDataRow();

                    xmlRow.Add("CRAWL_TEXT", msCrawl);

                    commandToSend.TemplateData = xmlRow.GetXMLString();

                    _talker.Talk(commandToSend);

                    msCrawl = "";

                }
               
            }
        }

        private void connectionClosed()
        {
            
        }

        private void connectionRefused()
        {

        }

        private void loadSponsors()
        {
            cboSponsors.Items.Add("<None>");
            cboSponsors.Items.Add("Coors Light");
            cboSponsors.Items.Add("Bud Light");            
        }
        
        private void chkShowRd1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowRd1 = Convert.ToInt16(chkShowRd1.Checked).ToString();
            Properties.Settings.Default.Save();

            _functions.gtRoundInfo[1].iShowRound = Convert.ToInt16(chkShowRd1.Checked);
        }

        private void chkShowRd2_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowRd2 = Convert.ToInt16(chkShowRd2.Checked).ToString();
            Properties.Settings.Default.Save();

            _functions.gtRoundInfo[2].iShowRound = Convert.ToInt16(chkShowRd2.Checked);
        }

        private void chkShowRd3_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowRd3 = Convert.ToInt16(chkShowRd3.Checked).ToString();
            Properties.Settings.Default.Save();

            _functions.gtRoundInfo[3].iShowRound = Convert.ToInt16(chkShowRd3.Checked);
        }

        private void chkShowRd4_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowRd4 = Convert.ToInt16(chkShowRd4.Checked).ToString();
            Properties.Settings.Default.Save();

            _functions.gtRoundInfo[4].iShowRound = Convert.ToInt16(chkShowRd4.Checked);
        }

        private void chkShowRd5_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowRd5 = Convert.ToInt16(chkShowRd5.Checked).ToString();
            Properties.Settings.Default.Save();

            _functions.gtRoundInfo[5].iShowRound = Convert.ToInt16(chkShowRd5.Checked);
        }

        private void chkShowRd6_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowRd6 = Convert.ToInt16(chkShowRd6.Checked).ToString();
            Properties.Settings.Default.Save();

            _functions.gtRoundInfo[6].iShowRound = Convert.ToInt16(chkShowRd6.Checked);
        }

        private void chkShowRd7_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowRd7 = Convert.ToInt16(chkShowRd7.Checked).ToString();
            Properties.Settings.Default.Save();

            _functions.gtRoundInfo[7].iShowRound = Convert.ToInt16(chkShowRd7.Checked);
        }

        private void btnAnimateCrawlIn_Click(object sender, EventArgs e)
        {
            if (btnAnimateCrawlIn.Text == "Animate Crawl IN")
            {
                btnAnimateCrawlIn.Text = "Animate Crawl OUT";

                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (Crawl.PlayoutCommands.CommandType)Enum.Parse(typeof(Crawl.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Crawl"));

                _talker.Talk(commandToSend);
            }
            else
            {
                btnAnimateCrawlIn.Text = "Animate Crawl IN";

                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (Crawl.PlayoutCommands.CommandType)Enum.Parse(typeof(Crawl.PlayoutCommands.CommandType), "HidePage");
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Crawl"));

                _talker.Talk(commandToSend);
            }
        }

        private void btnStartCrawl_Click(object sender, EventArgs e)
        {
            string sCrawl = "";

            if (btnStartCrawl.Text == "Start Crawl")
            {
                _functions.SetRoundCrawlState();

                btnStartCrawl.Text = "Stop Crawl";

                _functions.gbAddToCrawl = true;

                int iPickStart;
                int iLoop;

                for (iLoop = 1; iLoop <= 2; iLoop++)
                {
                    if (msCrawl == "")
                    {
                        iPickStart = _functions.FindNextPickStart();
                        sCrawl = sCrawl + _functions.AcquireString(iPickStart, 0);
                    }
                    else
                    {
                        sCrawl = sCrawl + msCrawl;
                        msCrawl = "";
                    }
                }
                
                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (Crawl.PlayoutCommands.CommandType)Enum.Parse(typeof(Crawl.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Crawl"));
                //commandToSend.Parameters.Add(new CommandParameter("MergeDataWithoutTransitions", "true"));

                XmlDataRow xmlRow = new XmlDataRow();

                xmlRow.Add("CRAWL_TEXT", sCrawl);
                xmlRow.Add("TITLE_1", "0");

                commandToSend.TemplateData = xmlRow.GetXMLString();

                _talker.Talk(commandToSend); 
            }
            else
            {
                btnStartCrawl.Text = "Start Crawl";

                msCrawl = "";

                _functions.StopCrawl();  

                PlayerCommand commandToSend = new PlayerCommand();

                commandToSend.Command = (Crawl.PlayoutCommands.CommandType)Enum.Parse(typeof(Crawl.PlayoutCommands.CommandType), "ShowPage");
                commandToSend.Parameters = new List<CommandParameter>();
                commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Crawl"));

                XmlDataRow xmlRow = new XmlDataRow();

                xmlRow.Add("CRAWL_TEXT", "");
                xmlRow.Add("TITLE_1", "1");

                commandToSend.TemplateData = xmlRow.GetXMLString();

                _talker.Talk(commandToSend); 

                              
            }
        }

        private void chkShowMessage_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowMessage = Convert.ToInt16(chkShowMessage.Checked).ToString();
            Properties.Settings.Default.Save();
        }

        private void btnSaveMessage_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Message = txtMessage.Text;
            Properties.Settings.Default.Save();
        }

        private void btnShowSponsor_Click(object sender, EventArgs e)
        {
            XmlDataRow xmlRow = new XmlDataRow();

            switch (cboSponsors.Text)
            {
                case "<None>":
                    xmlRow.Add("CRAWL_BACKGROUND", "Images\\still_ele\\CRAWL_new_16x9_safe_ele\\RED_CRAWL_keyable_16x9_safe_ele.tga");
                    xmlRow.Add("CHIP_1", "Images\\still_ele\\CRAWL_new_16x9_safe_ele\\ESPN_crawl_logo_ele.tga");
                    break;
                case "Coors Light":
                    xmlRow.Add("CRAWL_BACKGROUND", "Images\\still_ele\\CRAWL_new_16x9_safe_ele\\RED_CRAWL_standard_16x9_safe_ele.tga");
                    xmlRow.Add("CHIP_1", "Images\\still_ele\\CRAWL_new_16x9_safe_ele\\COORS_LIGHT_crawl_logo_ele.tga");
                    break;
                case "Bud Light":
                    xmlRow.Add("CRAWL_BACKGROUND", "Images\\still_ele\\CRAWL_new_16x9_safe_ele\\BLUE_budlight_CRAWL_keyable_16x9_safe_ele.tga");
                    xmlRow.Add("CHIP_1", "Images\\still_ele\\CRAWL_new_16x9_safe_ele\\BUD_LIGHT_crawl_logo_ele.tga");
                    break;
            }

            PlayerCommand commandToSend = new PlayerCommand();

            commandToSend.Command = (Crawl.PlayoutCommands.CommandType)Enum.Parse(typeof(Crawl.PlayoutCommands.CommandType), "ShowPage");
            commandToSend.Parameters = new List<CommandParameter>();
            commandToSend.Parameters.Add(new CommandParameter("TemplateName", "Crawl"));

            commandToSend.TemplateData = xmlRow.GetXMLString();

            _talker.Talk(commandToSend); 
        }
        

        public void setRoundCrawlTimer_Tick(object sender, EventArgs e)
        {
            _setRoundCrawlTimer.Stop();

            _functions.SetRoundCrawlState();

            _setRoundCrawlTimer.Start();
        }
    }
}
