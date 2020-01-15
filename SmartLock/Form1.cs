using System;
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
        Thread thread;
        List<Slot> slots = new List<Slot>();
        List<PictureBox> pbslots = new List<PictureBox>();
        List<Label> lblslots = new List<Label>();
        int verandernaamnummer;
        public Form1()
        {
            InitializeComponent();
            Show_Slot0(false);
            btnTerug.Visible = false;
            btnVeranderNaamSlot.Visible = false;
            btnSlotToevoegen.Visible = false;
            Show_Slotbeheer(false);
            Show_VeranderNaam(false);
            pnlSlotToevoegen.Visible = false;
            Pbslots_List();
            Lblslots_Lists();

            //tijdelijk verbergen van de extra sloten
            Show_Slot1(false);
            Show_Slot2(false);
            Show_Slot3(false);
            Show_Slot4(false);
            Show_Slot5(false);
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
        void Show_Slot0(bool state)
        {
            pbSlot0.Visible = state;
            lblSlot0.Visible = state;
        }
        void Show_Slot1(bool state)
        {
            pbSlot1.Visible = state;
            lblSlot1.Visible = state;
        }
        void Show_Slot2(bool state)
        {
            pbSlot2.Visible = state;
            lblSlot2.Visible = state;
        }
        void Show_Slot3(bool state)
        {
            pbSlot3.Visible = state;
            lblSlot3.Visible = state;
        }
        void Show_Slot4(bool state)
        {
            pbSlot4.Visible = state;
            lblSlot4.Visible = state;
        }
        void Show_Slot5(bool state)
        {
            pbSlot5.Visible = state;
            lblSlot5.Visible = state;
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
            btnNaamSlot0.Visible = state;
            btnNaamSlot1.Visible = state;
            btnNaamSlot2.Visible = state;
            btnNaamSlot3.Visible = state;
            btnNaamSlot4.Visible = state;
            btnNaamSlot5.Visible = state;
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
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("145.93.89.206"), 10000);
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
                string msg = "REGISTER" + " " + tbNieuweGebruikersnaam.Text + " " + tbWachtwoordNieuweGebruiker.Text + " " + tbModelnummer.Text;
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
            string msg = "LOGIN" + " " + tbGebruikersnaam.Text + " " + tbWachtwoord.Text;
            byte[] sendBuffer = Encoding.Default.GetBytes(msg);
            Sock.Send(sendBuffer);

            //bericht ontvangen van de server
            byte[] receiveBuffer = new byte[1024];
            int rec = Sock.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

            Array.Resize(ref receiveBuffer, rec);
            string message = Encoding.Default.GetString(receiveBuffer);

            //Splitten van het bericht van de server met een spatie tussen de berichten
            string[] words = message.Split(' ');

            if (words[0] == "LOGINSUCCEEDED")
            {
                int aantalsloten = Int32.Parse(words[1]);
                Make_Slots(aantalsloten);
                MessageBox.Show("Login succeeded");
                Show_Register(false);
                Show_Login(false);
                btnVeranderNaamSlot.Visible = true;
                btnSlotToevoegen.Visible = true;
                Make_thread();
            }
            else if (message == "LOGINFAILED")
            {
                MessageBox.Show("Login failed");
                Show_Login(true);
                Sock.Close();
            }
        }
        //functie om sloten aan te maken
        void Make_Slots(int aantalsloten)
        {
            for (int i = 0; i < aantalsloten; i++)
            {                
                Slot slot = new Slot(i.ToString(), Sock);
                slots.Add(slot);
                pbslots[i].Visible = true;
                lblslots[i].Visible = true;
                lblslots[i].Text = slot.Nameslot;

                if (slot.State == "LOCKED")
                {
                    Status_Locked(i);
                }
                else if (slot.State == "UNLOCKED")
                {
                    Status_Unlocked(i);
                }
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
                    for (int i = 0; i < slots.Count; i++)
                    {
                        State_slot(i);
                    }
                    
                }
            }
            //Exceptionhandling als de connectie verbreekt
            catch (SocketException) { }
        }

        //State opvragen van het slot bij de server
        void State_slot(int slotnummer)
        {
            Tuple<String, String> messages = slots[slotnummer].state_slot(Sock);
            string message = messages.Item1;
            string nameMessage = messages.Item2;

            if (slots[slotnummer].State != message)
            {
                MessageBox.Show("Status " + lblSlot0.Text + " is gewijzigd.");
            }
            slots[slotnummer].State = message;

            if (message == "UNLOCKED")
            {
                //MessageBox.Show("Slot is geopend");
                Status_Unlocked(slotnummer);
            }
            else if (message == "LOCKED")
            {
                //MessageBox.Show("Slot is gesloten");
                Status_Locked(slotnummer);
            }
            if (lblslots[slotnummer].Text != nameMessage)
            {
                MessageBox.Show("Naam " + lblSlot0.Text + " is gewijzigd naar: " + nameMessage);
                lblslots[slotnummer].Text = nameMessage;
            }            
        }

        //Slot afsluiten
        void Lock_Slot(int slotnummer)
        {
            string message = slots[slotnummer].lock_slot(Sock); 
            
            if (message == "LOCKED")
            {
                Status_Locked(slotnummer);
            }
        }
        //Slot openen
        void Unlock_Slot(int slotnummer)
        {
            string message = slots[slotnummer].unlock_slot(Sock);

            if (message == "UNLOCKED")
            {
                Status_Unlocked(slotnummer);
            }
        }
        //Label van het slot aanpassen
        void Change_name(string slotNaam, int slotnummer)
        {
            string message = slots[slotnummer].change_name(Sock, slotNaam, slotnummer);

            if (message == "CHANGE SUCCEEDED")
            {
                MessageBox.Show("Naam is gewijzigd");
                lblslots[slotnummer].Text = slotNaam;
                
            }
            else if (message == "CHANGE FAILED")
            {
                MessageBox.Show("Naam is niet gewijzigd");
            }
        }
        //Opslaan en controleren van een nieuwe gebruiker
        void Make_NewUser()
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

        //----------------------------------------------------------------------- Weergave slot --------------------------------------------------------------------------------------------------------------------
        //Toon gesloten slot
        void Status_Locked(int indexnummer)
        {
            pbslots[indexnummer].SizeMode = PictureBoxSizeMode.StretchImage;
            pbslots[indexnummer].Image = Image.FromFile("D:\\School\\Fontys\\Semester 1\\Software\\Verdieping\\Proftaak\\Afbeeldingen_Software\\LockedV2.png");
        }
        //Toon open slot
        void Status_Unlocked(int indexnummer)
        {
            
            pbslots[indexnummer].SizeMode = PictureBoxSizeMode.StretchImage;
            pbslots[indexnummer].Image = Image.FromFile("D:\\School\\Fontys\\Semester 1\\Software\\Verdieping\\Proftaak\\Afbeeldingen_Software\\UnlockedV2.png");
        }

        //Openen of sluiten van slot
        void Change_StateSlot(int slotnummer)
        {
            if (slots[slotnummer].State == "LOCKED")
            {
                Unlock_Slot(slotnummer);
            }
            else if (slots[slotnummer].State == "UNLOCKED")
            {
                Lock_Slot(slotnummer);
            }
        }

        void Pbslots_List()
        {
            pbslots.Add(pbSlot0);
            pbslots.Add(pbSlot1);
            pbslots.Add(pbSlot2);
            pbslots.Add(pbSlot3);
            pbslots.Add(pbSlot4);
            pbslots.Add(pbSlot5);
        }
        void Lblslots_Lists()
        {
            lblslots.Add(lblSlot0);
            lblslots.Add(lblSlot1);
            lblslots.Add(lblSlot2);
            lblslots.Add(lblSlot3);
            lblslots.Add(lblSlot4);
            lblslots.Add(lblSlot5);
        }
        //----------------------------------------------------------------------- Begin klik functies --------------------------------------------------------------------------------------------------------------
        //Slot 0 openen of sluiten
        private void PbSlot0_Click(object sender, EventArgs e)
        {
            Change_StateSlot(0);
        }
        //Slot 1 openen of sluiten
        private void PbSlot1_Click_1(object sender, EventArgs e)
        {
            Change_StateSlot(1);
        }
        //Slot 2 openen of sluiten
        private void PbSlot2_Click(object sender, EventArgs e)
        {
            Change_StateSlot(2);
        }
        //Slot 3 openen of sluiten
        private void PbSlot3_Click(object sender, EventArgs e)
        {
            Change_StateSlot(3);
        }
        //Slot 4 openen of sluiten
        private void PbSlot4_Click(object sender, EventArgs e)
        {
            Change_StateSlot(4);
        }
        //Slot 5 openen of sluiten
        private void PbSlot5_Click(object sender, EventArgs e)
        {
            Change_StateSlot(5);
        }

        //Nieuwe gegevens gebruiker verwerken (Regristratie)
        private void BtnRegisterGereed_Click(object sender, EventArgs e)
        {
            Make_NewUser();
        }
        //als de gebruiker op enter klikt (login pagina)
        private void BtnEnter_Click(object sender, EventArgs e)
        {
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
        //Klikken op slot 0 voor de naam te veranderen
        private void BtnNaamSlot0_Click(object sender, EventArgs e)
        {
            Show_VeranderNaam(true);
            verandernaamnummer = 0;
        }
        //klikken op slot 1 voor de naam te veranderen
        private void BtnNaamSlot1_Click(object sender, EventArgs e)
        {
            Show_VeranderNaam(true);
            verandernaamnummer = 1;
        }
        //klikken op slot 2 voor de naam te veranderen
        private void BtnNaamSlot2_Click(object sender, EventArgs e)
        {
            Show_VeranderNaam(true);
            verandernaamnummer = 2;
        }
        //klikken op slot 3 voor de naam te veranderen
        private void BtnNaamSlot3_Click(object sender, EventArgs e)
        {
            Show_VeranderNaam(true);
            verandernaamnummer = 3;
        }
        //klikken op slot 4 voor de naam te veranderen
        private void BtnNaamSlot4_Click(object sender, EventArgs e)
        {
            Show_VeranderNaam(true);
            verandernaamnummer = 4;
        }
        //klikken op slot 5 voor de naam te veranderen
        private void BtnNaamSlot5_Click(object sender, EventArgs e)
        {
            Show_VeranderNaam(true);
            verandernaamnummer = 5;
        }
        //Button voor op te slaan van de naamwijziging
        private void BtnNieuweNaamOpslaan_Click(object sender, EventArgs e)
        {
            Change_name(tbVeranderNaam.Text, verandernaamnummer);
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
           // Add_Slot2();
        }
        private void BtnExtraSlot1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Dit slot is al toegevoegd!\nKies een ander slot!");
        }
    }
}
