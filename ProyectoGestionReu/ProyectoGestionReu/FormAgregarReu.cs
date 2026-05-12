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
    public partial class FormAgregarReu : Form
    {
        private Usuario _lider;
        private Semillero _semillero;
        private Reunion reunionEditar;
        private bool esEdicion = false;

        private readonly List<string> todasLasHoras = new List<string>
        {
            "07:00", "08:00", "09:00", "10:00", "11:00",
            "13:00", "14:00", "15:00", "16:00", "17:00"
        };

        public FormAgregarReu(Usuario lider, Semillero semillero)
        {
            InitializeComponent();
            _lider = lider;
            _semillero = semillero;
            esEdicion = false;
        }

        public FormAgregarReu(Usuario lider, Semillero semillero, Reunion reunion)
        {
            InitializeComponent();
            _lider = lider;
            _semillero = semillero;
            reunionEditar = reunion;
            esEdicion = true;
        }

        private void FormAgregarReu_Load(object sender, EventArgs e)
        {
            cboDuracion.Items.Add("30 minutos");
            cboDuracion.Items.Add("1 hora");
            cboDuracion.Items.Add("1 hora 30 minutos");
            cboDuracion.Items.Add("2 horas");
            cboDuracion.SelectedIndex = 0;

            calendario.MinDate = DateTime.Today;
            calendario.MaxDate = DateTime.Today.AddMonths(1);

            CargarHorasDisponibles(DateTime.Today);
            CargarInvestigadores(DateTime.Today);

            if (esEdicion)
            {
                calendario.MinDate = DateTime.Today;
                calendario.MaxDate = DateTime.Today.AddMonths(1);
                calendario.SetDate(reunionEditar.fechaReunion.ToLocalTime());
                CargarHorasDisponibles(reunionEditar.fechaReunion.ToLocalTime());
                CargarInvestigadores(reunionEditar.fechaReunion.ToLocalTime());

                string horaEditar = reunionEditar.horaInicio.ToLocalTime().ToString("HH:mm");
                for (int i = 0; i < lstHoras.Items.Count; i++)
                {
                    if (lstHoras.Items[i].ToString().StartsWith(horaEditar))
                    {
                        lstHoras.SelectedIndex = i;
                        break;
                    }
                }

                txtMotivo.Text = reunionEditar.motivoReunion;

                for (int i = 0; i < chkInvestigadores.Items.Count; i++)
                {
                    Usuario inv = (Usuario)chkInvestigadores.Items[i];
                    if (reunionEditar.codigosInvestigadores.Contains(inv.codigoUsuario))
                        chkInvestigadores.SetItemChecked(i, true);
                }
            }
        }

        private void calendario_DateChanged(object sender, DateRangeEventArgs e)
        {
            CargarHorasDisponibles(e.Start);
            CargarInvestigadores(e.Start);
        }

        private void CargarHorasDisponibles(DateTime fecha)
        {
            lstHoras.Items.Clear();

            var conexion = new Conexion();
            var colReuniones = conexion.GetCollection<Reunion>("Reuniones");

            var todasReuniones = colReuniones.Find(_ => true).ToList();

            var horasOcupadas = todasReuniones
                .Where(r => r.fechaReunion.ToLocalTime().Date == fecha.Date &&
                            r.codigoLider == _lider.codigoUsuario)
                .Select(r => r.horaInicio.ToLocalTime().ToString("HH:mm"))
                .ToList();

            foreach (string hora in todasLasHoras)
            {
                TimeSpan horaTS = TimeSpan.Parse(hora);

                bool horaPasada = fecha.Date == DateTime.Today &&
                                  horaTS <= DateTime.Now.TimeOfDay.Add(new TimeSpan(2, 0, 0));

                if (horasOcupadas.Contains(hora) || horaPasada)
                    lstHoras.Items.Add(hora + "  ✗ No disponible");
                else
                    lstHoras.Items.Add(hora + "  ✓ Disponible");
            }
        }

        private void CargarInvestigadores(DateTime fecha)
        {
            var conexion = new Conexion();
            var colUsuarios = conexion.GetCollection<Usuario>("Usuarios");
            var colReuniones = conexion.GetCollection<Reunion>("Reuniones");

            var investigadores = colUsuarios
                .Find(u => u.codigoSemillero == _semillero.codigoSemillero && u.rolUsuario == "Investigador")
                .ToList();

            var todasReuniones = colReuniones.Find(_ => true).ToList();
            var investigadoresOcupados = todasReuniones
                .Where(r => r.fechaReunion.ToLocalTime().Date == fecha.Date)
                .SelectMany(r => r.codigosInvestigadores)
                .Distinct()
                .ToList();

            chkInvestigadores.Items.Clear();
            foreach (var inv in investigadores)
            {
                if (!investigadoresOcupados.Contains(inv.codigoUsuario))
                    chkInvestigadores.Items.Add(inv, false);
            }

            chkInvestigadores.DisplayMember = "nombreUsuario";
        }

        private string CalcularEstado(DateTime fechaReunion, DateTime horaInicio, DateTime horaFin)
        {
            DateTime ahora = DateTime.Now;
            DateTime inicioLocal = horaInicio.ToLocalTime();
            DateTime finLocal = horaFin.ToLocalTime();

            if (ahora < inicioLocal)
                return "Pendiente";
            else if (ahora >= inicioLocal && ahora <= finLocal)
                return "En curso";
            else
                return "Realizada";
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {



            // 1. Validar campos vacíos
            if (txtMotivo.Text.Trim() == "")
            {
                MessageBox.Show("Complete el motivo.", "Campo vacío", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validar que se seleccionó una hora
            if (lstHoras.SelectedIndex < 0)
            {
                MessageBox.Show("Seleccione una hora.", "Hora no seleccionada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Validar que la hora no esté ocupada
            if (lstHoras.SelectedItem.ToString().Contains("✗ No disponible"))
            {
                MessageBox.Show("Esa hora no está disponible.", "Hora no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 4. Validar que se seleccionó al menos un investigador
            if (chkInvestigadores.CheckedItems.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un investigador.", "Sin investigador", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 5. Validar domingo
            if (calendario.SelectionStart.DayOfWeek == DayOfWeek.Sunday)
            {
                MessageBox.Show("No se pueden programar reuniones los domingos.", "Fecha no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 6. Validar fecha pasada
            if (calendario.SelectionStart.Date < DateTime.Today)
            {
                MessageBox.Show("No puede programar una reunión en una fecha pasada.", "Fecha no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 7. Validar 2 horas de anticipación si es hoy
            string horaSeleccionada = todasLasHoras[lstHoras.SelectedIndex];
            TimeSpan horaTS = TimeSpan.Parse(horaSeleccionada);

            if (calendario.SelectionStart.Date == DateTime.Today && horaTS <= DateTime.Now.TimeOfDay.Add(new TimeSpan(2, 0, 0)))
            {
                MessageBox.Show("Debe programar la reunión con al menos 2 horas de anticipación.", "Hora no válida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conexion = new Conexion();
            var colReuniones = conexion.GetCollection<Reunion>("Reuniones");

            DateTime fecha = calendario.SelectionStart.Date;
            

            int minutos = 30;

            if (cboDuracion.SelectedItem.ToString() == "30 minutos")
                minutos = 30;
            else if (cboDuracion.SelectedItem.ToString() == "1 hora")
                minutos = 60;
            else if (cboDuracion.SelectedItem.ToString() == "1 hora 30 minutos")
                minutos = 90;
            else if (cboDuracion.SelectedItem.ToString() == "2 horas")
                minutos = 120;

            DateTime horaInicio = fecha.Add(horaTS);
            DateTime horaFin = horaInicio.AddMinutes(minutos);

            // Calcular estado
            string estado = CalcularEstado(fecha, horaInicio, horaFin);

            // Obtener investigadores seleccionados
            List<int> codigosSeleccionados = new List<int>();
            foreach (Usuario inv in chkInvestigadores.CheckedItems)
                codigosSeleccionados.Add(inv.codigoUsuario);

            // Obtener ultimo codigo
            var ultimaReunion = colReuniones
                .Find(_ => true)
                .SortByDescending(r => r.codigoReunion)
                .FirstOrDefault();

            int nuevoCodigo = ultimaReunion != null ? ultimaReunion.codigoReunion + 1 : 301;

            var nuevaReunion = new Reunion
            {
                codigoReunion = nuevoCodigo,
                fechaReunion = fecha.ToUniversalTime(),
                horaInicio = horaInicio.ToUniversalTime(),
                horaFin = horaFin.ToUniversalTime(),
                motivoReunion = txtMotivo.Text.Trim(),
                codigoLider = _lider.codigoUsuario,
                codigosInvestigadores = codigosSeleccionados,
                codigoSemillero = _semillero.codigoSemillero,
                estadoReunion = estado
            };

            if (esEdicion)
            {
                int duracionMinutos = (int)(reunionEditar.horaFin - reunionEditar.horaInicio).TotalMinutes;

                if (duracionMinutos == 30)
                    cboDuracion.SelectedItem = "30 minutos";
                else if (duracionMinutos == 60)
                    cboDuracion.SelectedItem = "1 hora";
                else if (duracionMinutos == 90)
                    cboDuracion.SelectedItem = "1 hora 30 minutos";
                else if (duracionMinutos == 120)
                    cboDuracion.SelectedItem = "2 horas";
                else
                    cboDuracion.SelectedItem = "30 minutos";

                var filtro = Builders<Reunion>.Filter.Eq(r => r.codigoReunion, reunionEditar.codigoReunion);
                var actualizacion = Builders<Reunion>.Update
                    .Set(r => r.fechaReunion, fecha.ToUniversalTime())
                    .Set(r => r.horaInicio, horaInicio.ToUniversalTime())
                    .Set(r => r.horaFin, horaFin.ToUniversalTime())
                    .Set(r => r.motivoReunion, txtMotivo.Text.Trim())
                    .Set(r => r.codigosInvestigadores, codigosSeleccionados)
                    .Set(r => r.estadoReunion, estado);

                colReuniones.UpdateOne(filtro, actualizacion);
                MessageBox.Show("Reunión actualizada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                colReuniones.InsertOne(nuevaReunion);
                MessageBox.Show("Reunión programada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}