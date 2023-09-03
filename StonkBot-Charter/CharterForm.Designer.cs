using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StonkBotChartoMatic
{
    public sealed partial class CharterForm : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private Chart chart1;
        private Chart chart2;

        private void InitializeComponent()
        {
            ChartArea chartArea1 = new ChartArea();
            Series series1 = new Series();
            ChartArea chartArea2 = new ChartArea();
            Series series2 = new Series();
            chart1 = new Chart();
            chart2 = new Chart();
            DatePicker = new DateTimePicker();
            CandleDrop = new ComboBox();
            MarketDrop = new ComboBox();
            UpdateButton = new Button();
            //ShowLabels = new CheckBox();
            ChartTypeDrop = new ComboBox();
            TextField = new Label();
            ((System.ComponentModel.ISupportInitialize)chart1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chart2).BeginInit();
            SuspendLayout();
            // 
            // chart1
            // 
            chart1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chart1.BorderlineColor = System.Drawing.Color.Transparent;
            chartArea1.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea1);
            chart1.Location = new System.Drawing.Point(0, 0);
            chart1.Margin = new Padding(4, 3, 4, 3);
            chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Name = "Series1";
            chart1.Series.Add(series1);
            chart1.Size = new System.Drawing.Size(1101, 400);
            chart1.TabIndex = 0;
            // 
            // chart2
            // 
            chart2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chart2.BorderlineColor = System.Drawing.Color.Transparent;
            chartArea2.Name = "ChartArea1";
            chart2.ChartAreas.Add(chartArea2);
            chart2.Location = new System.Drawing.Point(0, 392);
            chart2.Margin = new Padding(4, 3, 4, 3);
            chart2.Name = "chart2";
            series2.ChartArea = "ChartArea1";
            series2.Name = "Series1";
            chart2.Series.Add(series2);
            chart2.Size = new System.Drawing.Size(1101, 135);
            chart2.TabIndex = 1;
            // 
            // DatePicker
            // 
            DatePicker.Anchor = AnchorStyles.Bottom;
            DatePicker.Location = new System.Drawing.Point(152, 548);
            DatePicker.Margin = new Padding(4, 3, 4, 3);
            DatePicker.Name = "DatePicker";
            DatePicker.Size = new System.Drawing.Size(233, 23);
            DatePicker.TabIndex = 2;
            DatePicker.ValueChanged += DatePicker_ValueChanged;
            // 
            // CandleDrop
            // 
            CandleDrop.Anchor = AnchorStyles.Bottom;
            CandleDrop.Items.AddRange(new object[] { "1", "2", "5" });
            CandleDrop.Location = new System.Drawing.Point(540, 548);
            CandleDrop.Margin = new Padding(4, 3, 4, 3);
            CandleDrop.Name = "CandleDrop";
            CandleDrop.Size = new System.Drawing.Size(116, 23);
            CandleDrop.TabIndex = 0;
            CandleDrop.SelectedIndexChanged += CandleDrop_SelectedIndexChanged;
            // 
            // MarketDrop
            // 
            MarketDrop.Anchor = AnchorStyles.Bottom;
            MarketDrop.FormattingEnabled = true;
            MarketDrop.Items.AddRange(new object[] { "Both", "Day", "Night" });
            MarketDrop.Location = new System.Drawing.Point(416, 548);
            MarketDrop.Margin = new Padding(4, 3, 4, 3);
            MarketDrop.Name = "MarketDrop";
            MarketDrop.Size = new System.Drawing.Size(116, 23);
            MarketDrop.TabIndex = 3;
            MarketDrop.SelectedIndexChanged += MarketDrop_SelectedIndexChanged;
            // 
            // UpdateButton
            // 
            UpdateButton.Anchor = AnchorStyles.Bottom;
            UpdateButton.Location = new System.Drawing.Point(772, 548);
            UpdateButton.Margin = new Padding(4, 3, 4, 3);
            UpdateButton.Name = "UpdateButton";
            UpdateButton.Size = new System.Drawing.Size(88, 24);
            UpdateButton.TabIndex = 4;
            UpdateButton.Text = "Update!";
            UpdateButton.UseVisualStyleBackColor = true;
            UpdateButton.Click += UpdateButton_Click;
            /*// 
            // ShowLabels
            // 
            ShowLabels.Anchor = AnchorStyles.Bottom;
            ShowLabels.AutoSize = true;
            ShowLabels.Location = new System.Drawing.Point(664, 553);
            ShowLabels.Margin = new Padding(4, 3, 4, 3);
            ShowLabels.Name = "ShowLabels";
            ShowLabels.Size = new System.Drawing.Size(91, 19);
            ShowLabels.TabIndex = 5;
            ShowLabels.Text = "Show Labels";
            ShowLabels.UseVisualStyleBackColor = true;
            ShowLabels.CheckedChanged += ShowLabels_CheckedChanged;*/
            // 
            // ChartTypeDrop
            // 
            ChartTypeDrop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ChartTypeDrop.Items.AddRange(new object[] { "ES Candle Charts", "Flux Value Charts", "More Charts", "Blahblah Charts" });
            ChartTypeDrop.Location = new System.Drawing.Point(869, 0);
            ChartTypeDrop.Margin = new Padding(4, 3, 4, 3);
            ChartTypeDrop.Name = "ChartTypeDrop";
            ChartTypeDrop.Size = new System.Drawing.Size(233, 23);
            ChartTypeDrop.TabIndex = 6;
            ChartTypeDrop.SelectedIndexChanged += ChartTypeDrop_SelectedIndexChanged;
            // 
            // TextField
            // 
            TextField.Anchor = AnchorStyles.Top;
            TextField.AutoSize = true;
            TextField.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            TextField.Location = new System.Drawing.Point(0, 392);
            TextField.Margin = new Padding(4, 0, 4, 0);
            TextField.Name = "TextField";
            TextField.Size = new System.Drawing.Size(277, 15);
            TextField.TabIndex = 8;
            TextField.Text = "Cross-day AM average values for the 4 days shown:";
            TextField.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CharterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1101, 578);
            Controls.Add(TextField);
            Controls.Add(ChartTypeDrop);
            //Controls.Add(ShowLabels);
            Controls.Add(UpdateButton);
            Controls.Add(MarketDrop);
            Controls.Add(CandleDrop);
            Controls.Add(DatePicker);
            Controls.Add(chart2);
            Controls.Add(chart1);
            Margin = new Padding(4, 3, 4, 3);
            MaximumSize = new System.Drawing.Size(4477, 2486);
            MinimumSize = new System.Drawing.Size(1117, 617);
            Name = "CharterForm";
            Text = "StonkBot_ChartoMatic";
            ((System.ComponentModel.ISupportInitialize)chart1).EndInit();
            ((System.ComponentModel.ISupportInitialize)chart2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private DateTimePicker DatePicker;
        private ComboBox CandleDrop;
        private ComboBox MarketDrop;
        private Button UpdateButton;
        //private CheckBox ShowLabels;
        private ComboBox ChartTypeDrop;
        private Label TextField;
    }
}