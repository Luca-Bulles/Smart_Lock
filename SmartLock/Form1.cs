﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SmartLock
{
    public partial class Form1 : Form
    {
        Socket Sock;
        string state;
        Thread thread;
        public Form1()
        {
            InitializeComponent();
            Show_Slot1(false);
            btnTerug.Visible = false;
            btnVeranderNaamSlot.Visible = false;
            btnSlotToevoegen.Visible = false;
            Show_Slotbeheer(false);
            Show_VeranderNaam(false);
            pnlSlotToevoegen.Visible = false;

        }

        //Controle of de wachtwoorden overeenkomen
        bool Password_Control()
        {
            bool juist;
            string nieuwWachtwoord = tbWachtwoordNieuweGebruiker.Text;
            string nieuwWachtwoord2 = tbHerhaalWachtwoord.Text;
            if (nieuwWachtwoord != nieuwWachtwoord2)
            {
                juist = false;
                return juist;
            }
            else
            {
                juist = true;
                return juist;
            }

        }

        //----------------------------------------------------------------------- Begin Weergave functies ----------------------------------------------------------------------------------------------------------
        //wel of niet tonen van de login pagina
        void Show_Login(bool state)
        {
            pnlLogin.Visible = state;
            btnEnter.Visible = state;
            btnTerug.Visible = state;
            lblGebruikersnaam.Visible = state;
            lblWelkom.Visible = state;
            lblWachtwoord.Visible = state;
            lblVoerIn.Visible = state;
            tbGebruikersnaam.Visible = state;
            tbWachtwoord.Visible = state;

        }
        //wel of niet tonen van het slot
        void Show_Slot1(bool state)
        {
            pbSlot1.Visible = state;
            lblSlot1.Visible = state;
        }

        //wel of niet tonen van het regristratiescherm
        void Show_Register(bool state)
        {
            pnlRegister.Visible = state;
            btnInloggen.Visible = state;
            btnRegisterGereed.Visible = state;
            tbNieuweGebruikersnaam.Visible = state;
            tbWachtwoordNieuweGebruiker.Visible = state;
            tbModelnummer.Visible = state;
            tbHerhaalWachtwoord.Visible = state;
            lblNieuwGebruikersnaam.Visible = state;
            lblNieuwWachtwoord.Visible = state;
            lblModelnummer.Visible = state;
            lblWelkomRegister.Visible = state;
            lblRegistreerHier.Visible = state;
            lblBestaandeGebruiker.Visible = state;
            lblNieuwWachtwoord2.Visible = state;

        }

        //Tonen van slotbeheer
        void Show_Slotbeheer(bool state)
        {
            pnlSlotbeheer.Visible = state;
            btnNaamSlot1.Visible = state;
            btnNaamSlot2.Visible = state;
            btnNaamSlot3.Visible = state;
            btnNaamSlot4.Visible = state;
            btnNaamSlot5.Visible = state;
            btnNaamSlot6.Visible = state;
            lblHierNaamSlotaanpassen.Visible = state;
        }

        //tonen van het verander van naam scherm
        void Show_VeranderNaam(bool state)
        {
            pnlVeranderNaam.Visible = state;
            lblVoerNaamSlotIn.Visible = state;
            tbVeranderNaam.Visible = state;
        }

        //----------------------------------------------------------------------- Client Server Communicatie -------------------------------------------------------------------------------------------------------
        //Functie waar de socket in gemaakt wordt
        void Socket_Define()
        {
            //Maken van de Socket
            Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Verbinding vaststellen
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("145.93.88.241"), 10000);
            Sock.Connect(endPoint);

        }
        //Registratie
        void Client_Register()
        {
            if (tbWachtwoordNieuweGebruiker.Text == tbHerhaalWachtwoord.Text)
            {
                MessageBox.Show("Gebruikersnaam: " + tbNieuweGebruikersnaam.Text + "\nWachtwoord: " + tbWachtwoordNieuweGebruiker.Text + "\nNieuw Wachtwoord: " + tbHerhaalWachtwoord.Text + "\nModelnummer: " + tbModelnummer.Text);
                Socket_Define();
                //bericht naar de server sturen
                string msg = "CLIENT REGISTER" + " " + tbNieuweGebruikersnaam.Text + " " + tbWachtwoordNieuweGebruiker.Text + " " + tbModelnummer.Text;
                byte[] sendBuffer = Encoding.Default.GetBytes(msg);
                Sock.Send(sendBuffer);

                //bericht ontvangen van de server
                byte[] receiveBuffer = new byte[1024];
                int rec = Sock.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

                Array.Resize(ref receiveBuffer, rec);
                string message = Encoding.Default.GetString(receiveBuffer);

                if (message == "CREATION SUCCEEDED")
                {
                    MessageBox.Show("Creation Succeeded");
                    Show_Register(false);
                    Show_Login(true);
                    Sock.Close();
                }
                else if (message == "CREATION FAILED")
                {
                    MessageBox.Show("Creation Failed");
                    Sock.Close();
                }
            }
            else
            {
                MessageBox.Show("Wachtwoorden komen niet overeen, probeer het opnieuw!");
            }
        }

        //Login bestaande gebruiker
        void Client_Login()
        {
            Socket_Define();
            //bericht naar de server sturen
            string msg = "CLIENT LOGIN" + " " + tbGebruikersnaam.Text + " " + tbWachtwoord.Text;
            byte[] sendBuffer = Encoding.Default.GetBytes(msg);
            Sock.Send(sendBuffer);

            //bericht ontvangen van de server
            byte[] receiveBuffer = new byte[1024];
            int rec = Sock.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

            Array.Resize(ref receiveBuffer, rec);
            string message = Encoding.Default.GetString(receiveBuffer);

            if (message == "LOGIN SUCCEEDED")
            {
                MessageBox.Show("Login succeeded");
                Show_Register(false);
                Show_Login(false);
                Show_Slot1(true);
                btnVeranderNaamSlot.Visible = true;
                btnSlotToevoegen.Visible = true;
                State_slot1();
                Make_thread();

            }
            else if (message == "LOGIN FAILED")
            {
                MessageBox.Show("Login failed");
                Show_Login(true);
                Sock.Close();
            }

        }

        //Functie die een thread maakt
        void Make_thread()
        {
            thread = new Thread(Response_server);
            thread.IsBackground = true;
            Control.CheckForIllegalCrossThreadCalls = false;
            thread.Start();
        }

        //Constant wachten op reactie server, voor als iemand anders het slot opent of sluit
        async void Response_server()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(10000);
                    State_slot1();
                }
            }
            //Exceptionhandling als de connectie verbreekt
            catch (SocketException) { }
        }

        //State opvragen van het slot bij de server
        void State_slot1()
        {
            //bericht naar de server sturen
            string msg = "STATE";
            byte[] sendBuffer = Encoding.Default.GetBytes(msg);
            Sock.Send(sendBuffer);

            //bericht ontvangen van de server
            byte[] receiveBuffer = new byte[1024];
            int rec = Sock.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

            Array.Resize(ref receiveBuffer, rec);
            string message = Encoding.Default.GetString(receiveBuffer);
            if (state != message)
            {
                MessageBox.Show("Status " + lblSlot1.Text + " is gewijzigd.");
            }
            state = message;

            if (message == "UNLOCKED")
            {
                //MessageBox.Show("Slot is geopend");
                Status_Unlocked();

            }
            else if (message == "LOCKED")
            {
                //MessageBox.Show("Slot is gesloten");
                Status_Locked();
            }

        }

        //Slot afsluiten
        void Lock_Slot1()
        {

            //bericht naar de server sturen
            string msg = "LOCK";
            byte[] sendBuffer = Encoding.Default.GetBytes(msg);
            Sock.Send(sendBuffer);

            //bericht ontvangen van de server
            byte[] receiveBuffer = new byte[1024];
            int rec = Sock.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

            Array.Resize(ref receiveBuffer, rec);
            string message = Encoding.Default.GetString(receiveBuffer);
            state = message;

            if (message == "LOCKED")
            {
                //MessageBox.Show("Slot is gesloten");
                Status_Locked();
            }

        }
        //Slot openen
        void Unlock_Slot1()
        {
            //bericht naar de server sturen
            string msg = "UNLOCK";
            byte[] sendBuffer = Encoding.Default.GetBytes(msg);
            Sock.Send(sendBuffer);

            //bericht ontvangen van de server
            byte[] receiveBuffer = new byte[1024];
            int rec = Sock.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

            Array.Resize(ref receiveBuffer, rec);
            string message = Encoding.Default.GetString(receiveBuffer);
            state = message;

            if (message == "UNLOCKED")
            {
                //MessageBox.Show("Slot is geopend");
                Status_Unlocked();
            }
        }

        void Change_name(string slotNaam)
        {
            //bericht naar de server sturen
            string msg = "CHANGE NAME";
            byte[] sendBuffer = Encoding.Default.GetBytes(msg);
            Sock.Send(sendBuffer);

            string lockname = lblSlot1.Text;
            byte[] nameBuffer = Encoding.Default.GetBytes(lockname);
            Sock.Send(nameBuffer);

            //bericht ontvangen van de server
            byte[] receiveBuffer = new byte[1024];
            int rec = Sock.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

            Array.Resize(ref receiveBuffer, rec);
            string message = Encoding.Default.GetString(receiveBuffer);
            state = message;

            if (message == "CHANGE SUCCEEDED")
            {
                MessageBox.Show("Naam is gewijzigd");
                lblSlot1.Text = slotNaam;
                

            }
            else if (message == "CHANGE FAILED")
            {
                MessageBox.Show("Naam is niet gewijzigd");
            }
        }

        //----------------------------------------------------------------------- Weergave slot --------------------------------------------------------------------------------------------------------------------
        //Toon groen slot
        void Status_Locked()
        {
            pbSlot1.SizeMode = PictureBoxSizeMode.StretchImage;
            pbSlot1.Image = Image.FromFile("D:\\School\\Fontys\\Semester 1\\Software\\Verdieping\\Proftaak\\Afbeeldingen_Software\\Locked.jpg");
        }
        //Toon het rode slot
        void Status_Unlocked()
        {
            pbSlot1.SizeMode = PictureBoxSizeMode.StretchImage;
            pbSlot1.Image = Image.FromFile("D:\\School\\Fontys\\Semester 1\\Software\\Verdieping\\Proftaak\\Afbeeldingen_Software\\Unlocked.jpg");
        }

        //----------------------------------------------------------------------- Begin klik functies --------------------------------------------------------------------------------------------------------------
        //Bevestiging van het openen van het slot
        private void PbSlot1_Click(object sender, EventArgs e)
        {
            if (state == "LOCKED")
            {
                Unlock_Slot1();
            }
            else if (state == "UNLOCKED")
            {
                Lock_Slot1();
            }
        }
        //Nieuwe gegevens gebruiker verwerken (Regristratie)
        private void BtnRegisterGereed_Click(object sender, EventArgs e)
        {
            bool controle = Password_Control();
            if (controle == true)
            {
                Client_Register();
            }
            else
            {
                MessageBox.Show("Wachtwoorden komen niet overeen! Probeer het opnieuw!");
            }
        }
        //als de gebruiker op enter klikt (login pagina)
        private void BtnEnter_Click(object sender, EventArgs e)
        {
            Status_Locked();
            Client_Login();

        }

        //Terug naar het begin
        private void BtnTerug_Click(object sender, EventArgs e)
        {
            Show_Register(true);
            Show_Login(false);
        }
        //Gebruiker ziet nu de inlog pagina als hij al een account heeft gemaakt
        private void BtnInloggen_Click(object sender, EventArgs e)
        {
            Show_Register(false);
            Show_Login(true);
        }
        //Naam van het slot aanpassen - nog niet volledig
        private void BtnVeranderNaamSlot_Click(object sender, EventArgs e)
        {
            Show_Slotbeheer(true);
            Show_Login(false);
            Show_Register(false);
        }

        //Klikken op slot 1 voor de naam te veranderen
        private void BtnNaamSlot1_Click(object sender, EventArgs e)
        {
            Show_VeranderNaam(true);
        }

        private void BtnNieuweNaamOpslaan_Click(object sender, EventArgs e)
        {
            string slotNaam = tbVeranderNaam.Text;
            Change_name(slotNaam);
        }
        //Sluiten van het scherm om het wachtwoord te wijzigen
        private void BtnCloseNieuweNaam_Click(object sender, EventArgs e)
        {
            Show_VeranderNaam(false);
        }
        //Sluiten van het slotbeheer scherm 
        private void BtnCloseVeranderNaam_Click(object sender, EventArgs e)
        {
            Show_Slotbeheer(false);
        }
        //Toevoegen van een slot
        private void BtnSlotToevoegen_Click(object sender, EventArgs e)
        {
            //Picturebox is een class die ik gebruikt voor het maken van nieuwe sloten
            //Slot 2 maken en plaatsen
            PictureBox imageControl = new PictureBox();
            imageControl.Width = 235;
            imageControl.Height = 235;
            imageControl.Location = new Point(400, 283);
            Bitmap image = new Bitmap("D:\\School\\Fontys\\Semester 1\\Software\\Verdieping\\Proftaak\\Afbeeldingen_Software\\Locked.jpg");
            imageControl.SizeMode = PictureBoxSizeMode.StretchImage;
            imageControl.Image = (Image)image;
            Controls.Add(imageControl);

            //Label van slot 2 maken en plaatsen
            Label lblSlot2 = new Label();
            lblSlot2.Location = new Point(490, 251);
            lblSlot2.Text = "Slot 2";
            this.Controls.Add(lblSlot2);

        }

        private void BtnExtraSlot1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Dit slot is al toegevoegd!\nKies een ander slot!");
        }
    }
}
