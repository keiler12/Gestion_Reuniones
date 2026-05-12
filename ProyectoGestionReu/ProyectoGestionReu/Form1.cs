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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

      
        private void Form1_Load(object sender, EventArgs e)
        {

        }

       

        

        public void SoloNumeros(KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }
       

       

        private void btbsalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Deseas salir?", "salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3) == System.Windows.Forms.DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void txtIdUsuario_KeyPress(object sender, KeyPressEventArgs e)
        {
            SoloNumeros(e);
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            var conexion = new Conexion();
            var colUsuarios = conexion.GetCollection<Usuario>("Usuarios");
            var colSemilleros = conexion.GetCollection<Semillero>("Semillero");

            if (int.TryParse(txtIdUsuario.Text.Trim(), out int idConvertido))
            {
                var usuario = colUsuarios
                    .Find(u => u.codigoUsuario == idConvertido && u.contraseñaUsuario == txtContraseña.Text.Trim())
                    .FirstOrDefault();

                if (usuario != null)
                {
                    var semillero = colSemilleros
                        .Find(s => s.codigoSemillero == usuario.codigoSemillero)
                        .FirstOrDefault();

                    MessageBox.Show("Bienvenido " + usuario.rolUsuario + ": " + usuario.nombreUsuario);

                    if (usuario.rolUsuario == "Lider")
                    {
                        FormLiderMenu liderMenu = new FormLiderMenu(usuario, semillero);
                        liderMenu.Show();
                        this.Hide();
                    }
                    else if (usuario.rolUsuario == "Investigador")
                    {
                        Form investigador = new formInvestigador(usuario, semillero);
                        investigador.Show();
                        this.Hide();
                    }
                }
                else
                {
                    MessageBox.Show("Credenciales incorrectas");
                }
            }
        }
    }
}
