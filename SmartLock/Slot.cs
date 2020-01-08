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
    class Slot
    {
        //fields
        private string slotnummer;


        //contstructor
        public Slot(string slotnummer)
        {
            this.slotnummer = slotnummer;


        }
        private Tuple<String,String> state_slot(Socket Sock)
        {
            //bericht naar de server sturen
            string msg = "STATE " + slotnummer;

            byte[] sendBuffer = Encoding.Default.GetBytes(msg);
            Sock.Send(sendBuffer);

            //bericht ontvangen van de server (STATE)
            byte[] receiveBuffer = new byte[1024];
            int rec = Sock.Receive(receiveBuffer, 0, receiveBuffer.Length, 0);

            //bericht naar de server sturen
            string comfirm = "CONFIRM";
            byte[] sendConfirm = Encoding.Default.GetBytes(comfirm);
            Sock.Send(sendConfirm);

            //bericht ontvangen van de server (LABEL)
            byte[] nameBuffer = new byte[1024];
            int recName = Sock.Receive(nameBuffer, 0, nameBuffer.Length, 0);

            //Bericht STATE
            Array.Resize(ref receiveBuffer, rec);
            string message = Encoding.Default.GetString(receiveBuffer);

            //Bericht Label
            Array.Resize(ref nameBuffer, recName);
            string nameMessage = Encoding.Default.GetString(nameBuffer);

            return Tuple.Create(message, nameMessage);
        }

    }
        //state - Vraagt de status van het slot aan de server

        //lock - Vergrendelen van het slot

        //unlock - Ontgrendelen van het slot

        //rename
        //- Label van het slot aanpassen
    }
}
