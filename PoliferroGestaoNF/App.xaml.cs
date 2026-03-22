using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace PoliferroGestaoNF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Configurar cultura para português do Brasil
                var culture = new CultureInfo("pt-BR");
                culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
                culture.DateTimeFormat.LongDatePattern = "dd/MM/yyyy";
                culture.DateTimeFormat.DateSeparator = "/";

                // Aplicar cultura para a thread atual
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                // Aplicar cultura para toda a aplicação
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(
                        System.Windows.Markup.XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao configurar cultura: {ex.Message}");
            }
        }
    }
}