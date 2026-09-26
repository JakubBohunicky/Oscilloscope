using MathNet.Numerics.IntegralTransforms;
using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Osciloskop_software
{
  

    public partial class Form1 : Form
    {
        private FormsPlot fftPlot;
        private FormsPlot wavePlot;

        public Form1()
        {
            InitializeComponent();
            //InitializePlots();
        }

        //string dataIN;
        string[] pole = new string[300];
        string[] pole1 = new string[300];

        //public int[] int_pole1 = new int[500];
        //public int[] int_pole2 = new int[500];

        int max1 = 0, min1 = 500, max2 = 0, min2 = 500;
        int i = 0, j = 0;
        bool vykonat = true, pole1_2, backgch = false, ulozenie = false, zly_poradie = false, play = true;
        int priemer1, priemer2;

        private void button4_Click_1(object sender, EventArgs e)
        {
            play = !play;
        }

        private void serialPort2_DataReceived_1(object sender, SerialDataReceivedEventArgs e)
        {
            vykonat = true;
            string data = serialPort2.ReadExisting();

            if (vykonat)
            {


                if (data == "2222\r\n")
                {
                    i = 0;
                    pole1_2 = true;
                    backgch = false;
                    serialPort2.WriteLine("1");
                }
                else
                {
                    if (data == "9999\r\n")
                    {
                        //Invoke(new EventHandler(hodnotadat));
                        i = 0;
                        pole1_2 = false;
                        backgch = true;
                        serialPort2.WriteLine("1");
                        //this.BackColor = Color.Green;
                    }
                    else
                    {
                        if (data == "VYPIS\r\n")
                        {
                            Invoke(new EventHandler(hodnotadat));
                        }
                        else
                        {
                            if (data[0] == '0')
                            {
                                if (pole1_2) pole[i] = data;
                                else pole1[i] = data;
                                i++;
                                serialPort2.WriteLine("1");
                            }
                        }

                    }
                }
            }
        }
        static int FindClosestIndex(int[] numbers, int target)
        {
            // Výber len prvých 100 hodnôt
            var first100 = numbers.Take(100).ToArray();
            var candidates = first100
                .Select((value, index) => new { Value = value, Index = index, Difference = Math.Abs(value - target) })
                .OrderBy(x => x.Difference)
                .ToList();

            while (candidates.Any())
            {
                var closest = candidates.First(); // Najbližšia hodnota
                int closestValue = closest.Value;
                int closestIndex = closest.Index;

                // Kontrola nasledujúcich 5 hodnôt, či nie sú menšie
                bool valid = true;
                for (int i = closestIndex + 1; i <= closestIndex + 5 && i < first100.Length; i++)
                {
                    if (first100[i] < closestValue + 2)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    return closestIndex; // Ak spĺňa podmienku, vrátime index
                }

                candidates.RemoveAt(0); // Inak odstránime tento kandidát a skúšame ďalší
            }

            return 0; // Ak nič nevyhovuje, vráti -1
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void hodnotadat(object sender, EventArgs e)
        {

            int zosil1 = Convert.ToInt32(10 / Convert.ToDouble(comboBox3.Text));
            int zosil2 = Convert.ToInt32(10 / Convert.ToDouble(comboBox4.Text));
            int pocitadlo1 = 0, pocitadlo2 = 0, priemer1 = 0, priemer2 = 0;

            if (checkBox5.Checked == true) posun_x10_1 = 0; else posun_x10_1 = 0;
            if (checkBox4.Checked == true) posun_x10_2 = 0; else posun_x10_2 = 0;

            //posun_x10_1 = 50;
            //posun_x10_2 = 0;

            for (int c = 0; c < i; c++)
            {
                if (backgch && pole1[c] != null)
                {
                    vzorka2 = pole1[c].Split('/');
                    for (int p = 0; p < 8; p++)
                    {

                        GlobalData.int_pole2[pocitadlo2] = 255 - int.Parse(vzorka2[p]);
                        priemer2 += GlobalData.int_pole2[pocitadlo2];
                        pocitadlo2++;

                    }
                }

                if (checkBox2.Checked && pole[c] != null)
                {
                    vzorka1 = pole[c].Split('/');
                    for (int p = 0; p < 8; p++)
                    {

                        GlobalData.int_pole1[pocitadlo1] = 255 - int.Parse(vzorka1[p]);
                        priemer1 += GlobalData.int_pole1[pocitadlo1];
                        pocitadlo1++;

                    }
                }
            }


            if (checkBox2.Checked && (priemer1 > 0 && pocitadlo1 > 0)) priemer1 = priemer1 / pocitadlo1;
            if (backgch && (priemer2 > 0 && pocitadlo2 > 0)) priemer2 = priemer2 / pocitadlo2;


            //HLADANIE ZACIATKU PRE SYNCH
            /*for (int i=0; i<100; i++)
            {
                if ((GlobalData.int_pole1[i] - priemer1 + 130 - numericUpDown1.Value) < rozdiel) index_zaciatku = i;
            }*/
            if (play)
            {
                ulozenie = true;

                index_zaciatku = FindClosestIndex(GlobalData.int_pole1, 255 - (int)vScrollBar1.Value);
                //label3.Text = Convert.ToString(i);
                foreach (var series in chart1.Series) series.Points.Clear();

                label10.Text = Convert.ToString(i);
                if (checkBox_vyhladenie.Checked)
                { 
                    if (synch_button.Checked)
                    {
                        if (checkBox8.Checked)
                        {
                            for (int c = index_zaciatku + 1; c < 350 + index_zaciatku; c++)
                            {
                                if (check_box1.Checked)
                                {

                                    if (Math.Abs(GlobalData.int_pole2[c - 1] - GlobalData.int_pole2[c]) * 3 > Math.Abs(GlobalData.int_pole2[c - 1] - GlobalData.int_pole2[c + 1]))
                                    { GlobalData.int_pole2[c] = (GlobalData.int_pole2[c - 1] + GlobalData.int_pole2[c + 1]) / 2; }

                                    chart1.Series[1].Points.Add((GlobalData.int_pole2[c + posun_x10_2] - priemer2) + 165);
                                    if (GlobalData.int_pole2[c] < min2) min2 = GlobalData.int_pole2[c];
                                    if (GlobalData.int_pole2[c] > max2) max2 = GlobalData.int_pole2[c];

                                }


                                if (checkBox2.Checked)
                                {
                                    if (Math.Abs(GlobalData.int_pole1[c - 1] - GlobalData.int_pole1[c]) * 3 > Math.Abs(GlobalData.int_pole1[c - 1] - GlobalData.int_pole1[c + 1]))
                                    { GlobalData.int_pole1[c] = (GlobalData.int_pole1[c - 1] + GlobalData.int_pole1[c + 1]) / 2; }

                                    chart1.Series[0].Points.Add((GlobalData.int_pole1[c + posun_x10_1] - priemer1) + 165);
                                    if (GlobalData.int_pole1[c] < min1) min1 = GlobalData.int_pole1[c];
                                    if (GlobalData.int_pole1[c] > max1) max1 = GlobalData.int_pole1[c];

                                }
                            }
                        }
                        else
                        {
                            for (int c = index_zaciatku + 1; c < 350 + index_zaciatku; c++)
                            {
                                if (check_box1.Checked)
                                {
                                    if (Math.Abs(GlobalData.int_pole2[c - 1] - GlobalData.int_pole2[c]) * 3 > Math.Abs(GlobalData.int_pole2[c - 1] - GlobalData.int_pole2[c + 1]))
                                    { GlobalData.int_pole2[c] = (GlobalData.int_pole2[c - 1] + GlobalData.int_pole2[c + 1]) / 2; }

                                    chart1.Series[1].Points.Add(GlobalData.int_pole2[c + posun_x10_2] + 46);
                                    if (GlobalData.int_pole2[c] < min2) min2 = GlobalData.int_pole2[c];
                                    if (GlobalData.int_pole2[c] > max2) max2 = GlobalData.int_pole2[c];

                                }


                                if (checkBox2.Checked)
                                {
                                    if (Math.Abs(GlobalData.int_pole1[c - 1] - GlobalData.int_pole1[c]) * 3 > Math.Abs(GlobalData.int_pole1[c - 1] - GlobalData.int_pole1[c + 1]))
                                    { GlobalData.int_pole1[c] = (GlobalData.int_pole1[c - 1] + GlobalData.int_pole1[c + 1]) / 2; }

                                    chart1.Series[0].Points.Add(GlobalData.int_pole1[c + posun_x10_1] + 46);
                                    if (GlobalData.int_pole1[c] < min1) min1 = GlobalData.int_pole1[c];
                                    if (GlobalData.int_pole1[c] > max1) max1 = GlobalData.int_pole1[c];

                                }
                            }
                        }
                    }
                    else
                    {
                        if (checkBox8.Checked)
                        {
                            for (int c = 1; c < 350; c++)
                            {
                                if (check_box1.Checked)
                                {
                                    if (Math.Abs(GlobalData.int_pole2[c - 1] - GlobalData.int_pole2[c]) * 3 > Math.Abs(GlobalData.int_pole2[c - 1] - GlobalData.int_pole2[c + 1]))
                                    { GlobalData.int_pole2[c] = (GlobalData.int_pole2[c - 1] + GlobalData.int_pole2[c + 1]) / 2; }

                                    chart1.Series[1].Points.Add(GlobalData.int_pole2[c + posun_x10_2] - priemer2 + 165);
                                    if (GlobalData.int_pole2[c] < min2) min2 = GlobalData.int_pole2[c];
                                    if (GlobalData.int_pole2[c] > max2) max2 = GlobalData.int_pole2[c];

                                }


                                if (checkBox2.Checked)
                                {
                                    if (Math.Abs(GlobalData.int_pole1[c - 1] - GlobalData.int_pole1[c]) * 3 > Math.Abs(GlobalData.int_pole1[c - 1] - GlobalData.int_pole1[c + 1]))
                                    { GlobalData.int_pole1[c] = (GlobalData.int_pole1[c - 1] + GlobalData.int_pole1[c + 1]) / 2; }

                                    chart1.Series[0].Points.Add(GlobalData.int_pole1[c + posun_x10_1] - priemer1 + 165);
                                    if (GlobalData.int_pole1[c] < min1) min1 = GlobalData.int_pole1[c];
                                    if (GlobalData.int_pole1[c] > max1) max1 = GlobalData.int_pole1[c];

                                }
                            }
                        }
                        else
                        {
                            for (int c = 1; c < 350; c++)
                            {
                                if (check_box1.Checked)
                                {
                                    if (Math.Abs(GlobalData.int_pole2[c - 1] - GlobalData.int_pole2[c]) * 3 > Math.Abs(GlobalData.int_pole2[c - 1] - GlobalData.int_pole2[c + 1]))
                                    { GlobalData.int_pole2[c] = (GlobalData.int_pole2[c - 1] + GlobalData.int_pole2[c + 1]) / 2; }

                                    chart1.Series[1].Points.Add(GlobalData.int_pole2[c + posun_x10_2] + 46);
                                    if (GlobalData.int_pole2[c] < min2) min2 = GlobalData.int_pole2[c];
                                    if (GlobalData.int_pole2[c] > max2) max2 = GlobalData.int_pole2[c];

                                }


                                if (checkBox2.Checked)
                                {
                                    if (Math.Abs(GlobalData.int_pole1[c - 1] - GlobalData.int_pole1[c]) * 3 > Math.Abs(GlobalData.int_pole1[c - 1] - GlobalData.int_pole1[c + 1]))
                                    { GlobalData.int_pole1[c] = (GlobalData.int_pole1[c - 1] + GlobalData.int_pole1[c + 1]) / 2; }

                                    chart1.Series[0].Points.Add(GlobalData.int_pole1[c + posun_x10_1] + 46);
                                    if (GlobalData.int_pole1[c] < min1) min1 = GlobalData.int_pole1[c];
                                    if (GlobalData.int_pole1[c] > max1) max1 = GlobalData.int_pole1[c];

                                }
                            }
                        }
                    }
                }
                else
                {
                    if (synch_button.Checked)
                    {
                        if (checkBox8.Checked)
                        {
                            for (int c = index_zaciatku + 1; c < 350 + index_zaciatku; c++)
                            {
                                if (check_box1.Checked)
                                {

                                   

                                    chart1.Series[1].Points.Add((GlobalData.int_pole2[c + posun_x10_2] - priemer2) + 165);
                                    if (GlobalData.int_pole2[c] < min2) min2 = GlobalData.int_pole2[c];
                                    if (GlobalData.int_pole2[c] > max2) max2 = GlobalData.int_pole2[c];

                                }


                                if (checkBox2.Checked)
                                {
                                    

                                    chart1.Series[0].Points.Add((GlobalData.int_pole1[c + posun_x10_1] - priemer1) + 165);
                                    if (GlobalData.int_pole1[c] < min1) min1 = GlobalData.int_pole1[c];
                                    if (GlobalData.int_pole1[c] > max1) max1 = GlobalData.int_pole1[c];

                                }
                            }
                        }
                        else
                        {
                            for (int c = index_zaciatku + 1; c < 350 + index_zaciatku; c++)
                            {
                                if (check_box1.Checked)
                                {
                                    

                                    chart1.Series[1].Points.Add(GlobalData.int_pole2[c + posun_x10_2] + 46);
                                    if (GlobalData.int_pole2[c] < min2) min2 = GlobalData.int_pole2[c];
                                    if (GlobalData.int_pole2[c] > max2) max2 = GlobalData.int_pole2[c];

                                }


                                if (checkBox2.Checked)
                                {
                                    

                                    chart1.Series[0].Points.Add(GlobalData.int_pole1[c + posun_x10_1] + 46);
                                    if (GlobalData.int_pole1[c] < min1) min1 = GlobalData.int_pole1[c];
                                    if (GlobalData.int_pole1[c] > max1) max1 = GlobalData.int_pole1[c];

                                }
                            }
                        }
                    }
                    else
                    {
                        if (checkBox8.Checked)
                        {
                            for (int c = 1; c < 350; c++)
                            {
                                if (check_box1.Checked)
                                {
                                  

                                    chart1.Series[1].Points.Add(GlobalData.int_pole2[c + posun_x10_2] - priemer2 + 165);
                                    if (GlobalData.int_pole2[c] < min2) min2 = GlobalData.int_pole2[c];
                                    if (GlobalData.int_pole2[c] > max2) max2 = GlobalData.int_pole2[c];

                                }


                                if (checkBox2.Checked)
                                {


                                    chart1.Series[0].Points.Add(GlobalData.int_pole1[c + posun_x10_1] - priemer1 + 165);
                                    if (GlobalData.int_pole1[c] < min1) min1 = GlobalData.int_pole1[c];
                                    if (GlobalData.int_pole1[c] > max1) max1 = GlobalData.int_pole1[c];

                                }
                            }
                        }
                        else
                        {
                            for (int c = 1; c < 350; c++)
                            {
                                if (check_box1.Checked)
                                {
     

                                    chart1.Series[1].Points.Add(GlobalData.int_pole2[c + posun_x10_2] + 46);
                                    if (GlobalData.int_pole2[c] < min2) min2 = GlobalData.int_pole2[c];
                                    if (GlobalData.int_pole2[c] > max2) max2 = GlobalData.int_pole2[c];

                                }


                                if (checkBox2.Checked)
                                {


                                    chart1.Series[0].Points.Add(GlobalData.int_pole1[c + posun_x10_1] + 46);
                                    if (GlobalData.int_pole1[c] < min1) min1 = GlobalData.int_pole1[c];
                                    if (GlobalData.int_pole1[c] > max1) max1 = GlobalData.int_pole1[c];

                                }
                            }
                        }
                    }
                }


                label5.Text = "Max hodnota = " + Convert.ToString(Math.Round(((double)max1 / zosil1 / 5.5), 2)) + " V";
                label6.Text = "Min hodnota = " + Convert.ToString(Math.Round(((double)min1 / zosil1 / 5.5), 2)) + " V";
                label4.Text = "Vpp = " + Convert.ToString(Math.Round(Math.Round(((double)max1 / zosil1 / 5.5), 2) - Math.Round(((double)min1 / zosil1 / 5.5), 2), 2)) + " V";

                label11.Text = "Max hodnota = " + Convert.ToString(Math.Round(((double)max2 / zosil2) / 5.5, 2)) + " V";
                label10.Text = "Min hodnota = " + Convert.ToString(Math.Round(((double)min2 / zosil2) / 5.5, 2)) + " V";
                label7.Text = "Vpp = " + Convert.ToString(Math.Round(Math.Round(((double)max2 / zosil2 / 5.5), 2) - Math.Round(((double)min2 / zosil2 / 5.5), 2), 2)) + " V";

            }
            max1 = 0; min1 = 500; max2 = 0; min2 = 500;

            //label3.Text = comboBox3.Text;
            backgch = false;
            zly_poradie = false;
            ulozenie = true;

            if (check_box1.Checked) s_ch = " 10"; else s_ch = " 0";
            if (checkBox6.Checked) r1 = " 10"; else r1 = " 0";
            if (checkBox3.Checked) r2 = " 10"; else r2 = " 0";
            if (checkBox4.Checked) xr1 = " 10"; else xr1 = " 0";
            if (checkBox5.Checked) xr2 = " 10"; else xr2 = " 0";


            if (comboBox1.Text == "450ns") GlobalData.time = "1";
            if (comboBox1.Text == "625ns") GlobalData.time = "0";
            if (comboBox1.Text == "2.5us") GlobalData.time = "2";
            if (comboBox1.Text == "12us") GlobalData.time = "3";
            if (comboBox1.Text == "15us") GlobalData.time = "4";
            if (comboBox1.Text == "31us") GlobalData.time = "10";
            if (comboBox1.Text == "100us") GlobalData.time = "33";
            if (comboBox1.Text == "300us") GlobalData.time = "100";
            if (comboBox1.Text == "1ms") GlobalData.time = "333";
            if (comboBox1.Text == "3ms") GlobalData.time = "1000";


            serialPort2.WriteLine(Convert.ToString(zosil1) + " " + Convert.ToString(zosil2) + " " + GlobalData.time + s_ch + r1 + r2 + xr1 + xr2);




        }



        private void button3_Click_1(object sender, EventArgs e)
        {
            if (ulozenie)
            {
                using (StreamWriter sw = new StreamWriter("namerane_data"))
                {
                    sw.WriteLine("ZAZNAM Z OSCILOSKOPU :\n");
                    sw.WriteLine("Časová základňa = " + comboBox1.Text);
                    sw.WriteLine("Počet vzoriek na dielik = 50");
                    if (checkBox2.Checked == true && check_box1.Checked == true)
                    {
                        sw.WriteLine("nastavene zosilnenia : \n\tCH1 = " + comboBox3.Text + "V/div\n\tCH2 = " + comboBox4.Text + "V/div");
                        sw.WriteLine("Zobrazovane kanaly : CH1 a CH2");
                    }
                    else
                    {
                        if (checkBox2.Checked == true) { sw.WriteLine("Zobrazovane kanaly : CH1"); sw.WriteLine("nastavene zosilnenia : \n\tCH1 = " + comboBox3.Text + "V/div"); }
                        else
                        {
                            if (check_box1.Checked == true) { sw.WriteLine("Zobrazovane kanaly : CH2"); sw.WriteLine("nastavene zosilnenia : \n\tCH2 = " + comboBox4.Text + "V/div"); }
                            else MessageBox.Show("Nije zvoleny kanal.");
                        }

                        //else MessageBox.Show("Nieje zvoleny žiaden kanál.");
                    }
                    sw.WriteLine("NAMERANE DATA :\n");
                    if (checkBox2.Checked == true)
                    {
                        sw.WriteLine("CH1 : ");
                        for (int i = 0; i < 350; i++) sw.WriteLine(GlobalData.int_pole1[i]);
                        sw.WriteLine("");
                    }
                    if (check_box1.Checked == true)
                    {
                        sw.WriteLine("CH2 : ");
                        for (int i = 0; i < 350; i++) sw.WriteLine(GlobalData.int_pole2[i]);
                        sw.WriteLine("");
                    }
                }
            }
            else
            {
                MessageBox.Show("Niesu namerane hodnoty.");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (serialPort2.IsOpen)
            {
                serialPort2.Close();
                //label2.Visible = false;
            }
        }

        string s_ch, r1, r2, xr1, xr2;
        string[] vzorka1 = new string[15];
        string[] vzorka2 = new string[15];
        int index_zaciatku, rozdiel = 500, posun_x10_1 = 0, posun_x10_2 = 0;

        private void button1_Click_1(object sender, EventArgs e)
        {
            i = 0;

            if (!(serialPort2.IsOpen))
            {
                serialPort2.PortName = comboBox2.Text;
                serialPort2.BaudRate = 460800;
                serialPort2.Open();

                if (check_box1.Checked) s_ch = " 10"; else s_ch = " 0";
                //if (checkBox1.Checked) r1 = " 10"; else r1 = " 0";
                //if (checkBox3.Checked) r2 = " 10"; else r2 = " 0";
                r1 = " 0";
                r2 = " 0";
                if (checkBox4.Checked) xr1 = " 10"; else xr1 = " 0";
                if (checkBox5.Checked) xr2 = " 10"; else xr2 = " 0";
                int zosil1 = Convert.ToInt32(10 / Convert.ToDouble(comboBox3.Text));
                int zosil2 = Convert.ToInt32(10 / Convert.ToDouble(comboBox4.Text));

                if (comboBox1.Text == "450ns") GlobalData.time = "1";
                if (comboBox1.Text == "625ns") GlobalData.time = "0";
                if (comboBox1.Text == "2.5us") GlobalData.time = "2";
                if (comboBox1.Text == "12us") GlobalData.time = "3";
                if (comboBox1.Text == "15us") GlobalData.time = "4";
                if (comboBox1.Text == "31us") GlobalData.time = "10";
                if (comboBox1.Text == "100us") GlobalData.time = "33";
                if (comboBox1.Text == "300us") GlobalData.time = "100";
                if (comboBox1.Text == "1ms") GlobalData.time = "333";
                if (comboBox1.Text == "3ms") GlobalData.time = "1000";


                serialPort2.WriteLine(Convert.ToString(zosil1) + " " + Convert.ToString(zosil2) + " " + GlobalData.time + s_ch + r1 + r2 + xr1 + xr2);

            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            this.Text = "osciloskop";
            this.BackColor = System.Drawing.Color.LightGray;
            string[] ports = SerialPort.GetPortNames();
            string[] time_div = { "450ns", "625ns", "2.5us", "12us", "15us", "31us", "100us", "300us", "1ms", "3ms" };
            string[] u_div = { "0.1", "0.2", "0.5", "1", "2", "5", };
            comboBox2.Items.AddRange(ports);
            comboBox3.Items.AddRange(u_div);
            comboBox4.Items.AddRange(u_div);
            comboBox1.Items.AddRange(time_div);

            comboBox3.Text = "1";
            comboBox4.Text = "1";
            comboBox1.Text = "12us";

            serialPort2.ReadBufferSize = 4096;
            serialPort2.WriteBufferSize = 4096;
            //serialPort2.ReceivedBytesThreshold = 1;

            this.WindowState = FormWindowState.Maximized; // Maximalizácia formulára
            //this.Width = 900;
            //this.Height = 800;
            Series emptySeries = new Series("CH 2")
            {
                ChartType = SeriesChartType.Line // You can change to another type
            };
            chart1.Series.Add(emptySeries);
            chart1.Series[0].Name = "CH 1";
            chart1.Series[0].BorderWidth = 3;
            chart1.Series[1].BorderWidth = 3;

            chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
            chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            //chart1.ChartAreas[1].AxisY.LabelStyle.Enabled = false;
            //chart1.ChartAreas[1].AxisX.LabelStyle.Enabled = false;

            chart1.ChartAreas[0].AxisX.Minimum = 0;  // Začiatok osi X
            chart1.ChartAreas[0].AxisX.Maximum = 330; // Koniec osi X
            chart1.ChartAreas[0].AxisX.Interval = 50; // Interval medzi hodnotami


            chart1.ChartAreas[0].AxisY.Minimum = 0;  // Začiatok osi X
            chart1.ChartAreas[0].AxisY.Maximum = 330; // Koniec osi X
            chart1.ChartAreas[0].AxisY.Interval = 55; // Interval medzi hodnotami


            //int midY = this.Height / 2; // Find the vertical center
            Pen pen = new Pen(System.Drawing.Color.White, 4); // Define the line color and thickness
            Graphics g = this.CreateGraphics();

            label5.Text = "Max hodnota = ";
            label6.Text = "Min hodnota = ";
            label11.Text = "Max hodnota = ";
            label10.Text = "Min hodnota = ";
            //label14.Text = "frkvencia = "+Convert.ToString(GlobalData.frekvenica);

            
        }
        
        


        private void button5_Click_1(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            //form2.WindowState = FormWindowState.Maximized; // Maximize the window
            form2.Width = 800;
            form2.Height = 500;
            form2.Show();
        }
    } 
    public static class GlobalData
    {
        public static int[] int_pole1 = new int[700];
        public static int[] int_pole2 = new int[700];
        public static string time;
        public static int frekvenica = 1000;
    }
    

    public partial class Form2 : Form
    {
        private FormsPlot fftPlot;
        private FormsPlot wavePlot;

        public Form2()
        {
            //InitializeComponent();
            InitializePlots();
            PerformPlotting();

        }

        private void InitializePlots()
        {
            this.WindowState.Equals(FormWindowState.Maximized);
            // Create plots with 50% height each
            wavePlot = new FormsPlot
            {
                Dock = DockStyle.Top,
                Height = 200
            };
            wavePlot.Plot.Title("Time Domain Signal");
            wavePlot.Plot.YLabel("Amplitude");
            wavePlot.Plot.XLabel("Time");

            fftPlot = new FormsPlot
            {
                Dock = DockStyle.Bottom,
                Height = 300
            };
            fftPlot.Plot.Title("Frequency Domain (FFT)");
            fftPlot.Plot.YLabel("Magnitude");
            fftPlot.Plot.XLabel("Frequency (Hz)");

            Controls.Add(wavePlot);
            Controls.Add(fftPlot);
        }
        
        private void PerformPlotting()
        {   int[] vrokos = new int[350];
            // Sample rate and data size
            int sampleRate = 10000;
            if (GlobalData.time == "1") sampleRate = 77780000;
            if (GlobalData.time == "0") sampleRate = 115500000;
            if (GlobalData.time == "2") sampleRate = 18580000;
            if (GlobalData.time == "3") sampleRate = 4950000;
            if (GlobalData.time == "4") sampleRate = 3750000;
            if (GlobalData.time == "10") sampleRate = 1500000;
            if (GlobalData.time == "33") sampleRate = 500000;//ok
            if (GlobalData.time == "100") sampleRate = 166600;//ok
            if (GlobalData.time == "333") sampleRate = 50000;//ok
            if (GlobalData.time == "1000") sampleRate = 16660;

            for(int i=0; i<350; i++)
            {
                vrokos[i] = GlobalData.int_pole1[i];
            }

            //object int_pole1 = null;
            // Generate a mixed signal with multiple frequencies
            //double[] timeData = GenerateMixedSignal(sampleRate);
            int[] timeData = vrokos;


            // Plot the time domain signal
            double[] timePoints = Enumerable.Range(0, sampleRate)
                .Select(i => (double)i / sampleRate)
                .ToArray();

            wavePlot.Plot.Add.Scatter(timePoints, timeData);
            wavePlot.Refresh();

            // Perform FFT
            Complex[] fftResult = PerformFFT(timeData);

            // Only show the first half of the FFT (the rest is symmetrical)
            int halfLength = fftResult.Length / 2;
            double[] frequencies = Enumerable.Range(0, halfLength)
                .Select(i => (double)i * sampleRate / fftResult.Length)
                .ToArray();
            double[] magnitudes = fftResult.Take(halfLength)
                .Select(c => c.Magnitude)
                .ToArray();

            // Plot the FFT results
            fftPlot.Plot.Add.Scatter(frequencies, magnitudes);

            // Find multiple peaks in the FFT
            int[] peakIndices = FindPeaks(magnitudes, 3, 10);

            // Find the Y axis limit manually
            double yMax = magnitudes.Max();

            var peakFrequenciesText = "Detected Frequency Peaks:\n";

            // Mark each peak frequency
            foreach (int idx in peakIndices)
            {
                double frequency = frequencies[idx];
                double magnitude = magnitudes[idx];

                // Mark peak with a marker
                var marker = fftPlot.Plot.Add.Marker(frequency, magnitude);
                marker.Color = Colors.Red;
                marker.Size = 10;
                marker.Shape = MarkerShape.FilledCircle;

                // Add vertical line
                var vLine = fftPlot.Plot.Add.VerticalLine(frequency);
                vLine.Color = Colors.Red.WithAlpha(150);
                vLine.LineWidth = 1;
                vLine.LinePattern = LinePattern.Dashed;

                // Add to the frequency text
                peakFrequenciesText += $"- {frequency:F1} Hz (Magnitude: {magnitude:F2})\n";

                // Label the peak
                var peakLabel = fftPlot.Plot.Add.Text($"{frequency:F1} Hz", frequency, magnitude);
                peakLabel.LabelFontSize = 10;
                peakLabel.LabelBold = true;
                peakLabel.Alignment = Alignment.UpperCenter;
                peakLabel.LabelBackgroundColor = Colors.White.WithAlpha(200);
            }

            // Add measurements panel in the corner of the plot
            var statsLabel = fftPlot.Plot.Add.Text(peakFrequenciesText, 5, yMax * 0.9);
            statsLabel.LabelFontSize = 12;
            statsLabel.LabelBold = true;
            statsLabel.LabelBackgroundColor = Colors.White.WithAlpha(200);
            statsLabel.Alignment = Alignment.UpperLeft;

            fftPlot.Refresh();
        }

        private int[] FindPeaks(double[] magnitudes, int count, int minDistance)
        {
            // Find the indices of the top 'count' peaks that are at least 'minDistance' apart
            var peaks = new List<(int Index, double Value)>();

            // Skip DC component (first few bins)
            for (int i = 5; i < magnitudes.Length; i++)
            {
                // Check if this point is a local maximum
                if ((i == 0 || magnitudes[i] > magnitudes[i - 1]) &&
                    (i == magnitudes.Length - 1 || magnitudes[i] > magnitudes[i + 1]))
                {
                    peaks.Add((i, magnitudes[i]));
                }
            }

            // Sort peaks by magnitude (descending)
            peaks.Sort((a, b) => b.Value.CompareTo(a.Value));
            bool t =true;
            // Take the top 'count' peaks, ensuring they're minDistance apart
            var selectedPeaks = new List<int>();
            foreach (var peak in peaks)
            {
                // Check if this peak is far enough from all selected peaks
                bool isFarEnough = true;
                foreach (int selectedIdx in selectedPeaks)
                {
                    if (Math.Abs(peak.Index - selectedIdx) < minDistance)
                    {
                        isFarEnough = false;
                        break;
                    }
                }

                // If far enough, add it to selected peaks
                if (isFarEnough)
                {
                    selectedPeaks.Add(peak.Index);
                    //if (t) GlobalData.frekvenica = peak.Index; t = false;
                    if (selectedPeaks.Count >= count)
                        break;
                }
            }

            return selectedPeaks.ToArray();
        }

        private Complex[] PerformFFT(int[] data)
        {
            // Convert real data to complex
            Complex[] complexData = data.Select(d => new Complex(d, 0)).ToArray();

            // Perform the FFT
            Fourier.Forward(complexData, FourierOptions.Default);

            return complexData;
        }
    }
}
