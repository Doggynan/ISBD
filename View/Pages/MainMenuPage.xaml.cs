﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ISBD.Model;
using ISBD.ModelView.State;
using ISBD.Utils;

namespace ISBD.View.Pages
{
	/// <summary>
	/// Interaction logic for MainPage.xaml
	/// </summary>
	public partial class MainMenuPage : Page, IMainMenu
	{
		private Action<OsobaModel> OnSelectionChange;
		private Action<AddingNewItemEventArgs> OnItemAdd;
		private Action<HashSet<TransakcjaModel>> OnDelete;

		public MainMenuPage()
		{
			InitializeComponent();
			
			UsersViewsChooser.SelectionChanged += (sender, e) =>
			{
				OnSelectionChange?.Invoke((OsobaModel)UsersViewsChooser.SelectedItem);
			};

			HistoryTable.AddingNewItem += (sender, e) => { OnItemAdd?.Invoke(e); SortDataGrid(HistoryTable, 0, ListSortDirection.Descending); };
		}

		public void RegisterForSelectedUserChange(Action<OsobaModel> selectionAction)
		{
			OnSelectionChange += selectionAction;
		}

		public void UnregisterForSelectedUserChange(Action<OsobaModel> selectionAction)
		{
			OnSelectionChange -= selectionAction;
		}

		public void RegisterForAddingNewItem(Action<AddingNewItemEventArgs> newItemAction)
		{
			OnItemAdd += newItemAction;
		}

		public void UnregisterForAddingNewItem(Action<AddingNewItemEventArgs> newItemAction)
		{
			OnItemAdd -= newItemAction;
		}

		public void RegisterForDeleteRows(Action<HashSet<TransakcjaModel>> deleteRowsAction)
		{
			OnDelete += deleteRowsAction;
		}

		public void UnregisterForDeleteRows(Action<HashSet<TransakcjaModel>> deleteRowsAction)
		{
			OnDelete -= deleteRowsAction;
		}

		public List<TransakcjaModel> Transactions
		{
			set => HistoryTable.ItemsSource = value;
		}

		public List<OsobaModel> ValidUsers
		{
			set
			{
				UsersViewsChooser.ItemsSource = value;
				UsersViewsChooser.SelectedIndex = 0;
			}
		}

		public List<string> Categories
		{
			set => DataGridComboBoxColumn.ItemsSource = value;
		}
		public bool CanAdd
		{
			set => HistoryTable.CanUserAddRows = value;
		}

		public bool CanDelete { set => HistoryTable.CanUserDeleteRows = value; }

		public bool CanEdit { set => HistoryTable.IsReadOnly = !value; }

		private void DriversDataGrid_PreviewDeleteCommandHandler(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Command == DataGrid.DeleteCommand)
			{
				e.Handled = true;
				if (MessageBox.Show("Na pewno chcesz usunąć daną transakcję?", "Potwierdz!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					OnDelete?.Invoke( new HashSet<TransakcjaModel>(HistoryTable.SelectedCells.ToList().Select(cell => (TransakcjaModel)cell.Item)));
				}
			}
		}

		public static void SortDataGrid(DataGrid dataGrid, int columnIndex = 0, ListSortDirection sortDirection = ListSortDirection.Ascending)
		{
			var column = dataGrid.Columns[columnIndex];

			// Clear current sort descriptions
			dataGrid.Items.SortDescriptions.Clear();

			// Add the new sort description
			dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, sortDirection));

			// Apply sort
			foreach (var col in dataGrid.Columns)
			{
				col.SortDirection = null;
			}
			column.SortDirection = sortDirection;

			// Refresh items to display sort
			dataGrid.Items.Refresh();
		}
	}

	public class OsobaModel2NameConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var user = (OsobaModel)value;

			return $"{user.Imie} {user.Nazwisko} ({user.Login})";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class Double2Currency : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return string.Format(CultureInfo.CurrentUICulture, "{0:C2}", value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class IdK2Category : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Database.Database.Instance.Connect();

			long idK = (long) value;
			var category = Database.Database.Instance.SelectAll<KategoriaModel>().FirstOrDefault(cat => cat.IdK == idK);

			Database.Database.Instance.Dispose();

			return $"{category?.Nazwa}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Database.Database.Instance.Connect();

			string categoryName = (string)value;
			var category = Database.Database.Instance.SelectAll<KategoriaModel>().FirstOrDefault(cat => cat.Nazwa == categoryName);

			Database.Database.Instance.Dispose();
			return category.IdK;
		}
	}

	public class Transaction2Color : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is TransakcjaModel == false) return new SolidColorBrush(Colors.WhiteSmoke);
			Database.Database.Instance.Connect();

			TransakcjaModel tr = (TransakcjaModel)value;
			long idK = tr.IdK;
			var category = Database.Database.Instance.SelectAll<KategoriaModel>().FirstOrDefault(cat => cat.IdK == idK);
			if (category == null)
			{
				Database.Database.Instance.Dispose();
				return new SolidColorBrush(Colors.WhiteSmoke);
			}
			var symbol = Database.Database.Instance.SelectAll<SymbolModel>().FirstOrDefault(s => s.IdS == category.IdS);

			Database.Database.Instance.Dispose();

			var color = new SolidColorBrush(symbol?.Kolor ?? Colors.WhiteSmoke);

			return color;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class Transaction2ColorForeground : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is TransakcjaModel == false) return new SolidColorBrush(Colors.Black);
			Database.Database.Instance.Connect();

			TransakcjaModel tr = (TransakcjaModel)value;
			long idK = tr.IdK;
			var category = Database.Database.Instance.SelectAll<KategoriaModel>().FirstOrDefault(cat => cat.IdK == idK);
			if (category == null)
			{
				Database.Database.Instance.Dispose();
				return new SolidColorBrush(Colors.Black);
			}
			var symbol = Database.Database.Instance.SelectAll<SymbolModel>().FirstOrDefault(s => s.IdS == category.IdS);

			Database.Database.Instance.Dispose();

			if (symbol != null)
			{
				return new SolidColorBrush(symbol.Kolor.SaturatedValue() > 0.5f ? Colors.Black : Colors.White);
			}

			return new SolidColorBrush(Colors.Black);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
