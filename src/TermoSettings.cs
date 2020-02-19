using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.Windows.Forms;

namespace Termo
{
    public  class TermoSettings
    {

        public int RowCnt=0;
        public int ColCnt=0;
//        public bool isAutoLoad = false;

        public TermoSettings()
        {   

        }

        public bool SetDataGridInfo(DataGridView dg)
        {
            Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ClientSettingsSection Termosection = (ClientSettingsSection)Config.GetSectionGroup("Termo.InitFiles").Sections["Termo.Properties.Settings"];

            if (Config != null)
            {
                dg.Rows.Clear(); dg.Columns.Clear();
                RowCnt = Properties.Settings.Default.InitRowCount;
                ColCnt = Properties.Settings.Default.InitColumnCount;
                try
                {
                    if (RowCnt > 0 && ColCnt > 0)
                    {
                        for (int k = 0; k <= ColCnt; k++)
                        {
                            dg.Columns.Add("col"+ Convert.ToString(k), "... K/min");

                        }
                        for (int k = 0; k < RowCnt; k++)
                        {
                            dg.Rows.Add(new DataGridViewRow());
                        }
                        bool flgset = false;
                        //Termosection = (ClientSettingsSection)Config.GetSectionGroup("Termo.InitFiles").Sections["Termo.Properties.Settings"];
                        for (int k=0; k< RowCnt; k++)
                        {
                            for (int j=0; j<ColCnt; j++)
                            {
                                var C_str = Termosection.Settings.Get(Convert.ToString(k)+'_'+Convert.ToString(j));
                                if (C_str != null)
                                {
                                    dg[j,k].Value= C_str.Value.ValueXml.InnerText; ;flgset = true;
                                }
                            }
                        }
                        return flgset;
                    }
                    else
                    {
                        ClearDataGrid(dg); return false;
                    }
                }
                catch (Exception e)
                {
                    ClearDataGrid(dg); return false;
                }
            }
            else
            {
                return false;
            }
        }
        
        public void ClearDataGrid(DataGridView dg)
        {
            Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ClientSettingsSection Termosection = (ClientSettingsSection)Config.GetSectionGroup("Termo.InitFiles").Sections["Termo.Properties.Settings"];

            dg.Rows.Clear();
            dg.Columns.Clear();
            dg.Columns.Add("col0", "... K/min");
            RowCnt = 0; Properties.Settings.Default.InitRowCount = RowCnt;
            ColCnt = 0; Properties.Settings.Default.InitColumnCount = ColCnt;
            Termosection.Settings.Clear();
            
            Config.Save();
            ConfigurationManager.RefreshSection("Termo.InitFiles/Termo.Properties.Settings");
        }

        public void GetDataGridInfo(DataGridView dg)  //cохраняет состояние DataGridView в .config
        {
            Configuration Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ClientSettingsSection Termosection = (ClientSettingsSection)Config.GetSectionGroup("Termo.InitFiles").Sections["Termo.Properties.Settings"];

            RowCnt = dg.Rows.Count - 1;  ColCnt = dg.Columns.Count-1;
            Properties.Settings.Default.InitRowCount = RowCnt;
            Properties.Settings.Default.InitColumnCount = ColCnt;
            for (int i=0; i<dg.Rows.Count; i++)
                for (int j=0; j<dg.Columns.Count;j++)
                {
                    if (dg[j,i].Value != null)
                    {
                        // получаем значение параметра Str
                        string Key = Convert.ToString(i) + '_' + Convert.ToString(j);
                        var C_str = Termosection.Settings.Get(Key);
                        if (C_str != null)
                        {
                            Termosection.Settings.Remove(C_str);
                        }

                        // вручную создаем параметр с новым значением
                        var newSetting = new SettingElement(Key, SettingsSerializeAs.String);
                        newSetting.Value = new SettingValueElement();
                        newSetting.Value.ValueXml = new XmlDocument().CreateElement("value");
                        newSetting.Value.ValueXml.InnerText = (string)dg[j,i].Value;

                        // заменяем старый параметр на новый

                        Termosection.Settings.Add(newSetting);
                    }
                }

            Config.Save();
            ConfigurationManager.RefreshSection("Termo.InitFiles/Termo.Properties.Settings");
            Properties.Settings.Default.Save();
        }
    }
}
