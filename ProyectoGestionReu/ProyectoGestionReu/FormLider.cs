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
    public partial class formLider : Form
    {
        private Usuario _usuario;
        private Semillero _semillero;

        public formLider()
        {
            InitializeComponent();
        }

        public formLider(Usuario usuario, Semillero semillero)
        {
            InitializeComponent();
            _usuario = usuario;
            _semillero = semillero;
        }

        private void formLider_Load(object sender, EventArgs e)
        {
            panel2.BackColor = Color.FromArgb(20, 0, 0, 0);
            panel1.BackColor = Color.FromArgb(20, 0, 0, 0);

            CargarReuniones();

            cboParametro.Items.Add("Nombre");
            cboParametro.Items.Add("Fecha");

            
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
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            FormAgregarReu ventana = new FormAgregarReu(_usuario, _semillero);
            if (ventana.ShowDialog() == DialogResult.OK)
            {
                CargarReuniones();
            }
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (DataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione una reunión para editar.", "Sin selección", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int codigoReunion = Convert.ToInt32(DataGridView1.SelectedRows[0].Cells["Codigo"].Value);

            var conexion = new Conexion();
            var colReuniones = conexion.GetCollection<Reunion>("Reuniones");
            var reunion = colReuniones.Find(r => r.codigoReunion == codigoReunion).FirstOrDefault();

            if (reunion != null)
            {
                // Validar que la reunión no haya pasado
                if (reunion.fechaReunion.ToLocalTime().Date < DateTime.Today)
                {
                    MessageBox.Show("No puede editar una reunión que ya pasó.", "No permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                FormAgregarReu ventana = new FormAgregarReu(_usuario, _semillero, reunion);
                if (ventana.ShowDialog() == DialogResult.OK)
                {
                    CargarReuniones();
                }
            }

            

        }

        private void btnEliminar_Click_1(object sender, EventArgs e)
        {
            if (DataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione una reunión para cancelar.", "Sin selección", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int codigoReunion = Convert.ToInt32(DataGridView1.SelectedRows[0].Cells["Codigo"].Value);

            var conexion = new Conexion();
            var colReuniones = conexion.GetCollection<Reunion>("Reuniones");
            var reunion = colReuniones.Find(r => r.codigoReunion == codigoReunion).FirstOrDefault();

            // Validar que no esté ya cancelada o realizada
            if (reunion.estadoReunion == "Cancelada")
            {
                MessageBox.Show("Esta reunión ya está cancelada.", "No permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (reunion.estadoReunion == "Realizada")
            {
                MessageBox.Show("No puede cancelar una reunión que ya fue realizada.", "No permitido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmacion = MessageBox.Show(
                "¿Está seguro que desea cancelar esta reunión?",
                "Confirmar cancelación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmacion == DialogResult.Yes)
            {
                var filtro = Builders<Reunion>.Filter.Eq(r => r.codigoReunion, codigoReunion);
                var actualizacion = Builders<Reunion>.Update
                    .Set(r => r.estadoReunion, "Cancelada");

                colReuniones.UpdateOne(filtro, actualizacion);
                MessageBox.Show("Reunión cancelada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarReuniones();
            }
        }

        private void cboParametro_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboParametro.SelectedItem == null) return;

            cboValor.Items.Clear();

            var conexion = new Conexion();
            var colReuniones = conexion.GetCollection<Reunion>("Reuniones");
            var colUsuarios = conexion.GetCollection<Usuario>("Usuarios");

            var todasReuniones = colReuniones.Find(_ => true).ToList();

            if (cboParametro.SelectedItem.ToString() == "Nombre")
            {
                var codigosInvestigadores = todasReuniones
                    .Where(r => r.codigoLider == _usuario.codigoUsuario)
                    .SelectMany(r => r.codigosInvestigadores)
                    .Distinct()
                    .ToList();

                var investigadores = colUsuarios
                    .Find(u => codigosInvestigadores.Contains(u.codigoUsuario))
                    .ToList();

                foreach (var inv in investigadores)
                    cboValor.Items.Add(inv.nombreUsuario);
            }
            else if (cboParametro.SelectedItem.ToString() == "Fecha")
            {
                var fechas = todasReuniones
                    .Where(r => r.codigoLider == _usuario.codigoUsuario)
                    .Select(r => r.fechaReunion.ToLocalTime().ToString("dd/MM/yyyy"))
                    .Distinct()
                    .ToList();

                foreach (var fecha in fechas)
                    cboValor.Items.Add(fecha);
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

            var todasReuniones = vistaReuniones.Find(new BsonDocument()).ToList();

            List<BsonDocument> reunionesFiltradas;

            if (cboParametro.SelectedItem.ToString() == "Nombre")
            {
                string nombreSeleccionado = cboValor.SelectedItem.ToString();
                reunionesFiltradas = todasReuniones
                    .Where(r => r["infoInvestigadores"].AsBsonArray
                        .Any(i => i["nombreUsuario"].AsString == nombreSeleccionado))
                    .ToList();
            }
            else
            {
                string fechaSeleccionada = cboValor.SelectedItem.ToString();
                reunionesFiltradas = todasReuniones
                    .Where(r => r["fechaReunion"].ToUniversalTime().ToLocalTime().ToString("dd/MM/yyyy") == fechaSeleccionada)
                    .ToList();
            }

            var lista = reunionesFiltradas.Select(r => new
            {
                Codigo = r["codigoReunion"].ToInt32(),
                Fecha = r["fechaReunion"].ToUniversalTime().ToLocalTime().ToString("dd/MM/yyyy"),
                HoraInicio = r["horaInicio"].ToUniversalTime().ToLocalTime().ToString("HH:mm"),
                HoraFin = r["horaFin"].ToUniversalTime().ToLocalTime().ToString("HH:mm"),
                Motivo = r["motivoReunion"].AsString,
                Investigadores = string.Join(", ", r["infoInvestigadores"].AsBsonArray
                 .Select(i => i["nombreUsuario"].AsString)),
                Lider = r["infoLider"]["nombreUsuario"].AsString,
                Semillero = r["infoSemillero"]["nombreSemillero"].AsString,
                Estado = r["estadoReunion"].AsString
            }).ToList();

            DataGridView1.DataSource = lista;
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            cboParametro.SelectedIndex = -1;
            cboValor.Items.Clear();
            cboValor.SelectedIndex = -1;
            CargarReuniones();
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (DataGridView1.SelectedRows.Count == 0)
            {
                btnEditar.Enabled = false;
                btnEliminar.Enabled = false;
                return;
            }

            string estado = DataGridView1.SelectedRows[0].Cells["Estado"].Value.ToString();

            if (estado == "Pendiente")
            {
                btnEditar.Enabled = true;
                btnEliminar.Enabled = true;
            }
            else
            {
                btnEditar.Enabled = false;
                btnEliminar.Enabled = false;
            }
        }
    }
}