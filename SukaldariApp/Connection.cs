using System;
using MySql.Data.MySqlClient;

namespace SukaldariApp
{
    internal class Connection
    {
        private readonly string connectionString = "server='192.168.115.188';port='3306';user id='1taldea';password='1taldea';database='erronka1';SslMode='none'";

        // Método para obtener la conexión
        public MySqlConnection GetConnection()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(connectionString);
                return connection;
            }
            catch (Exception ex)
            {
                // Si hay un error al crear la conexión, lo lanzamos como excepción
                throw new Exception("Error al crear la conexión: " + ex.Message);
            }
        }

        // Método para probar la conexión a la base de datos
        public bool TestConnection()
        {
            try
            {
                using (MySqlConnection connection = GetConnection())
                {
                    connection.Open();  // Abrir la conexión
                    return connection.State == System.Data.ConnectionState.Open;  // Verificar si la conexión está abierta
                }
            }
            catch (Exception ex)
            {
                // Si hay un error al conectar, lo mostramos y retornamos falso
                Console.WriteLine("Error al probar la conexión: " + ex.Message);
                return false;
            }
        }

        // Método para ejecutar un comando SQL (si lo necesitas)
        public MySqlCommand CreateCommand(MySqlConnection connection, string query)
        {
            try
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                return command;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el comando: " + ex.Message);
            }
        }
    }
}
