using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssettoCorsaSharedMemory;
using System.IO.Ports;
using System.Diagnostics;

namespace ACDuino
{
    class Program
    {
        static SerialPort arduino;
        static string _gear;
        static string _laps;
        static string _speed;
        static string _position;
        static string _rpm;
        static string _com = "COM4";
        static int _baudrate = 57600;

        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando ACDuino por puerto serie COM4...");
            arduino = new SerialPort(_com, _baudrate);
            //arduino.DataReceived += new SerialDataReceivedEventHandler(ReadArduino);
            try
            {
                arduino.Open();
            } 
            catch (Exception e)
            {
                Console.WriteLine("No se ha encontrado controladora Arduino en " + _com);
                Console.ReadKey();
                return;
            }

            Console.WriteLine(" INICIADO");

            AssettoCorsa ac = new AssettoCorsa();
            ac.StaticInfoInterval = 5000; // Get StaticInfo updates ever 5 seconds
            ac.GraphicsInterval = 500;
            ac.PhysicsInterval = 5;
            //ac.StaticInfoUpdated += ac_StaticInfoUpdated; // Add event listener for StaticInfo
            ac.PhysicsUpdated += ac_PhysicsUpdated;
            ac.GraphicsUpdated += ac_GraphicsUpdated;
            ac.Start(); // Connect to shared memory and start interval timers 

            Console.ReadKey();
            arduino.Close();
        }

        static void ac_StaticInfoUpdated(object sender, StaticInfoEventArgs e)
        {
            // Print out some data from StaticInfo
            //Console.WriteLine("StaticInfo");
            //Console.WriteLine("  Car Model: " + e.StaticInfo.CarModel);
            //Console.WriteLine("  Track:     " + e.StaticInfo.Track);
            //Console.WriteLine("  Max RPM:   " + e.StaticInfo.MaxRpm);
            
        }

        static void ac_PhysicsUpdated(object sender, PhysicsEventArgs e)
        {
            double speed = Math.Floor(e.Physics.SpeedKmh);
            string strSpeed = "0000" + speed.ToString();
            _speed = strSpeed.Substring(strSpeed.Length - 3);
            _gear = e.Physics.Gear.ToString();            
            _rpm = "00000" + e.Physics.Rpms.ToString();
            _rpm = _rpm.Substring(_rpm.Length - 5);
            
            SendArduino();
        }

        static void ac_GraphicsUpdated(object sender, GraphicsEventArgs e)
        {
            int laps = e.Graphics.CompletedLaps;
            string strLaps = "0000" + laps;
            _laps = strLaps.Substring(strLaps.Length - 2);

            int position = e.Graphics.Position;
            string strPosition = "000" + position;
            _position = strPosition.Substring(strPosition.Length - 2);
        }

        static void SendArduino()
        {
            string texto1 = "1:" + _laps + " " + _gear + " " + _speed;
            string texto2 = "2:" + _position + " " + _rpm;
            Console.WriteLine("Enviando: " + texto1);
            Console.WriteLine("Enviando: " + texto2);
            arduino.Write(texto1 + ";");
            arduino.Write(texto2 + ";");

        }

        static void ReadArduino(object sender, SerialDataReceivedEventArgs e)
        {
            string aux = arduino.ReadLine();
            Debug.WriteLine(aux);
        }
    }
}
