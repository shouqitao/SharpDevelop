﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ICSharpCode.SharpDevelop.Gui
{
	/// <summary>
	/// Interaction logic for FontSelector.xaml
	/// </summary>
	public partial class FontSelector : UserControl, INotifyPropertyChanged
	{
		static readonly int[] fontSizes;
		
		static FontSelector()
		{
			fontSizes = Enumerable.Range(6, 19).ToArray();
		}
		
		public FontSelector()
		{
			DataContext = Fonts.SystemFontFamilies
				.OrderBy(ff => ff.Source)
				.Select(ff => new FontFamilyInfo(ff))
				.ToArray();
			InitializeComponent();
		}
		
		public static int[] FontSizes {
			get {
				return fontSizes;
			}
		}
		
		FontFamily selectedFontFamily;
		
		public FontFamily SelectedFontFamily {
			get { return selectedFontFamily; }
			set {
				if (selectedFontFamily != value) {
					selectedFontFamily = value;
					OnPropertyChanged("SelectedFontName");
					OnPropertyChanged();
				}
			}
		}
		
		public string SelectedFontName {
			get { return selectedFontFamily == null ? null : selectedFontFamily.Source; }
			set {
				if (selectedFontFamily == null || selectedFontFamily.Source != value) {
					selectedFontFamily = new FontFamily(value);
					OnPropertyChanged();
					OnPropertyChanged("SelectedFontFamily");
				}
			}
		}
		
		int selectedFontSize;
		
		public int SelectedFontSize {
			get { return selectedFontSize; }
			set {
				if (selectedFontSize != value) {
					selectedFontSize = value;
					OnPropertyChanged();
				}
			}
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged([CallerMemberName] string property = null)
		{
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}
		
		class FontFamilyInfo : INotifyPropertyChanged
		{
			public FontFamilyInfo(FontFamily fontFamily)
			{
				if (fontFamily == null)
					throw new ArgumentNullException("fontFamily");
				this.FontFamily = fontFamily;
				DetectMonospaced();
			}
			
			async void DetectMonospaced()
			{
				this.IsMonospaced = await Task.Run(() => DetectMonospaced(this.FontFamily));
			}

			bool DetectMonospaced(FontFamily fontFamily)
			{
				var tf = fontFamily.GetTypefaces().FirstOrDefault(t => t.Style == FontStyles.Normal);
				if (tf == null)
					return false;
				// determine if the length of i == m because I see no other way of
				// getting if a font is monospaced or not.
				FormattedText formatted = new FormattedText("i.", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
				                                            tf, 12f, Brushes.Black);
				FormattedText formatted2 = new FormattedText("mw", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
				                                             tf, 12f, Brushes.Black);
				return formatted.Width == formatted2.Width;
			}

			public FontFamily FontFamily { get; private set; }
			public string FontName {
				get { return FontFamily.Source; }
			}
			
			bool isMonospaced;
			
			public bool IsMonospaced {
				get { return isMonospaced; }
				private set {
					if (isMonospaced != value) {
						isMonospaced = value;
						OnPropertyChanged();
					}
				}
			}
			
			public event PropertyChangedEventHandler PropertyChanged;
			
			protected virtual void OnPropertyChanged([CallerMemberName] string property = null)
			{
				if (PropertyChanged != null) {
					PropertyChanged(this, new PropertyChangedEventArgs(property));
				}
			}
		}
	}
	
	#region Converters
	[ValueConversion(typeof(int), typeof(double))]
	public class SDFontSizeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			if (value is int)
				return ToDouble((int)value);
			throw new NotSupportedException("Cannot convert value of type " + value.GetType());
		}
		
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			if (value is double)
				return FromDouble((double)value);
			throw new NotSupportedException("Cannot convert value of type " + value.GetType());
		}
		
		public static int FromDouble(double value)
		{
			return (int)Math.Round(value * 72.0 / 96.0);
		}
		
		public static double ToDouble(int value)
		{
			return Math.Round(value * 96.0 / 72.0);
		}
	}
	
	[ValueConversion(typeof(bool), typeof(FontWeight))]
	class BoolToFontWeightConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value is bool) {
				return (bool)value ? FontWeights.Bold : FontWeights.Normal;
			}
			throw new NotSupportedException();
		}
		
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
	#endregion
}