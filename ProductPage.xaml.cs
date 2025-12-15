using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Багманов41
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public static List<Product> CurrentProducts = new List<Product>();
        private User _currentUser; // чтобы сохранить юзера
        private static Dictionary<string, int> _orderItems = new Dictionary<string, int>();

        public static event Action OnCartCleared;

        public static void ClearOrderItems()
        {
            _orderItems.Clear();
            OnCartCleared?.Invoke();
        }
        public ProductPage(User user)
        {
            InitializeComponent();

            // FIOTB - TextBlock для отображения ФИО
            FIOTB.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;

            // RoleTB - TextBlock для отображения роли
            switch (user.UserRole)
            {
                case 1:
                    RoleTB.Text = "Клиент";
                    break;
                case 2:
                    RoleTB.Text = "Менеджер";
                    break;
                case 3:
                    RoleTB.Text = "Администратор";
                    break;
            }

            var currentProducts = Bagmanov41Entities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProducts;

            ComboType.SelectedIndex = 0;
            UpdateProductes();
        }
        public static event EventHandler OrderItemsChanged;

        private static void OnOrderItemsChanged()
        {
            OrderItemsChanged?.Invoke(null, EventArgs.Empty);
        }
        private void AddToOrder(Product product)
        {
            string article = product.ProductArticleNumber;

            if (_orderItems.ContainsKey(article))
            {
                _orderItems[article]++;
            }
            else
            {
                _orderItems[article] = 1;
            }

            UpdateOrderButtonVisibility();
            MessageBox.Show($"Товар \"{product.ProductName}\" добавлен к заказу");
            OnOrderItemsChanged();
            UpdateOrderButtonVisibility();
        }
        private void UpdateOrderButtonVisibility()
        {
            // Простая проверка - если словарь пуст, скрываем кнопку
            bool hasItems = _orderItems.Count > 0;

            // Дополнительная проверка - считать общее количество
            int totalCount = _orderItems.Sum(item => item.Value);
            hasItems = totalCount > 0;

            ViewOrderBtn.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;

        }
        public ProductPage()
        {
            InitializeComponent();
            var currentProducts = Bagmanov41Entities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProducts;
            ComboType.SelectedIndex = 0;
            UpdateProductes();
        }
        private void UpdateItemCount()
        {
            var total = Bagmanov41Entities.GetContext().Product.Count();
            var displayed = ProductListView.Items.Count;
            ItemCountTextBlock.Text = $"кол-во {displayed} из {total}";
        }


        private void UpdateProductes()
        {
            var currentServices = Bagmanov41Entities.GetContext().Product.ToList();

            if (ComboType.SelectedIndex == 0)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 0 && Convert.ToInt32(p.ProductDiscountAmount) <= 100)).ToList();
            }

            if (ComboType.SelectedIndex == 1)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 0 && Convert.ToInt32(p.ProductDiscountAmount) <= 9.99)).ToList();
            }

            if (ComboType.SelectedIndex == 2)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 10 && Convert.ToInt32(p.ProductDiscountAmount) <= 14.99)).ToList();
            }
            if (ComboType.SelectedIndex == 3)
            {
                currentServices = currentServices.Where(p => (Convert.ToInt32(p.ProductDiscountAmount) >= 15)).ToList();
            }

            currentServices = currentServices.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            ProductListView.ItemsSource = currentServices.ToList();

            if (RButtonDown.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentServices.OrderByDescending(p => p.ProductCost).ToList();
            }

            if (RButtonUp.IsChecked.Value)
            {
                ProductListView.ItemsSource = currentServices.OrderBy(p => p.ProductCost).ToList();
            }
            UpdateItemCount();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProductes();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProductes();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProductes();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProductes();
        }

        private void AddToOrderMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedItem is Product selectedProduct)
            {
                AddToOrder(selectedProduct);
            }
        }

        private void ViewOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrderProducts = new List<OrderProduct>();
            var selectedProducts = new List<Product>();

            foreach (var item in _orderItems)
            {
                var product = Bagmanov41Entities.GetContext().Product
                    .FirstOrDefault(p => p.ProductArticleNumber == item.Key);

                if (product != null)
                {
                    selectedProducts.Add(product);
                    var orderProduct = new OrderProduct
                    {
                        ProductArticleNumber = product.ProductArticleNumber,
                        Count = item.Value
                    };
                    selectedOrderProducts.Add(orderProduct);
                }
            }

            var orderWindow = new OrderWindow(selectedOrderProducts, selectedProducts, _currentUser);
            orderWindow.Owner = Application.Current.MainWindow;

            // Подписываемся на событие закрытия окна
            orderWindow.Closed += (s, args) =>
            {
                // Когда окно закрывается (любым способом)
                UpdateOrderButtonVisibility();

                // Дополнительная проверка
                if (_orderItems.Count == 0)
                {
                    ViewOrderBtn.Visibility = Visibility.Collapsed;
                }
            };

            orderWindow.ShowDialog(); // Не используем результат, т.к. слушаем событие Closed

            // ИЛИ просто всегда обновляем после ShowDialog
            UpdateOrderButtonVisibility();
        }

       
    }
}
