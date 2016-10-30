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
            _sensor = new Sensor(@"./measurements.csv");
            activeConnectionslabel.Text = "Active server connections: 0"
                                        + "/" + _sensor.Threads;
            _sensor.WriteLogAction = s =>
            {
                var s2 = s + Environment.NewLine;
                textBox1.Text += s2;
                textBox1.Refresh();
            };
            _sensor.UpdateActiveConnAction = s =>
            {
                activeConnectionslabel.Text = "Active server connections: " 
                                        + s + "/" + _sensor.Threads;
                activeConnectionslabel.Refresh();
            };
            Text = _sensor.Name;
            nameLabel.Text += " " + _sensor.Name;
            ipAddressLabel.Text += " " + _sensor.Ip;
            portLabel.Text += " " + _sensor.Port;
            locationLabel.Text += " [" + _sensor.Longitude 
                                + ", " + _sensor.Latitude + "]";
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void buttonSendMeasure_Click(object sender, EventArgs e)
        {
            buttonStopSending.Enabled = true;
            buttonSendMeasure.Enabled = false;
            Task.Factory.StartNew(() => {
                _sensor.ConnectToNeighbour();
            });
        }

        private void buttonStopSending_Click(object sender, EventArgs e)
        {
            buttonSendMeasure.Enabled = true;
            buttonStopSending.Enabled = false;
            _sensor.StopNeighbourCommmunication();
        }
    }
}
