using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Mail;

namespace ProjetPrograComplémentaire
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            cmbProvider.SelectedIndex = 0; // Gmail par défaut
        }

        private void cmbProvider_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var selected = (cmbProvider.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString();
            switch (selected)
            {
                case "Gmail":
                    txtSmtpHost.Text = "smtp.gmail.com";
                    txtSmtpPort.Text = "587";
                    chkUseSsl.IsChecked = true;
                    break;
                case "Outlook.com / Hotmail":
                    // Outlook.com/Hotmail grand public
                    txtSmtpHost.Text = "smtp-mail.outlook.com";
                    txtSmtpPort.Text = "587";
                    chkUseSsl.IsChecked = true;
                    break;
                default:
                    // manuel
                    txtSmtpHost.Text = "";
                    txtSmtpPort.Text = "587";
                    chkUseSsl.IsChecked = true;
                    break;
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            txtSubject.Text = "Test WPF SMTP";
            txtBody.Text = "Bonjour,\n\nCeci est un message de test envoyé depuis une application WPF .NET Framework.";
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            btnSend.IsEnabled = false;
            lblStatus.Text = "Envoi en cours...";

            try
            {
                // Validations simples
                var from = new MailAddress(txtSenderEmail.Text.Trim());
                var to = new MailAddress(txtRecipientEmail.Text.Trim());
                if (string.IsNullOrWhiteSpace(pwdAppPassword.Password))
                    throw new InvalidOperationException("Le mot de passe d’application est vide.");

                if (!int.TryParse(txtSmtpPort.Text.Trim(), out int port))
                    throw new InvalidOperationException("Port SMTP invalide.");

                using (var message = new MailMessage(from, to))
                {
                    message.Subject = txtSubject.Text ?? "";
                    message.Body = txtBody.Text ?? "";
                    message.IsBodyHtml = false;

                    using (var client = new SmtpClient(txtSmtpHost.Text.Trim(), port))
                    {
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(from.Address, pwdAppPassword.Password);
                        client.EnableSsl = chkUseSsl.IsChecked == true;
                        client.Timeout = 20000;

                        // Assure TLS 1.2 sur anciens frameworks/OS
                        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                        await client.SendMailAsync(message);
                    }
                }

                lblStatus.Text = "Message envoyé ✅";
                MessageBox.Show("Message envoyé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (FormatException ex)
            {
                lblStatus.Text = "Adresse e-mail invalide.";
                MessageBox.Show("Vérifie les adresses e-mail.\n\n" + ex.Message, "Format incorrect", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SmtpFailedRecipientException ex)
            {
                lblStatus.Text = "Destinataire indisponible.";
                MessageBox.Show("Le destinataire a été refusé par le serveur.\n\n" + ex.Message, "Échec destinataire", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SmtpException ex)
            {
                lblStatus.Text = "Erreur SMTP.";
                MessageBox.Show("Échec d’envoi SMTP.\n\n" + ex.Message + "\n\nCauses fréquentes:\n- Mot de passe d’application non valide\n- 2FA non activée\n- Hôte/port/TLS incorrects", "Erreur SMTP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Erreur.";
                MessageBox.Show("Une erreur est survenue.\n\n" + ex.Message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnSend.IsEnabled = true;
            }
        }
    }
}