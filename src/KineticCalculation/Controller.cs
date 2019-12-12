using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Termo
{
    struct DataDSC
    {
        public List<double> T;
        public List<double> t;
        public List<double> DSC;
        public List<double> Mass;
        public List<double> Gas;
        public List<double> Sensit;

        public void Init()
        {
            T = new List<double>();
            t = new List<double>();
            DSC = new List<double>();
            Mass = new List<double>();
            Gas = new List<double>();
            Sensit = new List<double>();
        }
    };


    struct DataTg
    {
        public List<double> T;
        public List<double> Mass;

        public void Init()
        {
            T = new List<double>();
            Mass = new List<double>();
        }
    };


    class Controller
    {
        private Model TermoModel;

      //  private View TermoView;

        List<DataDSC> Data = new List<DataDSC>();
        List<DataTg> DataTg = new List<DataTg>();

        public void ViewDSC()
        {
         ///   TermoView.ShowDCSInit(Data);
        }

        public void ViewTg()
        {
           // TermoView.ShowTgInit(DataTg);
        }
        
        public void ReadDSC(string FileName)
        {
            DataDSC newDSC = new DataDSC();
            newDSC.Init();
            FileStream fs = new FileStream(FileName, FileMode.Open,FileAccess.Read);

            using (StreamReader streamReader = new StreamReader(fs, Encoding.ASCII))
            {
                string line = String.Empty;
                while (((line = streamReader.ReadLine()) != null) && (line.StartsWith("##") == false)) ;
                
                while (((line = streamReader.ReadLine()) != null)&&(line!=""))
                {
                    line = System.Text.RegularExpressions.Regex.Replace(line, @"\s+", " "); // удаляем лишние пробелы
                    line = line.Trim();
                    string []arr = line.Split();
                    newDSC.T.Add(double.Parse(arr[0]));
                    newDSC.t.Add(double.Parse(arr[1]));
                    newDSC.DSC.Add(double.Parse(arr[2]));
                    newDSC.Mass.Add(double.Parse(arr[3])); // Tg
                    
                }
            }

            Data.Add(newDSC);
        }

        public void ReadTg(string FileName)
        {
            DataTg newTg = new DataTg();
            newTg.Init();
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);

            using (StreamReader streamReader = new StreamReader(fs, Encoding.ASCII))
            {
                string line = String.Empty;
               // while (((line = streamReader.ReadLine()) != null) && (line.StartsWith("##") == false)) ;

                while (((line = streamReader.ReadLine()) != null) && (line != ""))
                {
                    line = System.Text.RegularExpressions.Regex.Replace(line, @"\s+", " "); // удаляем лишние пробелы
                    line = line.Trim();
                    string[] arr = line.Split();
                    newTg.T.Add(double.Parse(arr[0]));
                    newTg.Mass.Add(double.Parse(arr[1])); // Tg

                }
            }

            DataTg.Add(newTg);
        }


    }
}
