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
        public ProductPage()
        {
            InitializeComponent();

            var currentProducts = Bagmanov41Entities.GetContext().Product.ToList();

            ProductListView.ItemsSource = currentProducts;

            ComboType.SelectedIndex = 0;
            UpdateProductes();
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

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProductes();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProductes();
        }

        private void RButtonDown_Checked_1(object sender, RoutedEventArgs e)
        {
            UpdateProductes();
        }

        private void RButtonUp_Checked_1(object sender, RoutedEventArgs e)
        {
            UpdateProductes();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
