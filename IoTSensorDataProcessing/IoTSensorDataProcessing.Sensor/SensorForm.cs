using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IoTSensorDataProcessing.Sensor
{
    public partial class SensorForm : Form
    {
        private readonly Sensor _sensor;

        public SensorForm()
        {
            InitializeComponent();
            textBox1.Text = "";
            _sensor = new Sensor(@"./measurements.csv",writeLogAction: s =>
            {
                var s2 = s + Environment.NewLine;
                textBox1.Text += s2;
                textBox1.Refresh();
            });
            this.Text = _sensor.Name;
            nameLabel.Text += " " + _sensor.Name;
            ipAddressLabel.Text += " " + _sensor.Ip.ToString();
            portLabel.Text += " " + _sensor.Port;
            locationLabel.Text += " [" + _sensor.Longitude 
                                + ", " + _sensor.Latitude + "]";
        }

        private void textBox1_TextChanged(object sender, System.EventArgs e)
        {

        }

        private void buttonSendMeasure_Click(object sender, EventArgs e)
        {
            buttonStopSending.Enabled = true;
            buttonSendMeasure.Enabled = false;
            Task.Factory.StartNew(() => {
                _sensor.CommunicateWithNeighbour();
            });
        }

        private void buttonStopSending_Click(object sender, EventArgs e)
        {
            buttonSendMeasure.Enabled = true;
            buttonStopSending.Enabled = false;
            _sensor.StopCommmunicationWithNeighbour();
        }
    }
}
