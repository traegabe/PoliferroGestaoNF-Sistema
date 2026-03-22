using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace PoliferroGestaoNF
{
    public partial class FornecedorWindow : Window
    {
        private string pastaBase = @"C:\NOTAS_POLIFERRO";

        public FornecedorWindow()
        {
            InitializeComponent();
            CarregarFornecedores();
            ConfigurarEventos();
        }

        private void CarregarFornecedores()
        {
            try
            {
                var fornecedores = new List<string>();

                if (Directory.Exists(pastaBase))
                {
                    var pastas = Directory.GetDirectories(pastaBase);
                    foreach (var pasta in pastas)
                    {
                        fornecedores.Add(Path.GetFileName(pasta));
                    }
                }

                lstFornecedores.ItemsSource = fornecedores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar fornecedores: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigurarEventos()
        {
            btnCadastrar.Click += BtnCadastrar_Click;
            btnRemover.Click += BtnRemover_Click;
            btnAtualizar.Click += BtnAtualizar_Click;
            btnAdicionar.Click += BtnAdicionar_Click;
            btnFechar.Click += BtnFechar_Click;
        }

        private void BtnCadastrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string nomeFornecedor = txtNovoFornecedor.Text.Trim().ToUpper();

                if (string.IsNullOrWhiteSpace(nomeFornecedor))
                {
                    MessageBox.Show("Digite o nome do fornecedor!", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string pastaFornecedor = Path.Combine(pastaBase, nomeFornecedor);

                if (!Directory.Exists(pastaFornecedor))
                {
                    Directory.CreateDirectory(pastaFornecedor);
                    MessageBox.Show($"Fornecedor {nomeFornecedor} cadastrado com sucesso!",
                        "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                    txtNovoFornecedor.Clear();
                    CarregarFornecedores();
                }
                else
                {
                    MessageBox.Show("Este fornecedor já existe!", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao cadastrar fornecedor: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemover_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstFornecedores.SelectedItem == null)
                {
                    MessageBox.Show("Selecione um fornecedor para remover!", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string fornecedor = lstFornecedores.SelectedItem.ToString();

                var result = MessageBox.Show(
                    $"Tem certeza que deseja remover o fornecedor {fornecedor}?\n" +
                    "Todas as notas fiscais deste fornecedor serão excluídas!",
                    "Confirmar exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    string pastaFornecedor = Path.Combine(pastaBase, fornecedor);

                    if (Directory.Exists(pastaFornecedor))
                    {
                        Directory.Delete(pastaFornecedor, true);
                        MessageBox.Show("Fornecedor removido com sucesso!", "Sucesso",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        CarregarFornecedores();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao remover fornecedor: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            CarregarFornecedores();
        }

        private void BtnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            if (lstFornecedores.SelectedItem != null)
            {
                txtNovoFornecedor.Text = lstFornecedores.SelectedItem.ToString();
            }
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}