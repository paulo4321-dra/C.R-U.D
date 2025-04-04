using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace cadastrodeclientes
{
    public partial class frmCadastrodeClientes : Form
    {
        // Conexão com o banco de dados Mysql

        MySqlConnection conexao;
        string data_source = "datasource=localhost; username=root; password=; database=db_cadastro";

        public frmCadastrodeClientes()
        {
            InitializeComponent();
        }
        private bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(email);
        }
        private bool IsValidCPFLegth(string cpf)
        {
            cpf = cpf.Replace(".", "").Replace("-", "");

            if (cpf.Length !=  11 || !cpf.All(char.IsDigit))
            {
                return false;
            }
            return true;
        }
        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                //Validação de campos obrigatórios 

                if(string.IsNullOrEmpty(txtNomeCompleto.Text.Trim()) ||
                   string.IsNullOrEmpty(txtEmail.Text.Trim()) ||
                   string.IsNullOrEmpty(txtCPF.Text.Trim()))
                {
                    MessageBox.Show("Todos os campos devem ser preenchidos.",
                                    "Validação",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return; // impede o prosseguimento se algum campo estiver vazio
                }
                // Validação do E-mail

                string email = txtEmail.Text.Trim();
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("E-mail inválido. Certifique-se de que o E-mail está no formato correto.",
                                    "Validação",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                // Validação do CPF
                string cpf = txtCPF.Text.Trim();
                if (!IsValidCPFLegth(cpf))
                {
                    MessageBox.Show("CPF inválido. Certifique-se de que o CPF tenha 11 digitos númericos",
                                    "Validação",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }


                // cria a conexão com o banco de dados
                conexao = new MySqlConnection(data_source);
                conexao.Open();

                //Teste de abertura de banco
               // MessageBox.Show("Conexão aberta com sucesso");

                // Comando SQL para inserir um novo cliente no banco de dados
                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = conexao
                };
                cmd.Prepare();

                cmd.CommandText = "INSERT INTO dadosdecliente (nomecompleto, nomesocial, email, cpf) " +
                    "VALUES (@nomecompleto, @nomesocial, @email, @cpf)";

                cmd.Parameters.AddWithValue("@nomecompleto", txtNomeCompleto.Text.Trim());
                cmd.Parameters.AddWithValue("@nomesocial", txtNomeSocial.Text.Trim());
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@cpf", cpf);

                // Executar o comando de inserção no banco

                cmd.ExecuteNonQuery();

                // Menssagem de sucesso
                MessageBox.Show("Contato inserido com sucesso: ",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            catch (MySqlException ex)
            {
                // Trata erros relacionados ao MySQL

                MessageBox.Show("Erro. " + ex.Number + "Ocorreu: " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

               
            }

            catch (Exception ex)
            {
                //Trata outros tipos de erro

                MessageBox.Show("Ocorreu: " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
             }

            finally
            {
                // Garante que a conexão com o banco de dados será fechada, mesmo se ocorrer erro

                if(conexao != null && conexao.State == ConnectionState.Open)
                {
                    conexao.Close();

                    // Teste de fechamento de banco
                  //  MessageBox.Show("Conexão fechada com sucesso");
                }
            }
        }
    }
}
