using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gestion_Reu
{
    public partial class FormLiderMenu : Form
    {
        private Usuario _usuario;
        private Semillero _semillero;

        public FormLiderMenu()
        {
            InitializeComponent();
        }

        public FormLiderMenu(Usuario usuario, Semillero semillero)
        {
            InitializeComponent();
            _usuario = usuario;
            _semillero = semillero;
        }

        private void FormLiderMenu_Load_1(object sender, EventArgs e)
        {
            label1.Text = "Bienvenido, " + _usuario.nombreUsuario;
            label4.Text = "Semillero: " + _semillero.nombreSemillero;
            panel1.BackColor = Color.FromArgb(20, 0, 0, 0);
            panel2.BackColor = Color.FromArgb(20, 0, 0, 0);
            panel3.BackColor = Color.FromArgb(20, 0, 0, 0);

            DataGridView1.Visible = false;
            CargarUsuariosSemillero();
        }

        private void CargarUsuariosSemillero()
        {
            var conexion = new Conexion();
            var colUsuarios = conexion.GetCollection<Usuario>("Usuarios");

            var usuarios = colUsuarios
                .Find(u => u.codigoSemillero == _semillero.codigoSemillero)
                .ToList();

            var lista = usuarios.Select(u => new
            {
                Codigo = u.codigoUsuario,
                Nombre = u.nombreUsuario,
                Rol = u.rolUsuario,
                Correo = u.correoUsuario,
                Edad = u.edadUsuario,
                Genero = u.generoUsuario
            }).ToList();

            DataGridView2.AutoGenerateColumns = true;
            DataGridView2.DataSource = lista;
        }

        private void CargarReuniones()
        {
            var conexion = new Conexion();
            var vistaReuniones = conexion.GetCollection<BsonDocument>("Reun,se,u");

            var reuniones = vistaReuniones.Find(new BsonDocument()).ToList();


            var lista = reuniones.Select(r => new
            {
                Codigo = r["codigoReunion"].ToInt32(),
                Fecha = r["fechaReunion"].ToLocalTime().ToString("dd/MM/yyyy"),
                HoraInicio = r["horaInicio"].ToLocalTime().ToString("HH:mm"),
                HoraFin = r["horaFin"].ToLocalTime().ToString("HH:mm"),
                Motivo = r["motivoReunion"].AsString,
                Investigadores = string.Join(", ", r["infoInvestigadores"].AsBsonArray
                    .Select(i => i["nombreUsuario"].AsString)),
                Lider = r["infoLider"]["nombreUsuario"].AsString,
                Semillero = r["infoSemillero"]["nombreSemillero"].AsString,
                Estado = r["estadoReunion"].AsString
            }).ToList();

            DataGridView1.AutoGenerateColumns = true;
            DataGridView1.DataSource = lista;
            DataGridView1.ReadOnly = true;
        }

        private void btnAgreRe_Click(object sender, EventArgs e)
        {
            formLider lider = new formLider(_usuario, _semillero);
            lider.ShowDialog();
        }

        private void btnCons_Click(object sender, EventArgs e)
        {
            DataGridView1.Visible = true;
            CargarReuniones();
        }

        private void btbsalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Deseas salir?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Form1 salir = new Form1();
                salir.Show();
                this.Hide();
            }
        }
    }
}