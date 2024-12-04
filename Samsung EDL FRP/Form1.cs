using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;

namespace Samsung_EDL_FRP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadLoaderFiles();

            // Add a footer label
            Label footerLabel = new Label
            {
                Text = "    With ❤️ from u/PersonalityActual889",
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.Gray,
                Font = new Font("Arial", 10, FontStyle.Italic),
                Padding = new Padding(0, 0, 10, 0) // Add padding to give space from the edge

            };

            this.Controls.Add(footerLabel);
        }

        private void ScanPorts()
        {
            comboPorts.Items.Clear();
            var ports = SerialPort.GetPortNames();
            comboPorts.Items.AddRange(ports);

            if (ports.Length > 0)
            {
                comboPorts.SelectedIndex = 0; // Auto-select the first port
                log.AppendText($"Detected {ports.Length} COM ports: {string.Join(", ", ports)}\r\n");
            }
            else
            {
                log.AppendText("No COM ports detected.\r\n");
            }
        }

        private void LoadLoaderFiles()
        {
            comboMBNFiles.Items.Clear();
            string loadersDirectory = Path.Combine(Application.StartupPath, "Loaders");

            if (!Directory.Exists(loadersDirectory))
            {
                Directory.CreateDirectory(loadersDirectory); // Create the directory if it doesn't exist
                log.AppendText("Created 'Loaders' directory.\r\n");
            }

            var loaderFiles = Directory.GetFiles(loadersDirectory, "*.mbn");

            if (loaderFiles.Length > 0)
            {
                foreach (var file in loaderFiles)
                {
                    comboMBNFiles.Items.Add(Path.GetFileName(file)); // Add file names only
                }
                comboMBNFiles.SelectedIndex = 0; // Auto-select the first file
                log.AppendText($"Loaded {loaderFiles.Length} loader files.\r\n");
            }
            else
            {
                log.AppendText("No loader files found in the 'Loaders' directory.\r\n");
            }
        }

        private void StartFlashing()
        {
            log.Clear();
            pb.Value = 0;

            if (comboPorts.SelectedItem == null || comboMBNFiles.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port and a loader file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string port = comboPorts.SelectedItem.ToString();
            string selectedLoaderFile = comboMBNFiles.SelectedItem.ToString();
            string loadersDirectory = Path.Combine(Application.StartupPath, "Loaders");
            string loaderFile = Path.Combine(loadersDirectory, selectedLoaderFile);

            // Validate loader file
            if (!File.Exists(loaderFile))
            {
                MessageBox.Show("Selected loader file not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate emmcdl.exe in the "lib" folder
            string libDirectory = Path.Combine(Application.StartupPath, "lib");
            string emmcdlPath = Path.Combine(libDirectory, "emmcdl.exe");

            if (!File.Exists(emmcdlPath))
            {
                MessageBox.Show("emmcdl.exe not found in the 'lib' directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                log.AppendText("Starting flashing process...\r\n");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {emmcdlPath} -p {port} -f {selectedLoaderFile} -e persistent",
                        WorkingDirectory = loadersDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrWhiteSpace(args.Data))
                        log.AppendText(args.Data + "\r\n");
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrWhiteSpace(args.Data))
                        log.AppendText("Error: " + args.Data + "\r\n");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    log.AppendText("Flashing completed successfully.\r\n");
                }
                else
                {
                    log.AppendText("Flashing failed. Check the log for details.\r\n");
                }
            }
            catch (Exception ex)
            {
                log.AppendText($"Error: {ex.Message}\r\n");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ScanPorts();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StartFlashing();
        }
    }
}
