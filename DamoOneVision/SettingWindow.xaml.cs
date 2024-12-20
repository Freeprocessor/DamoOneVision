using DamoOneVision.Services;
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
using System.Windows.Shapes;
using DamoOneVision.Data;

namespace DamoOneVision
{
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
		//string InfraredCameraModelName;
		//double CircleCenterX;
		//double CircleCenterY;
		//double CircleMinRadius;
		//double CircleMaxRadius;
		//double BinarizedThreshold;

		public InfraredCameraModel Model { get; private set; }


		public SettingWindow( InfraredCameraModel model = null )
		{
			InitializeComponent();
			if (model != null)
			{
				Model = model;
				InfraredCameraModelNameText.Text = model.Name;
				CircleCenterXText.Text = model.CircleCenterX.ToString();
				CircleCenterYText.Text = model.CircleCenterY.ToString();
				CircleMinRadiusText.Text = model.CircleMinRadius.ToString();
				CircleMaxRadiusText.Text = model.CircleMaxRadius.ToString();
				BinarizedThresholdText.Text = model.BinarizedThreshold.ToString();

			}
			else
			{
				Model = new InfraredCameraModel();

			}
		}

		private void textBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			//double.TryParse( CircleCenterXText.Text, out CircleCenterX );
			//double.TryParse( CircleCenterYText.Text, out CircleCenterY );
			//double.TryParse( CircleMinRadiusText.Text, out CircleMinRadius );
			//double.TryParse( CircleMaxRadiusText.Text, out CircleMaxRadius );
			//double.TryParse( BinarizedThresholdText.Text, out BinarizedThreshold );
		}

		private void SaveButton_Click( object sender, RoutedEventArgs e )
		{
			// 입력 검증
			if (string.IsNullOrWhiteSpace( InfraredCameraModelNameText.Text ))
			{
				MessageBox.Show( "모델 이름을 입력하세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning );
				return;
			}

			if (!double.TryParse( CircleCenterXText.Text, out double CircleCenterX ) ||
				!double.TryParse( CircleCenterYText.Text, out double CircleCenterY ) ||
				!double.TryParse( CircleMinRadiusText.Text, out double CircleMinRadius ) ||
				!double.TryParse( CircleMaxRadiusText.Text, out double CircleMaxRadius ) ||
				!double.TryParse( BinarizedThresholdText.Text, out double BinarizedThreshold ))
			{
				MessageBox.Show( "Threshold 값은 유효한 숫자여야 합니다.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning );
				return;
			}

			// 모델 데이터 설정
			Model.Name = InfraredCameraModelNameText.Text.Trim();
			Model.CircleCenterX = CircleCenterX;
			Model.CircleCenterY = CircleCenterY;
			Model.CircleMinRadius = CircleMinRadius;
			Model.CircleMaxRadius = CircleMaxRadius;
			Model.BinarizedThreshold = BinarizedThreshold;

			// 창 닫기 및 결과 반환
			this.DialogResult = true;
			this.Close();
		}

		private void CancelButton_Click( object sender, RoutedEventArgs e )
		{
			this.DialogResult = false;
			this.Close();
		}


	}
}
