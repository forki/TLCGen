﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TLCGen.Views.Resources
{
    public class SpecialDataGrid : DataGrid
    {
        public Style SpecialGridCellStyle { get; set; }

        public SpecialDataGrid() : base()
        {
            this.PreviewMouseLeftButtonDown += DataGrid_PreviewMouseLeftButtonDown;
            this.PreviewTextInput += DataGrid_PreviewTextInput;

            // obsolete; doing this on datagrid level is better, cause it allows to use the cellstyle in xaml without overwriting this one
            //SpecialGridCellStyle = new Style(typeof(DataGridCell));
            //SpecialGridCellStyle.Setters.Add(new EventSetter(DataGridCell.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(DataGridCell_PreviewMouseLeftButtonDown)));
            //SpecialGridCellStyle.Setters.Add(new EventSetter(DataGridCell.PreviewTextInputEvent, new TextCompositionEventHandler(DataGridCell_PreviewTextInput)));
            //this.CellStyle = SpecialGridCellStyle;
        }

        public void DataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree
            while ((dep != null) &&
                  !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                GridColumnFastEdit(cell, e);
            }
        }

        public void DataGrid_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree
            while ((dep != null) &&
                  !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                GridColumnFastEdit(cell, e);
            }
        }

        private static void GridColumnFastEdit(DataGridCell cell, RoutedEventArgs e)
        {
            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
            if (dataGrid == null)
                return;

            if (!cell.IsFocused)
            {
                cell.Focus();
            }

            if (cell.Content is CheckBox)
            {
                if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                {
                    if (!cell.IsSelected)
                        cell.IsSelected = true;
                }
                else
                {
                    DataGridRow row = FindVisualParent<DataGridRow>(cell);
                    if (row != null && !row.IsSelected)
                    {
                        row.IsSelected = true;
                    }
                }
            }
            else
            {
                ComboBox cb = cell.Content as ComboBox;
                if (cb != null)
                {
                    //DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                    dataGrid.BeginEdit(e);
                    //cell.Dispatcher.Invoke(
                    // DispatcherPriority.Background,
                    // new Action(delegate { }));
                    //cb.IsDropDownOpen = false;
                }
            }
        }


        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
    }
}
