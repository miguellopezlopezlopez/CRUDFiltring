using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Collections.Generic;

namespace FiltringApp
{
    public partial class MainForm : Form
    {
        private MySqlConnection conexion;
        private string cadenaConexion;
        private string rutaFoto = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            CargarConfiguracion();
            CargarDatosUsuarios();
        }

        private void CargarConfiguracion()
        {
            string configPath = "config.yml";
            if (File.Exists(configPath))
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();

                string yamlContent = File.ReadAllText(configPath);
                Dictionary<string, string> config = deserializer.Deserialize<Dictionary<string, string>>(yamlContent);

                if (config.ContainsKey("host") && config.ContainsKey("port") && config.ContainsKey("database") && config.ContainsKey("user") && config.ContainsKey("password"))
                {
                    cadenaConexion = $"server={config["host"]};port={config["port"]};database={config["database"]};user={config["user"]};password={config["password"]};";
                    conexion = new MySqlConnection(cadenaConexion);
                }
                else
                {
                    MessageBox.Show("Error en la configuración YAML: faltan parámetros obligatorios.");
                    Environment.Exit(1);
                }
            }
            else
            {
                MessageBox.Show("Archivo de configuración no encontrado.");
                Environment.Exit(1);
            }
        }

        private void CargarDatosUsuarios()
        {
            try
            {
                conexion.Open();
                string consulta = "SELECT *  FROM Usuario";
                MySqlDataAdapter adaptador = new MySqlDataAdapter(consulta, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                dataGridViewUsuarios.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message);
            }
            finally
            {
                conexion.Close();
            }
        }

        private void btnAgregarUsuario_Click(object sender, EventArgs e)
        {
            RegistroForm registroForm = new RegistroForm();
            registroForm.ShowDialog();
            CargarDatosUsuarios();
        }

        private void btnEliminarUsuario_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsuarios.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dataGridViewUsuarios.SelectedRows[0].Cells["ID"].Value);
                try
                {
                    conexion.Open();
                    string consulta = "DELETE FROM Usuario WHERE ID=@id";
                    MySqlCommand cmd = new MySqlCommand(consulta, conexion);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Usuario eliminado correctamente.");
                    CargarDatosUsuarios();
                    }
                    
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar usuario: " + ex.Message);
                }
                finally
                {
                    conexion.Close();
                }
            }
        }

        private void btnActualizarUsuario_Click(object sender, EventArgs e)
        {
            if (dataGridViewUsuarios.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dataGridViewUsuarios.SelectedRows[0].Cells["ID"].Value);
                string usuario = txtUser.Text;
                string contraseña = txtPwd.Text;
                string nombre = txtNombre.Text;
                string apellido = txtApellido.Text;
                string genero = cmbGenero.SelectedItem?.ToString() ?? "";
                string ubicacion = txtUbicacion.Text;

                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contraseña) || string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) || string.IsNullOrEmpty(genero) || string.IsNullOrEmpty(rutaFoto) || string.IsNullOrEmpty(ubicacion))
                {
                    MessageBox.Show("Por favor, complete todos los campos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    conexion.Open();
                    string consulta = "UPDATE Usuario SET User=@user, Password=@password, Nombre=@nombre, Apellido=@apellido, Genero=@genero, Foto=@foto, Ubicacion=@ubicacion WHERE ID=@id";
                    MySqlCommand cmd = new MySqlCommand(consulta, conexion);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@user", usuario);
                    cmd.Parameters.AddWithValue("@password", contraseña);
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@apellido", apellido);
                    cmd.Parameters.AddWithValue("@genero", genero);
                    cmd.Parameters.AddWithValue("@foto", rutaFoto);
                    cmd.Parameters.AddWithValue("@ubicacion", ubicacion);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Usuario actualizado correctamente.");
                    CargarDatosUsuarios();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al actualizar usuario: " + ex.Message);
                }
                finally
                {
                    conexion.Close();
                }
            }
        }

        private void btnCargarFoto_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                rutaFoto = openFileDialog1.FileName;
                pictureBoxFoto.Image = Image.FromFile(rutaFoto);
                txtFoto.Text = rutaFoto;
            }
        }

        private void btnRegistrarUsuario_Click(object sender, EventArgs e)
        {
            string usuario = txtUser.Text;
            string contraseña = txtPwd.Text;
            string nombre = txtNombre.Text;
            string apellido = txtApellido.Text;
            string genero = cmbGenero.SelectedItem?.ToString() ?? "";
            string ubicacion = txtUbicacion.Text;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contraseña) || string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) || string.IsNullOrEmpty(genero) || string.IsNullOrEmpty(rutaFoto) || string.IsNullOrEmpty(ubicacion))
            {
                MessageBox.Show("Por favor, complete todos los campos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                conexion.Open();
                string consulta = "INSERT INTO Usuario (User, Password, Nombre, Apellido, Genero, Foto, Ubicacion) VALUES (@user, @password, @nombre, @apellido, @genero, @foto, @ubicacion)";
                MySqlCommand cmd = new MySqlCommand(consulta, conexion);
                cmd.Parameters.AddWithValue("@user", usuario);
                cmd.Parameters.AddWithValue("@password", contraseña);
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@apellido", apellido);
                cmd.Parameters.AddWithValue("@genero", genero);
                cmd.Parameters.AddWithValue("@foto", rutaFoto);
                cmd.Parameters.AddWithValue("@ubicacion", ubicacion);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Registro completado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarDatosUsuarios();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar usuario: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conexion.Close();
            }
        }
    }
}
