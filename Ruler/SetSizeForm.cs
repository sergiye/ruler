using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ruler
{
	public partial class SetSizeForm : Form
	{
		private readonly int originalWidth;
		private readonly int originalHeight;

		public SetSizeForm(int initWidth, int initHeight)
		{
			InitializeComponent();

			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			originalWidth = initWidth;
			originalHeight = initHeight;

			txtWidth.Text = initWidth.ToString();
			txtHeight.Text = initHeight.ToString();
		}

		private void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void BtnOkClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		public Size GetNewSize()
		{
			int width;
			int height;
		    var size = new Size
		                {
		                    Width = int.TryParse(txtWidth.Text, out width) ? width : originalWidth,
		                    Height = int.TryParse(txtHeight.Text, out height) ? height : originalHeight
		                };
		    return size;
		}
	}
}