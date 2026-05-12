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
    public partial class formInvestigador : Form
    {
        private Usuario _usuario;
        private Semillero _semillero;

        public formInvestigador()
        {
            InitializeComponent();
        }

        public formInvestigador(Usuario usuario, Semillero semillero)
        {
            InitializeComponent();
            _usuario = usuario;
            _semillero = semillero;
        }

        private void formInvestigador_Load(object sender, EventArgs e)
        {
            label1.Text = "Bienvenido, " + _usuario.nombreUsuario;
            label4.Text = "Semillero: " + _semillero.nombreSemillero;
            panel1.BackColor = Color.FromArgb(20, 0, 0, 0);
            panel2.BackColor = Color.FromArgb(20, 0, 0, 0);
            panel3.BackColor = Color.FromArgb(20, 0, 0, 0);
            panel4.BackColor = Color.FromArgb(20, 0, 0, 0);
            dataGridView1.Visible = false;
            CargarMiembrosSemillero();
        }

        private void CargarMiembrosSemillero()
        {
            var conexion = new Conexion();
            var colUsuarios = conexion.GetCollection<Usuario>("Usuarios");

            var usuarios = colUsuarios
                .Find(u => u.codigoSemillero == _semillero.codigoSemillero)
                .ToList();

            var lista = usuarios.Select(u => new
            {
                Nombre = u.nombreUsuario,
                Rol = u.rolUsuario
            }).ToList();

            DataGridView2.AutoGenerateColumns = true;
            DataGridView2.DataSource = lista;
        }


        private void CargarReuniones()
        {
            var conexion = new Conexion();
            var vistaReuniones = conexion.GetCollection<BsonDocument>("Reun,se,u");

            var reuniones = vistaReuniones.Find(new BsonDocument()).ToList();
           
            var lista = reuniones
                .Where(r => r["infoInvestigadores"].AsBsonArray.Any(i => i["nombreUsuario"].AsString == _usuario.nombreUsuario))
                .Select(r => new
                {
                    Codigo = r["codigoReunion"].ToInt32(),
                    Fecha = r["fechaReunion"].ToUniversalTime().ToLocalTime().ToString("dd/MM/yyyy"),
                    HoraInicio = r["horaInicio"].ToUniversalTime().ToLocalTime().ToString("HH:mm"),
                    HoraFin = r["horaFin"].ToUniversalTime().ToLocalTime().ToString("HH:mm"),
                    Motivo = r["motivoReunion"].AsString,
                    Lider = r["infoLider"]["nombreUsuario"].AsString,
                    Semillero = r["infoSemillero"]["nombreSemillero"].AsString,
                    Estado = r["estadoReunion"].AsString
                }).ToList();

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = lista;
            dataGridView1.ReadOnly = true;
        }
        private void btnCons_Click(object sender, EventArgs e)
        {
            dataGridView1.Visible = true;
            CargarReuniones();
        }

        private void btbsalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Deseas salir?", "salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3) == System.Windows.Forms.DialogResult.Yes)
            {
                Form1 salir = new Form1();
                salir.Show();
                this.Hide();
            }
        }

        private void cboParametro_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboParametro.SelectedItem == null) return;

            cboValor.Items.Clear();

            var conexion = new Conexion();
            var colReuniones = conexion.GetCollection<Reunion>("Reuniones");

            var todasReuniones = colReuniones.Find(_ => true).ToList()
                .Where(r => r.codigosInvestigadores.Contains(_usuario.codigoUsuario))
                .ToList();

            if (cboParametro.SelectedItem.ToString() == "Mes")
            {
                var meses = todasReuniones
                    .Select(r => r.fechaReunion.ToLocalTime().ToString("MMMM yyyy"))
                    .Distinct()
                    .ToList();

                foreach (var mes in meses)
                    cboValor.Items.Add(mes);
            }
            else if (cboParametro.SelectedItem.ToString() == "Año")
            {
                var años = todasReuniones
                    .Select(r => r.fechaReunion.ToLocalTime().Year.ToString())
                    .Distinct()
                    .ToList();

                foreach (var año in años)
                    cboValor.Items.Add(año);
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (cboParametro.SelectedIndex < 0 || cboValor.SelectedIndex < 0)
            {
                MessageBox.Show("Seleccione un parámetro y un valor.", "Sin selección", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conexion = new Conexion();
            var vistaReuniones = conexion.GetCollection<BsonDocument>("Reun,se,u");

            var todasReuniones = vistaReuniones.Find(new BsonDocument()).ToList()
                .Where(r => r["infoInvestigadores"].AsBsonArray
                    .Any(i => i["codigoUsuario"].ToInt32() == _usuario.codigoUsuario))
                .ToList();

            List<BsonDocument> reunionesFiltradas;

            if (cboParametro.SelectedItem.ToString() == "Mes")
            {
                string mesSeleccionado = cboValor.SelectedItem.ToString();
                reunionesFiltradas = todasReuniones
                    .Where(r => r["fechaReunion"].ToUniversalTime().ToLocalTime().ToString("MMMM yyyy") == mesSeleccionado)
                    .ToList();
            }
            else
            {
                string añoSeleccionado = cboValor.SelectedItem.ToString();
                reunionesFiltradas = todasReuniones
                    .Where(r => r["fechaReunion"].ToUniversalTime().ToLocalTime().Year.ToString() == añoSeleccionado)
                    .ToList();
            }

            var lista = reunionesFiltradas.Select(r => new
            {
                Codigo = r["codigoReunion"].ToInt32(),
                Fecha = r["fechaReunion"].ToUniversalTime().ToLocalTime().ToString("dd/MM/yyyy"),
                HoraInicio = r["horaInicio"].ToUniversalTime().ToLocalTime().ToString("HH:mm"),
                HoraFin = r["horaFin"].ToUniversalTime().ToLocalTime().ToString("HH:mm"),
                Motivo = r["motivoReunion"].AsString,
                Lider = r["infoLider"]["nombreUsuario"].AsString,
                Semillero = r["infoSemillero"]["nombreSemillero"].AsString,
                Estado = r["estadoReunion"].AsString
            }).ToList();

            dataGridView1.Visible = true;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = lista;
            dataGridView1.ReadOnly = true;
        }

        private void cboValor_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            cboParametro.SelectedIndex = -1;
            cboValor.Items.Clear();
            cboValor.SelectedIndex = -1;
            dataGridView1.Visible = false;
        }
    }
    
}

