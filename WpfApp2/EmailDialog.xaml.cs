using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MarkdownEditorDiploma
{
    public partial class EmailDialog : Window
    {
        private readonly string _markdownContent;
        private readonly Func<string, Task<string>> _htmlGeneratorAsync;
        private readonly Func<string, Task<bool>> _pdfGeneratorAsync;

        public EmailDialog(string markdownContent,
                          Func<string, Task<string>> htmlGeneratorAsync,
                          Func<string, Task<bool>> pdfGeneratorAsync)
        {
            InitializeComponent();
            _markdownContent = markdownContent;
            _htmlGeneratorAsync = htmlGeneratorAsync;
            _pdfGeneratorAsync = pdfGeneratorAsync;

            SmtpServerCombo.SelectedIndex = 0;
            PortCombo.SelectedIndex = 0;
            FormatCombo.SelectedIndex = 0;
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(YourEmailBox.Text))
            {
                ShowStatus("❌ Введите вашу почту", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowStatus("❌ Введите пароль приложения", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(ToEmailBox.Text))
            {
                ShowStatus("❌ Введите адрес получателя", false);
                return;
            }

            SendButton.IsEnabled = false;
            SendButton.Content = "⏳ Отправка...";
            ShowStatus("⏳ Подготовка письма...", true);

            try
            {
                string format = ((ComboBoxItem)FormatCombo.SelectedItem).Content.ToString();
                string attachmentName = "";
                Stream attachmentStream = null;
                string htmlBody = "";

                ShowStatus($"📄 Создание файла в формате {format}...", true);

                // Обработка разных форматов
                if (format.Contains("Markdown"))
                {
                    attachmentName = "document.md";
                    attachmentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_markdownContent));
                    htmlBody = $@"<pre style='background:#f4f4f4;padding:15px;border-left:3px solid #DB1A1A;'>{System.Security.SecurityElement.Escape(_markdownContent)}</pre>";
                }
                else if (format.Contains("HTML"))
                {
                    attachmentName = "document.html";
                    string fullHtml = await _htmlGeneratorAsync(_markdownContent);
                    attachmentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fullHtml));
                    htmlBody = "<p>🌐 HTML-версия документа прикреплена к письму.</p>";
                }
                else if (format.Contains("PDF"))
                {
                    attachmentName = "document.pdf";
                    string tempPdfPath = Path.GetTempFileName() + ".pdf";
                    bool pdfSuccess = await _pdfGeneratorAsync(tempPdfPath);

                    if (!pdfSuccess || !File.Exists(tempPdfPath))
                    {
                        ShowStatus("❌ Не удалось создать PDF файл", false);
                        return;
                    }

                    attachmentStream = new MemoryStream(File.ReadAllBytes(tempPdfPath));
                    File.Delete(tempPdfPath);
                    htmlBody = "<p>📑 PDF-версия документа прикреплена к письму.</p>";
                }

                ShowStatus("✉️ Отправка письма...", true);
                await SendEmailAsync(attachmentStream, attachmentName, htmlBody);

                ShowStatus("✅ Письмо успешно отправлено!", true);
                await Task.Delay(1500);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowStatus($"❌ Ошибка: {ex.Message}", false);
            }
            finally
            {
                SendButton.IsEnabled = true;
                SendButton.Content = "✉️ Отправить";
            }
        }

        private async Task SendEmailAsync(Stream attachmentStream, string fileName, string htmlBody)
        {
            string smtpServer = (SmtpServerCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "smtp.gmail.com";
            int port = int.Parse((PortCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "587");
            string yourEmail = YourEmailBox.Text.Trim();
            string password = PasswordBox.Password;
            string toEmail = ToEmailBox.Text.Trim();
            string subject = SubjectBox.Text.Trim();
            string plainMessage = MessageBox.Text.Trim();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", yourEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            // Формируем HTML-тело письма
            string fullHtml = $@"
<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'><title>{subject}</title></head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: linear-gradient(135deg, #DB1A1A, #FF4D4D); padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>📧 Doctus</h1>
        <p style='color: #FFD4D4; margin: 5px 0 0;'>Markdown редактор</p>
    </div>
    <div style='background: #f9f9f9; padding: 20px; border-radius: 0 0 10px 10px; border: 1px solid #ddd; border-top: none;'>
        <p><strong>📨 Сообщение от отправителя:</strong></p>
        <blockquote style='background: #f4f4f4; padding: 15px; border-left: 4px solid #DB1A1A; margin: 10px 0; font-style: italic;'>
            {plainMessage}
        </blockquote>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
        <p><strong>📎 Вложение:</strong> {fileName}</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
        <div style='background: #f0f0f0; padding: 15px; border-radius: 8px;'>
            <p><strong>🔧 Детали:</strong></p>
            <ul style='color: #666; font-size: 12px;'>
                <li>Создано в Doctus</li>
                <li>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</li>
            </ul>
        </div>
        <p style='color: #999; font-size: 11px; text-align: center; margin-top: 20px;'>
            Отправлено через Doctus — современный Markdown редактор
        </p>
    </div>
</body>
</html>";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = fullHtml;

            // Добавляем вложение (не закрываем поток сразу, нужно для отправки)
            if (attachmentStream != null)
            {
                attachmentStream.Position = 0;
                bodyBuilder.Attachments.Add(fileName, attachmentStream);
            }

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(yourEmail, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            // Закрываем поток после отправки
            attachmentStream?.Dispose();
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = message;
                StatusText.Foreground = isSuccess ? System.Windows.Media.Brushes.LightGreen : System.Windows.Media.Brushes.OrangeRed;
            });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}