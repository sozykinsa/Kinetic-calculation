using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.Windows.Forms;
using System.IO;

namespace Termo
{
    public  class TermoSettings  
                                 
                                 
    {
        public Configuration  Config=null;
        public ClientSettingsSection Termosection=null;
        public int RowCnt=0;         
        public int ColCnt=0;         
        public bool isAutoLoad = false;
        public string LeftCursor="44", RightCursor="100";

        public TermoSettings()   
        {
            try   
            {
                Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                Termosection = (ClientSettingsSection)Config.GetSectionGroup("Termo.InitFiles").Sections["Termo.Properties.Settings"];
                isAutoLoad = true;
                LeftCursor= Properties.Settings.Default.LefCursorValue;
                RightCursor= Properties.Settings.Default.RightCursorValue;
            }
            catch
            {
                Config = null;  isAutoLoad = false;    
            }

        }
        public void LoadCursorSettings(Label Left, Label Right)
        {
            Left.Text = LeftCursor;
            Right.Text = RightCursor;
        }

        public bool SetDataGridInfo(DataGridView dg)     
             
        {
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
                         

                        for (int k=0; k< RowCnt; k++)
                        {
                            for (int j=0; j<ColCnt; j++)
                            {
                                var C_str = Termosection.Settings.Get(Convert.ToString(k)+'_'+Convert.ToString(j));
                                if (C_str != null && File.Exists(C_str.Value.ValueXml.InnerText))
                                {
                                    dg[j,k].Value= C_str.Value.ValueXml.InnerText; ;flgset = true;
                                }
                            }
                        }

                         
                        bool isSet = false;
                        for (int i = RowCnt - 1; i >= 0; i--)
                        {
                            for (int j=0; j<ColCnt; j++)
                            {
                                if (Convert.ToString(dg[j, i].Value) != "")
                                {
                                    isSet = true;
                                }
                                else
                                {   
                                    Termosection.Settings.Remove(Termosection.Settings.Get(Convert.ToString(i) + '_' + Convert.ToString(j)));
                                }
                            }
                            if (!isSet)
                            { DataGridViewRow delrow = dg.Rows[i]; dg.Rows.Remove(delrow);
                              RowCnt = RowCnt - 1;
                            }
                        }
                        Properties.Settings.Default.InitRowCount = RowCnt;
                         
                        isSet = false;
                        for (int i = ColCnt - 1; i >= 0; i--)
                        {
                            for (int j = 0; j < RowCnt; j++)
                            {
                                if (Convert.ToString(dg[i, j].Value) != "")
                                {
                                    isSet = true; break;
                                }
                            }
                                if (!isSet)
                                {
                                    DataGridViewColumn delcol = dg.Columns[i]; dg.Columns.Remove(delcol);
                                    ColCnt = ColCnt - 1;
                                }
                        }
                        Properties.Settings.Default.InitRowCount = RowCnt;
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
            dg.Rows.Clear();
            dg.Columns.Clear();
            dg.Columns.Add("col0", "... K/min");
            RowCnt = 0; Properties.Settings.Default.InitRowCount = RowCnt;
            ColCnt = 0; Properties.Settings.Default.InitColumnCount = ColCnt;
            Termosection.Settings.Clear();
            
            Config.Save();
            ConfigurationManager.RefreshSection("Termo.InitFiles/Termo.Properties.Settings");
        }

        public void GetDataGridInfo(DataGridView dg,Label LeftCursor, Label RightCursor)   
        {
            if (isAutoLoad)
            {
                RowCnt = dg.Rows.Count - 1; ColCnt = dg.Columns.Count - 1;
                Properties.Settings.Default.InitRowCount = RowCnt;
                Properties.Settings.Default.InitColumnCount = ColCnt;
                Properties.Settings.Default.LefCursorValue = LeftCursor.Text;
                Properties.Settings.Default.RightCursorValue = RightCursor.Text;

                for (int i = 0; i < dg.Rows.Count; i++)
                    for (int j = 0; j < dg.Columns.Count; j++)
                    {
                        if (dg[j, i].Value != null)
                        {
                             
                            string Key = Convert.ToString(i) + '_' + Convert.ToString(j);
                            var C_str = Termosection.Settings.Get(Key);
                            if (C_str != null)
                            {
                                Termosection.Settings.Remove(C_str);
                            }

                             
                            var newSetting = new SettingElement(Key, SettingsSerializeAs.String);
                            newSetting.Value = new SettingValueElement();
                            newSetting.Value.ValueXml = new XmlDocument().CreateElement("value");
                            newSetting.Value.ValueXml.InnerText = (string)dg[j, i].Value;

                             

                            Termosection.Settings.Add(newSetting);
                        }
                    }
                Config.Save();
                ConfigurationManager.RefreshSection("Termo.InitFiles/Termo.Properties.Settings");
                Properties.Settings.Default.Save();
            }
        }
    }
}
