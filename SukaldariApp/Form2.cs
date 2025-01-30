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
        private Timer refreshTimer;

        public Form2()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;

         
            refreshTimer = new Timer();
            refreshTimer.Interval = 5000; // Intervalo de 5 segundos
            refreshTimer.Tick += RefreshTimer_Tick; // Evento que se ejecutará cada vez que el temporizador "haga tick"
            refreshTimer.Start(); // Iniciar el temporizador
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            LoadData(DateTime.Today); 
        }

        private void LoadData(DateTime selectedDate)
        {
            // Limpiar DataGridViews existentes en el Panel
            if (dataGridViews != null)
            {
                foreach (var gridView in dataGridViews)
                {
                    panel1.Controls.Remove(gridView);
                }
            }

            dataGridViews = new List<DataGridView>();

            // Crear conexión y consulta SQL
            Connection connection = new Connection();
            string query = @"
                SELECT ep.produktu_izena, ep.produktuaKop, ep.erreserba_id
                FROM eskaeraproduktua ep
                WHERE ep.eginda = 0"; // Se elimina el JOIN y solo se filtra por eginda = 0

            // Obtener datos de la base de datos para la fecha seleccionada
            DataTable dataTable = GetDataTable(connection, query, selectedDate);

            // Crear un DataGridView y asignar el DataSource
            DataGridView dataGridView = new DataGridView
            {
                DataSource = dataTable,  // Aquí usamos dataTable como fuente de datos
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // Ajusta las columnas al tamaño del contenido
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,  // Ajusta las filas al tamaño del contenido
                Size = new Size(800, 500), // Ajusta el tamaño según tus necesidades
                Visible = false
            };

            // Evento para marcar registros completados
            dataGridView.CellClick += DataGridView_CellClick;

            // Añadir el DataGridView a la lista y al panel
            dataGridViews.Add(dataGridView);
            panel1.Controls.Add(dataGridView);

            // Mostrar el primer DataGridView
            currentIndex = 0;
            if (dataGridViews.Count > 0)
            {
                dataGridViews[currentIndex].Visible = true;
            }
        }


        // Evento del temporizador para refrescar los datos
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // Llamar a LoadData para recargar los datos
            LoadData(DateTime.Today); // Usamos la fecha de hoy directamente
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
        private DataTable GetDataTable(Connection connection, string query, DateTime selectedDate)
        {
            using (MySqlConnection conn = connection.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@selectedDate", selectedDate.Date); // Se pasa la fecha seleccionada
                MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Hacer visibles los DataGridViews en el Panel
            if (dataGridViews != null && dataGridViews.Count > 0)
            {
                foreach (var gridView in dataGridViews)
                {
                    if (!panel1.Controls.Contains(gridView))
                    {
                        panel1.Controls.Add(gridView);
                    }
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Verificar si se ha seleccionado una fila en el DataGridView
            if (dataGridViews.Count > 0 && dataGridViews[currentIndex].SelectedCells.Count > 0)
            {
                // Obtener la fila seleccionada en el DataGridView
                int rowIndex = dataGridViews[currentIndex].SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridViews[currentIndex].Rows[rowIndex];

                // Obtener los valores de la fila
                var produktuaIzena = selectedRow.Cells["produktu_izena"].Value.ToString();
                var erreserbaId = selectedRow.Cells["erreserba_id"].Value.ToString();
                var produktuaKop = selectedRow.Cells["produktuaKop"].Value.ToString();

                // Llamar al método que realizará el UPDATE en la base de datos
                UpdateDatabase(produktuaIzena, produktuaKop, erreserbaId);
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una fila.");
            }
        }

        private void UpdateDatabase(string produktuIzena, string produktuaKop, string erreserbaId)
        {
            MySqlConnection conn = null;
            try
            {
                // Crear una conexión a la base de datos
                Connection connection = new Connection();
                conn = connection.GetConnection();  // Obtener la conexión

                // Asegurarse de que la conexión no es null antes de intentar abrirla
                if (conn == null)
                {
                    MessageBox.Show("No se pudo crear la conexión a la base de datos.");
                    return;
                }

                // Abrir la conexión si no está abierta
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();  // Intentar abrir la conexión
                }

                // Crear la consulta SQL para actualizar el valor de 'eginda' a 1
                string query = @"
            UPDATE eskaeraproduktua 
            SET eginda = 1 
            WHERE produktu_izena = @produktuIzena
            AND erreserba_id = @erreserbaId
            AND produktuaKop = @produktuaKop 
            AND eginda = 0";

                // Crear el comando SQL usando el método CreateCommand
                MySqlCommand cmd = connection.CreateCommand(conn, query);
                cmd.Parameters.AddWithValue("@produktuIzena", produktuIzena);
                cmd.Parameters.AddWithValue("@erreserbaId", erreserbaId);
                cmd.Parameters.AddWithValue("@produktuaKop", produktuaKop);

                // Ejecutar la consulta
                int rowsAffected = cmd.ExecuteNonQuery();

                // Verificar si la actualización fue exitosa
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Registro marcado como hecho.");
                    // Opcionalmente, puedes refrescar los datos aquí para reflejar el cambio
                    LoadData(DateTime.Today); // Vuelve a cargar los datos
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el registro. Verifique los valores.");
                }
            }
            catch (Exception ex)
            {
                // Capturar cualquier error y mostrarlo
                MessageBox.Show("Error al actualizar la base de datos: " + ex.Message);
            }
            finally
            {
                // Asegurarse de cerrar la conexión si está abierta
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }






    }
}
