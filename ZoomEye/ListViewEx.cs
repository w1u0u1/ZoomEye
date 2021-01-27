using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZoomEye
{
    public class ListViewEx : ListView
    {
        public int CurrentColumn;
        public bool asc;

        public ListViewEx()
        {
            SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }

        public void Export()
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Filter = "Text Documents(*.txt)|*.txt|All Files(*.*)|*.*";

            if (SFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(SFD.FileName);
                    ListView.ColumnHeaderCollection columns = Columns;

                    foreach (ColumnHeader col in columns)
                        sw.Write(col.Text + "\t");

                    sw.Write("\r\n");

                    foreach (ListViewItem item in Items)
                    {
                        for (int i = 0; i < columns.Count; i++)
                        {
                            try
                            {
                                sw.Write(item.SubItems[i].Text + "\t");
                            }
                            catch (Exception ex) { }
                        }
                        sw.Write("\r\n");
                    }

                    sw.Close();
                }
                catch (Exception ex) { }
            }
        }
    }

    public class ListViewItemComparer : System.Collections.IComparer
    {
        int Column;
        int Type;
        int param1;
        bool bAsc;

        public ListViewItemComparer()
        {

        }

        public ListViewItemComparer(int Column, bool bAsc, int Type, int param1)
        {
            this.Column = Column;
            this.Type = Type;
            this.bAsc = bAsc;
            this.param1 = param1;
        }

        public virtual int Compare(Object x, Object y)
        {
            switch (Type)
            {
                case 0:
                    {
                        int item1 = 0;
                        int item2 = 0;

                        try
                        {
                            if (bAsc)
                            {
                                switch (param1)
                                {
                                    case 10:
                                        item1 = Convert.ToInt32(((ListViewItem)(x)).SubItems[Column].Text);
                                        item2 = Convert.ToInt32(((ListViewItem)(y)).SubItems[Column].Text);
                                        break;
                                    case 16:
                                        item1 = Convert.ToInt32(((ListViewItem)(x)).SubItems[Column].Text, 16);
                                        item2 = Convert.ToInt32(((ListViewItem)(y)).SubItems[Column].Text, 16);
                                        break;
                                }
                            }
                            else
                            {
                                switch (param1)
                                {
                                    case 10:
                                        item1 = Convert.ToInt32(((ListViewItem)(y)).SubItems[Column].Text);
                                        item2 = Convert.ToInt32(((ListViewItem)(x)).SubItems[Column].Text);
                                        break;
                                    case 16:
                                        item1 = Convert.ToInt32(((ListViewItem)(y)).SubItems[Column].Text, 16);
                                        item2 = Convert.ToInt32(((ListViewItem)(x)).SubItems[Column].Text, 16);
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return 0;
                        }

                        if (item1 >= item2)
                            return 0;
                        else
                            return 1;
                    }
                    break;
                case 1:
                    {
                        if (!bAsc)
                        {
                            try
                            {
                                return String.Compare(((ListViewItem)(x)).SubItems[Column].Text, ((ListViewItem)(y)).SubItems[Column].Text);
                            }
                            catch (Exception ex)
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            try
                            {
                                return String.Compare(((ListViewItem)(y)).SubItems[Column].Text, ((ListViewItem)(x)).SubItems[Column].Text);
                            }
                            catch (Exception ex)
                            {
                                return 1;
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        if (bAsc)
                        {
                            try
                            {
                                return DateTime.Compare(DateTime.Parse(((ListViewItem)(y)).SubItems[Column].Text), DateTime.Parse(((ListViewItem)(x)).SubItems[Column].Text));
                            }
                            catch (Exception ex) { return 0; }
                        }
                        else
                        {
                            try
                            {
                                return DateTime.Compare(DateTime.Parse(((ListViewItem)(x)).SubItems[Column].Text), DateTime.Parse(((ListViewItem)(y)).SubItems[Column].Text));
                            }
                            catch (Exception ex)
                            {
                                return 1;
                            }
                        }
                    }
                    break;
            }
            return 0;
        }
    }
}
