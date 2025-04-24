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

        private int ?codigo_cliente = null;

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

                if (codigo_cliente == null)
                {
                    // Insert CREATE
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
                else
                {
                    // UPDATE
                    cmd.CommandText = $"UPDATE `dadosdecliente` SET " +
                        $"nomecompleto = @nomecompleto, " +
                        $"nomesocial = @nomesocial, " +
                        $"email = @email, " +
                        $"cpf = @cpf " +
                        $"WHERE codigo = @codigo";

                    cmd.Parameters.AddWithValue("@nomecompleto", txtNomeCompleto.Text.Trim());
                    cmd.Parameters.AddWithValue("@nomesocial", txtNomeSocial.Text.Trim());
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@cpf", cpf);
                    cmd.Parameters.AddWithValue("@codigo", codigo_cliente);


                    // Executar o comando de alteração no banco
                    cmd.ExecuteNonQuery();


                    // Mensagem de sucesso para dados atualizados
                    MessageBox.Show($"os dados com o código {codigo_cliente} foram alterados com sucesso!",
                                    "Sucesso",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }

                codigo_cliente = null;

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

        private void lstCliente_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            ListView.SelectedListViewItemCollection clientedaselecao = lstCliente.SelectedItems;

            btnExcluirCliente.Visible = true;

            foreach (ListViewItem item in clientedaselecao)
            {
                codigo_cliente = Convert.ToInt32 (item.SubItems[0].Text);

                // Exibe uma Message Box com o código do cliente

                MessageBox.Show("Código do cliente: " + codigo_cliente.ToString(),
                                "Código selecionado",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                txtNomeCompleto.Text = item.SubItems[1].Text;
                txtNomeSocial.Text = item.SubItems[2].Text;
                txtEmail.Text = item.SubItems[3].Text;
                txtCPF.Text = item.SubItems[4].Text;
            }

            tabControl1.SelectedIndex = 0;
        }

        private void btnNovoCliente_Click(object sender, EventArgs e)
        {
            zerar_formulario();
        }

        private void zerar_formulario()
        {
            codigo_cliente = null;

            txtNomeCompleto.Text = String.Empty;
            txtNomeSocial.Text = " ";
            txtEmail.Text = " ";
            txtCPF.Text = " ";

            txtNomeCompleto.Focus();

            btnExcluirCliente.Visible = false;

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            excluir_cliente();
        }

        private void btnExcluirCliente_Click(object sender, EventArgs e)
        {
            excluir_cliente();
        }

        private void excluir_cliente()
        {
            
            try
            {
                DialogResult opcaoDigitada = MessageBox.Show("Tem certeza que deseja excluir o registro de código : " + codigo_cliente,
                                  "Tem certeza?",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Warning);

                if (opcaoDigitada == DialogResult.Yes)
                {
                    conexao = new MySqlConnection(data_source);
                    conexao.Open();

                    MySqlCommand cdm = new MySqlCommand();

                    cdm.Connection = conexao;

                    cdm.Prepare();

                    cdm.CommandText = "DELETE FROM dadosdecliente WHERE codigo = @codigo";

                    cdm.Parameters.AddWithValue("@codigo", codigo_cliente);

                    cdm.ExecuteNonQuery();

                    // Excluir no Banco de Dados
                    MessageBox.Show("Os dados do cliente foram EXCLUIDOS!",
                                    "Sucesso",
                                     MessageBoxButtons.OK,
                                     MessageBoxIcon.Information);

                    carregar_clientes();

                    zerar_formulario();

                  

                }
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

                if (conexao != null && conexao.State == ConnectionState.Open)
                {
                    conexao.Close();

                    // Teste de fechamento de banco
                    //  MessageBox.Show("Conexão fechada com sucesso");
                }
            }

            MessageBox.Show("Excluir código " + codigo_cliente);


        }
    }
}
