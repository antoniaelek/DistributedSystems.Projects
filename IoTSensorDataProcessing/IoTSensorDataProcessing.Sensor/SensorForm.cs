using System.Windows.Forms;

namespace IoTSensorDataProcessing.Sensor
{
    public partial class SensorForm : Form
    {
        private Sensor _sensor;

        public SensorForm()
        {
            InitializeComponent();
            textBox1.Text = "";
            _sensor = new Sensor(@"./measurements.csv");
            _sensor.WriteLogAction = s =>
            {
                textBox1.Text += s;
                Refresh();
            };
            nameLabel.Text += " " + _sensor.Name;
            ipAddressLabel.Text += " " + _sensor.Ip.ToString();
            portLabel.Text += " " + _sensor.Port;
            locationLabel.Text += " [" + _sensor.Longitude 
                                + ", " + _sensor.Latitude + "]";
        }

        private void textBox1_TextChanged(object sender, System.EventArgs e)
        {

        }
    }
}
