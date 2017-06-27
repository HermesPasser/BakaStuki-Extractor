using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BakaTsukiFormater
{
    static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            
            //Console.WriteLine("BakaTsuki Page Formater by Hermes Passer (gladiocitrico.blogspot.com)");

            //switch (args.Length)
            //{
            //    case 0:
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form1());
                //    break;

                //case 2:
                //    //code...
                //    break;
                //default:
                //    Console.WriteLine("Wrong number of arguments.");
                //    Console.WriteLine("No arguments: Open the gui.");
                //    Console.WriteLine("Two arguments: [file-to-be-formated-or-url] [destination-path]");
                //    break;
            //}

        }
    }
}
