using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Багманов41
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private string currentCaptcha = "";
        private int failedAttempts = 0;
        private DispatcherTimer blockTimer;

        public AuthPage()
        {
            InitializeComponent();

            // Скрыть капчу при первом запуске
            HideCaptcha();

            // Инициализация таймера блокировки
            blockTimer = new DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(10);
            blockTimer.Tick += BlockTimer_Tick;
        }

        // Скрыть элементы капчи
        private void HideCaptcha()
        {
            if (captchaOneChar != null)
                captchaOneChar.Visibility = Visibility.Collapsed;
            if (captchaTwoChar != null)
                captchaTwoChar.Visibility = Visibility.Collapsed;
            if (captchaThreeChar != null)
                captchaThreeChar.Visibility = Visibility.Collapsed;
            if (captchaFourChar != null)
                captchaFourChar.Visibility = Visibility.Collapsed;
        }

        // Показать элементы капчи
        private void ShowCaptcha()
        {
            if (captchaOneChar != null)
                captchaOneChar.Visibility = Visibility.Visible;
            if (captchaTwoChar != null)
                captchaTwoChar.Visibility = Visibility.Visible;
            if (captchaThreeChar != null)
                captchaThreeChar.Visibility = Visibility.Visible;
            if (captchaFourChar != null)
                captchaFourChar.Visibility = Visibility.Visible;
        }

        // Генерация случайного символа (цифра или латинская буква)
        private char GenerateRandomChar()
        {
            Random random = new Random();

            // 50% вероятность цифры, 50% вероятность буквы
            if (random.Next(2) == 0)
            {
                // Цифра 0-9
                return (char)('0' + random.Next(10));
            }
            else
            {
                // Латинская буква A-Z или a-z
                if (random.Next(2) == 0)
                {
                    // Заглавная буква A-Z
                    return (char)('A' + random.Next(26));
                }
                else
                {
                    // Строчная буква a-z
                    return (char)('a' + random.Next(26));
                }
            }
        }

        // Генерация новой капчи
        private void GenerateCaptcha()
        {
            currentCaptcha = "";

            // Генерируем 4 случайных символа
            for (int i = 0; i < 4; i++)
            {
                currentCaptcha += GenerateRandomChar();
            }

            // Отображаем символы в TextBlock
            if (captchaOneChar != null)
                captchaOneChar.Text = currentCaptcha[0].ToString();
            if (captchaTwoChar != null)
                captchaTwoChar.Text = currentCaptcha[1].ToString();
            if (captchaThreeChar != null)
                captchaThreeChar.Text = currentCaptcha[2].ToString();
            if (captchaFourChar != null)
                captchaFourChar.Text = currentCaptcha[3].ToString();
        }

        // Проверка введенной капчи
        private bool CheckCaptcha(string userInput)
        {
            return userInput == currentCaptcha;
        }

        // Блокировка кнопки входа
        private void BlockLoginButton()
        {
            LoginBtn.IsEnabled = false;

            // Запускаем таймер блокировки
            blockTimer.Start();

            // Обновляем текст кнопки для отображения времени блокировки
            LoginBtn.Content = "Заблокировано (10 сек)";
        }

        // Разблокировка кнопки входа
        private void UnblockLoginButton()
        {
            LoginBtn.IsEnabled = true;
            LoginBtn.Content = "Войти";
        }

        // Обработчик таймера блокировки
        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            UnblockLoginButton();
            blockTimer.Stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            User guestUser = new User
            {
                UserLogin = "Гость",
                UserName = "Гость",
                UserRole = 0
            };

            NavigationService.Navigate(new ProductPage(guestUser));
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTB.Text;
            string password = PassTB.Text;

            if (login == "" || password == "")
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            // Если капча показана, проверяем ее
            if (captchaOneChar != null && captchaOneChar.Visibility == Visibility.Visible)
            {
                // Проверяем капчу
                string captchaInput = CaptchaInputTB.Text;
                if (!CheckCaptcha(captchaInput))
                {
                    MessageBox.Show("Неверная капча! Блокировка на 10 секунд.");
                    BlockLoginButton();
                    GenerateCaptcha(); // Генерируем новую капчу
                    CaptchaInputTB.Text = "";
                    return;
                }
            }

            User user = Bagmanov41Entities.GetContext().User.ToList()
                .Find(p => p.UserLogin == login && p.UserPassword == password);

            if (user != null)
            {
                // Сброс счетчика неудачных попыток
                failedAttempts = 0;
                HideCaptcha();

                // Скрываем поле ввода капчи
                if (CaptchaInputTB != null)
                {
                    CaptchaInputTB.Visibility = Visibility.Collapsed;
                }

                NavigationService.Navigate(new ProductPage(user));
                LoginTB.Text = "";
                PassTB.Text = "";
            }
            else
            {
                failedAttempts++;

                if (failedAttempts == 1)
                {
                    // Первая неудачная попытка - показываем капчу
                    MessageBox.Show("Неверный логин или пароль. Введите капчу.");
                    ShowCaptcha();
                    GenerateCaptcha();

                    // Показываем поле ввода капчи
                    if (CaptchaInputTB != null)
                    {
                        CaptchaInputTB.Visibility = Visibility.Visible;
                        CaptchaInputTB.Focus();
                    }
                }
                else if (failedAttempts >= 2)
                {
                    // Вторая и последующие неудачные попытки - блокировка
                    MessageBox.Show("Неверная авторизация! Блокировка на 10 секунд.");
                    BlockLoginButton();
                    GenerateCaptcha(); // Генерируем новую капчу
                }
            }
        }

        // Кнопка обновления капчи
        private void RefreshCaptchaBtn_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }
    }
}