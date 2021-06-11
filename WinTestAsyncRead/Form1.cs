using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AsyncStream;
using System.IO;
using System.Reflection;

namespace WinTestAsyncRead
{
    delegate void SetValueDelegate(Object obj, Object val, Object[] index);
    delegate object GetValueDelegate(Object obj, Object[] index);

    public partial class Form1 : Form
    {
        string fileName = "";
        public Form1()
        {
            InitializeComponent();
            label2.Text = AsyncStreamState.None.ToString();

        }

        AsyncStreamReader reader;

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
                reader = new AsyncStreamReader(fileName);
                reader.OnReadedBytes += new EventHandler<AsyncReadEventArgs>(reader_OnReadedBytes);
                reader.OnEndRead += new EventHandler<AsyncReadEventArgs>(reader_OnEndRead);
                reader.OnStateChanged += new EventHandler<AsyncStreamStateChangeArgs>(reader_OnStateChanged);
                reader.OnError += new EventHandler<AsyncReadErrorEventArgs>(reader_OnError);
                SetControlProperty(label2, "Text", reader.State.ToString());
            }
        }

        void reader_OnError(object sender, AsyncReadErrorEventArgs e)
        {
            if (MessageBox.Show(e.Error.Message + Environment.NewLine + e.Error.InnerException, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
            {
                SetControlProperty(label2, "Text", reader.State.ToString());
            }
        }

        void reader_OnStateChanged(object sender, AsyncStreamStateChangeArgs e)
        {
            SetControlProperty(label2, "Text", e.CurrentState.ToString());
        }

        void reader_OnEndRead(object sender, AsyncReadEventArgs e)
        {
            textBox1.Text = Encoding.UTF8.GetString(e.Result);
        }

        void reader_OnReadedBytes(object sender, AsyncReadEventArgs e)
        {
            try
            {
                object m = e.PercentReaded;
                SetControlProperty(progressBar1, "Value", m);
                SetControlProperty(label3, "Text", m.ToString() + "%");
            }
            catch (Exception exc)
            {
                if (MessageBox.Show(exc.Message + Environment.NewLine + exc.InnerException, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error) == DialogResult.OK)
                    return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string fileName = new FileInfo(reader.Path).Name;
            reader.BeginRead();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            reader.StopRead(true);
        }

        bool pause = false;

        private void button4_Click(object sender, EventArgs e)
        {
            if (!pause)
            {
                reader.PauseRead();
                button4.Text = "Resume";
                pause = true;
            }
            else
            {
                reader.ResumeRead();
                button4.Text = "Pause";
                pause = false;
            }
        }

        #region Control properties (SetControlProperty, GetControlProperty)

        private void SetControlProperty(Control ctrl, String propName, Object val)
        {
            PropertyInfo propInfo = ctrl.GetType().GetProperty(propName);
            Delegate dgtSetValue = new SetValueDelegate(propInfo.SetValue);
            ctrl.Invoke(dgtSetValue, new Object[3] { ctrl, val, /*index*/null });
        }
        private object GetControlProperty(Control ctrl, String propName)
        {
            PropertyInfo propInfo = ctrl.GetType().GetProperty(propName);
            Delegate dgtGetValue = new GetValueDelegate(propInfo.GetValue);
            object o = ctrl.Invoke(dgtGetValue, new Object[2] { ctrl, /*index*/null });
            return o;
        }

        #endregion
    }
}
