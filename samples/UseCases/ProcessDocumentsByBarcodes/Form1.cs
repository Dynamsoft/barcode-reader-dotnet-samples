using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using Dynamsoft;
using Dynamsoft.DBR;

namespace ProcessDocumentsByBarcodes
{
    public partial class Form1 : Form
    {
        private Mode mode = Mode.Rename;
        private Label rdbSelectedFormat = null;
        readonly string strMessageBoxCaption = "Barcode Document Processing Demo";
        private readonly BarcodeReader barcodeReader;
        string strRenameDocumentFolder = "";
        string strSplitDocumentFolder = "";
        string strClassifyDocumentFolder = "";
        Label lbRenameLastFormat = null;
        Label lbSplitLastFormat = null;
        Label lbClassifyLastFormat = null;
        private int formatid = (int)EnumBarcodeFormat.BF_ALL;

        public Form1()
        {
            InitializeComponent();
            InitialDefaultValue();

            // Initialize license
            /*
            // By setting organizaion ID as "200001", a 7-day trial license will be used for license verification.
            // Note that network connection is required for this license to work.
            //
            // When using your own license, locate the following line and specify your Organization ID.
            // organizationID = "200001";
            //
            // If you don't have a license yet, you can request a trial from https://www.dynamsoft.com/customer/license/trialLicense?product=dbr&utm_source=samples&package=dotnet
            */
            DMDLSConnectionParameters connectionInfo = BarcodeReader.InitDLSConnectionParameters();
            connectionInfo.OrganizationID = "200001";
            EnumErrorCode errorCode = BarcodeReader.InitLicenseFromDLS(connectionInfo, out string errorMsg);
            if (errorCode != EnumErrorCode.DBR_SUCCESS)
            {
                MessageBox.Show("License initiation failed: " + errorMsg, strMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Create an instance of Barcode Reader
            barcodeReader = new BarcodeReader();
        }

        private void InitialDefaultValue()
        {
            string strDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            if (strDesktop.EndsWith(Path.DirectorySeparatorChar.ToString()))
                tbOutputFolder.Text = strDesktop + "temp";
            else
                tbOutputFolder.Text = strDesktop + Path.DirectorySeparatorChar + "temp";

            lbRenameLastFormat = lbUnknown;
            lbSplitLastFormat = lbUnknown;
            lbClassifyLastFormat = lbUnknown;
            rdbSelectedFormat = lbUnknown;
            int index = Environment.CurrentDirectory.LastIndexOf("bin");
            if (index >= 0)
            {
                string dir = Environment.CurrentDirectory.Substring(0, index);
                dir += "Demo";
                if (Directory.Exists(dir))
                {
                    strRenameDocumentFolder = dir + Path.DirectorySeparatorChar + "Rename";
                    strSplitDocumentFolder = dir + Path.DirectorySeparatorChar + "Split";
                    strClassifyDocumentFolder = dir + Path.DirectorySeparatorChar + "Classify";
                    lbRenameLastFormat = lbEAN13;
                    lbSplitLastFormat = lbCode128;
                    lbClassifyLastFormat = lbCode39;
                }
            }

            Mode_Changed(rdbRename, null);
            lbProcess.Focus();
            lbModeInfo.Visible = false;
        }



        #region Move Window
        private Point m_mosPosition = Point.Empty;
        private void lbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            m_mosPosition = new Point(-e.X, -e.Y);
        }

        private void lbTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point location = Control.MousePosition;
                location.Offset(m_mosPosition);
                this.Location = location;
            }
        }
        #endregion

        /// <summary>
        /// Close main window
        /// </summary>
        private void lbClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// minimize main window
        /// </summary>
        private void lbMinimum_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        #region Mode

        enum Mode
        {
            Rename,
            Classify,
            Split
        }

        private void Mode_Changed(object sender, EventArgs e)
        {
            Label lb = (Label)sender;
            string tag = lb.Tag.ToString();
            picboxMode.Image = (Image)Properties.Resources.ResourceManager.GetObject(tag);
            lbRename.ForeColor = Color.Black;
            lbSplit.ForeColor = Color.Black;
            lbClassify.ForeColor = Color.Black;
            rdbRename.Image = Properties.Resources.radio_unchecked;
            rdbSplit.Image = Properties.Resources.radio_unchecked;
            rdbClassify.Image = Properties.Resources.radio_unchecked;
            switch (tag)
            {
                case "rename":
                    lbRename.ForeColor = Color.Green;
                    rdbRename.Image = Properties.Resources.radio_checked;
                    mode = Mode.Rename;
                    tbInputFolder.Text = strRenameDocumentFolder;
                    if (lbRenameLastFormat != null)
                        Format_Changed(lbRenameLastFormat, null);
                    break;
                case "split":
                    lbSplit.ForeColor = Color.Green;
                    rdbSplit.Image = Properties.Resources.radio_checked;
                    mode = Mode.Split;
                    tbInputFolder.Text = strSplitDocumentFolder;
                    if (lbSplitLastFormat != null)
                        Format_Changed(lbSplitLastFormat, null);
                    break;
                case "classify":
                    lbClassify.ForeColor = Color.Green;
                    rdbClassify.Image = Properties.Resources.radio_checked;
                    mode = Mode.Classify;
                    tbInputFolder.Text = strClassifyDocumentFolder;
                    if (lbClassifyLastFormat != null)
                        Format_Changed(lbClassifyLastFormat, null);
                    break;
                default: break;
            }
        }

        #endregion

        #region Barcode Format

        private void Format_Changed(object sender, EventArgs e)
        {
            Label lb = (Label)sender;
            string tag = lb.Tag == null ? null : lb.Tag.ToString();
            if (tag == null || tag.Length == 0)
            {
                Point loc = lb.Location;
                loc.X = loc.X - 8;
                loc.Y = loc.Y + 8;
                Control control = GetChildAtPoint(loc);
                if (control is Label)
                    Format_Changed(control, null);
            }
            else if (tag.StartsWith("bf-"))
            {
                tag = tag.Substring(3);
                rdbSelectedFormat.Image = Properties.Resources.radio_unchecked;
                rdbSelectedFormat = lb;
                rdbSelectedFormat.Image = Properties.Resources.radio_checked;
                switch (tag)
                {
                    case "code 39":
                        formatid = (int)EnumBarcodeFormat.BF_CODE_39;
                        break;
                    case "code 93":
                        formatid = (int)EnumBarcodeFormat.BF_CODE_93;
                        break;
                    case "code 128":
                        formatid = (int)EnumBarcodeFormat.BF_CODE_128;
                        break;
                    case "codabar":
                        formatid = (int)EnumBarcodeFormat.BF_CODABAR;
                        break;
                    case "ean-13":
                        formatid = (int)EnumBarcodeFormat.BF_EAN_13;
                        break;
                    case "ean-8":
                        formatid = (int)EnumBarcodeFormat.BF_EAN_8;
                        break;
                    case "upc-a":
                        formatid = (int)EnumBarcodeFormat.BF_UPC_A;
                        break;
                    case "upc-e":
                        formatid = (int)EnumBarcodeFormat.BF_UPC_E;
                        break;
                    case "interleaved 2 of 5":
                        formatid = (int)EnumBarcodeFormat.BF_ITF;
                        break;
                    case "industrial 2 of 5":
                        formatid = (int)EnumBarcodeFormat.BF_INDUSTRIAL_25;
                        break;
                    case "qrcode":
                        formatid = (int)EnumBarcodeFormat.BF_QR_CODE;
                        break;
                    case "pdf417":
                        formatid = (int)EnumBarcodeFormat.BF_PDF417;
                        break;
                    case "datamatrix":
                        formatid = (int)EnumBarcodeFormat.BF_DATAMATRIX;
                        break;
                    case "aztec":
                        formatid = (int)EnumBarcodeFormat.BF_AZTEC;
                        break;
                    default: formatid = (int)EnumBarcodeFormat.BF_ALL; break;
                }
            }
        }

        #endregion

        private void lbInputBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            dlg.Description = "Select the folder where the documents are in.";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbInputFolder.Text = dlg.SelectedPath;
            }
        }

        private void lbOutputBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            dlg.Description = "Select the folder where the output documents will be put in.";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                tbOutputFolder.Text = dlg.SelectedPath;
            }
        }

        /// <summary>
        /// Process documents
        /// </summary>
        private void lbProcess_Click(object sender, EventArgs e)
        {
            // check settings
            string strInputFolder = tbInputFolder.Text.Trim().TrimEnd(Path.DirectorySeparatorChar);
            string strOutputFolder = tbOutputFolder.Text.Trim().TrimEnd(Path.DirectorySeparatorChar);
            if (strInputFolder.Length == 0)
            {
                MessageBox.Show("Input folder is not set.", strMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (strOutputFolder.Length == 0)
            {
                MessageBox.Show("Output folder is not set.", strMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Directory.Exists(strInputFolder))
            {
                MessageBox.Show("Input folder doesn't exist.", strMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Directory.Exists(strOutputFolder))
            {
                bool bReturn = true;
                if (MessageBox.Show("Output folder doesn't exist. Would you like to create it?", strMessageBoxCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    try
                    {
                        Directory.CreateDirectory(strOutputFolder);
                        bReturn = false;
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show("Failed to create folder. " + exp.Message, strMessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                if (bReturn)
                    return;
            }

            tbLog.Clear();
            AppendLogText("Start processing...");

            string[] files = Directory.GetFiles(strInputFolder);
            if (files != null)
            {
                foreach (string strFile in files)
                {
                    if (IsImageFile(strFile))
                    {
                        try
                        {
                            AppendLogText(string.Format("Processing file {0}", strFile));

                            PublicRuntimeSettings tempParameterSettings = barcodeReader.GetRuntimeSettings();
                            tempParameterSettings.BarcodeFormatIds = formatid;
                            barcodeReader.UpdateRuntimeSettings(tempParameterSettings);

                            TextResult[] barcodes = barcodeReader.DecodeFile(strFile, "");
                            if (barcodes == null || barcodes.Length <= 0)
                            {
                                AppendLogText("There is no barcode detected.");
                            }
                            else
                            {
                                switch (mode)
                                {
                                    case Mode.Rename:
                                        DoRename(strFile, strOutputFolder, barcodes);
                                        strRenameDocumentFolder = strInputFolder;
                                        lbRenameLastFormat = rdbSelectedFormat;
                                        break;
                                    case Mode.Split:
                                        DoSplit(strFile, strOutputFolder, barcodes);
                                        strSplitDocumentFolder = strInputFolder;
                                        lbSplitLastFormat = rdbSelectedFormat;
                                        break;
                                    case Mode.Classify:
                                        DoClassify(strFile, strOutputFolder, barcodes);
                                        strClassifyDocumentFolder = strInputFolder;
                                        lbClassifyLastFormat = rdbSelectedFormat;
                                        break;
                                }
                            }
                        }
                        catch (Exception exp)
                        {
                            AppendLogText(exp.Message);
                        }
                    }
                }
            }

            AppendLogText("Completed.");
        }

        /// <summary>
        /// Using barcode value to rename documents.
        /// </summary>
        /// <param name="strFile">path of source file needs to be renamed</param>
        /// <param name="strOutputFolder">path of folder that renamed documents saved in</param>
        /// <param name="barcodes">detected barcode results</param>
        private void DoRename(string strFile, string strOutputFolder, TextResult[] barcodes)
        {
            // In this mode, we take the first barcode result as the new file name
            string strBarcodeText = barcodes[0].BarcodeText;
            if (strBarcodeText.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                AppendLogText(string.Format("{0} contains character(s) that are not allowed in file names, skip renaming.", strBarcodeText));
            }
            else
            {
                FileInfo fileInfo = new FileInfo(strFile);
                string strOutputFileName = strBarcodeText + fileInfo.Extension;
                string strOutputFile = strOutputFolder + Path.DirectorySeparatorChar + strOutputFileName;
                if (File.Exists(strOutputFile))
                {
                    AppendLogText(string.Format("{0} exists, skip renaming.", strOutputFile));
                }
                else
                {
                    fileInfo.CopyTo(strOutputFile);
                    AppendLogText(string.Format("Barcode Value: {0}", strBarcodeText));
                    AppendLogText(string.Format("Renamed to {0}.", strOutputFile));
                }
            }
        }

        /// <summary>
        /// Using barcode value to split multi-page documents.
        /// </summary>
        /// <param name="strFile">path of source file needs to be splitted</param>
        /// <param name="strOutputFolder">the path of folder that splitted documents saved in</param>
        /// <param name="barcodes">detected barcode results</param>
        private void DoSplit(string strFile, string strOutputFolder, TextResult[] barcodes)
        {
            if (!strFile.EndsWith(".tiff", true, System.Globalization.CultureInfo.CurrentCulture) && !strFile.EndsWith(".tif", true, System.Globalization.CultureInfo.CurrentCulture))
            {
                AppendLogText("It's not a tiff file, skip splitting.");
                return;
            }

            List<int> separators = new List<int>();
            List<string> values = new List<string>();
            foreach (TextResult result in barcodes)
            {
                if (result.LocalizationResult.PageNumber >= 0)
                {
                    if (!separators.Contains(result.LocalizationResult.PageNumber))
                    {
                        separators.Add(result.LocalizationResult.PageNumber);
                        string strBarcodeText = result.BarcodeText;
                        if (result.Exception != null && !result.Exception.Contains(((int)EnumErrorCode.DMERR_TRIAL_LICENSE).ToString()))
                            strBarcodeText = strBarcodeText.Substring(strBarcodeText.IndexOf("]") + 2);
                        values.Add(strBarcodeText);
                    }
                }
            }


            Image img = Image.FromFile(strFile);
            int iFrameCount = 1;
            FrameDimension dimension = FrameDimension.Page;
            if (img.FrameDimensionsList != null && img.FrameDimensionsList.Length > 0)
            {
                dimension = new FrameDimension(img.FrameDimensionsList[0]);
                iFrameCount = img.GetFrameCount(dimension);
            }

            for (int i = 1; i <= separators.Count; i++)
            {
                int start = separators[i - 1];
                int end = start;
                if (i != separators.Count)
                    end = separators[i];
                else
                    end = iFrameCount;

                string strOutputFileName = values[i - 1] + ".tiff";
                string strOutputFile = strOutputFolder + Path.DirectorySeparatorChar + strOutputFileName;

                if (File.Exists(strOutputFile))
                {
                    AppendLogText(string.Format("{0} exists, skip splitting pages({1}-{2}) in {3}", strOutputFile, start + 1, end, strFile));
                }

                ImageCodecInfo tiffCodeInfo = null;
                ImageCodecInfo[] codeinfos = ImageCodecInfo.GetImageDecoders();
                foreach (ImageCodecInfo codeinfo in codeinfos)
                {
                    if (codeinfo.FormatID == ImageFormat.Tiff.Guid)
                    {
                        tiffCodeInfo = codeinfo;
                        break;
                    }
                }

                System.Drawing.Imaging.EncoderParameters encoderParams = null;
                if (end - start == 1)
                {
                    encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                    encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)System.Drawing.Imaging.EncoderValue.CompressionLZW);
                }
                else
                {
                    encoderParams = new System.Drawing.Imaging.EncoderParameters(2);
                    encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)System.Drawing.Imaging.EncoderValue.CompressionLZW);
                    encoderParams.Param[1] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)System.Drawing.Imaging.EncoderValue.MultiFrame);
                }

                img.SelectActiveFrame(dimension, start);
                img.Save(strOutputFile, tiffCodeInfo, encoderParams);
                start++;
                if (start < end)
                {
                    encoderParams.Param[1] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.SaveFlag, (long)System.Drawing.Imaging.EncoderValue.FrameDimensionPage);
                    for (int k = start; k < end; k++)
                    {
                        img.SelectActiveFrame(dimension, k);
                        img.SaveAdd(img, encoderParams);
                    }
                }
                AppendLogText(string.Format("Page: {0}", separators[i - 1] + 1));
                AppendLogText(string.Format("Barcode Value: {0}", values[i - 1]));
                AppendLogText(string.Format("Splitted it to file: {0}", strOutputFile));
            }

            img.Dispose();
        }

        /// <summary>
        /// Using barcode value to classify documents.
        /// </summary>
        /// <param name="strFile">path of source file needs to be classified</param>
        /// <param name="strOutputFolder">path of folder that classified documents saved in</param>
        /// <param name="barcodes">detected barcode results</param>
        private void DoClassify(string strFile, string strOutputFolder, TextResult[] barcodes)
        {
            // In this mode, we take the first barcode result as the parent folder name
            string strBarcodeText = barcodes[0].BarcodeText;
            if (strBarcodeText.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                AppendLogText(string.Format("{0} contains character(s) that are not allowed in file names, skip classifying.", strBarcodeText));
            }
            else
            {
                FileInfo fileInfo = new FileInfo(strFile);
                string strOutputFileName = fileInfo.Name;
                string strOutDir = strOutputFolder + Path.DirectorySeparatorChar + strBarcodeText + Path.DirectorySeparatorChar;
                if (!Directory.Exists(strOutDir))
                    Directory.CreateDirectory(strOutDir);

                string strOutputFile = strOutDir + strOutputFileName;
                if (File.Exists(strOutputFile))
                {
                    AppendLogText(string.Format("{0} exists, skip classifying.", strOutputFile));
                }
                else
                {
                    fileInfo.CopyTo(strOutputFile);
                    AppendLogText(string.Format("Barcode Value: {0}", strBarcodeText));
                    AppendLogText(string.Format("Classified to {0}.", strOutputFile));
                }
            }
        }


        private bool IsImageFile(string strFileName)
        {
            bool ret = false;
            if (strFileName != null)
            {
                strFileName = strFileName.ToLower();
                if (strFileName.EndsWith(".tiff") || strFileName.EndsWith(".tif") ||
                    strFileName.EndsWith(".jpeg") || strFileName.EndsWith(".jpg") || strFileName.EndsWith(".jpe") || strFileName.EndsWith(".jfif") ||
                    strFileName.EndsWith(".bmp") || strFileName.EndsWith(".png") || strFileName.EndsWith(".gif"))
                    ret = true;
            }
            return ret;
        }

        private void AppendLogText(string strText)
        {
            tbLog.AppendText(strText + Environment.NewLine);
            tbLog.Refresh();
        }

        #region button mouse event

        private void lbButton_MouseEnter(object sender, EventArgs e)
        {
            Label lb = (Label)sender;
            string strTag = lb.Tag.ToString();
            lb.Image = (Image)Properties.Resources.ResourceManager.GetObject(strTag + "_enter");
        }

        private void lbButton_MouseLeave(object sender, EventArgs e)
        {
            Label lb = (Label)sender;
            string strTag = lb.Tag.ToString();
            lb.Image = (Image)Properties.Resources.ResourceManager.GetObject(strTag + "_normal");
        }

        private void lbButton_MouseDown(object sender, MouseEventArgs e)
        {
            Label lb = (Label)sender;
            string strTag = lb.Tag.ToString();
            lb.Image = (Image)Properties.Resources.ResourceManager.GetObject(strTag + "_down");
        }

        private void lbButton_MouseUp(object sender, MouseEventArgs e)
        {
            Label lb = (Label)sender;
            string strTag = lb.Tag.ToString();
            lb.Image = (Image)Properties.Resources.ResourceManager.GetObject(strTag + "_enter");

            if (lb.Name.Contains("Browse"))
            {
                Point point = lb.PointToClient(Control.MousePosition);
                if (!lb.ClientRectangle.Contains(point))
                    lbButton_MouseLeave(lb, null);
            }
        }

        private void lbMode_MouseHover(object sender, EventArgs e)
        {
            Label lb = sender as Label;
            if (lb != null && lb.Tag != null)
            {
                switch (lb.Tag.ToString())
                {
                    case "rename":
                        lbModeInfo.Text = "Use the barcodes on the first pages to rename the output documents.";
                        break;
                    case "split":
                        lbModeInfo.Text = "Use the barcodes found anywhere in the input file to create new documents.";
                        break;
                    case "classify":
                        lbModeInfo.Text = "Classify documents into individual output folders by barcodes on the first pages of the input files.";
                        break;
                    default: lbModeInfo.Text = ""; break;
                }

                if (lbModeInfo.Text != null && lbModeInfo.Text.Length > 0)
                {
                    lbModeInfo.Location = new Point(lb.Right + 10, lb.Top);
                    lbModeInfo.Visible = true;
                }
            }
        }

        private void lbMode_MouseLeave(object sender, EventArgs e)
        {
            if (lbModeInfo.Visible)
                lbModeInfo.Visible = false;
        }

        #endregion

    }
}
