namespace IoTSensorDataProcessing.Sensor
{
    partial class SensorForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.nameLabel = new System.Windows.Forms.Label();
            this.portLabel = new System.Windows.Forms.Label();
            this.ipAddressLabel = new System.Windows.Forms.Label();
            this.locationLabel = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.activeConnectionslabel = new System.Windows.Forms.Label();
            this.buttonSendMeasure = new System.Windows.Forms.Button();
            this.buttonStopSending = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nameLabel.Location = new System.Drawing.Point(6, 16);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(44, 15);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name:";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.portLabel.Location = new System.Drawing.Point(6, 52);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(32, 15);
            this.portLabel.TabIndex = 1;
            this.portLabel.Text = "Port:";
            // 
            // ipAddressLabel
            // 
            this.ipAddressLabel.AutoSize = true;
            this.ipAddressLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ipAddressLabel.Location = new System.Drawing.Point(6, 34);
            this.ipAddressLabel.Name = "ipAddressLabel";
            this.ipAddressLabel.Size = new System.Drawing.Size(68, 15);
            this.ipAddressLabel.TabIndex = 2;
            this.ipAddressLabel.Text = "IP address:";
            // 
            // locationLabel
            // 
            this.locationLabel.AutoSize = true;
            this.locationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.locationLabel.Location = new System.Drawing.Point(6, 70);
            this.locationLabel.Name = "locationLabel";
            this.locationLabel.Size = new System.Drawing.Size(54, 15);
            this.locationLabel.TabIndex = 3;
            this.locationLabel.Text = "Location";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 131);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(369, 161);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.activeConnectionslabel);
            this.groupBox1.Controls.Add(this.nameLabel);
            this.groupBox1.Controls.Add(this.ipAddressLabel);
            this.groupBox1.Controls.Add(this.locationLabel);
            this.groupBox1.Controls.Add(this.portLabel);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(369, 113);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Info";
            // 
            // activeConnectionslabel
            // 
            this.activeConnectionslabel.AutoSize = true;
            this.activeConnectionslabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.activeConnectionslabel.Location = new System.Drawing.Point(6, 89);
            this.activeConnectionslabel.Name = "activeConnectionslabel";
            this.activeConnectionslabel.Size = new System.Drawing.Size(156, 15);
            this.activeConnectionslabel.TabIndex = 4;
            this.activeConnectionslabel.Text = "Active server connections: 0";
            // 
            // buttonSendMeasure
            // 
            this.buttonSendMeasure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSendMeasure.Location = new System.Drawing.Point(13, 296);
            this.buttonSendMeasure.Name = "buttonSendMeasure";
            this.buttonSendMeasure.Size = new System.Drawing.Size(118, 23);
            this.buttonSendMeasure.TabIndex = 6;
            this.buttonSendMeasure.Text = "Send measurements";
            this.buttonSendMeasure.UseVisualStyleBackColor = true;
            this.buttonSendMeasure.Click += new System.EventHandler(this.buttonSendMeasure_Click);
            // 
            // buttonStopSending
            // 
            this.buttonStopSending.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStopSending.Enabled = false;
            this.buttonStopSending.Location = new System.Drawing.Point(138, 296);
            this.buttonStopSending.Name = "buttonStopSending";
            this.buttonStopSending.Size = new System.Drawing.Size(118, 23);
            this.buttonStopSending.TabIndex = 7;
            this.buttonStopSending.Text = "Stop sending";
            this.buttonStopSending.UseVisualStyleBackColor = true;
            this.buttonStopSending.Click += new System.EventHandler(this.buttonStopSending_Click);
            // 
            // SensorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 331);
            this.Controls.Add(this.buttonStopSending);
            this.Controls.Add(this.buttonSendMeasure);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox1);
            this.Name = "SensorForm";
            this.Text = "SensorForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.Label ipAddressLabel;
        private System.Windows.Forms.Label locationLabel;
        private System.Windows.Forms.RichTextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonSendMeasure;
        private System.Windows.Forms.Button buttonStopSending;
        private System.Windows.Forms.Label activeConnectionslabel;
    }
}