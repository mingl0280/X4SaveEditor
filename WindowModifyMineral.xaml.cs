using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Shapes;

namespace X4SaveEditor
{
    /// <summary>
    /// Interaction logic for WindowModifyMineral.xaml
    /// </summary>
    public partial class WindowModifyMineral : Window
    {
        private const int DefaultValue = 49500;
        private const int DefaultTime = 36000;

        public int RespawnTime
        {
            get;set;
        }
        public int RechargeValue
        {
            get; set;
        }

        public WindowModifyMineral()
        {
            InitializeComponent();
            RechargeModifierInput.TextChanged += UpdateRechargeValue;
            RechargeValueInput.TextChanged += UpdateRechargeModifier;

            RespawnTimeInput.KeyUp += ValidateRespawnTimeInput;
            RechargeModifierInput.KeyUp += ValidateRechargeModifierInput;
            RechargeValueInput.KeyUp += ValidateRechargeValueInput;
        }
        private void ValidateRespawnTimeInput(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (int.TryParse(RespawnTimeInput.Text, out _) && int.Parse(RespawnTimeInput.Text) > 0)
            {
                RespawnTimeInput.BorderBrush = Brushes.Gray; // Valid input
            }
            else
            {
                RespawnTimeInput.BorderBrush = Brushes.Red; // Invalid input
            }
        }
        private void ValidateRechargeModifierInput(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (double.TryParse(RechargeModifierInput.Text, out var value) && value > 0)
            {
                RechargeModifierInput.BorderBrush = Brushes.Gray; // Valid input
            }
            else
            {
                RechargeModifierInput.BorderBrush = Brushes.Red; // Invalid input
            }
            if ((sender as TextBox) == RechargeValueInput)
            {
                return;
            }
            ValidateRechargeValueInput(sender, e);
        }

        private void ValidateRechargeValueInput(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (int.TryParse(RechargeValueInput.Text, out var value) && value > 0)
            {
                RechargeValueInput.BorderBrush = Brushes.Gray; // Valid input
            }
            else
            {
                RechargeValueInput.BorderBrush = Brushes.Red; // Invalid input
            }
            if ((sender as TextBox) == RechargeModifierInput)
            {
                return;
            }
            ValidateRechargeModifierInput(sender, e);
        }

        private void UpdateRechargeValue(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(RechargeModifierInput.Text, out var modifier))
            {
                RechargeValueInput.TextChanged -= UpdateRechargeModifier;
                RechargeValueInput.Text = (DefaultValue * modifier).ToString(CultureInfo.InvariantCulture);
                RechargeValueInput.TextChanged += UpdateRechargeModifier;
            }
        }

        private void UpdateRechargeModifier(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(RechargeValueInput.Text, out var value))
            {
                RechargeModifierInput.TextChanged -= UpdateRechargeValue;
                RechargeModifierInput.Text = (value / (double)DefaultValue).ToString("F3", CultureInfo.InvariantCulture);
                RechargeModifierInput.TextChanged += UpdateRechargeValue;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            RespawnTimeInput.Text = "36000"; // Default value
            RechargeModifierInput.Text = "1.000"; // Default value
            RechargeValueInput.Text = "49500"; // Default value
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(RespawnTimeInput.Text, out _) || int.Parse(RespawnTimeInput.Text) <= 0)
            {
                var result = MessageBox.Show("Invalid respawn time value. Hit OK to return and edit, hit cancel to use cancel edit.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    DialogResult = false;
                    Close();
                }
            }
            if (!double.TryParse(RechargeModifierInput.Text, out var value) || value <= 0)
            {
                var result = MessageBox.Show("Invalid recharge modifier value. Hit OK to return and edit, hit cancel to use cancel edit.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    DialogResult = false;
                    Close();
                }
            }
            if (!int.TryParse(RechargeValueInput.Text, out var rechargeValue) || rechargeValue <= 0)
            {
                var result = MessageBox.Show("Invalid recharge value. Hit OK to return and edit, hit cancel to use cancel edit.", "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    DialogResult = false;
                    Close();
                }
            }
            RespawnTime = int.Parse(RespawnTimeInput.Text);
            RechargeValue = int.Parse(RechargeValueInput.Text);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
