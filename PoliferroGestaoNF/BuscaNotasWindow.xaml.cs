using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace PoliferroGestaoNF
{
    public partial class BuscaNotasWindow : Window
    {
        private string pastaBase = @"C:\NOTAS_POLIFERRO";
        private List<NotaFiscal> todasNotas = new List<NotaFiscal>();

        public class NotaFiscal
        {
            public string Fornecedor { get; set; }
            public string NumeroNota { get; set; }
            public DateTime DataNota { get; set; }
            public string NomeArquivo { get; set; }
            public string CaminhoCompleto { get; set; }
        }

        public BuscaNotasWindow()
        {
            InitializeComponent();
            CarregarFornecedores();
            CarregarTodasNotas();
            ConfigurarEventos();
        }

        private void CarregarFornecedores()
        {
            try
            {
                var fornecedores = new List<string> { "TODOS" };

                if (Directory.Exists(pastaBase))
                {
                    var pastas = Directory.GetDirectories(pastaBase);
                    foreach (var pasta in pastas)
                    {
                        fornecedores.Add(Path.GetFileName(pasta));
                    }
                }

                cmbFornecedorBusca.ItemsSource = fornecedores;
                cmbFornecedorBusca.SelectedIndex = 0; // Seleciona "TODOS"
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar fornecedores: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CarregarTodasNotas()
        {
            try
            {
                todasNotas.Clear();

                if (!Directory.Exists(pastaBase))
                {
                    return;
                }

                var pastas = Directory.GetDirectories(pastaBase);

                foreach (var pasta in pastas)
                {
                    string fornecedor = Path.GetFileName(pasta);
                    var arquivos = Directory.GetFiles(pasta, "*.pdf");

                    foreach (var arquivo in arquivos)
                    {
                        string nomeArquivo = Path.GetFileName(arquivo);

                        // Extrair data e número do nome do arquivo (formato: yyyy-MM-dd_NF{numero}.pdf)
                        try
                        {
                            string dataStr = nomeArquivo.Substring(0, 10);
                            DateTime data = DateTime.ParseExact(dataStr, "yyyy-MM-dd", null);

                            string numeroStr = nomeArquivo.Replace(dataStr + "_NF", "").Replace(".pdf", "");

                            todasNotas.Add(new NotaFiscal
                            {
                                Fornecedor = fornecedor,
                                NumeroNota = numeroStr,
                                DataNota = data,
                                NomeArquivo = nomeArquivo,
                                CaminhoCompleto = arquivo
                            });
                        }
                        catch
                        {
                            // Se não conseguir extrair data, adiciona com data mínima
                            todasNotas.Add(new NotaFiscal
                            {
                                Fornecedor = fornecedor,
                                NumeroNota = "N/A",
                                DataNota = DateTime.MinValue,
                                NomeArquivo = nomeArquivo,
                                CaminhoCompleto = arquivo
                            });
                        }
                    }
                }

                // Ordenar por data decrescente (mais recentes primeiro)
                todasNotas = todasNotas.OrderByDescending(n => n.DataNota).ToList();

                AtualizarTotalEncontrados(todasNotas.Count);
                dgNotas.ItemsSource = todasNotas;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar notas: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigurarEventos()
        {
            btnBuscar.Click += BtnBuscar_Click;
            btnLimparBusca.Click += BtnLimparBusca_Click;
            btnFechar.Click += BtnFechar_Click;
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fornecedorFiltro = cmbFornecedorBusca.SelectedItem?.ToString();
                string numeroFiltro = txtNumeroNotaBusca.Text.Trim();
                DateTime? dataInicial = dateInicial.SelectedDate;
                DateTime? dataFinal = dateFinal.SelectedDate;

                var resultados = todasNotas.AsEnumerable();

                // Filtrar por fornecedor
                if (!string.IsNullOrEmpty(fornecedorFiltro) && fornecedorFiltro != "TODOS")
                {
                    resultados = resultados.Where(n => n.Fornecedor == fornecedorFiltro);
                }

                // Filtrar por número da nota
                if (!string.IsNullOrEmpty(numeroFiltro))
                {
                    resultados = resultados.Where(n =>
                        n.NumeroNota.Contains(numeroFiltro) ||
                        n.NomeArquivo.Contains(numeroFiltro));
                }

                // Filtrar por data inicial
                if (dataInicial.HasValue)
                {
                    resultados = resultados.Where(n => n.DataNota >= dataInicial.Value);
                }

                // Filtrar por data final
                if (dataFinal.HasValue)
                {
                    // Adiciona 1 dia para incluir todo o dia final
                    DateTime dataFinalAjustada = dataFinal.Value.AddDays(1);
                    resultados = resultados.Where(n => n.DataNota < dataFinalAjustada);
                }

                var listaResultados = resultados.ToList();

                AtualizarTotalEncontrados(listaResultados.Count);
                dgNotas.ItemsSource = listaResultados;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar notas: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLimparBusca_Click(object sender, RoutedEventArgs e)
        {
            // Limpar filtros
            cmbFornecedorBusca.SelectedIndex = 0;
            txtNumeroNotaBusca.Clear();
            dateInicial.SelectedDate = null;
            dateFinal.SelectedDate = null;

            // Voltar a mostrar todas as notas
            dgNotas.ItemsSource = todasNotas;
            AtualizarTotalEncontrados(todasNotas.Count);
        }

        private void BtnVisualizarNota_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                string caminhoArquivo = btn.Tag.ToString();

                if (File.Exists(caminhoArquivo))
                {
                    // Abre o PDF com o visualizador padrão do Windows
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = caminhoArquivo,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("Arquivo não encontrado!", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir arquivo: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAbrirPasta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                string caminhoArquivo = btn.Tag.ToString();
                string pasta = Path.GetDirectoryName(caminhoArquivo);

                if (Directory.Exists(pasta))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = pasta,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("Pasta não encontrada!", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir pasta: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AtualizarTotalEncontrados(int total)
        {
            txtTotalEncontrados.Text = $"Total de notas encontradas: {total}";
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}