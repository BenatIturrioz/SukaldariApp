using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace SukaldariApp
{
    public partial class Form2 : Form
    {
        private List<DataGridView> dataGridViews;
        private int currentIndex;

        public Form2()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;

            // Suscribir eventos a los elementos del formulario
            dateTimePicker1.ValueChanged += DateTimePicker1_ValueChanged;
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            button3.Click += Button3_Click; // Botón para ocultar filas completadas
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            LoadData(dateTimePicker1.Value);
        }

        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            LoadData(dateTimePicker1.Value);
        }

        private void LoadData(DateTime selectedDate)
        {
            // Limpiar DataGridViews existentes en el Panel
            if (dataGridViews != null)
            {
                foreach (var dataGridView in dataGridViews)
                {
                    panel1.Controls.Remove(dataGridView);
                }
            }

            dataGridViews = new List<DataGridView>();

            Connection connection = new Connection();
            string query = @"
                SELECT ep.produktu_izena, produktuaKop
                FROM eskaeraproduktua ep
                JOIN erreserba e ON e.id = ep.erreserba_id
                WHERE e.data = @selectedDate"; // Ajusta la consulta según tu base de datos

            DataTable dataTable = GetDataTable(connection, query, selectedDate);
            var groupedData = GroupDataByEskaeraId(dataTable);

            foreach (var group in groupedData)
            {
                DataGridView dataGridView = new DataGridView
                {
                    DataSource = group.Value,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // Ajusta las columnas al tamaño del contenido
                    AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,  // Ajusta las filas al tamaño del contenido
                    Size = new Size(800, 500), // Ajusta el tamaño según tus necesidades
                    Visible = false
                };

                dataGridView.CellClick += DataGridView_CellClick; // Evento para marcar registros completados

                dataGridViews.Add(dataGridView);
                panel1.Controls.Add(dataGridView);
            }

            currentIndex = 0;
            if (dataGridViews.Count > 0)
            {
                dataGridViews[currentIndex].Visible = true;
            }
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridView dataGridView = (DataGridView)sender;
                DataGridViewRow row = dataGridView.Rows[e.RowIndex];
                row.DefaultCellStyle.BackColor = Color.LightGray; // Cambiar el color de fondo para indicar que está completado
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (dataGridViews != null && dataGridViews.Count > 0)
            {
                dataGridViews[currentIndex].Visible = false;
                currentIndex = (currentIndex - 1 + dataGridViews.Count) % dataGridViews.Count;
                dataGridViews[currentIndex].Visible = true;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (dataGridViews != null && dataGridViews.Count > 0)
            {
                dataGridViews[currentIndex].Visible = false;
                currentIndex = (currentIndex + 1) % dataGridViews.Count;
                dataGridViews[currentIndex].Visible = true;
            }
        }

        private void Button3_Click(object sender, EventArgs e) // Botón para ocultar filas completadas
        {
            if (dataGridViews != null && dataGridViews.Count > 0)
            {
                foreach (var dataGridView in dataGridViews)
                {
                    for (int i = dataGridView.Rows.Count - 1; i >= 0; i--)
                    {
                        if (dataGridView.Rows[i].DefaultCellStyle.BackColor == Color.LightCoral)
                        {
                            dataGridView.Rows.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private DataTable GetDataTable(Connection connection, string query, DateTime selectedDate)
        {
            using (MySqlConnection conn = connection.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@selectedDate", selectedDate.Date);
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        private Dictionary<int, DataTable> GroupDataByEskaeraId(DataTable dataTable)
        {
            Dictionary<int, DataTable> groupedData = new Dictionary<int, DataTable>();

            foreach (DataRow row in dataTable.Rows)
            {
                int eskaeraId = Convert.ToInt32(row["erreserba_id"]);

                if (!groupedData.ContainsKey(eskaeraId))
                {
                    groupedData[eskaeraId] = dataTable.Clone();
                }

                groupedData[eskaeraId].ImportRow(row);
            }

            return groupedData;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Hacer visibles los DataGridViews en el Panel
            if (dataGridViews != null && dataGridViews.Count > 0)
            {
                foreach (var dataGridView in dataGridViews)
                {
                    if (!panel1.Controls.Contains(dataGridView))
                    {
                        panel1.Controls.Add(dataGridView);
                    }
                }
            }
        }
    }
}













