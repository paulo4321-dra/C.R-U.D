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

            // Configuração inicial da Listview para a exibição dos dados dos clientes
            lstCliente.View = View.Details;
            lstCliente.LabelEdit = true;
            lstCliente.AllowColumnReorder = true;
            lstCliente.FullRowSelect = true;
            lstCliente.GridLines = true;

            // Definindo as colunas da Listview
            lstCliente.Columns.Add("Codigo", 100, HorizontalAlignment.Left);
            lstCliente.Columns.Add("Nome Completo", 200, HorizontalAlignment.Left);
            lstCliente.Columns.Add("Nome Social", 200, HorizontalAlignment.Left);
            lstCliente.Columns.Add("E-mail", 200, HorizontalAlignment.Left);
            lstCliente.Columns.Add("CPF", 200, HorizontalAlignment.Left);


            carregar_clientes();
        }

        private void carregar_clientes_com_query(string query)
        {
            try
            {
                conexao = new MySqlConnection(data_source);
                conexao.Open();

                MySqlCommand cmd = new MySqlCommand(query, conexao);

                if (query.Contains("@q"))
                {
                    cmd.Parameters.AddWithValue("@q", "%" + txtBuscar.Text + "%");
                }

                MySqlDataReader reader = cmd.ExecuteReader();

                lstCliente.Items.Clear();

                while (reader.Read())
                {
                    string[] row =
                    {
                        Convert.ToString(reader.GetInt32(0)),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetString(4)
                    };

                    lstCliente.Items.Add(new ListViewItem(row));
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Erro. " + ex.Number + "Ocorreu: " + ex.Message,
                       "Erro",
                       MessageBoxButtons.OK,
                       MessageBoxIcon.Error);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu: " + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            finally
            {
                if (conexao != null && conexao.State == ConnectionState.Open)
                {
                    conexao.Close();
                }

            }
        }

        private void carregar_clientes()
        {
            string query = "SELECT * FROM dadosdecliente ORDER BY codigo DESC";
            carregar_clientes_com_query(query);
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

                // Limpar os campos após os sucesso
                txtNomeCompleto.Text = String.Empty;
                txtNomeSocial.Text = " ";
                txtEmail.Text = " ";
                txtCPF.Text = " ";


                // Recarregar os clientes na Listview
                carregar_clientes();


                // Muda para a aba de pesquisa
                tabControl1.SelectedIndex = 1;
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

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM dadosdecliente WHERE nomecompleto LIKE @q OR nomesocial LIKE @q ORDER BY codigo DESC";
            carregar_clientes_com_query(query);
        }
    }
}
