using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Demo.WindowsForms.Forms
{
   public partial class Message : Form
   {
      public Message()
      {
         InitializeComponent();
      }

      private void button2_Click(object sender, EventArgs e)
      {
         Close();
      }

      private void button1_Click(object sender, EventArgs e)
      {
         Close();
      }
   }
}
