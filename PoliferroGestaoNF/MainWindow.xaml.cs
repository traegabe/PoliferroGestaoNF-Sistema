using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;

namespace PoliferroGestaoNF
{
    public partial class MainWindow : Window
    {
        private string pastaBase = @"C:\NOTAS_POLIFERRO";
        private string arquivoSelecionado = "";
        private bool webView2Inicializado = false;

        public MainWindow()
        {
            InitializeComponent();
            CriarPastaBase();
            CarregarFornecedores();
            ConfigurarEventos();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await InicializarWebView2();
        }

        private async System.Threading.Tasks.Task InicializarWebView2()
        {
            try
            {
                txtStatus.Text = "Inicializando visualizador de PDF...";
                txtStatus.Foreground = System.Windows.Media.Brushes.Orange;

                await webViewPDF.EnsureCoreWebView2Async(null);

                webViewPDF.CoreWebView2.NewWindowRequested += (s, args) =>
                {
                    args.Handled = true;
                };

                webView2Inicializado = true;
                txtStatus.Text = "Visualizador de PDF pronto!";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Erro ao inicializar visualizador";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;

                MessageBox.Show($"Erro ao inicializar WebView2: {ex.Message}\n\n" +
                    "O sistema continuará funcionando, mas a visualização de PDF será limitada.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CriarPastaBase()
        {
            try
            {
                if (!Directory.Exists(pastaBase))
                {
                    Directory.CreateDirectory(pastaBase);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao criar pasta base: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                cmbFornecedor.ItemsSource = fornecedores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar fornecedores: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigurarEventos()
        {
            btnGerenciarFornecedores.Click += BtnGerenciarFornecedores_Click;
            btnSelecionarPDF.Click += BtnSelecionarPDF_Click;
            btnSalvarNota.Click += BtnSalvarNota_Click;
            btnLimpar.Click += BtnLimpar_Click;
            btnBuscarNotas.Click += BtnBuscarNotas_Click;
        }

        private void BtnGerenciarFornecedores_Click(object sender, RoutedEventArgs e)
        {
            var fornecedorWindow = new FornecedorWindow();
            fornecedorWindow.Owner = this;
            fornecedorWindow.ShowDialog();
            CarregarFornecedores();
        }

        private void BtnSelecionarPDF_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos PDF|*.pdf",
                Title = "Selecione a Nota Fiscal em PDF"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                arquivoSelecionado = openFileDialog.FileName;
                txtCaminhoPDF.Text = arquivoSelecionado;
                VisualizarPDF(arquivoSelecionado);
            }
        }

        private async void VisualizarPDF(string caminhoArquivo)
        {
            try
            {
                borderSemPDF.Visibility = Visibility.Collapsed;
                borderLoading.Visibility = Visibility.Visible;
                webViewPDF.Visibility = Visibility.Collapsed;

                if (webView2Inicializado && File.Exists(caminhoArquivo))
                {
                    webViewPDF.CoreWebView2.Navigate(caminhoArquivo);
                    await System.Threading.Tasks.Task.Delay(500);

                    webViewPDF.Visibility = Visibility.Visible;
                    borderLoading.Visibility = Visibility.Collapsed;

                    txtStatus.Text = "PDF carregado com sucesso!";
                    txtStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    borderLoading.Visibility = Visibility.Collapsed;
                    borderSemPDF.Visibility = Visibility.Visible;

                    if (!webView2Inicializado)
                    {
                        txtStatus.Text = "Visualizador não inicializado";
                        txtStatus.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                borderLoading.Visibility = Visibility.Collapsed;
                borderSemPDF.Visibility = Visibility.Visible;

                txtStatus.Text = "Erro ao carregar PDF";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;

                MessageBox.Show($"Erro ao visualizar PDF: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== MÉTODO: BUSCAR NOTAS ==========
        private void BtnBuscarNotas_Click(object sender, RoutedEventArgs e)
        {
            var buscaWindow = new BuscaNotasWindow();
            buscaWindow.Owner = this;
            buscaWindow.ShowDialog();
        }

        // ========== MÉTODO: VERIFICAR SE NOTA JÁ EXISTE ==========
        private bool VerificarNotaDuplicada(string fornecedor, string numeroNota)
        {
            try
            {
                string pastaFornecedor = Path.Combine(pastaBase, fornecedor);

                if (!Directory.Exists(pastaFornecedor))
                {
                    return false;
                }

                var arquivos = Directory.GetFiles(pastaFornecedor, "*.pdf");

                foreach (string arquivo in arquivos)
                {
                    string nomeArquivo = Path.GetFileName(arquivo);

                    if (nomeArquivo.Contains($"_NF{numeroNota}.pdf"))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao verificar duplicidade: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ========== MÉTODO: LISTAR NOTAS DO FORNECEDOR ==========
        private List<string> ListarNotasDoFornecedor(string fornecedor)
        {
            var notas = new List<string>();

            try
            {
                string pastaFornecedor = Path.Combine(pastaBase, fornecedor);

                if (Directory.Exists(pastaFornecedor))
                {
                    var arquivos = Directory.GetFiles(pastaFornecedor, "*.pdf");
                    foreach (var arquivo in arquivos)
                    {
                        notas.Add(Path.GetFileName(arquivo));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao listar notas: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return notas;
        }

        // ========== NOVO MÉTODO: VALIDAR SE DATA É FUTURA ==========
        private bool ValidarDataFutura(DateTime dataSelecionada)
        {
            DateTime hoje = DateTime.Today;

            if (dataSelecionada > hoje)
            {
                MessageBox.Show(
                    $"❌ DATA INVÁLIDA!\n\n" +
                    $"A data da nota não pode ser futura.\n" +
                    $"Data informada: {dataSelecionada:dd/MM/yyyy}\n" +
                    $"Data atual: {hoje:dd/MM/yyyy}",
                    "ERRO - DATA FUTURA",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        // ========== MÉTODO SALVAR NOTA COM BLOQUEIO TOTAL E VALIDAÇÃO DE DATA ==========
        private void BtnSalvarNota_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validações básicas
                if (cmbFornecedor.SelectedItem == null)
                {
                    MessageBox.Show("Selecione um fornecedor!", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNumeroNota.Text))
                {
                    MessageBox.Show("Informe o número da nota!", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (dateNota.SelectedDate == null)
                {
                    MessageBox.Show("Selecione a data da nota!", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(arquivoSelecionado))
                {
                    MessageBox.Show("Selecione o arquivo PDF!", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string fornecedor = cmbFornecedor.SelectedItem.ToString();
                string numeroNota = txtNumeroNota.Text.Trim();
                DateTime dataNota = dateNota.SelectedDate.Value;

                // ===== NOVA VALIDAÇÃO: DATA FUTURA =====
                if (!ValidarDataFutura(dataNota))
                {
                    return; // Cancela a operação
                }

                // Criar pasta do fornecedor se não existir
                string pastaFornecedor = Path.Combine(pastaBase, fornecedor);
                if (!Directory.Exists(pastaFornecedor))
                {
                    Directory.CreateDirectory(pastaFornecedor);
                }

                // ===== VERIFICAÇÃO DE NOTA DUPLICADA =====
                bool notaJaExiste = VerificarNotaDuplicada(fornecedor, numeroNota);

                if (notaJaExiste)
                {
                    MessageBox.Show(
                        $"❌ NOTA FISCAL JÁ CADASTRADA!\n\n" +
                        $"Já existe uma nota com o número {numeroNota} para o fornecedor {fornecedor}.\n\n" +
                        $"Operação cancelada. Não é permitido cadastrar notas com o mesmo número para o mesmo fornecedor.",
                        "ERRO - NOTA DUPLICADA",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                // Gerar novo nome do arquivo
                string novoNome = $"{dataNota:yyyy-MM-dd}_NF{numeroNota}.pdf";
                string caminhoDestino = Path.Combine(pastaFornecedor, novoNome);

                // Salvar o arquivo
                File.Copy(arquivoSelecionado, caminhoDestino, false);

                MessageBox.Show($"✅ Nota fiscal SALVA com sucesso!\n\n" +
                    $"Fornecedor: {fornecedor}\n" +
                    $"Nota: {numeroNota}\n" +
                    $"Data: {dataNota:dd/MM/yyyy}\n\n" +
                    $"Local: {caminhoDestino}",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LimparCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar nota: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
        }

        private void LimparCampos()
        {
            cmbFornecedor.SelectedItem = null;
            txtNumeroNota.Clear();
            dateNota.SelectedDate = null;
            txtCaminhoPDF.Clear();
            arquivoSelecionado = "";

            try
            {
                if (webView2Inicializado && webViewPDF.CoreWebView2 != null)
                {
                    webViewPDF.CoreWebView2.NavigateToString("<html><body style='background:#E0E0E0; display:flex; justify-content:center; align-items:center; font-family:Arial; color:gray;'>Nenhum PDF selecionado</body></html>");
                }

                borderSemPDF.Visibility = Visibility.Visible;
                webViewPDF.Visibility = Visibility.Collapsed;
                borderLoading.Visibility = Visibility.Collapsed;

                txtStatus.Text = "Pronto para visualizar PDF";
                txtStatus.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch { }
        }
    }
}